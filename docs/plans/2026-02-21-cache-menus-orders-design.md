# Cache Menus and Ordered Menus for Faster Cold Start — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Persist menu and order data in AsyncStorage so cold starts show cached content instantly while background-refreshing from the network.

**Architecture:** Mirror the existing billing store caching pattern — serialize/deserialize Date fields, read from AsyncStorage on startup, write-through on every successful fetch. Screens load cache first, then fire background refreshes.

**Tech Stack:** Zustand, AsyncStorage (`@react-native-async-storage/async-storage`), Jest

---

## Task 1: Add AsyncStorage caching to menuStore

**Files:**
- Modify: `src/app/src-rn/store/menuStore.ts`

**Step 1: Add AsyncStorage import and serialization helpers**

At the top of `menuStore.ts`, add:

```typescript
import AsyncStorage from '@react-native-async-storage/async-storage';

const MENU_CACHE_KEY = 'menus_items';

/** Serialize menu items for AsyncStorage (Date -> ISO string). */
function serializeMenuItems(items: GourmetMenuItem[]): string {
  return JSON.stringify(items.map((item) => ({
    ...item,
    day: item.day.toISOString(),
  })));
}

/** Deserialize menu items from AsyncStorage (ISO string -> Date). */
function deserializeMenuItems(json: string): GourmetMenuItem[] {
  return JSON.parse(json).map((item: any) => ({
    ...item,
    day: new Date(item.day),
  }));
}
```

**Step 2: Add `loadCachedMenus` to MenuState interface**

Add to the `MenuState` interface:

```typescript
loadCachedMenus: () => Promise<void>;
```

**Step 3: Implement `loadCachedMenus` in the store**

Add to the store implementation:

```typescript
loadCachedMenus: async () => {
  const cached = await AsyncStorage.getItem(MENU_CACHE_KEY);
  if (cached) {
    const items = deserializeMenuItems(cached);
    set({ items });
  }
},
```

**Step 4: Add AsyncStorage write to `fetchMenus`**

In `fetchMenus`, after `set({ items, lastFetched: Date.now(), loading: false })`, add:

```typescript
await AsyncStorage.setItem(MENU_CACHE_KEY, serializeMenuItems(items));
```

**Step 5: Add AsyncStorage write to `refreshAvailability`**

In `refreshAvailability`, after `set({ items: merged, lastFetched: Date.now(), refreshing: false })`, add:

```typescript
await AsyncStorage.setItem(MENU_CACHE_KEY, serializeMenuItems(merged));
```

**Step 6: Run tests**

Run: `cd /Users/radaiko/dev/private/SnackPilot/.worktrees/feat-cache-menus/src/app && npm test -- --testPathPattern=menuStore`

Expected: Tests may fail because AsyncStorage is not mocked. That's ok — we fix it in Task 2.

**Step 7: Commit**

```bash
git add src-rn/store/menuStore.ts
git commit -m "feat: add AsyncStorage caching to menuStore (#11)"
```

---

## Task 2: Add menuStore caching tests

**Files:**
- Modify: `src/app/src-rn/__tests__/store/menuStore.test.ts`

**Step 1: Add AsyncStorage mock**

At the very top of the test file (before other mocks), add the same AsyncStorage mock used in billingStore.test.ts:

```typescript
jest.mock('@react-native-async-storage/async-storage', () => {
  const store: Record<string, string> = {};
  return {
    __esModule: true,
    default: {
      getItem: jest.fn((key: string) => Promise.resolve(store[key] ?? null)),
      setItem: jest.fn((key: string, value: string) => { store[key] = value; return Promise.resolve(); }),
      removeItem: jest.fn((key: string) => { delete store[key]; return Promise.resolve(); }),
      clear: jest.fn(() => { Object.keys(store).forEach(k => delete store[k]); return Promise.resolve(); }),
    },
  };
});
```

**Step 2: Add caching tests**

Add a new `describe('caching')` block:

