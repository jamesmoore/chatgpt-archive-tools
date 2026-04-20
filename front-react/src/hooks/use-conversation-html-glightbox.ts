import { useCallback, useEffect, useRef } from "react";

type GlightboxInstance = {
    destroy?: () => void;
};

declare global {
    interface Window {
        GLightbox?: (options?: { selector?: string }) => GlightboxInstance;
    }
}

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

type UseConversationHtmlGlightboxOptions = {
    conversationId: string | undefined;
    format: string | undefined;
};

export function useConversationHtmlGlightbox({
    conversationId,
    format,
}: UseConversationHtmlGlightboxOptions) {
    const lightboxRef = useRef<GlightboxInstance | null>(null);

    useEffect(() => {
        if (format !== "html") {
            return;
        }

        void preloadGlightbox().catch((preloadError) => {
            console.error("Failed to preload GLightbox.", preloadError);
        });
    }, [format]);

    const enhanceImagesWithLightbox = useCallback(async (container: HTMLElement) => {
        await preloadGlightbox();

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
    }, [conversationId]);

    useEffect(() => () => {
        lightboxRef.current?.destroy?.();
        lightboxRef.current = null;
    }, []);

    return { enhanceImagesWithLightbox };
}
