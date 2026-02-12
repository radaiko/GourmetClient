import { create } from 'zustand';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { useAuthStore } from './authStore';
import { useVentopayAuthStore } from './ventopayAuthStore';
import { GourmetBill, GourmetMonthlyBilling } from '../types/billing';
import { VentopayTransaction, VentopayMonthlyBilling } from '../types/ventopay';

const GOURMET_CACHE_PREFIX = 'billing_';
const VENTOPAY_CACHE_PREFIX = 'ventopay_billing_';

/** Build a "YYYY-MM" key for a month offset from today. */
function monthKeyFromOffset(offset: number): string {
  const now = new Date();
  const target = new Date(now.getFullYear(), now.getMonth() - offset, 1);
  const yyyy = target.getFullYear();
  const mm = String(target.getMonth() + 1).padStart(2, '0');
  return `${yyyy}-${mm}`;
}

/** German month label from a "YYYY-MM" key. */
function monthLabel(key: string): string {
  const [yyyy, mm] = key.split('-');
  const months = [
    'Jänner', 'Februar', 'März', 'April', 'Mai', 'Juni',
    'Juli', 'August', 'September', 'Oktober', 'November', 'Dezember',
  ];
  return `${months[parseInt(mm, 10) - 1]} ${yyyy}`;
}

function computeGourmetTotals(bills: GourmetBill[]) {
  let totalGross = 0;
  let totalSubsidy = 0;
  let totalDiscount = 0;
  let totalBilling = 0;
  for (const bill of bills) {
    for (const item of bill.items) {
      totalGross += item.total;
      totalSubsidy += item.subsidy;
      totalDiscount += item.discountValue;
    }
    totalBilling += bill.billing;
  }
  return { totalGross, totalSubsidy, totalDiscount, totalBilling };
}

function computeVentopayTotal(transactions: VentopayTransaction[]): number {
  return transactions.reduce((sum, t) => sum + t.amount, 0);
}

/** Serialize Gourmet bills for AsyncStorage (Date -> ISO string). */
function serializeBills(bills: GourmetBill[]): string {
  return JSON.stringify(bills.map((b) => ({
    ...b,
    billDate: b.billDate.toISOString(),
  })));
}

/** Deserialize Gourmet bills from AsyncStorage (ISO string -> Date). */
function deserializeBills(json: string): GourmetBill[] {
  return JSON.parse(json).map((b: any) => ({
    ...b,
    billDate: new Date(b.billDate),
  }));
}

/** Serialize Ventopay transactions for AsyncStorage. */
function serializeTransactions(transactions: VentopayTransaction[]): string {
  return JSON.stringify(transactions.map((t) => ({
    ...t,
    date: t.date.toISOString(),
  })));
}

/** Deserialize Ventopay transactions from AsyncStorage. */
function deserializeTransactions(json: string): VentopayTransaction[] {
  return JSON.parse(json).map((t: any) => ({
    ...t,
    date: new Date(t.date),
  }));
}

/** Get first and last day of a month from a "YYYY-MM" key. */
function monthDateRange(key: string): { from: Date; to: Date } {
  const [yyyy, mm] = key.split('-').map(Number);
  const from = new Date(yyyy, mm - 1, 1);
  const to = new Date(yyyy, mm, 0); // last day of month
  return { from, to };
}

export type BillingSource = 'all' | 'gourmet' | 'ventopay';

interface BillingState {
  /** Gourmet billing data per month, keyed by "YYYY-MM". */
  gourmetMonths: Record<string, GourmetMonthlyBilling>;
  /** Ventopay billing data per month, keyed by "YYYY-MM". */
  ventopayMonths: Record<string, VentopayMonthlyBilling>;
  /** Which month is currently selected (index 0-2). */
  selectedMonthIndex: number;
  /** Active source filter. */
  sourceFilter: BillingSource;
  loading: boolean;
  error: string | null;

  /** Available month options (current + 2 past). */
  getMonthOptions: () => { key: string; label: string; offset: number }[];
  /** Get the currently selected month's Gourmet billing. */
  getSelectedGourmetBilling: () => GourmetMonthlyBilling | null;
  /** Get the currently selected month's Ventopay billing. */
  getSelectedVentopayBilling: () => VentopayMonthlyBilling | null;

  fetchBilling: (monthOffset?: number) => Promise<void>;
  fetchVentopayBilling: (monthOffset?: number) => Promise<void>;
  selectMonth: (index: number) => void;
  setSourceFilter: (source: BillingSource) => void;
  loadCachedMonths: () => Promise<void>;
}

