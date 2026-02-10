export interface GourmetOrderedMenu {
  positionId: string;
  eatingCycleId: string;
  date: Date;
  title: string;
  subtitle: string;
  approved: boolean;
}

export interface OrderDateGroup {
  date: Date;
  orders: GourmetOrderedMenu[];
}
