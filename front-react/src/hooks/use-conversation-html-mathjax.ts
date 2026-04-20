import { useCallback, useEffect } from "react";
import { preloadMathJax } from "./conversation-html-assets";

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