export const useBillingStore = create<BillingState>((set, get) => ({
  gourmetMonths: {},
  ventopayMonths: {},
  selectedMonthIndex: 0,
  sourceFilter: 'all',
  loading: false,
  error: null,

  getMonthOptions: () =>
    [0, 1, 2].map((offset) => {
      const key = monthKeyFromOffset(offset);
      return { key, label: monthLabel(key), offset };
    }),

  getSelectedGourmetBilling: () => {
    const options = get().getMonthOptions();
    const opt = options[get().selectedMonthIndex];
    return opt ? get().gourmetMonths[opt.key] ?? null : null;
  },

  getSelectedVentopayBilling: () => {
    const options = get().getMonthOptions();
    const opt = options[get().selectedMonthIndex];
    return opt ? get().ventopayMonths[opt.key] ?? null : null;
  },

  selectMonth: (index: number) => {
    set({ selectedMonthIndex: index });
    get().fetchBilling(index);
    get().fetchVentopayBilling(index);
  },

  setSourceFilter: (source: BillingSource) => {
    set({ sourceFilter: source });
  },

  /** Load any previously cached months from AsyncStorage on startup. */
  loadCachedMonths: async () => {
    const options = get().getMonthOptions();
    const gourmetMonths: Record<string, GourmetMonthlyBilling> = {};
    const ventopayMonths: Record<string, VentopayMonthlyBilling> = {};

    for (const opt of options) {
      // Load Gourmet cache
      const gourmetCached = await AsyncStorage.getItem(GOURMET_CACHE_PREFIX + opt.key);
      if (gourmetCached) {
        const bills = deserializeBills(gourmetCached);
        const totals = computeGourmetTotals(bills);
        gourmetMonths[opt.key] = {
          monthKey: opt.key,
          label: opt.label,
          bills,
          ...totals,
          fetchedAt: 0,
        };
      }

      // Load Ventopay cache
      const ventopayCached = await AsyncStorage.getItem(VENTOPAY_CACHE_PREFIX + opt.key);
      if (ventopayCached) {
        const transactions = deserializeTransactions(ventopayCached);
        ventopayMonths[opt.key] = {
          monthKey: opt.key,
          label: opt.label,
          transactions,
          total: computeVentopayTotal(transactions),
          fetchedAt: 0,
        };
      }
    }

    set({
      gourmetMonths: { ...get().gourmetMonths, ...gourmetMonths },
      ventopayMonths: { ...get().ventopayMonths, ...ventopayMonths },
    });
  },

  fetchBilling: async (monthOffset?: number) => {
    const offset = monthOffset ?? get().selectedMonthIndex;
    const options = get().getMonthOptions();
    const opt = options[offset];
    if (!opt) return;

    if (get().loading) return;

    const isCurrentMonth = offset === 0;
    const existing = get().gourmetMonths[opt.key];

    // For past months, if we already have cached data, skip the fetch
    if (!isCurrentMonth && existing && existing.bills.length > 0) {
      return;
    }

    set({ loading: true, error: null });
    try {
      const api = useAuthStore.getState().api;
      const bills = await api.getBillings(String(opt.offset));
      const totals = computeGourmetTotals(bills);

      const monthData: GourmetMonthlyBilling = {
        monthKey: opt.key,
        label: opt.label,
        bills,
        ...totals,
        fetchedAt: Date.now(),
      };

      await AsyncStorage.setItem(GOURMET_CACHE_PREFIX + opt.key, serializeBills(bills));

      set({
        gourmetMonths: { ...get().gourmetMonths, [opt.key]: monthData },
        loading: false,
      });
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Abrechnung konnte nicht geladen werden';
      set({ error: message, loading: false });
    }
  },

  fetchVentopayBilling: async (monthOffset?: number) => {
    const offset = monthOffset ?? get().selectedMonthIndex;
    const options = get().getMonthOptions();
    const opt = options[offset];
    if (!opt) return;

    const isCurrentMonth = offset === 0;
    const existing = get().ventopayMonths[opt.key];

    // For past months, if we already have cached data, skip the fetch
    if (!isCurrentMonth && existing && existing.transactions.length > 0) {
      return;
    }

    // Check if Ventopay is authenticated
    const ventopayAuth = useVentopayAuthStore.getState();
    if (ventopayAuth.status !== 'authenticated') return;

    try {
      const { from, to } = monthDateRange(opt.key);
      const transactions = await ventopayAuth.api.getTransactions(from, to);
      const total = computeVentopayTotal(transactions);

      const monthData: VentopayMonthlyBilling = {
        monthKey: opt.key,
        label: opt.label,
        transactions,
        total,
        fetchedAt: Date.now(),
      };

      await AsyncStorage.setItem(
        VENTOPAY_CACHE_PREFIX + opt.key,
        serializeTransactions(transactions)
      );

      set({
        ventopayMonths: { ...get().ventopayMonths, [opt.key]: monthData },
      });
    } catch (err) {
      // Don't overwrite Gourmet errors; Ventopay failures are non-blocking
      console.warn('Ventopay billing fetch failed:', err);
    }
  },
}));
