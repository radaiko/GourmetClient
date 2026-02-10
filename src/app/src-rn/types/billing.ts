/** A single item within a bill (e.g., one meal) */
export interface GourmetBillingItem {
  id: string;
  articleId: string;
  count: number;
  description: string;
  total: number;
  subsidy: number;
  discountValue: number;
  isCustomMenu: boolean;
}

/** A bill (receipt) for a single transaction */
export interface GourmetBill {
  billNr: number;
  billDate: Date;
  location: string;
  items: GourmetBillingItem[];
  billing: number; // Total billed amount after subsidy/discount
}

/** Billing data for a specific month */
export interface GourmetMonthlyBilling {
  /** Month key in "YYYY-MM" format */
  monthKey: string;
  /** Display label (e.g., "Jänner 2026") */
  label: string;
  bills: GourmetBill[];
  totalGross: number;
  totalSubsidy: number;
  totalDiscount: number;
  totalBilling: number;
  fetchedAt: number; // timestamp
}
