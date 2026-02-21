# Menu Reordering Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Enable users to change their existing menu orders by tapping ordered cards to mark them for cancellation, selecting alternatives, and confirming all changes at once.

**Architecture:** Extend the existing `menuStore` with a `pendingCancellations` Set alongside `pendingOrders`. The `togglePendingOrder` function routes to the appropriate set based on whether the item is already ordered. The `submitOrders` function is expanded to cancel old orders before placing new ones. The `MenuCard` component is updated to make ordered items tappable and show a "pending cancellation" visual state.

**Tech Stack:** TypeScript, Zustand, React Native, Jest

---

### Task 1: Add `pendingCancellations` to menuStore state

**Files:**
- Modify: `src/app/src-rn/store/menuStore.ts:10-30` (MenuState interface)
- Modify: `src/app/src-rn/store/menuStore.ts:36-44` (initial state)
- Test: `src/app/src-rn/__tests__/store/menuStore.test.ts`

**Step 1: Write the failing test**

Add to `src/app/src-rn/__tests__/store/menuStore.test.ts`, inside the `menuStore` describe block, after the existing `togglePendingOrder` describe:

```typescript
describe('pendingCancellations', () => {
  it('togglePendingOrder on ordered item adds to pendingCancellations', () => {
    const items = [
      makeItem({ id: 'menu-001', day: new Date(2026, 1, 10), ordered: true }),
    ];
    useMenuStore.setState({ items });

    useMenuStore.getState().togglePendingOrder('menu-001', new Date(2026, 1, 10));

    expect(useMenuStore.getState().pendingCancellations.size).toBe(1);
    expect(useMenuStore.getState().pendingOrders.size).toBe(0);
  });

  it('togglePendingOrder on non-ordered item still adds to pendingOrders', () => {
    const items = [
      makeItem({ id: 'menu-001', day: new Date(2026, 1, 10), ordered: false }),
    ];
    useMenuStore.setState({ items });

    useMenuStore.getState().togglePendingOrder('menu-001', new Date(2026, 1, 10));

    expect(useMenuStore.getState().pendingOrders.size).toBe(1);
    expect(useMenuStore.getState().pendingCancellations.size).toBe(0);
  });

  it('second toggle on ordered item removes from pendingCancellations', () => {
    const items = [
      makeItem({ id: 'menu-001', day: new Date(2026, 1, 10), ordered: true }),
    ];
    useMenuStore.setState({ items });

    const date = new Date(2026, 1, 10);
    useMenuStore.getState().togglePendingOrder('menu-001', date);
    expect(useMenuStore.getState().pendingCancellations.size).toBe(1);

    useMenuStore.getState().togglePendingOrder('menu-001', date);
    expect(useMenuStore.getState().pendingCancellations.size).toBe(0);
  });
});
```

Also update the `beforeEach` to reset the new state. Add `pendingCancellations: new Set(),` to the `useMenuStore.setState` call in `beforeEach`:

```typescript
beforeEach(() => {
  jest.clearAllMocks();
  useMenuStore.setState({
    items: [],
    lastFetched: null,
    loading: false,
    refreshing: false,
    error: null,
    selectedDate: new Date(),
    pendingOrders: new Set(),
    pendingCancellations: new Set(),
    orderProgress: null,
  });
});
```

**Step 2: Run test to verify it fails**

Run: `cd src/app && npx jest --testPathPattern=menuStore.test.ts --verbose 2>&1 | tail -30`
Expected: FAIL — `pendingCancellations` property does not exist on MenuState

**Step 3: Write minimal implementation**

In `src/app/src-rn/store/menuStore.ts`:

Add to the `MenuState` interface (after line 18, `orderProgress`):

```typescript
pendingCancellations: Set<string>; // Set of "menuId|dateStr" keys for ordered items to cancel
```

Add initial state (after line 44, the `orderProgress: null` line):

```typescript
pendingCancellations: new Set(),
```

Modify `togglePendingOrder` (replace lines 122-131) to route based on whether the item is currently ordered:

