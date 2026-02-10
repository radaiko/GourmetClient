/**
 * Format date as MM-dd-yyyy (Gourmet system format)
 */
export function formatGourmetDate(date: Date): string {
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  const year = date.getFullYear();
  return `${month}-${day}-${year}`;
}

/**
 * Parse MM-dd-yyyy format to Date
 */
export function parseGourmetDate(dateStr: string): Date {
  const [month, day, year] = dateStr.split('-').map(Number);
  return new Date(year, month - 1, day);
}

/**
 * Parse dd.MM.yyyy HH:mm:ss format to Date (used in orders)
 */
export function parseGourmetOrderDate(dateStr: string): Date {
  const [datePart, timePart] = dateStr.split(' ');
  const [day, month, year] = datePart.split('.').map(Number);
  const [hours, minutes, seconds] = (timePart || '00:00:00').split(':').map(Number);
  return new Date(year, month - 1, day, hours, minutes, seconds);
}

/**
 * Format date for display (e.g., "Mo., 10. Feb.")
 */
export function formatDisplayDate(date: Date): string {
  return date.toLocaleDateString('de-AT', {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
  });
}

/**
 * Local date key (YYYY-MM-DD) without timezone shifting.
 * Unlike toISOString() which converts to UTC first (shifting dates in CET/CEST),
 * this always uses the local date components.
 */
export function localDateKey(date: Date): string {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

/**
 * Check if two dates are the same calendar day
 */
export function isSameDay(a: Date, b: Date): boolean {
  return (
    a.getFullYear() === b.getFullYear() &&
    a.getMonth() === b.getMonth() &&
    a.getDate() === b.getDate()
  );
}

/**
 * Check if ordering is blocked for a given menu date.
 * Today's menu cannot be ordered after 12:30 Europe/Vienna time.
 * Future dates are never blocked.
 */
export function isOrderingCutoff(menuDate: Date): boolean {
  const now = new Date();
  if (!isSameDay(menuDate, now)) return false;

  const fmt = new Intl.DateTimeFormat('en-US', {
    timeZone: 'Europe/Vienna',
    hour: 'numeric',
    minute: 'numeric',
    hour12: false,
  });
  const parts = fmt.formatToParts(now);
  const hour = Number(parts.find((p) => p.type === 'hour')?.value ?? 0);
  const minute = Number(parts.find((p) => p.type === 'minute')?.value ?? 0);
  return hour * 60 + minute >= 12 * 60 + 30;
}
