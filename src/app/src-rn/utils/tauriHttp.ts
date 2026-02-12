/** No-op on native (iOS/Android). The .web.ts version handles desktop. */
export function installTauriHttpProxy(): void {}
export async function resetTauriHttp(): Promise<void> {}