```typescript
describe('caching', () => {
  it('fetchMenus writes items to AsyncStorage', async () => {
    const AsyncStorage = require('@react-native-async-storage/async-storage').default;
    const items = [makeItem()];
    mockApi.getMenus.mockResolvedValue(items);

    await useMenuStore.getState().fetchMenus();

    expect(AsyncStorage.setItem).toHaveBeenCalled();
    const [key] = AsyncStorage.setItem.mock.calls[0];
    expect(key).toBe('menus_items');
  });

  it('loadCachedMenus restores items from AsyncStorage', async () => {
    const AsyncStorage = require('@react-native-async-storage/async-storage').default;
    const items = [makeItem({ day: new Date(2026, 1, 10) })];

    // Simulate a previous fetchMenus that cached data
    mockApi.getMenus.mockResolvedValue(items);
    await useMenuStore.getState().fetchMenus();

    // Reset state to simulate cold start
    useMenuStore.setState({ items: [], lastFetched: null });

    // Load from cache
    await useMenuStore.getState().loadCachedMenus();

    const restored = useMenuStore.getState().items;
    expect(restored).toHaveLength(1);
    expect(restored[0].day).toBeInstanceOf(Date);
    expect(restored[0].day.getFullYear()).toBe(2026);
    expect(restored[0].id).toBe('menu-001');
  });

  it('loadCachedMenus does nothing when cache is empty', async () => {
    await useMenuStore.getState().loadCachedMenus();
    expect(useMenuStore.getState().items).toEqual([]);
  });
});
```

**Step 3: Run tests**

Run: `cd /Users/radaiko/dev/private/SnackPilot/.worktrees/feat-cache-menus/src/app && npm test -- --testPathPattern=menuStore`

Expected: All tests PASS.

**Step 4: Commit**

```bash
git add src-rn/__tests__/store/menuStore.test.ts
git commit -m "test: add menuStore caching tests (#11)"
```

---

## Task 3: Add AsyncStorage caching to orderStore

**Files:**
- Modify: `src/app/src-rn/store/orderStore.ts`

**Step 1: Add AsyncStorage import and serialization helpers**

At the top of `orderStore.ts`, add:

```typescript
import AsyncStorage from '@react-native-async-storage/async-storage';

const ORDER_CACHE_KEY = 'orders_list';

/** Serialize orders for AsyncStorage (Date -> ISO string). */
function serializeOrders(orders: GourmetOrderedMenu[]): string {
  return JSON.stringify(orders.map((o) => ({
    ...o,
    date: o.date.toISOString(),
  })));
}

/** Deserialize orders from AsyncStorage (ISO string -> Date). */
function deserializeOrders(json: string): GourmetOrderedMenu[] {
  return JSON.parse(json).map((o: any) => ({
    ...o,
    date: new Date(o.date),
  }));
}
```

**Step 2: Add `loadCachedOrders` to OrderState interface**

Add to the `OrderState` interface:

```typescript
loadCachedOrders: () => Promise<void>;
```

**Step 3: Implement `loadCachedOrders` in the store**

Add to the store implementation:

```typescript
loadCachedOrders: async () => {
  const cached = await AsyncStorage.getItem(ORDER_CACHE_KEY);
  if (cached) {
    const orders = deserializeOrders(cached);
    set({ orders });
  }
},
```

**Step 4: Add AsyncStorage write to `fetchOrders`**

In `fetchOrders`, after `set({ orders, loading: false })`, add:

```typescript
await AsyncStorage.setItem(ORDER_CACHE_KEY, serializeOrders(orders));
```

**Step 5: Run tests**

Run: `cd /Users/radaiko/dev/private/SnackPilot/.worktrees/feat-cache-menus/src/app && npm test -- --testPathPattern=orderStore`

Expected: Tests may fail because AsyncStorage is not mocked. Fixed in Task 4.

**Step 6: Commit**

```bash
git add src-rn/store/orderStore.ts
git commit -m "feat: add AsyncStorage caching to orderStore (#11)"
```

---

## Task 4: Add orderStore caching tests

**Files:**
- Modify: `src/app/src-rn/__tests__/store/orderStore.test.ts`

**Step 1: Add AsyncStorage mock**

At the very top of the test file (before other mocks), add:

```typescript
jest.mock('@react-native-async-storage/async-storage', () => {
  const store: Record<string, string> = {};
  return {
    __esModule: true,
    default: {
      getItem: jest.fn((key: string) => Promise.resolve(store[key] ?? null)),
      setItem: jest.fn((key: string, value: string) => { store[key] = value; return Promise.resolve(); }),
      removeItem: jest.fn((key: string) => { delete store[key]; return Promise.resolve(); }),
      clear: jest.fn(() => { Object.keys(store).forEach(k => delete store[k]); return Promise.resolve(); }),
    },
  };
});
```

**Step 2: Add caching tests**

