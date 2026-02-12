import { create } from 'zustand';

// No-op on native platforms

interface UpdateState {
  pendingVersion: string | null;
  checking: boolean;
}

export const useUpdateStore = create<UpdateState>(() => ({
  pendingVersion: null,
  checking: false,
}));

export async function applyUpdate(): Promise<void> {}

export async function checkForDesktopUpdates(_userInitiated = false): Promise<void> {}
