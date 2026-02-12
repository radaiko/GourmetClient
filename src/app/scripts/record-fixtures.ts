/**
 * Fixture recorder script.
 *
 * Logs into Gourmet and Ventopay, saves raw HTML/JSON responses as fixture
 * files, then sanitizes personal data so fixtures are safe to commit.
 *
 * Usage:
 *   cp .env.example .env   # fill in real credentials (in project root)
 *   npm run record-fixtures
 *
 * Requires: GOURMET_USERNAME, GOURMET_PASSWORD, VENTOPAY_USERNAME, VENTOPAY_PASSWORD in .env
 */

import * as path from 'path';
import dotenv from 'dotenv';

// Load .env from project root (two levels up from src/app/scripts/)
dotenv.config({ path: path.resolve(__dirname, '../../../.env') });

import axios from 'axios';
import { wrapper } from 'axios-cookiejar-support';
import { CookieJar } from 'tough-cookie';
import * as cheerio from 'cheerio';
import * as fs from 'fs';

// ---------------------------------------------------------------------------
// Config
// ---------------------------------------------------------------------------

const GOURMET_BASE = 'https://alaclickneu.gourmet.at';
const VENTOPAY_BASE = 'https://my.ventopay.com/mocca.website';
const VENTOPAY_COMPANY_ID = '0da8d3ec-0178-47d5-9ccd-a996f04acb61';

const FIXTURE_DIR = path.resolve(__dirname, '../src-rn/__tests__/fixtures');

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function ensureDir(dir: string) {
  fs.mkdirSync(dir, { recursive: true });
}

function saveFixture(relativePath: string, content: string) {
  const fullPath = path.join(FIXTURE_DIR, relativePath);
  ensureDir(path.dirname(fullPath));
  fs.writeFileSync(fullPath, content, 'utf-8');
  console.log(`  Saved ${relativePath} (${content.length} bytes)`);
}

function requireEnv(name: string): string {
  const val = process.env[name];
  if (!val) {
    throw new Error(`Missing environment variable: ${name}. Add it to .env`);
  }
  return val;
}

// ---------------------------------------------------------------------------
// Gourmet recording
// ---------------------------------------------------------------------------

async function recordGourmet() {
  console.log('\n=== Recording Gourmet fixtures ===\n');

  const jar = new CookieJar();
  const client = wrapper(axios.create({
    jar,
    maxRedirects: 5,
    validateStatus: (s) => s >= 200 && s < 400,
  }));

  const username = requireEnv('GOURMET_USERNAME');
  const password = requireEnv('GOURMET_PASSWORD');

  // Step 1: GET login page
  console.log('1. GET /start/ (login page)');
  const loginPageRes = await client.get(`${GOURMET_BASE}/start/`, { responseType: 'text' });
  const loginPageHtml: string = loginPageRes.data;
  saveFixture('gourmet/login-page.html', loginPageHtml);

  // Extract CSRF tokens
  const $ = cheerio.load(loginPageHtml);
  const form = $('form:first-of-type');
  const ufprt = form.find('input[name="ufprt"]').attr('value');
  const ncforminfo = form.find('input[name="__ncforminfo"]').attr('value');
  if (!ufprt || !ncforminfo) {
    throw new Error('Could not extract ufprt / __ncforminfo from login page');
  }
  console.log(`   ufprt length: ${ufprt.length}, __ncforminfo length: ${ncforminfo.length}`);

  // Step 2: POST login (multipart/form-data)
  console.log('2. POST /start/ (login)');
  const loginRes = await client.postForm(`${GOURMET_BASE}/start/`, {
    Username: username,
    Password: password,
    RememberMe: 'false',
    ufprt,
    __ncforminfo: ncforminfo,
  }, { responseType: 'text' });
  const loginSuccessHtml: string = loginRes.data;

  if (!loginSuccessHtml.includes('/einstellungen/') && !loginSuccessHtml.includes('loginname')) {
    throw new Error('Gourmet login failed - check credentials');
  }
  console.log('   Login successful');
  saveFixture('gourmet/login-success.html', loginSuccessHtml);

  // Extract user info for billing requests and sanitization
  const $post = cheerio.load(loginSuccessHtml);
  let shopModelId = $post('#shopModel').attr('value') || '';
  let eaterId = $post('#eater').attr('value') || '';
  let staffGroupId = $post('#staffGroup').attr('value') || '';
  const displayName = $post('span.loginname').text().trim();

  // If user info not in POST response, fetch start page again
  if (!shopModelId || !eaterId || !staffGroupId) {
    console.log('   User info not in POST response, re-fetching /start/...');
    const startRes = await client.get(`${GOURMET_BASE}/start/`, { responseType: 'text' });
    const startHtml: string = startRes.data;
    const $start = cheerio.load(startHtml);
    shopModelId = $start('#shopModel').attr('value') || '';
    eaterId = $start('#eater').attr('value') || '';
    staffGroupId = $start('#staffGroup').attr('value') || '';
  }

  console.log(`   User: ${displayName}, shopModelId: ${shopModelId.substring(0, 10)}...`);

  // Step 3: GET menus page 0
  console.log('3. GET /menus/ (page 0)');
  const menusRes = await client.get(`${GOURMET_BASE}/menus/`, { responseType: 'text' });
  const menusPage0Html: string = menusRes.data;
  saveFixture('gourmet/menus-page-0.html', menusPage0Html);

  // Step 4: GET subsequent menu pages until no next link
  let pageNum = 1;
  let lastHtml = menusPage0Html;
  while (pageNum < 10) {
    const $page = cheerio.load(lastHtml);
    if ($page('a[class*="menues-next"]').length === 0) {
      console.log(`   No more menu pages after page ${pageNum - 1}`);
      break;
    }

    console.log(`4. GET /menus/?page=${pageNum}`);
    const pageRes = await client.get(`${GOURMET_BASE}/menus/`, {
      params: { page: String(pageNum) },
      responseType: 'text',
    });
    lastHtml = pageRes.data;
    saveFixture(`gourmet/menus-page-${pageNum}.html`, lastHtml);
    pageNum++;
  }

  // Step 5: GET orders page
  console.log('5. GET /bestellungen/ (orders)');
  const ordersRes = await client.get(`${GOURMET_BASE}/bestellungen/`, { responseType: 'text' });
  saveFixture('gourmet/orders-page.html', ordersRes.data);

  // Step 6: POST billing (current month)
  console.log('6. POST GetMyBillings (current month)');
  const billingCurrentRes = await client.post(
    `${GOURMET_BASE}/umbraco/api/AlaMyBillingApi/GetMyBillings`,
    { eaterId, shopModelId, checkLastMonthNumber: '0' },
    {
      headers: { 'Content-Type': 'application/json' },
      responseType: 'text',
    },
  );
  saveFixture('gourmet/billing-current.json', billingCurrentRes.data);

  // Step 7: POST billing (last month)
  console.log('7. POST GetMyBillings (last month)');
  const billingLastRes = await client.post(
    `${GOURMET_BASE}/umbraco/api/AlaMyBillingApi/GetMyBillings`,
    { eaterId, shopModelId, checkLastMonthNumber: '1' },
    {
      headers: { 'Content-Type': 'application/json' },
      responseType: 'text',
    },
  );
  saveFixture('gourmet/billing-last-month.json', billingLastRes.data);

  console.log('\nGourmet recording complete.');

  // Return values needed for sanitization
  return { username, shopModelId, eaterId, staffGroupId, displayName };
}

