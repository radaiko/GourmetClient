/**
 * Desktop HTTP proxy — patches Axios to route requests through Tauri's Rust side.
 *
 * This file uses the .web.ts extension so it's only bundled for web/desktop.
 * On native (iOS/Android), the no-op tauriHttp.ts is used instead.
 *
 * Bypasses the webview's CORS/CSP restrictions without touching the scraping layer.
 *
 * IMPORTANT: The patch runs at module load time (not via a function call) so that
 * axios.create is patched BEFORE any Zustand stores import and create their
 * Axios instances.  _layout.tsx imports this module before the store modules,
 * guaranteeing correct ordering.
 */
import axios from 'axios';
import type { InternalAxiosRequestConfig, AxiosResponse } from 'axios';
import { isDesktop } from './platform';

interface TauriHttpResponse {
  status: number;
  headers: Record<string, string>;
  setCookies: string[];
  body: string;
  url: string;
}

function getInvoke(): ((cmd: string, args?: any) => Promise<any>) | null {
  if (typeof window !== 'undefined' && '__TAURI_INTERNALS__' in window) {
    return (window as any).__TAURI_INTERNALS__.invoke;
  }
  return null;
}

async function tauriAdapter(config: InternalAxiosRequestConfig): Promise<AxiosResponse> {
  const invoke = getInvoke();
  if (!invoke) throw new Error('Tauri IPC not available');

  // Build full URL
  const baseURL = config.baseURL || '';
  let url = config.url || '';
  if (!url.startsWith('http')) {
    url = baseURL + (url.startsWith('/') ? '' : '/') + url;
  }

  // Append query params
  if (config.params) {
    const params = new URLSearchParams(config.params);
    url += (url.includes('?') ? '&' : '?') + params.toString();
  }

  // Build headers — exclude Cookie (Rust cookie jar handles it)
  const headers: Record<string, string> = {};
  if (config.headers) {
    const raw = typeof config.headers.toJSON === 'function'
      ? config.headers.toJSON()
      : config.headers;
    for (const [key, value] of Object.entries(raw)) {
      if (key.toLowerCase() !== 'cookie' && typeof value === 'string') {
        headers[key] = value;
      }
    }
  }

  const method = (config.method || 'GET').toUpperCase();
  const contentType = headers['Content-Type'] || headers['content-type'] || '';

  let body: string | undefined;
  let formData: Record<string, string> | undefined;

  if (config.data != null) {
    if (contentType.includes('multipart/form-data') || config.data instanceof FormData) {
      // Multipart: extract key-value pairs
      if (config.data instanceof FormData) {
        formData = {};
        config.data.forEach((value, key) => { formData![key] = value.toString(); });
      } else if (typeof config.data === 'object') {
        formData = {};
        for (const [k, v] of Object.entries(config.data)) {
          formData[k] = String(v);
        }
      }
      // Remove Content-Type so Rust/reqwest sets it with the correct boundary
      delete headers['Content-Type'];
      delete headers['content-type'];
    } else if (typeof config.data === 'string') {
      body = config.data;
    } else if (typeof config.data === 'object') {
      if (contentType.includes('application/json')) {
        body = JSON.stringify(config.data);
      } else {
        body = new URLSearchParams(config.data as Record<string, string>).toString();
      }
    }
  }

  const result: TauriHttpResponse = await invoke('http_request', {
    request: { url, method, headers, body, formData },
  });

  // Build response headers; re-attach set-cookie as array
  const responseHeaders: Record<string, any> = { ...result.headers };
  if (result.setCookies.length > 0) {
    responseHeaders['set-cookie'] = result.setCookies;
  }

  // Parse body based on expected response type
  let data: any = result.body;
  if (config.responseType !== 'text') {
    try { data = JSON.parse(result.body); } catch { /* keep as string */ }
  }

  // Validate status
  const validateStatus = config.validateStatus || ((s: number) => s >= 200 && s < 300);
  if (!validateStatus(result.status)) {
    const error = new Error(`Request failed with status ${result.status}`) as any;
    error.response = { data, status: result.status, headers: responseHeaders, config };
    error.config = config;
    error.isAxiosError = true;
    throw error;
  }

  return {
    data,
    status: result.status,
    statusText: '',
    headers: responseHeaders,
    config,
    request: {},
  };
}

// Patch axios.create at module load time so all Axios instances (including those
// created by Zustand stores during their module evaluation) use the Tauri adapter.
// No-op on browser web (non-Tauri) and never bundled on native.
if (isDesktop()) {
  const originalCreate = axios.create.bind(axios);
  axios.create = function patchedCreate(config?: any) {
    return originalCreate({ ...config, adapter: tauriAdapter });
  };
}

/** @deprecated Patch now runs automatically at module load. Kept for back-compat. */
export function installTauriHttpProxy(): void {}

/** Reset the Rust HTTP client (clears cookies). Call on logout. */
export async function resetTauriHttp(): Promise<void> {
  const invoke = getInvoke();
  if (invoke) {
    await invoke('http_reset');
  }
}
