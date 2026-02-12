import { GourmetMenuItem, GourmetMenuCategory } from '../types/menu';
import { GourmetOrderedMenu } from '../types/order';
import { GourmetBill } from '../types/billing';
import { VentopayTransaction } from '../types/ventopay';

// ── Seeded pseudo-random number generator (deterministic per day) ──

function seededRandom(seed: number): () => number {
  let s = seed;
  return () => {
    s = (s * 1103515245 + 12345) & 0x7fffffff;
    return s / 0x7fffffff;
  };
}

function todaySeed(): number {
  const d = new Date();
  return d.getFullYear() * 10000 + (d.getMonth() + 1) * 100 + d.getDate();
}

// ── Menu dish pools ──

interface Dish {
  title: string;
  subtitle: string;
  allergens: string[];
}

const MENU1_DISHES: Dish[] = [
  { title: 'Wiener Schnitzel', subtitle: 'mit Petersilerdäpfel und Preiselbeeren', allergens: ['A', 'C', 'G'] },
  { title: 'Schweinsbraten', subtitle: 'mit Semmelknödel und Sauerkraut', allergens: ['A', 'C', 'G'] },
  { title: 'Tafelspitz', subtitle: 'mit Apfelkren und Schnittlauchsauce', allergens: ['A', 'G', 'L'] },
  { title: 'Rindsgulasch', subtitle: 'mit Nockerl und Essiggurkerl', allergens: ['A', 'C', 'G'] },
  { title: 'Backhendl', subtitle: 'mit Erdäpfelsalat', allergens: ['A', 'C', 'G'] },
  { title: 'Cordon Bleu', subtitle: 'mit Reis und Preiselbeeren', allergens: ['A', 'C', 'G'] },
  { title: 'Zwiebelrostbraten', subtitle: 'mit Bratkartoffeln und Röstzwiebeln', allergens: ['A', 'G', 'L'] },
  { title: 'Faschierter Braten', subtitle: 'mit Erdäpfelpüree und Bratensauce', allergens: ['A', 'C', 'G'] },
  { title: 'Kalbsrahmgulasch', subtitle: 'mit Butternockerl', allergens: ['A', 'C', 'G'] },
  { title: 'Gebackene Leber', subtitle: 'mit Erdäpfelsalat und Preiselbeeren', allergens: ['A', 'C', 'G'] },
];

const MENU2_DISHES: Dish[] = [
  { title: 'Gemüselasagne', subtitle: 'mit Blattsalat', allergens: ['A', 'C', 'G'] },
  { title: 'Spinatknödel', subtitle: 'mit Parmesan und brauner Butter', allergens: ['A', 'C', 'G'] },
  { title: 'Käsespätzle', subtitle: 'mit Röstzwiebeln und grünem Salat', allergens: ['A', 'C', 'G'] },
  { title: 'Pasta Primavera', subtitle: 'mit Saisongemüse und Basilikum', allergens: ['A', 'C'] },
  { title: 'Kartoffelgratin', subtitle: 'mit buntem Gemüse', allergens: ['A', 'G'] },
  { title: 'Topfenknödel', subtitle: 'mit Butterbröseln und Apfelmus', allergens: ['A', 'C', 'G'] },
  { title: 'Gemüse-Curry', subtitle: 'mit Basmatireis und Naan-Brot', allergens: ['A', 'G'] },
  { title: 'Flammkuchen', subtitle: 'mit Sauerrahm, Zwiebeln und Speck', allergens: ['A', 'G'] },
  { title: 'Eierschwammerlgulasch', subtitle: 'mit Semmelknödel', allergens: ['A', 'C', 'G'] },
  { title: 'Palatschinken', subtitle: 'mit Topfenfülle und Vanillesauce', allergens: ['A', 'C', 'G'] },
];

