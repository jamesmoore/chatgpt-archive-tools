import { useCallback } from "react";
import hljs from "highlight.js";

// Re-running the enhancement pass is expected, so mark blocks we have already
// highlighted and skip them on later renders.
const HIGHLIGHTED_ATTRIBUTE = "data-chatgpt-archive-highlighted";

export function useConversationHtmlHighlight() {
    const highlightCodeBlocks = useCallback((container: HTMLElement) => {
        container.querySelectorAll(`pre code:not([${HIGHLIGHTED_ATTRIBUTE}])`).forEach((block) => {
            hljs.highlightElement(block as HTMLElement);
            block.setAttribute(HIGHLIGHTED_ATTRIBUTE, "true");
        });
    }, []);

    return { highlightCodeBlocks };
}
