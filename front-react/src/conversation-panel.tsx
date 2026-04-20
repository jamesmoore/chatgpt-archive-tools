import { useCallback, useMemo } from "react";
import type { MouseEvent } from "react";
import { useParams } from "react-router-dom";
import hljs from "highlight.js";
import DOMPurify from "dompurify";
import { useConversation } from "./hooks/use-conversation";
import { useConversationHtmlEnhancements } from "./hooks/use-conversation-html-enhancements";
import { useHighlightThemeStyles } from "./hooks/use-highlight-theme-styles";
import { useWrapPreference } from "./hooks/use-wrap-preference";
import LoadingSpinner from "./loading-spinner";

const SAFE_LINK_PROTOCOLS = new Set(["http:", "https:", "mailto:", "tel:"]);

function isSafeHref(href: string): boolean {
    if (href.startsWith("#") || href.startsWith("/") || href.startsWith("./") || href.startsWith("../")) {
        return true;
    }

    try {
        const url = new URL(href, window.location.origin);
        return SAFE_LINK_PROTOCOLS.has(url.protocol);
    } catch {
        return false;
    }
}

export function ConversationPanel() {

    const { id, format } = useParams();
    const { data: content, error, isLoading } = useConversation(id, format);
    const { isWrapped } = useWrapPreference();
    const sanitizedContent = useMemo(
        () =>
            format === "html" && content
                ? DOMPurify.sanitize(content)
                : "",
        [content, format]
    );
    const handleConversationLinkClick = useCallback((event: MouseEvent<HTMLDivElement>) => {
        const target = event.target as HTMLElement | null;
        const link = target?.closest<HTMLAnchorElement>("a[href]");
        const href = link?.getAttribute("href");

        if (!href) {
            return;
        }

        event.preventDefault();
        event.stopPropagation();

        if (!isSafeHref(href)) {
            return;
        }

        window.open(new URL(href, window.location.origin).toString(), "_blank", "noopener,noreferrer");
    }, []);

    useHighlightThemeStyles();

    const contentRef = useConversationHtmlEnhancements({
        content: sanitizedContent,
        conversationId: id,
        format,
    });

    if (!id) {
        return <div className="flex-1 flex justify-center items-center text-red-600">
            No chat ID provided.
        </div>;
    }

    if (error) {
        return <div className="flex-1 flex justify-center items-center text-red-600">
            {error instanceof Error ? error.message : "Failed to load chat."}
        </div>;
    }

    if (isLoading) {
        return <LoadingSpinner />;
    }

    if (format === "html" && content) {
        return (
            <div
                ref={contentRef}
                className="conversation-html flex-1 w-full overflow-y-auto px-4 pb-6"
                onClick={handleConversationLinkClick}
                dangerouslySetInnerHTML={{ __html: sanitizedContent }}
            />
        );
    }

    if (format === 'markdown' || format === 'json') {
        const value = content || "";
        const highlighted = hljs.highlight(value, { language: format, ignoreIllegals: true }).value;

        return (
            <div className='overflow-x-auto'>
                <pre className={isWrapped ? 'whitespace-pre-wrap break-words' : 'min-w-max whitespace-pre'}>
                    <code
                        className={`bg-transparent! hljs language-${format}`}
                        dangerouslySetInnerHTML={{ __html: highlighted }}
                    />
                </pre>
            </div>
        );
    }

    return (<>{content || ""}</>);
}
