export interface GourmetMenuItem {
  id: string;
  day: Date;
  title: string;
  subtitle: string;
  allergens: string[];
  available: boolean;
  ordered: boolean;
  category: GourmetMenuCategory;
  price: string;
}

export enum GourmetMenuCategory {
  Menu1 = 'MENÜ I',
  Menu2 = 'MENÜ II',
  Menu3 = 'MENÜ III',
  SoupAndSalad = 'SUPPE & SALAT',
  Unknown = 'UNKNOWN',
}

export interface GourmetDayMenu {
  date: Date;
  items: GourmetMenuItem[];
}

export interface GourmetUserInfo {
  username: string;
  shopModelId: string;
  eaterId: string;
  staffGroupId: string;
}
