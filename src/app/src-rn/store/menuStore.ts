import { create } from 'zustand';
import { GourmetMenuItem, GourmetDayMenu, GourmetMenuCategory } from '../types/menu';
import { useAuthStore } from './authStore';
import { useOrderStore } from './orderStore';
import { MENU_CACHE_VALIDITY_MS } from '../utils/constants';
import { isSameDay, isOrderingCutoff, localDateKey } from '../utils/dateUtils';

const MAIN_MENU_CATEGORIES = new Set([
  GourmetMenuCategory.Menu1,
  GourmetMenuCategory.Menu2,
  GourmetMenuCategory.Menu3,
]);

export type OrderProgress = 'adding' | 'confirming' | 'cancelling' | 'refreshing' | null;

interface MenuState {
  items: GourmetMenuItem[];
  lastFetched: number | null;
  loading: boolean;
  refreshing: boolean;
  error: string | null;
  selectedDate: Date;
  pendingOrders: Set<string>; // Set of "menuId|dateStr" keys for items to order
  orderProgress: OrderProgress; // Non-blocking background order step

  fetchMenus: (force?: boolean) => Promise<void>;
  refreshAvailability: () => Promise<void>;
  setSelectedDate: (date: Date) => void;
  togglePendingOrder: (menuId: string, date: Date) => void;
  clearPendingOrders: () => void;
  submitOrders: () => Promise<void>;
  getAvailableDates: () => Date[];
  getMenusForDate: (date: Date) => GourmetMenuItem[];
  getDayMenus: () => GourmetDayMenu[];
  getPendingCount: () => number;
}

function makePendingKey(menuId: string, date: Date): string {
  return `${menuId}|${localDateKey(date)}`;
}

