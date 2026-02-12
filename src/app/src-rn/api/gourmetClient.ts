import axios, { AxiosInstance, AxiosResponse } from 'axios';
import { GOURMET_BASE_URL } from '../utils/constants';

const GOURMET_ORIGIN = 'https://alaclickneu.gourmet.at';

/**
 * Low-level HTTP client for the Gourmet website.
 *
 * Cookies are managed manually via interceptors to prevent stale native
 * cookies (NSURLSession/OkHttp) from persisting across app restarts and
 * interfering with fresh logins.
 *
 * All Gourmet forms use enctype="multipart/form-data", so postForm uses
 * axios.postForm() which handles multipart encoding + boundary automatically.
 *
 * CRITICAL: Do not add custom User-Agent headers.
 * CRITICAL: Do not add request throttling/delays.
 * CRITICAL: withCredentials MUST be false — we manage cookies manually.
 *           Setting it to true causes the native layer (NSURLSession) to also
 *           manage cookies, creating a dual-cookie conflict.
 */
export class GourmetHttpClient {
  private client: AxiosInstance;
  private cookies: Map<string, string> = new Map();
  private lastPageUrl: string = '';

  constructor() {
    this.client = axios.create({
      baseURL: GOURMET_BASE_URL,
      withCredentials: false,
      maxRedirects: 5,
      validateStatus: (status) => status >= 200 && status < 400,
    });

    // Intercept responses to capture Set-Cookie headers
    this.client.interceptors.response.use((response) => {
      const setCookie = response.headers['set-cookie'];
      if (setCookie) {
        const cookieArray = Array.isArray(setCookie) ? setCookie : [setCookie];
        for (const cookie of cookieArray) {
          const [nameValue] = cookie.split(';');
          const eqIndex = nameValue.indexOf('=');
          if (eqIndex > 0) {
            const name = nameValue.substring(0, eqIndex).trim();
            const value = nameValue.substring(eqIndex + 1).trim();
            this.cookies.set(name, value);
          }
        }
      }
      return response;
    });

    // Intercept requests to inject stored cookies
    this.client.interceptors.request.use((config) => {
      if (this.cookies.size > 0) {
        const cookieStr = Array.from(this.cookies.entries())
          .map(([name, value]) => `${name}=${value}`)
          .join('; ');
        config.headers['Cookie'] = cookieStr;
      }
      return config;
    });
  }

  /** GET request returning HTML string */
  async get(url: string, params?: Record<string, string>): Promise<string> {
    const response: AxiosResponse<string> = await this.client.get(url, {
      params,
      responseType: 'text',
    });
    this.lastPageUrl = typeof url === 'string' && url.startsWith('http')
      ? url
      : `${GOURMET_BASE_URL}${url.startsWith('/') ? '' : '/'}${url}`;

    console.log('[Gourmet GET]', url, 'status:', response.status, 'length:', response.data?.length);
    return response.data;
  }

  /** POST form data (multipart/form-data) returning HTML string.
   *  All Gourmet forms use enctype="multipart/form-data". */
  async postForm(url: string, data: Record<string, string>): Promise<string> {
    console.log('[Gourmet POST]', url, 'keys:', Object.keys(data));

    const response: AxiosResponse<string> = await this.client.postForm(
      url,
      data,
      {
        headers: {
          'Origin': GOURMET_ORIGIN,
          'Referer': this.lastPageUrl || url,
        },
        responseType: 'text',
      }
    );

    console.log('[Gourmet POST] response status:', response.status, 'length:', response.data?.length);
    return response.data;
  }

  /** POST JSON data returning JSON response */
  async postJson<T>(url: string, data: unknown): Promise<T> {
    const response: AxiosResponse<T> = await this.client.post(url, data, {
      headers: {
        'Content-Type': 'application/json',
        'Origin': GOURMET_ORIGIN,
        'Referer': this.lastPageUrl || url,
      },
    });
    return response.data;
  }

  /** Reset client (for logout - clears all stored cookies) */
  resetClient(): void {
    this.cookies.clear();
    this.lastPageUrl = '';
  }
}
