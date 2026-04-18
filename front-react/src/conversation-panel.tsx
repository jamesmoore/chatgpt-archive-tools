import { useParams } from "react-router-dom";
import hljs from "highlight.js";
import { useConversation } from "./hooks/use-conversation";
import { useConversationHtmlEnhancements } from "./hooks/use-conversation-html-enhancements";
import { useHighlightThemeStyles } from "./hooks/use-highlight-theme-styles";
import { useWrapPreference } from "./hooks/use-wrap-preference";
import LoadingSpinner from "./loading-spinner";

export function ConversationPanel() {

    const { id, format } = useParams();
    const { data: content, error, isLoading } = useConversation(id, format);
    const { isWrapped } = useWrapPreference();

    useHighlightThemeStyles();

    const contentRef = useConversationHtmlEnhancements({
        content,
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
                dangerouslySetInnerHTML={{ __html: content }}
            />
        );
    }

    if (format === 'markdown' || format === 'json') {
        const value = content || "";
        const highlighted = hljs.highlight(value, { language: format, ignoreIllegals: true }).value;

        return (
            <div className='overflow-x-auto'>
                <pre className={isWrapped ? 'whitespace-pre-wrap wrap-break-word' : 'min-w-max whitespace-pre'}>
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