const MENU3_DISHES: Dish[] = [
  { title: 'Grillhendl', subtitle: 'mit Pommes frites und Cole Slaw', allergens: ['A', 'G', 'M'] },
  { title: 'Fischfilet', subtitle: 'mit Dillsauce und Petersilerdäpfel', allergens: ['A', 'C', 'D', 'G'] },
  { title: 'Putengeschnetzeltes', subtitle: 'mit Reis und Champignons', allergens: ['A', 'G'] },
  { title: 'Cevapcici', subtitle: 'mit Djuvec-Reis und Ajvar', allergens: ['A', 'C'] },
  { title: 'Hühnercurry', subtitle: 'mit Jasminreis und Mango-Chutney', allergens: ['A', 'G'] },
  { title: 'Leberkäse', subtitle: 'mit Spiegelei und Erdäpfelsalat', allergens: ['A', 'C', 'G'] },
  { title: 'Bratwürstel', subtitle: 'mit Senf und Sauerkraut', allergens: ['A', 'M'] },
  { title: 'Puten-Wrap', subtitle: 'mit Salat, Tomaten und Joghurt-Dressing', allergens: ['A', 'G'] },
  { title: 'Lachs gegrillt', subtitle: 'mit Zitronenbutter und Gemüsereis', allergens: ['D', 'G'] },
  { title: 'Spaghetti Bolognese', subtitle: 'mit Parmesan', allergens: ['A', 'C', 'G'] },
];

const SOUP_SALAD_DISHES: Dish[] = [
  { title: 'Frittatensuppe', subtitle: 'Klare Rindsuppe mit Frittaten', allergens: ['A', 'C', 'G'] },
  { title: 'Kürbiscremesuppe', subtitle: 'mit Kürbiskernöl und Croutons', allergens: ['A', 'G'] },
  { title: 'Gemischter Salat', subtitle: 'mit Hausdressing', allergens: ['M'] },
  { title: 'Grießnockerlsuppe', subtitle: 'Klare Suppe mit Grießnockerl', allergens: ['A', 'C', 'G'] },
  { title: 'Tomatencremesuppe', subtitle: 'mit Basilikum und Croutons', allergens: ['A', 'G'] },
  { title: 'Kartoffelsuppe', subtitle: 'mit Einlage und Brot', allergens: ['A', 'G', 'L'] },
  { title: 'Caesar Salad', subtitle: 'mit Hühnerstreifen und Parmesan', allergens: ['A', 'C', 'G'] },
  { title: 'Leberknödelsuppe', subtitle: 'Klare Rindsuppe mit Leberknödel', allergens: ['A', 'C', 'G'] },
  { title: 'Gemüsesuppe', subtitle: 'mit frischem Saisongemüse', allergens: ['A', 'L'] },
  { title: 'Blattsalat', subtitle: 'mit Kernöl-Dressing und Kürbiskernen', allergens: ['H'] },
];

const BILLING_DESCRIPTIONS = [
  'Menü I', 'Menü II', 'Menü III', 'Suppe & Salat',
];

const BILLING_PRICES = [6.80, 5.90, 6.20, 4.20, 5.50, 6.50, 5.80, 6.00, 4.80, 5.20];

// ── Helper functions ──

function getWeekdays(count: number, startFrom: Date): Date[] {
  const days: Date[] = [];
  const current = new Date(startFrom);
  current.setHours(0, 0, 0, 0);

  while (days.length < count) {
    const dow = current.getDay();
    if (dow >= 1 && dow <= 5) {
      days.push(new Date(current));
    }
    current.setDate(current.getDate() + 1);
  }
  return days;
}

function getPastWeekdaysOfMonth(year: number, month: number, today: Date): Date[] {
  const days: Date[] = [];
  const d = new Date(year, month, 1);
  while (d.getMonth() === month && d <= today) {
    if (d.getDay() >= 1 && d.getDay() <= 5) {
      days.push(new Date(d));
    }
    d.setDate(d.getDate() + 1);
  }
  return days;
}

// ── Generators ──

