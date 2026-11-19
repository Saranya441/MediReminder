import axios from "axios";

const API_ROOT = import.meta.env.VITE_API_BASE_URL || "/api";
const TOKEN_KEY = "medireminder_token";
const USER_KEY = "medireminder_user";

export const apiClient = axios.create({ baseURL: API_ROOT, timeout: 20000 });

export function getToken() {
  return sessionStorage.getItem(TOKEN_KEY) || localStorage.getItem(TOKEN_KEY);
}

export function getStoredUser() {
  const raw = sessionStorage.getItem(USER_KEY) || localStorage.getItem(USER_KEY);
  if (!raw) return null;
  try { return JSON.parse(raw); } catch { return null; }
}

export function saveSession(token, user, rememberMe = false) {
  clearSession();
  const storage = rememberMe ? localStorage : sessionStorage;
  storage.setItem(TOKEN_KEY, token);
  storage.setItem(USER_KEY, JSON.stringify(user));
  window.dispatchEvent(new Event("medireminder:session"));
}

export function updateStoredUser(user) {
  const storage = sessionStorage.getItem(TOKEN_KEY) ? sessionStorage : localStorage;
  storage.setItem(USER_KEY, JSON.stringify(user));
  window.dispatchEvent(new Event("medireminder:session"));
}

export function clearSession() {
  sessionStorage.removeItem(TOKEN_KEY);
  sessionStorage.removeItem(USER_KEY);
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
  window.dispatchEvent(new Event("medireminder:session"));
}

apiClient.interceptors.request.use((config) => {
  const token = getToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401 && getToken()) {
      clearSession();
      window.dispatchEvent(new Event("medireminder:session-expired"));
    }
    return Promise.reject(error);
  },
);

export function errorMessage(error, fallback = "Something went wrong. Please try again.") {
  const data = error?.response?.data;
  if (typeof data?.message === "string") return data.message;
  if (typeof data?.Message === "string") return data.Message;
  if (Array.isArray(data?.errors)) return data.errors.join(" ");
  if (data?.errors && typeof data.errors === "object") {
    return Object.values(data.errors).flat().join(" ") || fallback;
  }
  return error?.message === "Network Error"
    ? "Cannot reach the MediReminder API. Check that the backend is running."
    : fallback;
}
