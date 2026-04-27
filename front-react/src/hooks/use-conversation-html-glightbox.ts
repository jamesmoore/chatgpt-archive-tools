import type GLightbox from "glightbox";
import { useCallback, useEffect, useRef } from "react";

let glightboxPromise: Promise<typeof GLightbox> | null = null;

function preloadGlightbox(): Promise<typeof GLightbox> {
    glightboxPromise ??= (async () => {
        try {
            console.debug("Preloading GLightbox instance.");
            const [{ default: glightboxFactory }] = await Promise.all([
                import("glightbox"),
                import("glightbox/dist/css/glightbox.css"),
            ]);

            return glightboxFactory;
        } catch (preloadError) {
            glightboxPromise = null;
            throw preloadError;
        }
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
    const lightboxRef = useRef<ReturnType<typeof GLightbox> | null>(null);

    useEffect(() => {
        if (format !== "html") {
            return;
        }

        void preloadGlightbox().catch((preloadError) => {
            console.error("Failed to preload GLightbox.", preloadError);
        });
    }, [format]);

    const enhanceImagesWithLightbox = useCallback(async (container: HTMLElement) => {
        const createLightbox = await preloadGlightbox();

        console.debug("Using GLightbox instance.");
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

        lightboxRef.current?.destroy();
        lightboxRef.current = createLightbox({
            selector: ".conversation-html .glightbox",
        });
    }, [conversationId]);

    useEffect(() => () => {
        console.debug("Cleaning up GLightbox instance.");
        lightboxRef.current?.destroy();
        lightboxRef.current = null;
    }, []);

    return { enhanceImagesWithLightbox };
}
