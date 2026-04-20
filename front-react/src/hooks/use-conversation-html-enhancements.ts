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
    }, [
        content,
        enhanceImagesWithLightbox,
        format,
        highlightCodeBlocks,
        scrollToMessage,
        typesetMath,
    ]);

    return contentRef;
}
