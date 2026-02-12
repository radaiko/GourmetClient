jest.mock('../../api/gourmetApi');
jest.mock('../../store/authStore', () => {
  const mockApi = {
    getOrders: jest.fn(),
    confirmOrders: jest.fn(),
    cancelOrders: jest.fn(),
  };
  return {
    useAuthStore: {
      getState: () => ({ api: mockApi }),
      setState: jest.fn(),
      subscribe: jest.fn(),
    },
  };
});

import { useOrderStore } from '../../store/orderStore';
import { useAuthStore } from '../../store/authStore';
import { GourmetOrderedMenu } from '../../types/order';

const mockApi = (useAuthStore as any).getState().api;

function makeOrder(overrides: Partial<GourmetOrderedMenu> = {}): GourmetOrderedMenu {
  return {
    positionId: 'P1',
    eatingCycleId: 'E1',
    date: new Date(),
    title: 'MENÜ I',
    subtitle: '',
    approved: true,
    ...overrides,
  };
}

beforeEach(() => {
  jest.clearAllMocks();
  useOrderStore.setState({
    orders: [],
    loading: false,
    cancellingId: null,
    error: null,
  });
});

describe('orderStore', () => {
  describe('fetchOrders', () => {
    it('sets loading during fetch', async () => {
      mockApi.getOrders.mockResolvedValue([]);

      const promise = useOrderStore.getState().fetchOrders();
      expect(useOrderStore.getState().loading).toBe(true);

      await promise;
      expect(useOrderStore.getState().loading).toBe(false);
    });

    it('stores fetched orders', async () => {
      const orders = [makeOrder()];
      mockApi.getOrders.mockResolvedValue(orders);

      await useOrderStore.getState().fetchOrders();

      expect(useOrderStore.getState().orders).toEqual(orders);
    });

    it('sets error on failure', async () => {
      mockApi.getOrders.mockRejectedValue(new Error('Fetch failed'));

      await useOrderStore.getState().fetchOrders();

      expect(useOrderStore.getState().error).toBe('Fetch failed');
      expect(useOrderStore.getState().loading).toBe(false);
    });
  });

  describe('confirmOrders', () => {
    it('calls api.confirmOrders', async () => {
      mockApi.confirmOrders.mockResolvedValue(undefined);
      mockApi.getOrders.mockResolvedValue([]);

      await useOrderStore.getState().confirmOrders();

      expect(mockApi.confirmOrders).toHaveBeenCalled();
    });

    it('refreshes orders after confirm', async () => {
      mockApi.confirmOrders.mockResolvedValue(undefined);
      mockApi.getOrders.mockResolvedValue([]);

      await useOrderStore.getState().confirmOrders();

      expect(mockApi.getOrders).toHaveBeenCalled();
    });
  });

  describe('cancelOrder', () => {
    it('calls api.cancelOrders with positionId', async () => {
      mockApi.cancelOrders.mockResolvedValue(undefined);
      mockApi.getOrders.mockResolvedValue([]);

      await useOrderStore.getState().cancelOrder('P1');

      expect(mockApi.cancelOrders).toHaveBeenCalledWith(['P1']);
    });

    it('sets cancellingId during operation', async () => {
      let resolveFn: () => void;
      mockApi.cancelOrders.mockReturnValue(new Promise<void>((r) => { resolveFn = r; }));
      mockApi.getOrders.mockResolvedValue([]);

      const promise = useOrderStore.getState().cancelOrder('P1');
      expect(useOrderStore.getState().cancellingId).toBe('P1');

      resolveFn!();
      await promise;
      expect(useOrderStore.getState().cancellingId).toBe(null);
    });
  });

  describe('computed getters', () => {
    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + 7);
    const pastDate = new Date();
    pastDate.setDate(pastDate.getDate() - 7);

    beforeEach(() => {
      useOrderStore.setState({
        orders: [
          makeOrder({ positionId: 'P1', date: futureDate, approved: true }),
          makeOrder({ positionId: 'P2', date: pastDate, approved: false }),
        ],
      });
    });

    it('getUpcomingOrders returns future orders', () => {
      const upcoming = useOrderStore.getState().getUpcomingOrders();
      expect(upcoming).toHaveLength(1);
      expect(upcoming[0].positionId).toBe('P1');
    });

    it('getPastOrders returns past orders', () => {
      const past = useOrderStore.getState().getPastOrders();
      expect(past).toHaveLength(1);
      expect(past[0].positionId).toBe('P2');
    });

    it('getUnconfirmedCount returns count of unapproved orders', () => {
      expect(useOrderStore.getState().getUnconfirmedCount()).toBe(1);
    });
  });
});
