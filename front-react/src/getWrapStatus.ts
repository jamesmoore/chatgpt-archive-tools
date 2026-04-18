export const WRAP_STORAGE_KEY = 'conversation-wrap-enabled';
export const WRAP_STATUS_EVENT = 'conversation-wrap-change';

export function getWrapStatus(): boolean {
  return localStorage.getItem(WRAP_STORAGE_KEY) === 'true';
}

export function setWrapStatus(newValue: boolean) {
  localStorage.setItem(WRAP_STORAGE_KEY, String(newValue));

  if (typeof window !== 'undefined') {
    window.dispatchEvent(new CustomEvent<boolean>(WRAP_STATUS_EVENT, { detail: newValue }));
  }
}
