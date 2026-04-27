const tokenStorageKey = "token";

export const getStoredToken = () => {
  const persistedToken = window.localStorage.getItem(tokenStorageKey);

  if (persistedToken) {
    return persistedToken;
  }

  const sessionToken = window.sessionStorage.getItem(tokenStorageKey);

  if (sessionToken) {
    window.localStorage.setItem(tokenStorageKey, sessionToken);
    window.sessionStorage.removeItem(tokenStorageKey);
  }

  return sessionToken;
};

export const setStoredToken = (token: string) => {
  window.localStorage.setItem(tokenStorageKey, token);
  window.sessionStorage.removeItem(tokenStorageKey);
};

export const clearStoredToken = () => {
  window.localStorage.removeItem(tokenStorageKey);
  window.sessionStorage.removeItem(tokenStorageKey);
};

export { tokenStorageKey };
