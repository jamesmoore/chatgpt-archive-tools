import { useCallback, useEffect, useRef } from "react";
import hljs from "highlight.js";

type GlightboxInstance = {
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

function preloadGlightbox(): Promise<void> {
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

type UseConversationHtmlEnhancementsOptions = {
    content: string | undefined;
    conversationId: string | undefined;
    format: string | undefined;
};

export function useConversationHtmlEnhancements({
    content,
    conversationId,
    format,
}: UseConversationHtmlEnhancementsOptions) {
    const contentRef = useRef<HTMLDivElement>(null);
    const lightboxRef = useRef<GlightboxInstance | null>(null);

    const scrollToMessage = useCallback(() => {
        let targetMessageId = window.location.hash
            .replace(/^#/, "")
            .replace(/^msg-/, "");

        try {
            if (targetMessageId) {
                targetMessageId = decodeURIComponent(targetMessageId);
            }
        } catch {
            // If decoding fails, fall back to the raw value to avoid breaking existing behavior.
        }

        if (!contentRef.current || !targetMessageId) {
            return;
        }

        const element = contentRef.current.querySelector<HTMLElement>(
            `#${CSS.escape(`msg-${targetMessageId}`)}`
        );

        element?.scrollIntoView({ block: "start" });
    }, []);

    useEffect(() => {
        window.addEventListener("hashchange", scrollToMessage);
        return () => window.removeEventListener("hashchange", scrollToMessage);
    }, [scrollToMessage]);

    useEffect(() => {
        if (format !== "html") {
            return;
        }

        void preloadMathJax().catch((preloadError) => {
            console.error("Failed to preload MathJax.", preloadError);
        });

        void preloadGlightbox().catch((preloadError) => {
            console.error("Failed to preload GLightbox.", preloadError);
        });
    }, [format]);

    useEffect(() => {
        if (format !== "html" || !content || !contentRef.current) {
            return;
        }

        const container = contentRef.current;
        let isDisposed = false;

        const enhanceConversationHtml = async () => {
            try {
                container.querySelectorAll("pre code").forEach((block) => {
                    hljs.highlightElement(block as HTMLElement);
                });

                await preloadMathJax();
                if (!isDisposed && window.MathJax?.typesetPromise) {
                    await window.MathJax.typesetPromise([container]);
                }

                await preloadGlightbox();
                if (isDisposed) {
                    return;
                }

                container.querySelectorAll("img").forEach((image) => {
                    if (image.closest("a.glightbox")) {
                        return;
                    }

                    const src = image.getAttribute("src");
                    if (!src) {
                        return;
                    }

                    const link = document.createElement("a");
                    link.href = src;
                    link.className = "glightbox";
                    link.dataset.gallery = `conversation-${conversationId ?? "default"}`;
                    image.parentNode?.insertBefore(link, image);
                    link.appendChild(image);
                });

                lightboxRef.current?.destroy?.();
                lightboxRef.current = window.GLightbox?.({
                    selector: ".conversation-html .glightbox",
                }) ?? null;

                requestAnimationFrame(() => scrollToMessage());
            } catch (enhancementError) {
                console.error("Failed to enhance conversation HTML.", enhancementError);
            }
        };

        void enhanceConversationHtml();

        return () => {
            isDisposed = true;
            lightboxRef.current?.destroy?.();
            lightboxRef.current = null;
        };
    }, [content, conversationId, format, scrollToMessage]);

    return contentRef;
}