```typescript
togglePendingOrder: (menuId: string, date: Date) => {
  const key = makePendingKey(menuId, date);
  const item = get().items.find(
    (i) => i.id === menuId && localDateKey(i.day) === localDateKey(date)
  );
  const isOrdered = item?.ordered ?? false;

  if (isOrdered) {
    const cancellations = new Set(get().pendingCancellations);
    if (cancellations.has(key)) {
      cancellations.delete(key);
    } else {
      cancellations.add(key);
    }
    set({ pendingCancellations: cancellations });
  } else {
    const pending = new Set(get().pendingOrders);
    if (pending.has(key)) {
      pending.delete(key);
    } else {
      pending.add(key);
    }
    set({ pendingOrders: pending });
  }
},
```

Modify `clearPendingOrders` (replace line 133):

```typescript
clearPendingOrders: () => set({ pendingOrders: new Set(), pendingCancellations: new Set() }),
```

**Step 4: Run test to verify it passes**

Run: `cd src/app && npx jest --testPathPattern=menuStore.test.ts --verbose 2>&1 | tail -30`
Expected: All tests PASS

**Step 5: Commit**

```bash
git add src/app/src-rn/store/menuStore.ts src/app/src-rn/__tests__/store/menuStore.test.ts
git commit -m "feat: add pendingCancellations state to menuStore (#9)"
```

---

### Task 2: Update `getPendingCount` and add `getPendingCancellationCount`

**Files:**
- Modify: `src/app/src-rn/store/menuStore.ts:215` (getPendingCount)
- Test: `src/app/src-rn/__tests__/store/menuStore.test.ts`

**Step 1: Write the failing test**

Add to the `computed getters` describe block in `menuStore.test.ts`:

```typescript
it('getPendingCount includes both pendingOrders and pendingCancellations', () => {
  useMenuStore.setState({
    items: [makeItem()],
    pendingOrders: new Set(['menu-001|2026-02-10']),
    pendingCancellations: new Set(['menu-002|2026-02-11']),
  });

  expect(useMenuStore.getState().getPendingCount()).toBe(2);
});

it('getPendingCancellationCount returns only cancellations count', () => {
  useMenuStore.setState({
    items: [makeItem()],
    pendingOrders: new Set(['menu-001|2026-02-10']),
    pendingCancellations: new Set(['menu-002|2026-02-11']),
  });

  expect(useMenuStore.getState().getPendingCancellationCount()).toBe(1);
});
```

**Step 2: Run test to verify it fails**

Run: `cd src/app && npx jest --testPathPattern=menuStore.test.ts --verbose 2>&1 | tail -30`
Expected: FAIL — `getPendingCount` returns 1 instead of 2, `getPendingCancellationCount` is not a function

**Step 3: Write minimal implementation**

In `src/app/src-rn/store/menuStore.ts`:

Add to `MenuState` interface (after `getPendingCount`):

```typescript
getPendingCancellationCount: () => number;
```

Replace `getPendingCount` (line 215):

```typescript
getPendingCount: () => get().pendingOrders.size + get().pendingCancellations.size,

getPendingCancellationCount: () => get().pendingCancellations.size,
```

**Step 4: Run test to verify it passes**

Run: `cd src/app && npx jest --testPathPattern=menuStore.test.ts --verbose 2>&1 | tail -30`
Expected: All tests PASS

**Step 5: Commit**

```bash
git add src/app/src-rn/store/menuStore.ts src/app/src-rn/__tests__/store/menuStore.test.ts
git commit -m "feat: update getPendingCount to include cancellations (#9)"
```

---

### Task 3: Expand `submitOrders` to handle cancellations

**Files:**
- Modify: `src/app/src-rn/store/menuStore.ts:135-189` (submitOrders)
- Modify: `src/app/src-rn/__tests__/store/menuStore.test.ts` (mock orderStore properly)
- Test: `src/app/src-rn/__tests__/store/menuStore.test.ts`

**Step 1: Write the failing tests**

The existing `orderStore` mock needs to be updated to support `cancelOrder`. Replace the orderStore mock at the top of `menuStore.test.ts` (lines 16-22):

```typescript
const mockCancelOrder = jest.fn().mockResolvedValue(undefined);
const mockFetchOrders = jest.fn().mockResolvedValue(undefined);
jest.mock('../../store/orderStore', () => ({
  useOrderStore: {
    getState: () => ({
      fetchOrders: mockFetchOrders,
      cancelOrder: mockCancelOrder,
      orders: [],
    }),
    setState: jest.fn(),
    subscribe: jest.fn(),
  },
}));
```

