# Menu Reordering Design

**Issue**: [#9 — Enable reordering of menus on Menu Page](https://github.com/radaiko/SnackPilot/issues/9)
**Date**: 2026-02-21
**Approach**: Extend existing pending orders system (Approach A)

## Overview

Users can tap already-ordered menu cards to mark them for cancellation, then select different menus for the same day. All changes (cancellations + new orders) are confirmed with a single FAB press. The system handles the cancel-then-reorder flow behind the scenes.

## State Changes

### menuStore additions

- `pendingCancellations: Set<string>` — keys like `"menuId|dateStr"` for ordered items the user wants to cancel
- `togglePendingOrder` modified: when called on an ordered item, toggles in `pendingCancellations`; otherwise toggles in `pendingOrders` (existing behavior)
- `submitOrders` expanded: (1) cancel items in `pendingCancellations`, (2) add items in `pendingOrders`, (3) confirm, (4) refresh
- `getPendingCount` returns `pendingOrders.size + pendingCancellations.size`
- `clearPendingOrders` clears both sets

### No new API logic

Reuses existing `cancelOrder(positionId)` and `addToCart(items)` APIs.

## UI Changes

### MenuCard

| Item State | Behavior |
|---|---|
| **Ordered** | Green bg, "Bestellt" badge, **tappable** — tap adds to pendingCancellations |
| **Ordered + pending cancel** | Desaturated style, strikethrough subtitle, "Wird storniert" badge, tappable to undo |
| **Available** | Normal, tappable (no change) |
| **Selected (pending new)** | Blue/primary bg, checkmark (no change) |
| **Unavailable** | Greyed out, "Ausverkauft" badge, disabled (no change) |
| **Cutoff** | "Geschlossen" badge, disabled (no change) |

### Removed: inline "Stornieren" button

Cancellation is unified to tap-to-toggle, consistent with how selecting new orders works.

### FAB label

- Only new orders → "Bestellen (N)"
- Only cancellations → "Stornieren (N)"
- Both (reorder) → "Änderungen bestätigen (N)"

## Submit Flow

```
submitOrders():
  1. Collect pendingCancellations → resolve to positionIds via orderStore
  2. Collect pendingOrders → list of { menuId, date }
  3. Block if any today items past cutoff (12:30)
  4. Optimistic UI: mark cancelled items as ordered=false, new items as ordered=true
  5. Cancel orders (orderProgress: 'cancelling')
  6. Add to cart (orderProgress: 'adding')
  7. Confirm orders (orderProgress: 'confirming')
  8. Refresh orders + menus (orderProgress: 'refreshing')
  9. Clear orderProgress

  On error: revert optimistic update via fresh fetch, show error message
```

### Error handling

If cancellation succeeds but new order fails, the old order is lost. An error message is shown. No atomic swap exists in the external API, so this is accepted.

## Testing

1. **menuStore**: togglePendingOrder routes correctly (ordered→pendingCancellations, not ordered→pendingOrders), getPendingCount includes both, submitOrders handles cancel+add+mixed, optimistic update+rollback
2. **MenuCard**: ordered items are tappable, pending cancellation shows correct visual, unavailable items stay disabled
3. **Screen-level**: FAB label reflects pending composition

## Decisions

- **Inline on current view**: Keep existing day-by-day navigation, no separate reorder screen
- **Multi-select preserved**: Users can have multiple menus per day (e.g., MENU I + SUPPE & SALAT)
- **Tap-to-toggle unified**: Removed Stornieren button, all changes via tap + FAB confirm
- **Cancel then add**: No confirmation dialog before reorder since error handling shows the actual state
