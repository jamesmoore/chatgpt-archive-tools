import { useCallback } from "react";
import hljs from "highlight.js";

export function useConversationHtmlHighlight() {
    const highlightCodeBlocks = useCallback((container: HTMLElement) => {
        container.querySelectorAll("pre code").forEach((block) => {
            hljs.highlightElement(block as HTMLElement);
        });
    }, []);

    return { highlightCodeBlocks };
}
