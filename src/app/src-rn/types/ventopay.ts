/** A single transaction from the Ventopay system (vending machines, POS) */
export interface VentopayTransaction {
  id: string;
  date: Date;
  amount: number;
  restaurant: string;
  location: string;
}

/** Monthly billing data aggregated from Ventopay transactions */
export interface VentopayMonthlyBilling {
  monthKey: string;
  label: string;
  transactions: VentopayTransaction[];
  total: number;
  fetchedAt: number;
}