Add these tests inside the `submitOrders` describe block:

```typescript
it('cancels orders from pendingCancellations before adding new ones', async () => {
  const date = new Date(2026, 5, 10);
  const items = [
    makeItem({ id: 'menu-001', day: date, ordered: true, category: GourmetMenuCategory.Menu1 }),
    makeItem({ id: 'menu-002', day: date, ordered: false, category: GourmetMenuCategory.Menu2 }),
  ];
  useMenuStore.setState({ items });

  // Simulate: orderStore has the order we want to cancel
  const mockOrders = [{ positionId: 'P1', eatingCycleId: 'E1', date, title: 'MENÜ I', subtitle: '', approved: true }];
  (require('../../store/orderStore').useOrderStore.getState as any) = () => ({
    fetchOrders: mockFetchOrders,
    cancelOrder: mockCancelOrder,
    orders: mockOrders,
  });

  // Mark menu-001 for cancellation (it's ordered)
  useMenuStore.getState().togglePendingOrder('menu-001', date);
  // Select menu-002 as new order
  useMenuStore.getState().togglePendingOrder('menu-002', date);

  await useMenuStore.getState().submitOrders();

  // Cancel should be called before addToCart
  expect(mockCancelOrder).toHaveBeenCalledWith('P1');
  expect(mockApi.addToCart).toHaveBeenCalled();
  expect(mockApi.confirmOrders).toHaveBeenCalled();
});

it('handles cancellation-only submit (no new orders)', async () => {
  const date = new Date(2026, 5, 10);
  const items = [
    makeItem({ id: 'menu-001', day: date, ordered: true, category: GourmetMenuCategory.Menu1 }),
  ];
  useMenuStore.setState({ items });

  const mockOrders = [{ positionId: 'P1', eatingCycleId: 'E1', date, title: 'MENÜ I', subtitle: '', approved: true }];
  (require('../../store/orderStore').useOrderStore.getState as any) = () => ({
    fetchOrders: mockFetchOrders,
    cancelOrder: mockCancelOrder,
    orders: mockOrders,
  });

  useMenuStore.getState().togglePendingOrder('menu-001', date);

  await useMenuStore.getState().submitOrders();

  expect(mockCancelOrder).toHaveBeenCalledWith('P1');
  expect(mockApi.addToCart).not.toHaveBeenCalled();
});

it('clears both pendingOrders and pendingCancellations after submit', async () => {
  const date = new Date(2026, 5, 10);
  const items = [
    makeItem({ id: 'menu-001', day: date, ordered: true, category: GourmetMenuCategory.Menu1 }),
    makeItem({ id: 'menu-002', day: date, ordered: false, category: GourmetMenuCategory.Menu2 }),
  ];
  useMenuStore.setState({ items });

  const mockOrders = [{ positionId: 'P1', eatingCycleId: 'E1', date, title: 'MENÜ I', subtitle: '', approved: true }];
  (require('../../store/orderStore').useOrderStore.getState as any) = () => ({
    fetchOrders: mockFetchOrders,
    cancelOrder: mockCancelOrder,
    orders: mockOrders,
  });

  useMenuStore.getState().togglePendingOrder('menu-001', date);
  useMenuStore.getState().togglePendingOrder('menu-002', date);

  await useMenuStore.getState().submitOrders();

  expect(useMenuStore.getState().pendingOrders.size).toBe(0);
  expect(useMenuStore.getState().pendingCancellations.size).toBe(0);
});
```

**Step 2: Run test to verify it fails**

Run: `cd src/app && npx jest --testPathPattern=menuStore.test.ts --verbose 2>&1 | tail -30`
Expected: FAIL — `mockCancelOrder` not called, submitOrders doesn't process pendingCancellations

**Step 3: Write minimal implementation**

Replace the entire `submitOrders` function in `src/app/src-rn/store/menuStore.ts` (lines 135-189):

