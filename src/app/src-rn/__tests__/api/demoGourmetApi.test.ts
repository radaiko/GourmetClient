import { DemoGourmetApi } from '../../api/demoGourmetApi';
import { GourmetMenuCategory } from '../../types/menu';

describe('DemoGourmetApi', () => {
  let api: DemoGourmetApi;

  beforeEach(() => {
    api = new DemoGourmetApi();
  });

  describe('login', () => {
    it('returns fake user info', async () => {
      const userInfo = await api.login('demo', 'demo1234!');
      expect(userInfo.username).toBe('Demo User');
      expect(userInfo.shopModelId).toBeTruthy();
      expect(userInfo.eaterId).toBeTruthy();
      expect(userInfo.staffGroupId).toBeTruthy();
    });

    it('sets authenticated state', async () => {
      expect(api.isAuthenticated()).toBe(false);
      await api.login('demo', 'demo1234!');
      expect(api.isAuthenticated()).toBe(true);
    });
  });

  describe('getUserInfo', () => {
    it('returns null before login', () => {
      expect(api.getUserInfo()).toBeNull();
    });

    it('returns user info after login', async () => {
      await api.login('demo', 'demo1234!');
      const info = api.getUserInfo();
      expect(info).not.toBeNull();
      expect(info!.username).toBe('Demo User');
    });
  });

  describe('getMenus', () => {
    it('returns menu items for 10 weekdays with 4 categories each', async () => {
      await api.login('demo', 'demo1234!');
      const menus = await api.getMenus();

      // 10 days * 4 categories = 40 items
      expect(menus).toHaveLength(40);

      // All items should be available
      expect(menus.every((m) => m.available)).toBe(true);

      // All 4 categories should be present
      const categories = new Set(menus.map((m) => m.category));
      expect(categories.has(GourmetMenuCategory.Menu1)).toBe(true);
      expect(categories.has(GourmetMenuCategory.Menu2)).toBe(true);
      expect(categories.has(GourmetMenuCategory.Menu3)).toBe(true);
      expect(categories.has(GourmetMenuCategory.SoupAndSalad)).toBe(true);
    });

    it('only generates weekday menus (no Sat/Sun)', async () => {
      await api.login('demo', 'demo1234!');
      const menus = await api.getMenus();

      for (const item of menus) {
        const dow = item.day.getDay();
        expect(dow).toBeGreaterThanOrEqual(1);
        expect(dow).toBeLessThanOrEqual(5);
      }
    });

    it('returns deterministic data across calls', async () => {
      await api.login('demo', 'demo1234!');
      const menus1 = await api.getMenus();
      const menus2 = await api.getMenus();

      expect(menus1.map((m) => m.title)).toEqual(menus2.map((m) => m.title));
    });
  });

  describe('orders workflow', () => {
    beforeEach(async () => {
      await api.login('demo', 'demo1234!');
    });

    it('starts with no orders', async () => {
      const orders = await api.getOrders();
      expect(orders).toHaveLength(0);
    });

    it('addToCart creates unconfirmed orders', async () => {
      const menus = await api.getMenus();
      const item = menus[0];

      await api.addToCart([{ date: item.day, menuId: item.id }]);

      const orders = await api.getOrders();
      expect(orders).toHaveLength(1);
      expect(orders[0].title).toBe(item.title);
      expect(orders[0].approved).toBe(false);
    });

    it('confirmOrders marks all orders as approved', async () => {
      const menus = await api.getMenus();
      await api.addToCart([
        { date: menus[0].day, menuId: menus[0].id },
        { date: menus[1].day, menuId: menus[1].id },
      ]);

      await api.confirmOrders();

      const orders = await api.getOrders();
      expect(orders.every((o) => o.approved)).toBe(true);
    });

    it('cancelOrders removes specified orders', async () => {
      const menus = await api.getMenus();
      await api.addToCart([
        { date: menus[0].day, menuId: menus[0].id },
        { date: menus[1].day, menuId: menus[1].id },
      ]);

      const orders = await api.getOrders();
      expect(orders).toHaveLength(2);

      await api.cancelOrders([orders[0].positionId]);

      const remaining = await api.getOrders();
      expect(remaining).toHaveLength(1);
      expect(remaining[0].positionId).toBe(orders[1].positionId);
    });

    it('getMenus reflects ordered state', async () => {
      const menus = await api.getMenus();
      const item = menus[0];

      await api.addToCart([{ date: item.day, menuId: item.id }]);

      const updatedMenus = await api.getMenus();
      const ordered = updatedMenus.find(
        (m) => m.day.getTime() === item.day.getTime() && m.subtitle === item.subtitle
      );
      expect(ordered?.ordered).toBe(true);
    });
  });

  describe('getBillings', () => {
    it('returns bills for current month', async () => {
      await api.login('demo', 'demo1234!');
      const bills = await api.getBillings('0');

      expect(bills.length).toBeGreaterThan(0);
      for (const bill of bills) {
        expect(bill.billNr).toBeDefined();
        expect(bill.items.length).toBeGreaterThan(0);
        expect(bill.billing).toBeGreaterThan(0);
        expect(bill.location).toBe('Betriebsrestaurant');
      }
    });

    it('returns deterministic data', async () => {
      await api.login('demo', 'demo1234!');
      const bills1 = await api.getBillings('0');
      const bills2 = await api.getBillings('0');
      expect(bills1.length).toBe(bills2.length);
    });
  });

  describe('logout', () => {
    it('clears authenticated state', async () => {
      await api.login('demo', 'demo1234!');
      expect(api.isAuthenticated()).toBe(true);

      await api.logout();
      expect(api.isAuthenticated()).toBe(false);
      expect(api.getUserInfo()).toBeNull();
    });

    it('clears orders', async () => {
      await api.login('demo', 'demo1234!');
      const menus = await api.getMenus();
      await api.addToCart([{ date: menus[0].day, menuId: menus[0].id }]);

      await api.logout();
      await api.login('demo', 'demo1234!');

      const orders = await api.getOrders();
      expect(orders).toHaveLength(0);
    });
  });
});