export function generateDemoMenus(): GourmetMenuItem[] {
  const rand = seededRandom(todaySeed());
  const today = new Date();
  today.setHours(0, 0, 0, 0);

  // Find the Monday of the current week (or today if Monday)
  const startDate = new Date(today);
  const dow = startDate.getDay();
  const diff = dow === 0 ? -6 : 1 - dow; // Monday
  startDate.setDate(startDate.getDate() + diff);

  const weekdays = getWeekdays(10, startDate);
  const items: GourmetMenuItem[] = [];

  const pools = [
    { dishes: MENU1_DISHES, category: GourmetMenuCategory.Menu1, idPrefix: 'demo-m1', price: '6,00 €' },
    { dishes: MENU2_DISHES, category: GourmetMenuCategory.Menu2, idPrefix: 'demo-m2', price: '6,00 €' },
    { dishes: MENU3_DISHES, category: GourmetMenuCategory.Menu3, idPrefix: 'demo-m3', price: '6,00 €' },
    { dishes: SOUP_SALAD_DISHES, category: GourmetMenuCategory.SoupAndSalad, idPrefix: 'demo-ss', price: '2,50 €' },
  ];

  for (let dayIndex = 0; dayIndex < weekdays.length; dayIndex++) {
    const day = weekdays[dayIndex];

    for (const pool of pools) {
      const dishIndex = (dayIndex + Math.floor(rand() * 3)) % pool.dishes.length;
      const dish = pool.dishes[dishIndex];

      items.push({
        id: `${pool.idPrefix}-${dayIndex}`,
        day,
        // title = category text (used for detectCategory), subtitle = dish description (displayed in card)
        title: pool.category,
        subtitle: `${dish.title} ${dish.subtitle}`,
        allergens: dish.allergens,
        available: true,
        ordered: false,
        category: pool.category,
        price: pool.price,
      });
    }
  }

  return items;
}

export function generateDemoBillings(checkLastMonthNumber: string): GourmetBill[] {
  const monthOffset = parseInt(checkLastMonthNumber, 10) || 0;
  const now = new Date();
  const targetYear = now.getFullYear();
  const targetMonth = now.getMonth() - monthOffset;
  const targetDate = new Date(targetYear, targetMonth, 1);
  const actualYear = targetDate.getFullYear();
  const actualMonth = targetDate.getMonth();

  const today = new Date();
  today.setHours(23, 59, 59, 999);

  const weekdays = getPastWeekdaysOfMonth(actualYear, actualMonth, today);
  const rand = seededRandom(actualYear * 100 + actualMonth);
  const bills: GourmetBill[] = [];

  for (let i = 0; i < weekdays.length; i++) {
    const day = weekdays[i];
    const descIndex = Math.floor(rand() * BILLING_DESCRIPTIONS.length);
    const priceIndex = Math.floor(rand() * BILLING_PRICES.length);
    const price = BILLING_PRICES[priceIndex];
    const subsidy = 1.50;

    bills.push({
      billNr: 100000 + i,
      billDate: day,
      location: 'Betriebsrestaurant',
      items: [
        {
          id: `demo-bill-item-${i}`,
          articleId: `demo-art-${descIndex}`,
          count: 1,
          description: BILLING_DESCRIPTIONS[descIndex],
          total: price,
          subsidy,
          discountValue: 0,
          isCustomMenu: false,
        },
      ],
      billing: price - subsidy,
    });
  }

  return bills;
}

export function generateDemoTransactions(fromDate: Date, toDate: Date): VentopayTransaction[] {
  const now = new Date();
  const rand = seededRandom(now.getFullYear() * 100 + now.getMonth());
  const transactions: VentopayTransaction[] = [];

  const current = new Date(fromDate);
  current.setHours(0, 0, 0, 0);
  const end = new Date(toDate);
  end.setHours(23, 59, 59, 999);

  let id = 0;
  while (current <= end) {
    const dow = current.getDay();
    if (dow >= 1 && dow <= 5) {
      // ~40% chance of a coffee transaction on any weekday
      const roll = rand();
      if (roll < 0.4) {
        const amounts = [0.50, 1.00, 1.20, 1.50, 2.00, 2.50];
        const amount = amounts[Math.floor(rand() * amounts.length)];
        const hour = 7 + Math.floor(rand() * 4); // 7-10 AM
        const minute = Math.floor(rand() * 60);
        const txDate = new Date(current);
        txDate.setHours(hour, minute, 0, 0);

        transactions.push({
          id: `demo-vp-${id++}`,
          date: txDate,
          amount,
          restaurant: 'Kaffeeautomat',
          location: 'Kaffeeautomat EG',
        });
      }
    }
    current.setDate(current.getDate() + 1);
  }

  return transactions;
}

// ── Order helpers ──

let orderIdCounter = 1;

export function createDemoOrder(
  date: Date,
  menuId: string,
  title: string,
  subtitle: string
): GourmetOrderedMenu {
  const id = orderIdCounter++;
  return {
    positionId: `demo-pos-${id}`,
    eatingCycleId: `demo-cycle-${id}`,
    date,
    title,
    subtitle,
    approved: false,
  };
}
