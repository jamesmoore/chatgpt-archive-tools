import { useParams } from "react-router-dom";
import { useState, useEffect, useRef } from "react";
import hljs from "highlight.js";
import githubStyles from "highlight.js/styles/github.css?inline";
import githubDarkStyles from "highlight.js/styles/github-dark.css?inline";
import { useConversation } from "./hooks/use-conversation";
import { getWrapStatus } from "./getWrapStatus";
import { useTheme } from "./components/theme-provider";
import LoadingSpinner from "./loading-spinner";

export function ConversationPanel() {

    const { id, format } = useParams();
    const { data: content, error, isLoading } = useConversation(id, format);
    const { theme } = useTheme();
    const [isWrapped, setIsWrapped] = useState(() => getWrapStatus());

    const iframeRef = useRef<HTMLIFrameElement>(null);

    useEffect(() => {
        const handleStorageChange = () => {
            setIsWrapped(getWrapStatus());
        };
        window.addEventListener('storage', handleStorageChange);
        return () => window.removeEventListener('storage', handleStorageChange);
    }, []);

    useEffect(() => {
        const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
        const resolvedTheme = theme === "system" ? (mediaQuery.matches ? "dark" : "light") : theme;
        const styleId = "conversation-panel-hljs-theme";
        let style = document.getElementById(styleId) as HTMLStyleElement | null;

        if (!style) {
            style = document.createElement("style");
            style.id = styleId;
            document.head.appendChild(style);
        }

        style.textContent = resolvedTheme === "dark" ? githubDarkStyles : githubStyles;

        if (theme !== "system") return;

        const handleChange = (event: MediaQueryListEvent) => {
            if (!style) return;
            style.textContent = event.matches ? githubDarkStyles : githubStyles;
        };

        mediaQuery.addEventListener("change", handleChange);
        return () => mediaQuery.removeEventListener("change", handleChange);
    }, [theme]);

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

    function scrollToMessage() {
        const targetMessageId = window.location.hash
            .replace(/^#/, "")
            .replace(/^msg-/, "");
        if (!iframeRef.current || !targetMessageId) return;

        const doc = iframeRef.current.contentDocument;
        const element = doc?.getElementById(`msg-${targetMessageId}`);

        element?.scrollIntoView({ block: "start" });
    }

    if (format === "html" && content) {
        return (
            <iframe
                srcDoc={content}
                className="flex-1 w-full border-none"
                title="Chat HTML"
                sandbox="allow-scripts allow-same-origin"
                referrerPolicy="no-referrer"
                ref={iframeRef}
                onLoad={() => scrollToMessage()}
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
