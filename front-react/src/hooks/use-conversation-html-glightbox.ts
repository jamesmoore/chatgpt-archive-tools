import { useCallback, useEffect, useRef } from "react";

type GlightboxInstance = {
    destroy?: () => void;
};

type GlightboxFactory = (options?: { selector?: string }) => GlightboxInstance;

let glightboxPromise: Promise<GlightboxFactory> | null = null;

function preloadGlightbox(): Promise<GlightboxFactory> {
    glightboxPromise ??= (async () => {
        const [{ default: glightboxFactory }] = await Promise.all([
            import("glightbox"),
            import("glightbox/dist/css/glightbox.css"),
        ]);

        return glightboxFactory as unknown as GlightboxFactory;
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
        const createLightbox = await preloadGlightbox();

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
    }, [conversationId]);

    useEffect(() => () => {
        lightboxRef.current?.destroy?.();
        lightboxRef.current = null;
    }, []);

    return { enhanceImagesWithLightbox };
}