```typescript
submitOrders: async () => {
  const { pendingOrders, pendingCancellations } = get();
  if (pendingOrders.size === 0 && pendingCancellations.size === 0) return;

  const api = useAuthStore.getState().api;
  const orderStoreState = useOrderStore.getState();

  // --- Resolve cancellations to positionIds ---
  const cancellationPositionIds: string[] = [];
  for (const key of pendingCancellations) {
    const [menuId, dateStr] = key.split('|');
    // Find the matching menu item to get its category
    const menuItem = get().items.find(
      (i) => i.id === menuId && localDateKey(i.day) === dateStr
    );
    if (!menuItem) continue;

    // Find the matching order by category + date
    const order = orderStoreState.orders.find(
      (o) => o.title === menuItem.category && localDateKey(o.date) === dateStr
    );
    if (order) {
      cancellationPositionIds.push(order.positionId);
    }
  }

  // --- Resolve new orders ---
  const newOrderItems = Array.from(pendingOrders).map((key) => {
    const [menuId, dateStr] = key.split('|');
    const [y, m, d] = dateStr.split('-').map(Number);
    return { menuId, date: new Date(y, m - 1, d) };
  });

  // Block today's orders after 12:30 Vienna time
  const blocked = newOrderItems.filter((i) => isOrderingCutoff(i.date));
  if (blocked.length > 0) {
    set({ error: 'Bestellung für heute geschlossen (Bestellschluss 12:30)' });
    return;
  }

  // --- Optimistic UI update ---
  const cancelKeys = new Set(pendingCancellations);
  const orderKeys = new Set(pendingOrders);
  const optimisticItems = get().items.map((item) => {
    const key = makePendingKey(item.id, item.day);
    if (cancelKeys.has(key)) {
      return { ...item, ordered: false };
    }
    if (orderKeys.has(key)) {
      return { ...item, ordered: true };
    }
    return item;
  });
  set({
    items: optimisticItems,
    pendingOrders: new Set(),
    pendingCancellations: new Set(),
    error: null,
  });

  try {
    // Step 1: Cancel orders
    if (cancellationPositionIds.length > 0) {
      set({ orderProgress: 'cancelling' });
      for (const positionId of cancellationPositionIds) {
        await orderStoreState.cancelOrder(positionId);
      }
    }

    // Step 2: Add new orders to cart
    if (newOrderItems.length > 0) {
      set({ orderProgress: 'adding' });
      await api.addToCart(newOrderItems);

      set({ orderProgress: 'confirming' });
      await api.confirmOrders();
    }

    // Step 3: Refresh
    set({ orderProgress: 'refreshing' });
    await useOrderStore.getState().fetchOrders();
    await get().fetchMenus(true);

    set({ orderProgress: null });
  } catch (err) {
    const message = err instanceof Error ? err.message : 'Bestellung konnte nicht aufgegeben werden';
    set({ error: message, orderProgress: null });
    // Revert optimistic update on failure
    try {
      const freshApi = useAuthStore.getState().api;
      const freshItems = await freshApi.getMenus();
      set({ items: freshItems, lastFetched: Date.now() });
    } catch {
      // Silent — keep optimistic state if revert also fails
    }
  }
},
```

**Step 4: Run test to verify it passes**

Run: `cd src/app && npx jest --testPathPattern=menuStore.test.ts --verbose 2>&1 | tail -30`
Expected: All tests PASS

**Step 5: Commit**

```bash
git add src/app/src-rn/store/menuStore.ts src/app/src-rn/__tests__/store/menuStore.test.ts
git commit -m "feat: expand submitOrders to handle cancellations (#9)"
```

---

### Task 4: Update MenuCard to support pending cancellation state

**Files:**
- Modify: `src/app/src-rn/components/MenuCard.tsx`

**Step 1: Update MenuCard props and logic**

Replace the `MenuCardProps` interface and component function in `src/app/src-rn/components/MenuCard.tsx`:

New props — replace `onCancel` and `isCancelling` with `isPendingCancel`:

```typescript
interface MenuCardProps {
  item: GourmetMenuItem;
  isSelected: boolean;
  ordered: boolean;
  isPendingCancel: boolean;
  onToggle: () => void;
}
```

Replace the component body:

