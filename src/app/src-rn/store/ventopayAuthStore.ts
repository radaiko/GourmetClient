import { create } from 'zustand';
import * as secureStorage from '../utils/secureStorage';
import { VentopayApi } from '../api/ventopayApi';

const CREDENTIALS_KEY_USER = 'ventopay_username';
const CREDENTIALS_KEY_PASS = 'ventopay_password';

type VentopayAuthStatus = 'idle' | 'loading' | 'authenticated' | 'error' | 'no_credentials';

interface VentopayAuthState {
  status: VentopayAuthStatus;
  error: string | null;
  api: VentopayApi;

  login: (username: string, password: string) => Promise<boolean>;
  loginWithSaved: () => Promise<boolean>;
  logout: () => Promise<void>;
  saveCredentials: (username: string, password: string) => Promise<void>;
  getSavedCredentials: () => Promise<{ username: string; password: string } | null>;
  clearCredentials: () => Promise<void>;
}

export const useVentopayAuthStore = create<VentopayAuthState>((set, get) => ({
  status: 'idle',
  error: null,
  api: new VentopayApi(),

  login: async (username: string, password: string) => {
    set({ status: 'loading', error: null });
    try {
      await get().api.login(username, password);
      set({ status: 'authenticated', error: null });
      return true;
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Ventopay login failed';
      set({ status: 'error', error: message });
      return false;
    }
  },

  loginWithSaved: async () => {
    const creds = await get().getSavedCredentials();
    if (!creds) {
      set({ status: 'no_credentials', error: null });
      return false;
    }
    return get().login(creds.username, creds.password);
  },

  logout: async () => {
    try {
      await get().api.logout();
    } finally {
      set({ status: 'idle', error: null });
    }
  },

  saveCredentials: async (username: string, password: string) => {
    await secureStorage.setItem(CREDENTIALS_KEY_USER, username);
    await secureStorage.setItem(CREDENTIALS_KEY_PASS, password);
  },

  getSavedCredentials: async () => {
    const username = await secureStorage.getItem(CREDENTIALS_KEY_USER);
    const password = await secureStorage.getItem(CREDENTIALS_KEY_PASS);
    if (!username || !password) return null;
    return { username, password };
  },

  clearCredentials: async () => {
    await secureStorage.deleteItem(CREDENTIALS_KEY_USER);
    await secureStorage.deleteItem(CREDENTIALS_KEY_PASS);
  },
}));
