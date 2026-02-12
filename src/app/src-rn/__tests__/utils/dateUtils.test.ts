describe('dateUtils', () => {
  let formatGourmetDate: typeof import('../../utils/dateUtils').formatGourmetDate;
  let parseGourmetDate: typeof import('../../utils/dateUtils').parseGourmetDate;
  let parseGourmetOrderDate: typeof import('../../utils/dateUtils').parseGourmetOrderDate;
  let localDateKey: typeof import('../../utils/dateUtils').localDateKey;
  let isSameDay: typeof import('../../utils/dateUtils').isSameDay;
  let isOrderingCutoff: typeof import('../../utils/dateUtils').isOrderingCutoff;

  beforeEach(() => {
    jest.resetModules();
    const mod = require('../../utils/dateUtils');
    formatGourmetDate = mod.formatGourmetDate;
    parseGourmetDate = mod.parseGourmetDate;
    parseGourmetOrderDate = mod.parseGourmetOrderDate;
    localDateKey = mod.localDateKey;
    isSameDay = mod.isSameDay;
    isOrderingCutoff = mod.isOrderingCutoff;
  });

  describe('formatGourmetDate', () => {
    it('formats a date as MM-dd-yyyy', () => {
      const date = new Date(2026, 1, 10); // Feb 10 2026
      expect(formatGourmetDate(date)).toBe('02-10-2026');
    });

    it('pads single-digit month and day', () => {
      const date = new Date(2026, 0, 5); // Jan 5 2026
      expect(formatGourmetDate(date)).toBe('01-05-2026');
    });
  });

  describe('parseGourmetDate', () => {
    it('parses MM-dd-yyyy to correct Date', () => {
      const date = parseGourmetDate('02-10-2026');
      expect(date.getFullYear()).toBe(2026);
      expect(date.getMonth()).toBe(1); // February
      expect(date.getDate()).toBe(10);
    });

    it('handles January dates', () => {
      const date = parseGourmetDate('01-01-2026');
      expect(date.getFullYear()).toBe(2026);
      expect(date.getMonth()).toBe(0); // January
      expect(date.getDate()).toBe(1);
    });

    it('roundtrips with formatGourmetDate', () => {
      const original = new Date(2026, 5, 15); // Jun 15 2026
      const formatted = formatGourmetDate(original);
      const parsed = parseGourmetDate(formatted);
      expect(parsed.getFullYear()).toBe(original.getFullYear());
      expect(parsed.getMonth()).toBe(original.getMonth());
      expect(parsed.getDate()).toBe(original.getDate());
    });
  });

  describe('parseGourmetOrderDate', () => {
    it('parses dd.MM.yyyy HH:mm:ss format', () => {
      const date = parseGourmetOrderDate('10.02.2026 12:30:00');
      expect(date.getFullYear()).toBe(2026);
      expect(date.getMonth()).toBe(1); // February
      expect(date.getDate()).toBe(10);
      expect(date.getHours()).toBe(12);
      expect(date.getMinutes()).toBe(30);
      expect(date.getSeconds()).toBe(0);
    });

    it('handles missing time part', () => {
      const date = parseGourmetOrderDate('05.01.2026');
      expect(date.getFullYear()).toBe(2026);
      expect(date.getMonth()).toBe(0); // January
      expect(date.getDate()).toBe(5);
      expect(date.getHours()).toBe(0);
      expect(date.getMinutes()).toBe(0);
      expect(date.getSeconds()).toBe(0);
    });
  });

  describe('localDateKey', () => {
    it('returns YYYY-MM-DD for a date', () => {
      const date = new Date(2026, 1, 10); // Feb 10 2026
      expect(localDateKey(date)).toBe('2026-02-10');
    });

    it('pads single-digit month and day', () => {
      const date = new Date(2026, 0, 5); // Jan 5 2026
      expect(localDateKey(date)).toBe('2026-01-05');
    });
  });

  describe('isSameDay', () => {
    it('returns true for same day with different times', () => {
      const a = new Date(2026, 1, 10, 8, 0, 0);
      const b = new Date(2026, 1, 10, 18, 30, 0);
      expect(isSameDay(a, b)).toBe(true);
    });

    it('returns false for different days', () => {
      const a = new Date(2026, 1, 10);
      const b = new Date(2026, 1, 11);
      expect(isSameDay(a, b)).toBe(false);
    });
  });

  describe('isOrderingCutoff', () => {
    beforeEach(() => {
      jest.useFakeTimers();
    });

    afterEach(() => {
      jest.useRealTimers();
    });

    it('returns false for today before 12:30 Vienna time', () => {
      // Feb 10 2026, 10:00 Vienna (CET = UTC+1) -> UTC 09:00
      jest.setSystemTime(new Date('2026-02-10T09:00:00Z'));
      const menuDate = new Date(2026, 1, 10);
      expect(isOrderingCutoff(menuDate)).toBe(false);
    });

    it('returns true for today after 12:30 Vienna time', () => {
      // Feb 10 2026, 12:31 Vienna (CET = UTC+1) -> UTC 11:31
      jest.setSystemTime(new Date('2026-02-10T11:31:00Z'));
      const menuDate = new Date(2026, 1, 10);
      expect(isOrderingCutoff(menuDate)).toBe(true);
    });

    it('returns false for a future date', () => {
      // Current time: Feb 10 2026, 14:00 Vienna -> UTC 13:00
      jest.setSystemTime(new Date('2026-02-10T13:00:00Z'));
      const futureDate = new Date(2026, 1, 11); // Feb 11
      expect(isOrderingCutoff(futureDate)).toBe(false);
    });
  });
});
