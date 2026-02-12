import { DemoVentopayApi } from '../../api/demoVentopayApi';

describe('DemoVentopayApi', () => {
  let api: DemoVentopayApi;

  beforeEach(() => {
    api = new DemoVentopayApi();
  });

  describe('login', () => {
    it('sets authenticated state', async () => {
      expect(api.isAuthenticated()).toBe(false);
      await api.login('demo', 'demo1234!');
      expect(api.isAuthenticated()).toBe(true);
    });
  });

  describe('getTransactions', () => {
    it('returns transactions within date range', async () => {
      await api.login('demo', 'demo1234!');

      const from = new Date();
      from.setDate(1);
      from.setHours(0, 0, 0, 0);
      const to = new Date();

      const transactions = await api.getTransactions(from, to);

      // Should return some transactions (exact count depends on date/seed)
      expect(Array.isArray(transactions)).toBe(true);

      for (const tx of transactions) {
        expect(tx.id).toBeTruthy();
        expect(tx.amount).toBeGreaterThan(0);
        expect(tx.restaurant).toBe('Kaffeeautomat');
        expect(tx.date.getTime()).toBeGreaterThanOrEqual(from.getTime());
        expect(tx.date.getTime()).toBeLessThanOrEqual(to.getTime() + 86400000);
      }
    });

    it('returns deterministic data', async () => {
      await api.login('demo', 'demo1234!');

      const from = new Date(2026, 0, 1);
      const to = new Date(2026, 0, 31);

      const tx1 = await api.getTransactions(from, to);
      const tx2 = await api.getTransactions(from, to);

      expect(tx1.length).toBe(tx2.length);
      expect(tx1.map((t) => t.id)).toEqual(tx2.map((t) => t.id));
    });
  });

  describe('logout', () => {
    it('clears authenticated state', async () => {
      await api.login('demo', 'demo1234!');
      expect(api.isAuthenticated()).toBe(true);

      await api.logout();
      expect(api.isAuthenticated()).toBe(false);
    });
  });
});
