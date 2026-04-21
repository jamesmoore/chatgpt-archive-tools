import { useCallback, useEffect } from "react";

const INLINE_MATH_DELIMITERS = [["$", "$"], ["\\(", "\\)"]] as const;
const DISPLAY_MATH_DELIMITERS = [["$$", "$$"], ["\\[", "\\]"]] as const;
const DISABLED_ACCESSIBILITY_MENU_SETTINGS = {
    speech: false,
    braille: false,
    enrich: false,
    collapsible: false,
    assistiveMml: false,
} as const;

type MathJaxWindow = {
    version?: string;
    config?: MathJaxWindow;
    tex?: {
        inlineMath?: string[][];
        displayMath?: string[][];
    };
    options?: {
        enableSpeech?: boolean;
        enableBraille?: boolean;
        enableEnrichment?: boolean;
        enableComplexity?: boolean;
        enableExplorer?: boolean;
        menuOptions?: {
            settings?: {
                speech?: boolean;
                braille?: boolean;
                enrich?: boolean;
                collapsible?: boolean;
                assistiveMml?: boolean;
            };
        };
    };
    startup?: {
        promise?: Promise<unknown>;
        typeset?: boolean;
    };
    typesetPromise?: (elements?: HTMLElement[]) => Promise<unknown>;
    typesetClear?: (elements?: HTMLElement[]) => void;
};

declare global {
    interface Window {
        MathJax?: MathJaxWindow;
    }
}

let mathJaxPromise: Promise<MathJaxWindow> | null = null;
let mathJaxTypesetQueue = Promise.resolve();

function waitForNextAnimationFrame(): Promise<void> {
    return new Promise<void>((resolve) => {
        requestAnimationFrame(() => resolve());
    });
}

function ensureMathJaxConfiguration(): MathJaxWindow {
    window.MathJax ??= {
        tex: {
            inlineMath: [...INLINE_MATH_DELIMITERS.map((pair) => [...pair])],
            displayMath: [...DISPLAY_MATH_DELIMITERS.map((pair) => [...pair])],
        },
        options: {
            enableSpeech: false,
            enableBraille: false,
            enableEnrichment: false,
            enableComplexity: false,
            enableExplorer: false,
            menuOptions: {
                settings: {
                    ...DISABLED_ACCESSIBILITY_MENU_SETTINGS,
                },
            },
        },
        startup: {
            typeset: false,
        },
    };

    return window.MathJax;
}

async function preloadMathJax(): Promise<MathJaxWindow> {
    mathJaxPromise ??= (async () => {
        ensureMathJaxConfiguration();

        try {
            await import("mathjax/tex-mml-chtml.js");
            const mathJax = window.MathJax;

            if (!mathJax) {
                throw new Error("MathJax runtime was not attached to window.");
            }

            await mathJax.startup?.promise;
            return mathJax;
        } catch (preloadError) {
            mathJaxPromise = null;
            throw preloadError;
        }
    })();

    return mathJaxPromise;
}

function enqueueMathJaxWork(work: () => Promise<void>) {
    const queuedWork = mathJaxTypesetQueue.then(work, work);
    mathJaxTypesetQueue = queuedWork.catch(() => undefined);
    return queuedWork;
}

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

    const typesetMath = useCallback((container: HTMLElement) => {
        return enqueueMathJaxWork(async () => {
            await waitForNextAnimationFrame();

            const mathJax = await preloadMathJax();

            // Conversation content is replaced between views, so clear tracked
            // MathJax items before typesetting the current container again.
            mathJax.typesetClear?.();
            await waitForNextAnimationFrame();
            await mathJax.typesetPromise?.([container]);
        });
    }, []);

    return { typesetMath };
}
