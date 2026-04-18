import { useEffect } from "react";
import githubStyles from "highlight.js/styles/github.css?inline";
import githubDarkStyles from "highlight.js/styles/github-dark.css?inline";
import { useTheme } from "../components/theme-provider";

type ResolvedTheme = "dark" | "light";

function resolveTheme(theme: "dark" | "light" | "system", mediaQuery: MediaQueryList): ResolvedTheme {
    if (theme === "system") {
        return mediaQuery.matches ? "dark" : "light";
    }

    return theme;
}

export function useHighlightThemeStyles() {
    const { theme } = useTheme();

    useEffect(() => {
        const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
        const styleId = "conversation-panel-hljs-theme";
        let style = document.getElementById(styleId) as HTMLStyleElement | null;

        if (!style) {
            style = document.createElement("style");
            style.id = styleId;
            document.head.appendChild(style);
        }

        const applyTheme = (resolvedTheme: ResolvedTheme) => {
            style.textContent = resolvedTheme === "dark" ? githubDarkStyles : githubStyles;
        };

        applyTheme(resolveTheme(theme, mediaQuery));

        if (theme !== "system") {
            return;
        }

        const handleChange = (event: MediaQueryListEvent) => {
            applyTheme(event.matches ? "dark" : "light");
        };

        mediaQuery.addEventListener("change", handleChange);
        return () => mediaQuery.removeEventListener("change", handleChange);
    }, [theme]);
}