// ---------------------------------------------------------------------------
// Ventopay recording
// ---------------------------------------------------------------------------

async function recordVentopay() {
  console.log('\n=== Recording Ventopay fixtures ===\n');

  const jar = new CookieJar();
  const client = wrapper(axios.create({
    jar,
    maxRedirects: 5,
    validateStatus: (s) => s >= 200 && s < 400,
  }));

  const username = requireEnv('VENTOPAY_USERNAME');
  const password = requireEnv('VENTOPAY_PASSWORD');

  // Step 1: GET login page
  console.log('1. GET Login.aspx');
  const loginPageRes = await client.get(`${VENTOPAY_BASE}/Login.aspx`, { responseType: 'text' });
  const loginPageHtml: string = loginPageRes.data;
  saveFixture('ventopay/login-page.html', loginPageHtml);

  // Extract ASP.NET state
  const $ = cheerio.load(loginPageHtml);
  const viewState = $('#__VIEWSTATE').attr('value') || '';
  const viewStateGenerator = $('#__VIEWSTATEGENERATOR').attr('value') || '';
  const eventValidation = $('#__EVENTVALIDATION').attr('value') || '';
  const lastFocus = $('#__LASTFOCUS').attr('value') || '';
  const eventTarget = $('#__EVENTTARGET').attr('value') || '';
  const eventArgument = $('#__EVENTARGUMENT').attr('value') || '';

  if (!viewState || !viewStateGenerator || !eventValidation) {
    throw new Error('Could not extract ASP.NET state from login page');
  }
  console.log(`   VIEWSTATE length: ${viewState.length}`);

  // Step 2: POST login (url-encoded)
  console.log('2. POST Login.aspx (login)');
  const loginData = new URLSearchParams({
    __LASTFOCUS: lastFocus,
    __EVENTTARGET: eventTarget,
    __EVENTARGUMENT: eventArgument,
    __VIEWSTATE: viewState,
    __VIEWSTATEGENERATOR: viewStateGenerator,
    __EVENTVALIDATION: eventValidation,
    DropDownList1: VENTOPAY_COMPANY_ID,
    TxtUsername: username,
    TxtPassword: password,
    BtnLogin: 'Login',
    languageRadio: 'DE',
  });

  const loginRes = await client.post(
    `${VENTOPAY_BASE}/Login.aspx`,
    loginData.toString(),
    {
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      responseType: 'text',
    },
  );
  const loginSuccessHtml: string = loginRes.data;

  if (!/href="Ausloggen\.aspx"/i.test(loginSuccessHtml)) {
    throw new Error('Ventopay login failed - check credentials');
  }
  console.log('   Login successful');
  saveFixture('ventopay/login-success.html', loginSuccessHtml);

  // Step 3: GET transactions (current month)
  const now = new Date();
  const fromDate = `01.${String(now.getMonth() + 1).padStart(2, '0')}.${now.getFullYear()}`;
  const untilDate = `${String(now.getDate()).padStart(2, '0')}.${String(now.getMonth() + 1).padStart(2, '0')}.${now.getFullYear()}`;

  console.log(`3. GET Transaktionen.aspx (${fromDate} - ${untilDate})`);
  const transRes = await client.get(`${VENTOPAY_BASE}/Transaktionen.aspx`, {
    params: { fromDate, untilDate },
    responseType: 'text',
  });
  saveFixture('ventopay/transactions-page.html', transRes.data);

  // Step 4: GET transactions (empty - far future date range)
  const emptyFrom = '01.01.2099';
  const emptyUntil = '31.12.2099';
  console.log(`4. GET Transaktionen.aspx (empty: ${emptyFrom} - ${emptyUntil})`);
  const emptyRes = await client.get(`${VENTOPAY_BASE}/Transaktionen.aspx`, {
    params: { fromDate: emptyFrom, untilDate: emptyUntil },
    responseType: 'text',
  });
  saveFixture('ventopay/transactions-empty.html', emptyRes.data);

  console.log('\nVentopay recording complete.');

  return { username };
}