```typescript
export function MenuCard({ item, isSelected, ordered, isPendingCancel, onToggle }: MenuCardProps) {
  const { colors } = useTheme();
  const styles = createStyles(colors);
  const cutoff = isOrderingCutoff(item.day);

  // Ordered items are tappable (to mark for cancellation)
  // Available items are tappable (to select for ordering)
  // Unavailable/cutoff items that are NOT ordered are disabled
  const canInteract = ordered || (item.available && !cutoff);

  return (
    <Pressable
      style={[
        styles.card,
        ordered && !isPendingCancel && styles.cardOrdered,
        isPendingCancel && styles.cardPendingCancel,
        isSelected && styles.cardSelected,
        (!item.available || cutoff) && !ordered && styles.cardDisabled,
      ]}
      onPress={canInteract ? onToggle : undefined}
      disabled={!canInteract}
    >
      <View style={styles.badgeRow}>
        {isPendingCancel && (
          <View style={styles.pendingCancelBadge}>
            <Text style={styles.pendingCancelBadgeText}>Wird storniert</Text>
          </View>
        )}
        {ordered && !isPendingCancel && (
          <View style={styles.orderedBadge}>
            <Text style={styles.orderedBadgeText}>Bestellt</Text>
          </View>
        )}
        {!ordered && !item.available && (
          <View style={styles.stockBadge}>
            <Text style={styles.stockBadgeText}>Ausverkauft</Text>
          </View>
        )}
        {cutoff && !ordered && item.available && (
          <View style={styles.cutoffBadge}>
            <Text style={styles.cutoffBadgeText}>Geschlossen</Text>
          </View>
        )}
      </View>
      <Text
        style={[
          styles.subtitle,
          isSelected && styles.textSelected,
          isPendingCancel && styles.textPendingCancel,
        ]}
        numberOfLines={4}
      >
        {item.subtitle}
      </Text>
      <View style={styles.bottomRow}>
        <Text
          style={[
            styles.allergens,
            isSelected && styles.textSelected,
            isPendingCancel && styles.textPendingCancel,
          ]}
          numberOfLines={1}
        >
          {item.allergens.length > 0 ? `Allergene: ${item.allergens.join(', ')}` : ''}
        </Text>
        <Text
          style={[
            styles.price,
            isSelected && styles.textSelected,
            isPendingCancel && styles.textPendingCancel,
          ]}
        >
          {item.price}
        </Text>
      </View>
      {isSelected && (
        <View style={styles.checkmark}>
          <Text style={styles.checkmarkText}>&#10003;</Text>
        </View>
      )}
    </Pressable>
  );
}
```

Add the new styles to `createStyles` (add after `cardDisabled`):

```typescript
cardPendingCancel: {
  opacity: 0.55,
  borderStyle: 'dashed' as const,
},
pendingCancelBadge: {
  backgroundColor: useFlatStyle ? c.warningSurface : c.glassWarning,
  paddingHorizontal: 8,
  paddingVertical: 2,
  borderRadius: 12,
  borderWidth: useFlatStyle ? 1 : 0.5,
  borderColor: c.warning,
},
pendingCancelBadgeText: {
  fontSize: 10,
  fontWeight: '700',
  color: c.warningText,
},
textPendingCancel: {
  textDecorationLine: 'line-through' as const,
  color: c.textTertiary,
},
```

Remove the `cancelButton`, `cancelText` styles (no longer needed).

Remove the `ActivityIndicator` import (no longer needed).

**Step 2: Run all tests to verify nothing breaks**

Run: `cd src/app && npx jest --verbose 2>&1 | tail -30`
Expected: All existing tests PASS (MenuCard has no unit tests, so this is a sanity check)

**Step 3: Commit**

```bash
git add src/app/src-rn/components/MenuCard.tsx
git commit -m "feat: update MenuCard for pending cancellation state (#9)"
```

---

### Task 5: Update MenusScreen to pass new props and show dynamic FAB label

**Files:**
- Modify: `src/app/app/(tabs)/index.tsx`

**Step 1: Update the screen component**

In `src/app/app/(tabs)/index.tsx`:

1. Add `pendingCancellations` and `getPendingCancellationCount` to the useMenuStore destructuring (line 56-72):

```typescript
const {
  items,
  loading,
  refreshing,
  error,
  selectedDate,
  pendingOrders,
  pendingCancellations,
  orderProgress,
  fetchMenus,
  refreshAvailability,
  setSelectedDate,
  togglePendingOrder,
  submitOrders,
  getAvailableDates,
  getMenusForDate,
  getPendingCount,
  getPendingCancellationCount,
} = useMenuStore();
```

