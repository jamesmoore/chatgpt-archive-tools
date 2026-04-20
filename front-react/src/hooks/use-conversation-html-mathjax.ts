import { useCallback, useEffect } from "react";

declare global {
    interface Window {
        MathJax?: {
            tex?: {
                inlineMath?: string[][];
                displayMath?: string[][];
            };
            startup?: {
                promise?: Promise<unknown>;
                typeset?: boolean;
            };
            typesetPromise?: (elements?: HTMLElement[]) => Promise<unknown>;
        };
    }
}

let mathJaxPromise: Promise<void> | null = null;

function loadScript(id: string, src: string): Promise<void> {
    return new Promise((resolve, reject) => {
        const existingScript = document.getElementById(id) as HTMLScriptElement | null;

        if (existingScript?.dataset.loaded === "true") {
            resolve();
            return;
        }

        const script = existingScript ?? document.createElement("script");

        const handleLoad = () => {
            script.dataset.loaded = "true";
            resolve();
        };

        const handleError = () => {
            reject(new Error(`Failed to load script: ${src}`));
        };

        script.addEventListener("load", handleLoad, { once: true });
        script.addEventListener("error", handleError, { once: true });

        if (!existingScript) {
            script.id = id;
            script.src = src;
            script.async = true;
            document.head.appendChild(script);
        }
    });
}

function preloadMathJax(): Promise<void> {
    mathJaxPromise ??= (async () => {
        window.MathJax ??= {
            tex: {
                inlineMath: [["$", "$"], ["\\(", "\\)"]],
                displayMath: [["$$", "$$"], ["\\[", "\\]"]],
            },
            startup: {
                typeset: false,
            },
        };

        await loadScript(
            "conversation-panel-mathjax",
            "https://cdn.jsdelivr.net/npm/mathjax@4/tex-mml-chtml.js"
        );

        await window.MathJax?.startup?.promise;
    })();

    return mathJaxPromise;
}

type UseConversationHtmlMathJaxOptions = {
    format: string | undefined;
};

export function useConversationHtmlMathJax({ format }: UseConversationHtmlMathJaxOptions) {
    useEffect(() => {
        if (format !== "html") {
            return;
        }

        void preloadMathJax().catch((preloadError) => {
            console.error("Failed to preload MathJax.", preloadError);
        });
    }, [format]);

    const typesetMath = useCallback(async (container: HTMLElement) => {
        await preloadMathJax();

        if (window.MathJax?.typesetPromise) {
            await window.MathJax.typesetPromise([container]);
        }
    }, []);

    return { typesetMath };
}
