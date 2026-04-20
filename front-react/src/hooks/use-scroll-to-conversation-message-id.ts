import { useCallback, useEffect } from "react";

type UseScrollToConversationMessageIdOptions = {
    contentRef: React.RefObject<HTMLDivElement | null>;
};

export function useScrollToConversationMessageId({
    contentRef,
}: UseScrollToConversationMessageIdOptions) {
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
    }, [contentRef]);

    useEffect(() => {
        window.addEventListener("hashchange", scrollToMessage);
        return () => window.removeEventListener("hashchange", scrollToMessage);
    }, [scrollToMessage]);

    return { scrollToMessage };
}