2. Remove `cancellingId` and `cancelOrder` from the orderStore destructuring (line 74). Keep `orders` and `fetchOrders`:

```typescript
const { orders, fetchOrders } = useOrderStore();
```

3. Remove the `useDialog` import and `const { confirm } = useDialog();` (line 18/49) — no longer needed since we removed the confirmation dialog for cancellation.

4. Remove the entire `handleCancelFromMenu` callback (lines 130-155) — replaced by pending cancellation system.

5. Add an `isPendingCancel` callback (after the existing `isPending` callback):

```typescript
const isPendingCancel = useCallback(
  (item: GourmetMenuItem) => {
    const key = `${item.id}|${localDateKey(item.day)}`;
    return pendingCancellations.has(key);
  },
  [pendingCancellations]
);
```

6. Compute the FAB label dynamically:

```typescript
const pendingCount = getPendingCount();
const cancellationCount = getPendingCancellationCount();
const newOrderCount = pendingCount - cancellationCount;

const fabLabel = useMemo(() => {
  if (cancellationCount > 0 && newOrderCount > 0) {
    return `Änderungen bestätigen (${pendingCount})`;
  }
  if (cancellationCount > 0) {
    return `Stornieren (${cancellationCount})`;
  }
  return `Bestellen (${newOrderCount})`;
}, [pendingCount, cancellationCount, newOrderCount]);
```

7. Update the `MenuCard` rendering in the FlatList (replace lines 298-310):

```typescript
{group.items.map((item) => {
  const isOrdered = item.ordered || orderedCategories.has(item.category);
  return (
    <MenuCard
      key={`${item.id}-${formatGourmetDate(item.day)}`}
      item={item}
      isSelected={isPending(item)}
      ordered={isOrdered}
      isPendingCancel={isPendingCancel(item)}
      onToggle={() => togglePendingOrder(item.id, item.day)}
    />
  );
})}
```

8. Update both FAB labels (mobile line 373 and desktop line 346) to use `fabLabel`:

```typescript
<Text style={styles.fabText}>{fabLabel}</Text>
```

9. Remove the `orderPositionByCategory` useMemo (lines 119-128) — no longer needed.

10. Remove the `useDialog` import (line 18).

**Step 2: Run all tests to verify nothing breaks**

Run: `cd src/app && npx jest --verbose 2>&1 | tail -30`
Expected: All tests PASS

**Step 3: Commit**

```bash
git add src/app/app/(tabs)/index.tsx
git commit -m "feat: update MenusScreen for reordering UX (#9)"
```

---

### Task 6: Run full test suite and fix any issues

**Step 1: Run all tests**

Run: `cd src/app && npx jest --verbose 2>&1 | tail -60`
Expected: All 178+ tests PASS

**Step 2: Fix any failures**

If any tests fail, investigate and fix. Common issues:
- The menuStore test mock for orderStore may need the `orders` property to be accessible
- Existing tests that reference `onCancel`/`isCancelling` props may need updating

**Step 3: Commit if fixes were needed**

```bash
git add -A
git commit -m "fix: resolve test failures from reordering changes (#9)"
```

---

### Task 7: Visual verification on iOS Simulator

**Step 1: Build and run the app**

Run: `cd src/app && npx expo run:ios`

**Step 2: Verify the Menu tab**

Use iOS Simulator MCP tools to verify:

1. Navigate to Menu tab
2. Screenshot the menu page — verify ordered items show green "Bestellt" badge
3. Tap an ordered item — verify it shows "Wird storniert" badge with strikethrough and dashed border
4. Tap an available item — verify it shows blue/primary selected state with checkmark
5. Verify FAB shows correct label:
   - With only cancellation → "Stornieren (1)"
   - With only new order → "Bestellen (1)"
   - With both → "Änderungen bestätigen (2)"
6. Tap FAB — verify the reorder flow works (cancels then orders)
7. Verify unavailable items remain greyed out and non-tappable
8. Navigate between days — verify pending changes persist across day navigation

**Step 3: Commit if any visual fixes were needed**

```bash
git add -A
git commit -m "fix: visual adjustments for menu reordering (#9)"
```