export const useMenuStore = create<MenuState>((set, get) => ({
  items: [],
  lastFetched: null,
  loading: false,
  refreshing: false,
  error: null,
  selectedDate: new Date(),
  pendingOrders: new Set(),
  orderProgress: null,

  fetchMenus: async (force = false) => {
    const { lastFetched, loading } = get();
    if (loading) return;

    // Check cache validity
    if (!force && lastFetched && Date.now() - lastFetched < MENU_CACHE_VALIDITY_MS) {
      return;
    }

    set({ loading: true, error: null });
    try {
      const api = useAuthStore.getState().api;
      const items = await api.getMenus();
      set({ items, lastFetched: Date.now(), loading: false });

      // Auto-select the first available date only if current selection is gone
      const dates = get().getAvailableDates();
      const current = get().selectedDate;
      const stillExists = dates.some((d) => d.toDateString() === current.toDateString());
      if (dates.length > 0 && !stillExists) {
        set({ selectedDate: dates[0] });
      }
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Menüs konnten nicht geladen werden';
      set({ error: message, loading: false });
    }
  },

  /**
   * Background refresh: fetch fresh data and merge only available/ordered
   * into the cached items. No loading spinner, no replacing static fields.
   */
  refreshAvailability: async () => {
    const { refreshing, items } = get();
    if (refreshing || items.length === 0) return;

    const minVisible = new Promise((r) => setTimeout(r, 800));
    set({ refreshing: true });
    try {
      const api = useAuthStore.getState().api;
      const freshItems = await api.getMenus();

      // Build lookup from fresh data
      const freshMap = new Map<string, GourmetMenuItem>();
      for (const item of freshItems) {
        freshMap.set(`${item.id}|${localDateKey(item.day)}`, item);
      }

      // Merge: keep cached static fields, update only available/ordered
      const merged = get().items.map((cached) => {
        const key = `${cached.id}|${localDateKey(cached.day)}`;
        const fresh = freshMap.get(key);
        if (fresh) {
          freshMap.delete(key);
          return { ...cached, available: fresh.available, ordered: fresh.ordered };
        }
        return cached;
      });

      // Append any brand-new items not in cache
      for (const fresh of freshMap.values()) {
        merged.push(fresh);
      }

      // Keep banner visible for at least 800ms so the user notices it
      await minVisible;
      set({ items: merged, lastFetched: Date.now(), refreshing: false });
    } catch {
      await minVisible;
      // Silent fail — cached data remains visible
      set({ refreshing: false });
    }
  },

  setSelectedDate: (date: Date) => set({ selectedDate: date }),

  togglePendingOrder: (menuId: string, date: Date) => {
    const key = makePendingKey(menuId, date);
    const pending = new Set(get().pendingOrders);
    if (pending.has(key)) {
      pending.delete(key);
    } else {
      // Enforce: only 1 main menu (I/II/III) per day
      const item = get().items.find((i) => i.id === menuId && isSameDay(i.day, date));
      if (item && MAIN_MENU_CATEGORIES.has(item.category)) {
        const dateKey = localDateKey(date);
        // Remove any other main menu pending for this date
        for (const existingKey of pending) {
          const [existingId, existingDateStr] = existingKey.split('|');
          if (existingDateStr !== dateKey) continue;
          const existingItem = get().items.find(
            (i) => i.id === existingId && localDateKey(i.day) === dateKey
          );
          if (existingItem && MAIN_MENU_CATEGORIES.has(existingItem.category)) {
            pending.delete(existingKey);
          }
        }
      }
      pending.add(key);
    }
    set({ pendingOrders: pending });
  },

  clearPendingOrders: () => set({ pendingOrders: new Set() }),

  submitOrders: async () => {
    const { pendingOrders } = get();
    if (pendingOrders.size === 0) return;

    const api = useAuthStore.getState().api;
    const orderItems = Array.from(pendingOrders).map((key) => {
      const [menuId, dateStr] = key.split('|');
      const [y, m, d] = dateStr.split('-').map(Number);
      return { menuId, date: new Date(y, m - 1, d) };
    });

    // Block today's orders after 12:30 Vienna time
    const blocked = orderItems.filter((i) => isOrderingCutoff(i.date));
    if (blocked.length > 0) {
      set({ error: 'Bestellung für heute geschlossen (Bestellschluss 12:30)' });
      return;
    }

    // Optimistically mark items as ordered and clear pending selection
    const orderedKeys = new Set(pendingOrders);
    const optimisticItems = get().items.map((item) => {
      const key = makePendingKey(item.id, item.day);
      if (orderedKeys.has(key)) {
        return { ...item, ordered: true };
      }
      return item;
    });
    set({ items: optimisticItems, pendingOrders: new Set(), error: null, orderProgress: 'adding' });

    try {
      await api.addToCart(orderItems);

      set({ orderProgress: 'confirming' });
      await api.confirmOrders();

      // Fetch orders first so the UI has authoritative order data
      // even if the menu page hasn't updated the checkboxes yet
      set({ orderProgress: 'refreshing' });
      await useOrderStore.getState().fetchOrders();
      await get().fetchMenus(true);

      set({ orderProgress: null });
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Bestellung konnte nicht aufgegeben werden';
      set({ error: message, orderProgress: null });
      // Revert optimistic update on failure (silent, keep error visible)
      try {
        const freshApi = useAuthStore.getState().api;
        const freshItems = await freshApi.getMenus();
        set({ items: freshItems, lastFetched: Date.now() });
      } catch {
        // Silent — keep optimistic state if revert also fails
      }
    }
  },

  getAvailableDates: () => {
    const { items } = get();
    const dateSet = new Map<string, Date>();
    for (const item of items) {
      const key = localDateKey(item.day);
      if (!dateSet.has(key)) {
        dateSet.set(key, item.day);
      }
    }
    return Array.from(dateSet.values()).sort((a, b) => a.getTime() - b.getTime());
  },

  getMenusForDate: (date: Date) => {
    return get().items.filter((item) => isSameDay(item.day, date));
  },

  getDayMenus: () => {
    const dates = get().getAvailableDates();
    return dates.map((date) => ({
      date,
      items: get().getMenusForDate(date),
    }));
  },

  getPendingCount: () => get().pendingOrders.size,
}));