Add a new `describe('caching')` block:

```typescript
describe('caching', () => {
  it('fetchOrders writes orders to AsyncStorage', async () => {
    const AsyncStorage = require('@react-native-async-storage/async-storage').default;
    const orders = [makeOrder()];
    mockApi.getOrders.mockResolvedValue(orders);

    await useOrderStore.getState().fetchOrders();

    expect(AsyncStorage.setItem).toHaveBeenCalled();
    const [key] = AsyncStorage.setItem.mock.calls[0];
    expect(key).toBe('orders_list');
  });

  it('loadCachedOrders restores orders from AsyncStorage', async () => {
    const AsyncStorage = require('@react-native-async-storage/async-storage').default;
    const orders = [makeOrder({ date: new Date(2026, 1, 10) })];

    // Simulate a previous fetchOrders that cached data
    mockApi.getOrders.mockResolvedValue(orders);
    await useOrderStore.getState().fetchOrders();

    // Reset state to simulate cold start
    useOrderStore.setState({ orders: [], loading: false });

    // Load from cache
    await useOrderStore.getState().loadCachedOrders();

    const restored = useOrderStore.getState().orders;
    expect(restored).toHaveLength(1);
    expect(restored[0].date).toBeInstanceOf(Date);
    expect(restored[0].date.getFullYear()).toBe(2026);
    expect(restored[0].positionId).toBe('P1');
  });

  it('loadCachedOrders does nothing when cache is empty', async () => {
    await useOrderStore.getState().loadCachedOrders();
    expect(useOrderStore.getState().orders).toEqual([]);
  });
});
```

**Step 3: Run tests**

Run: `cd /Users/radaiko/dev/private/SnackPilot/.worktrees/feat-cache-menus/src/app && npm test -- --testPathPattern=orderStore`

Expected: All tests PASS.

**Step 4: Commit**

```bash
git add src-rn/__tests__/store/orderStore.test.ts
git commit -m "test: add orderStore caching tests (#11)"
```

---

## Task 5: Integrate cache loading into screens

**Files:**
- Modify: `src/app/app/(tabs)/index.tsx`
- Modify: `src/app/app/(tabs)/orders.tsx`

**Step 1: Update index.tsx (Menus tab)**

Import `loadCachedMenus` from menuStore and `loadCachedOrders` from orderStore. In `triggerRefresh`, load cache first, then background-refresh:

```typescript
const triggerRefresh = useCallback(() => {
  const auth = useAuthStore.getState().status;
  if (auth !== 'authenticated') return;

  const { loadCachedMenus } = useMenuStore.getState();
  const { loadCachedOrders } = useOrderStore.getState();

  // Load cache first for instant display
  Promise.all([loadCachedMenus(), loadCachedOrders()]).catch(() => {}).finally(() => {
    const cached = useMenuStore.getState().items.length > 0;
    if (cached) {
      refreshAvailability();
    } else {
      fetchMenus();
    }
    fetchOrders();
  });
}, [fetchMenus, refreshAvailability, fetchOrders]);
```

**Step 2: Update orders.tsx (Orders tab)**

Import `loadCachedOrders` from orderStore. Update the useFocusEffect:

```typescript
const { loadCachedOrders } = useOrderStore.getState();

useFocusEffect(
  useCallback(() => {
    if (authStatus === 'authenticated') {
      loadCachedOrders().catch(() => {}).finally(() => {
        fetchOrders();
        fetchMenus();
      });
    }
  }, [authStatus, fetchOrders, fetchMenus])
);
```

**Step 3: Run full test suite**

Run: `cd /Users/radaiko/dev/private/SnackPilot/.worktrees/feat-cache-menus/src/app && npm test`

Expected: All tests PASS.

**Step 4: Commit**

```bash
git add app/(tabs)/index.tsx app/(tabs)/orders.tsx
git commit -m "feat: load cached menus/orders on screen focus for instant startup (#11)"
```

---

## Task 6: Run full test suite and verify

**Step 1: Run all tests**

Run: `cd /Users/radaiko/dev/private/SnackPilot/.worktrees/feat-cache-menus/src/app && npm test`

Expected: All tests PASS (199 + new caching tests).

**Step 2: Verify no TypeScript errors**

Run: `cd /Users/radaiko/dev/private/SnackPilot/.worktrees/feat-cache-menus/src/app && npx tsc --noEmit`

Expected: No errors.
