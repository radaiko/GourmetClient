# Cache Menus and Ordered Menus for Faster Cold Start

**Issue:** [#11](https://github.com/radaiko/SnackPilot/issues/11)
**Date:** 2026-02-21

## Problem

On cold start, the Menus and Orders tabs show a loading spinner while data is fetched from the network. The Billing tab already caches data in AsyncStorage and shows it instantly — menus and orders should do the same.

## Design

Mirror the billing store's AsyncStorage caching pattern for both menuStore and orderStore.

### UX: Show stale + background refresh

On cold start:
1. Load cached data from AsyncStorage immediately (no spinner)
2. Kick off a background network fetch regardless of cache age
3. Update state silently when fresh data arrives

### menuStore changes

- Add `loadCachedMenus()` — reads `menus_items` from AsyncStorage, deserializes `day` Date fields, sets `items` state
- Modify `fetchMenus()` — after successful fetch, write items to AsyncStorage
- Add `serializeMenuItems()` / `deserializeMenuItems()` helpers for Date conversion

Cache key: `menus_items`

### orderStore changes

- Add `loadCachedOrders()` — reads `orders_list` from AsyncStorage, deserializes `date` Date fields, sets `orders` state
- Modify `fetchOrders()` — after successful fetch, write orders to AsyncStorage
- Add `serializeOrders()` / `deserializeOrders()` helpers for Date conversion

Cache key: `orders_list`

### Screen integration

**index.tsx (Menus tab):** In `triggerRefresh`, call `loadCachedMenus()` first. If cached data exists, use `refreshAvailability()` for background update; otherwise full `fetchMenus()`.

**orders.tsx (Orders tab):** In `useFocusEffect`, call `loadCachedOrders()` before `fetchOrders()`.

### Tests

Add tests for serialize/deserialize round-trips and loadCached* functions, mirroring the existing billingStore test patterns.

## Out of scope

- Changing storage backend (AsyncStorage is sufficient for this data size)
- Cache eviction / TTL for persisted data (background refresh always runs)
- Any changes to web scraping logic
