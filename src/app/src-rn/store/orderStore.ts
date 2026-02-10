import { create } from 'zustand';
import { GourmetOrderedMenu } from '../types/order';
import { useAuthStore } from './authStore';

interface OrderState {
  orders: GourmetOrderedMenu[];
  loading: boolean;
  cancellingId: string | null; // positionId currently being cancelled
  error: string | null;

  fetchOrders: () => Promise<void>;
  confirmOrders: () => Promise<void>;
  cancelOrder: (positionId: string) => Promise<void>;
  getUpcomingOrders: () => GourmetOrderedMenu[];
  getPastOrders: () => GourmetOrderedMenu[];
  getUnconfirmedCount: () => number;
}

export const useOrderStore = create<OrderState>((set, get) => ({
  orders: [],
  loading: false,
  cancellingId: null,
  error: null,

  fetchOrders: async () => {
    if (get().loading) return;

    set({ loading: true, error: null });
    try {
      const api = useAuthStore.getState().api;
      const orders = await api.getOrders();
      set({ orders, loading: false });
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to fetch orders';
      set({ error: message, loading: false });
    }
  },

  confirmOrders: async () => {
    set({ loading: true, error: null });
    try {
      const api = useAuthStore.getState().api;
      await api.confirmOrders();
      set({ loading: false });
      // Refresh orders to reflect confirmed state
      await get().fetchOrders();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to confirm orders';
      set({ error: message, loading: false });
    }
  },

  cancelOrder: async (positionId: string) => {
    set({ cancellingId: positionId, error: null });
    try {
      const api = useAuthStore.getState().api;
      await api.cancelOrders([positionId]);
      set({ cancellingId: null });
      // Refresh orders after cancellation
      await get().fetchOrders();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to cancel order';
      set({ error: message, cancellingId: null });
    }
  },

  getUpcomingOrders: () => {
    const now = new Date();
    now.setHours(0, 0, 0, 0);
    return get().orders.filter((o) => o.date >= now);
  },

  getPastOrders: () => {
    const now = new Date();
    now.setHours(0, 0, 0, 0);
    return get().orders.filter((o) => o.date < now);
  },

  getUnconfirmedCount: () => get().orders.filter((o) => !o.approved).length,
}));
