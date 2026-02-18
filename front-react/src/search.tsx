import { useEffect, useRef, useState } from 'react'
import { Search as SearchIcon, X } from 'lucide-react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import DOMPurify from 'dompurify'
import {
    Accordion,
    AccordionContent,
    AccordionItem,
    AccordionTrigger,
} from './components/ui/accordion'
import {
    InputGroup,
    InputGroupAddon,
    InputGroupButton,
    InputGroupInput,
} from './components/ui/input-group'
import { useSearch } from './hooks/use-search'

const sanitizeSnippet = (snippet: string) =>
    DOMPurify.sanitize(snippet, {
        ALLOWED_TAGS: ['b'],
        ALLOWED_ATTR: [],
    })

export function Search() {
    const navigate = useNavigate()
    const [searchParams, setSearchParams] = useSearchParams()
    const urlQuery = searchParams.get('q') ?? ''
    const [query, setQuery] = useState(urlQuery)
    const [debouncedQuery, setDebouncedQuery] = useState(urlQuery)

    const inputRef = useRef<HTMLInputElement>(null)

    // Auto-focus the search input when the component mounts
    useEffect(() => {
        inputRef.current?.focus()
    }, [])

    // Guard: when we write the URL ourselves, skip the URL→state sync
    const isOurUrlUpdate = useRef(false)

    // URL → state: only rehydrate on external navigation (back/forward)
    useEffect(() => {
        if (isOurUrlUpdate.current) {
            isOurUrlUpdate.current = false
            return
        }
        setQuery(urlQuery)
        setDebouncedQuery(urlQuery)
    }, [urlQuery])

    // Debounce timer
    useEffect(() => {
        const timeoutId = window.setTimeout(() => {
            setDebouncedQuery(query)
        }, 350)

        return () => window.clearTimeout(timeoutId)
    }, [query])

    // State → URL: write debounced value to URL
    useEffect(() => {
        const trimmed = debouncedQuery.trim()
        if (trimmed !== urlQuery) {
            isOurUrlUpdate.current = true
            setSearchParams(
                trimmed.length > 0 ? { q: trimmed } : {},
                { replace: true },
            )
        }
    }, [debouncedQuery, urlQuery, setSearchParams])

    const normalizedQuery = debouncedQuery.trim()
    const { data, isFetching, isError, error } = useSearch(normalizedQuery)
    const results = data ?? []

    const handleClear = () => {
        setQuery('')
        setDebouncedQuery('')
        isOurUrlUpdate.current = true
        setSearchParams({}, { replace: true })
        inputRef.current?.focus()
    }

    const resultCountLabel = isFetching
        ? 'Searching...'
        : `${results.length} ${results.length === 1 ? 'result' : 'results'}`

    return (
        <div className='overflow-x-auto [scrollbar-gutter:stable]'>
            <div className="container mx-auto flex max-w-xl flex-col gap-4 py-8 px-4 lg:px-0">
                <InputGroup>
                    <InputGroupInput
                        ref={inputRef}
                        placeholder="Search..."
                        value={query}
                        onChange={(event) => setQuery(event.target.value)}
                    />
                    <InputGroupAddon>
                        <SearchIcon />
                    </InputGroupAddon>
                    <InputGroupAddon align="inline-end">
                        {query.length > 0 ? (
                            <InputGroupButton
                                size="icon-xs"
                                variant="ghost"
                                aria-label="Clear search"
                                onClick={handleClear}
                            >
                                <X />
                            </InputGroupButton>
                        ) : null}
                        {resultCountLabel}
                    </InputGroupAddon>
                </InputGroup>

                {normalizedQuery.length === 0 ? (
                    <div className="text-muted-foreground text-sm">
                        Start typing to search chats.
                    </div>
                ) : isError ? (
                    <div className="text-sm text-destructive">
                        Search failed: {error.message}
                    </div>
                ) : results.length === 0 && !isFetching ? (
                    <div className="text-muted-foreground text-sm">No results found.</div>
                ) : (
                    <div className="flex flex-col divide-y rounded-md border">
                        {results.map((result) => {
                            const visibleMessages = result.messages.slice(0, 3)
                            const extraMessages = result.messages.slice(3)
                            const handleNavigate = (messageId: string) =>
                                navigate(
                                    messageId ? 
                                    `/conversation/${encodeURIComponent(result.conversationId)}/html#msg-${encodeURIComponent(messageId)}` :
                                    `/conversation/${encodeURIComponent(result.conversationId)}/html`
                                )

                            return (
                                <div
                                    key={result.conversationId}
                                    className="flex w-full flex-col"
                                >
                                    <button
                                        type="button"
                                        onClick={() => handleNavigate("")}
                                        className="hover:bg-muted/50 flex w-full flex-col gap-1 p-3 text-left"
                                    >
                                        <div className="font-medium">
                                            {result.conversationTitle}
                                        </div>
                                    </button>
                                    <div className="flex w-full flex-col">
                                        {visibleMessages.map((message) => (
                                            <button
                                                key={message.messageId}
                                                type="button"
                                                onClick={() => handleNavigate(message.messageId)}
                                                className="hover:bg-muted/50 w-full px-3 py-2 text-left text-muted-foreground text-sm"
                                            >
                                                <span
                                                    className="block"
                                                    dangerouslySetInnerHTML={{
                                                        __html: sanitizeSnippet(message.snippet),
                                                    }}
                                                />
                                            </button>
                                        ))}
                                        {extraMessages.length > 0 ? (
                                            <Accordion
                                                type="single"
                                                collapsible
                                                className="w-full"
                                            >
                                                <AccordionItem value="more">
                                                    <AccordionTrigger className="group px-3 py-2 text-xs text-muted-foreground hover:no-underline">
                                                        <span className="group-data-[state=open]:hidden">
                                                            Show {extraMessages.length} more
                                                        </span>
                                                        <span className="hidden group-data-[state=open]:inline">
                                                            Show less
                                                        </span>
                                                    </AccordionTrigger>
                                                    <AccordionContent className="pt-0 pb-2">
                                                        <div className="flex w-full flex-col">
                                                            {extraMessages.map((message) => (
                                                                <button
                                                                    key={message.messageId}
                                                                    type="button"
                                                                    onClick={() => handleNavigate(message.messageId)}
                                                                    className="hover:bg-muted/50 w-full px-3 py-2 text-left text-muted-foreground text-sm"
                                                                >
                                                                    <span
                                                                        className="block"
                                                                        dangerouslySetInnerHTML={{
                                                                            __html: sanitizeSnippet(message.snippet),
                                                                        }}
                                                                    />
                                                                </button>
                                                            ))}
                                                        </div>
                                                    </AccordionContent>
                                                </AccordionItem>
                                            </Accordion>
                                        ) : null}
                                    </div>
                                </div>
                            )
                        })}
                    </div>
                )}
            </div>
        </div>
    )
}