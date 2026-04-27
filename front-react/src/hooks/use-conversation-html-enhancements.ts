import { useEffect, useRef } from "react";
import { useConversationHtmlGlightbox } from "./use-conversation-html-glightbox";
import { useConversationHtmlHighlight } from "./use-conversation-html-highlight";
import { useConversationHtmlMathJax } from "./use-conversation-html-mathjax";
import { useScrollToConversationMessageId } from "./use-scroll-to-conversation-message-id";

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
    const { scrollToMessage } = useScrollToConversationMessageId({ contentRef });
    const { highlightCodeBlocks } = useConversationHtmlHighlight();
    const { typesetMath } = useConversationHtmlMathJax({ format });
    const { enhanceImagesWithLightbox } = useConversationHtmlGlightbox({
        conversationId,
        format,
    });

    useEffect(() => {
        // The panel can re-render when the user re-selects the current
        // conversation even if the HTML string is identical, so enhancements
        // need to follow committed renders instead of only content changes.
        if (format !== "html" || !content || !contentRef.current) {
            return;
        }

        const container = contentRef.current;
        let isDisposed = false;

        const enhanceConversationHtml = async () => {
            try {
                highlightCodeBlocks(container);

                await typesetMath(container);
                if (isDisposed) {
                    return;
                }

                await enhanceImagesWithLightbox(container);
                requestAnimationFrame(() => scrollToMessage());
            } catch (enhancementError) {
                console.error("Failed to enhance conversation HTML.", enhancementError);
            }
        };

        void enhanceConversationHtml();

        return () => {
            isDisposed = true;
        };
    });

    return contentRef;
}
