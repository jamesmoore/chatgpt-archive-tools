import { useCallback, useEffect, useRef } from "react";
import hljs from "highlight.js";

type GlightboxInstance = {
    destroy?: () => void;
};

type GlightboxFactory = (options?: Record<string, unknown>) => GlightboxInstance;

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
            typesetClear?: (elements?: HTMLElement[]) => void;
            typesetPromise?: (elements?: HTMLElement[]) => Promise<unknown>;
        };
    }
}

let mathJaxPromise: Promise<void> | null = null;
let glightboxPromise: Promise<GlightboxFactory> | null = null;

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

        await import("mathjax/tex-mml-chtml.js");

        await window.MathJax?.startup?.promise;
    })();

    return mathJaxPromise;
}

async function preloadGlightbox(): Promise<GlightboxFactory> {
    glightboxPromise ??= (async () => {
        await import("glightbox/dist/css/glightbox.min.css");
        const { default: createLightbox } = await import("glightbox");
        return createLightbox as GlightboxFactory;
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
                    window.MathJax.typesetClear?.([container]);
                    await window.MathJax.typesetPromise([container]);
                }

                const createLightbox = await preloadGlightbox();
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
                lightboxRef.current = createLightbox({
                    selector: ".conversation-html .glightbox",
                });

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
