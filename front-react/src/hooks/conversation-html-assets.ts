export type GlightboxInstance = {
    destroy?: () => void;
};

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
        GLightbox?: (options?: { selector?: string }) => GlightboxInstance;
    }
}

let mathJaxPromise: Promise<void> | null = null;
let glightboxPromise: Promise<void> | null = null;

function loadStylesheet(id: string, href: string) {
    if (document.getElementById(id)) {
        return;
    }

    const link = document.createElement("link");
    link.id = id;
    link.rel = "stylesheet";
    link.href = href;
    document.head.appendChild(link);
}

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

export function preloadMathJax(): Promise<void> {
    mathJaxPromise ??= (async () => {
        window.MathJax ??= {
            tex: {
                inlineMath: [["$", "$"] , ["\\(", "\\)"]],
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

export function preloadGlightbox(): Promise<void> {
    glightboxPromise ??= (async () => {
        loadStylesheet(
            "conversation-panel-glightbox-style",
            "https://cdn.jsdelivr.net/npm/glightbox/dist/css/glightbox.min.css"
        );

        await loadScript(
            "conversation-panel-glightbox-script",
            "https://cdn.jsdelivr.net/npm/glightbox/dist/js/glightbox.min.js"
        );
    })();

    return glightboxPromise;
}
