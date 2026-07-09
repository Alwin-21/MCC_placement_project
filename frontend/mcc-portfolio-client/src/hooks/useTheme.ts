/**
 * useTheme — global theme hook for MCC Portfolio Platform
 *
 * Stores the theme in localStorage under the single key "mcc-theme" so that
 * changing the mode on any page (home, dashboard, admin) reflects everywhere,
 * including across open browser tabs via the browser's `storage` event.
 */

import { useEffect, useState } from "react";

const THEME_KEY = "mcc-theme";

export type ThemeMode = "dark" | "light";

export function useTheme(): [ThemeMode, () => void] {
  const [themeMode, setThemeMode] = useState<ThemeMode>("dark");

  // Read initial value from localStorage (runs only client-side)
  useEffect(() => {
    if (typeof window === "undefined") return;
    const saved = localStorage.getItem(THEME_KEY) as ThemeMode | null;
    if (saved === "light" || saved === "dark") {
      setThemeMode(saved);
    }
  }, []);

  // Listen for changes made in OTHER tabs/windows
  useEffect(() => {
    if (typeof window === "undefined") return;

    const handleStorage = (e: StorageEvent) => {
      if (e.key === THEME_KEY && (e.newValue === "light" || e.newValue === "dark")) {
        setThemeMode(e.newValue as ThemeMode);
      }
    };

    window.addEventListener("storage", handleStorage);
    return () => window.removeEventListener("storage", handleStorage);
  }, []);

  const toggleTheme = () => {
    setThemeMode((prev) => {
      const next: ThemeMode = prev === "dark" ? "light" : "dark";
      if (typeof window !== "undefined") {
        localStorage.setItem(THEME_KEY, next);
      }
      return next;
    });
  };

  return [themeMode, toggleTheme];
}
