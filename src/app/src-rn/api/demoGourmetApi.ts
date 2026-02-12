import { GourmetMenuItem, GourmetUserInfo } from '../types/menu';
import { GourmetOrderedMenu } from '../types/order';
import { GourmetBill } from '../types/billing';
import {
  generateDemoMenus,
  generateDemoBillings,
  createDemoOrder,
} from './demoData';

/**
 * Demo implementation of GourmetApi that returns fake data.
 * Same public interface as GourmetApi — no network requests.
 */
export class DemoGourmetApi {
  private userInfo: GourmetUserInfo | null = null;
  private orders: GourmetOrderedMenu[] = [];
  private menus: GourmetMenuItem[] | null = null;

  async login(_username: string, _password: string): Promise<GourmetUserInfo> {
    this.userInfo = {
      username: 'Demo User',
      shopModelId: 'demo-shop-1',
      eaterId: 'demo-eater-1',
      staffGroupId: 'demo-staff-1',
    };
    return this.userInfo;
  }

  getUserInfo(): GourmetUserInfo | null {
    return this.userInfo;
  }

  async getMenus(): Promise<GourmetMenuItem[]> {
    if (!this.menus) {
      this.menus = generateDemoMenus();
    }
    // Mark items that have been ordered (match by subtitle since title is category name)
    return this.menus.map((item) => ({
      ...item,
      ordered: this.orders.some(
        (o) =>
          o.date.getTime() === item.day.getTime() &&
          o.subtitle === item.subtitle
      ),
    }));
  }

  async getOrders(): Promise<GourmetOrderedMenu[]> {
    return [...this.orders];
  }

  async addToCart(items: { date: Date; menuId: string }[]): Promise<void> {
    if (!this.menus) {
      this.menus = generateDemoMenus();
    }

    for (const item of items) {
      const menuItem = this.menus.find(
        (m) =>
          m.id === item.menuId &&
          m.day.getTime() === item.date.getTime()
      );

      // title = category name, subtitle = dish description (matches real parser output)
      const title = menuItem?.title ?? 'Demo Menü';
      const subtitle = menuItem?.subtitle ?? '';

      this.orders.push(createDemoOrder(item.date, item.menuId, title, subtitle));
    }
  }

  async confirmOrders(): Promise<void> {
    this.orders = this.orders.map((o) => ({ ...o, approved: true }));
  }

  async cancelOrders(positionIds: string[]): Promise<void> {
    const idSet = new Set(positionIds);
    this.orders = this.orders.filter((o) => !idSet.has(o.positionId));
  }

  async getBillings(checkLastMonthNumber: string): Promise<GourmetBill[]> {
    return generateDemoBillings(checkLastMonthNumber);
  }

  async logout(): Promise<void> {
    this.userInfo = null;
    this.orders = [];
    this.menus = null;
  }

  isAuthenticated(): boolean {
    return this.userInfo !== null;
  }
}