// ---------------------------------------------------------------------------
// Sanitization
// ---------------------------------------------------------------------------

function sanitizeFixtures(
  gourmet: { username: string; shopModelId: string; eaterId: string; staffGroupId: string; displayName: string },
  ventopay: { username: string },
) {
  console.log('\n=== Sanitizing fixtures ===\n');

  const replacements: [string, string][] = [
    [gourmet.username, 'TestUser'],
    [gourmet.shopModelId, 'SM-TEST-123'],
    [gourmet.eaterId, 'EATER-TEST-456'],
    [gourmet.staffGroupId, 'SG-TEST-789'],
  ];

  // Add display name replacement (may differ from username)
  if (gourmet.displayName && gourmet.displayName !== gourmet.username) {
    replacements.push([gourmet.displayName, 'Test User']);
  }

  // Add Ventopay username if different from Gourmet
  if (ventopay.username !== gourmet.username) {
    replacements.push([ventopay.username, 'TestUser']);
  }

  // Filter out empty strings to avoid infinite replacement loops
  const validReplacements = replacements.filter(([from]) => from.length > 0);

  // Walk all fixture files
  function walkDir(dir: string): string[] {
    const files: string[] = [];
    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
      const fullPath = path.join(dir, entry.name);
      if (entry.isDirectory()) {
        files.push(...walkDir(fullPath));
      } else {
        files.push(fullPath);
      }
    }
    return files;
  }

  const files = walkDir(FIXTURE_DIR);
  for (const filePath of files) {
    let content = fs.readFileSync(filePath, 'utf-8');
    let changed = false;

    for (const [from, to] of validReplacements) {
      if (content.includes(from)) {
        content = content.replaceAll(from, to);
        changed = true;
      }
    }

    if (changed) {
      fs.writeFileSync(filePath, content, 'utf-8');
      console.log(`  Sanitized ${path.relative(FIXTURE_DIR, filePath)}`);
    }
  }

  console.log('\nSanitization complete.');
}

// ---------------------------------------------------------------------------
// Main
// ---------------------------------------------------------------------------

async function main() {
  console.log('Fixture recorder');
  console.log(`Output directory: ${FIXTURE_DIR}`);

  let gourmetInfo: Awaited<ReturnType<typeof recordGourmet>> | null = null;
  let ventopayInfo: Awaited<ReturnType<typeof recordVentopay>> | null = null;

  try {
    gourmetInfo = await recordGourmet();
  } catch (err) {
    console.error('\nGourmet recording FAILED:', err instanceof Error ? err.message : err);
  }

  try {
    ventopayInfo = await recordVentopay();
  } catch (err) {
    console.error('\nVentopay recording FAILED:', err instanceof Error ? err.message : err);
  }

  // Sanitize whatever we managed to record
  if (gourmetInfo || ventopayInfo) {
    sanitizeFixtures(
      gourmetInfo ?? { username: '', shopModelId: '', eaterId: '', staffGroupId: '', displayName: '' },
      ventopayInfo ?? { username: '' },
    );
  }

  console.log('\nDone.');
}

main().catch((err) => {
  console.error('Fatal error:', err);
  process.exit(1);
});
