const STORAGE_KEY = "medireminder-appearance";
const DARK_QUERY = "(prefers-color-scheme: dark)";

export function resolveAppearance(appearance) {
  if (appearance === "dark" || appearance === "light") return appearance;
  return window.matchMedia(DARK_QUERY).matches ? "dark" : "light";
}

export function applyAppearance(appearance, { persist = false } = {}) {
  const preference = ["light", "dark", "system"].includes(appearance)
    ? appearance
    : "system";
  if (persist) window.localStorage.setItem(STORAGE_KEY, preference);

  const root = document.documentElement;
  root.dataset.themePreference = preference;
  root.dataset.theme = resolveAppearance(preference);

  if (root._appearanceMediaHandler) {
    window.matchMedia(DARK_QUERY).removeEventListener("change", root._appearanceMediaHandler);
    root._appearanceMediaHandler = null;
  }

  if (preference === "system") {
    const media = window.matchMedia(DARK_QUERY);
    const updateTheme = (event) => {
      root.dataset.theme = event.matches ? "dark" : "light";
    };
    media.addEventListener("change", updateTheme);
    root._appearanceMediaHandler = updateTheme;
  }
}

export function initializeAppearance() {
  const saved = window.localStorage.getItem(STORAGE_KEY) || "system";
  applyAppearance(saved);
}
