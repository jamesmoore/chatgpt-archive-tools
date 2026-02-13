import { useEffect, useRef, useState } from 'react'
import { Search as SearchIcon, X } from 'lucide-react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import {
    InputGroup,
    InputGroupAddon,
    InputGroupButton,
    InputGroupInput,
} from './components/ui/input-group'
import { useSearch } from './hooks/use-search'

export function Search() {
    const navigate = useNavigate()
    const [searchParams, setSearchParams] = useSearchParams()
    const urlQuery = searchParams.get('q') ?? ''
    const [query, setQuery] = useState(urlQuery)
    const [debouncedQuery, setDebouncedQuery] = useState(urlQuery)

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
    }

    const resultCountLabel = isFetching
        ? 'Searching...'
        : `${results.length} ${results.length === 1 ? 'result' : 'results'}`

    return (
        <div className='overflow-x-auto [scrollbar-gutter:stable]'>
            <div className="container mx-auto flex max-w-xl flex-col gap-4 py-8 px-4 lg:px-0">
                <InputGroup>
                    <InputGroupInput
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
                        Start typing to search conversations.
                    </div>
                ) : isError ? (
                    <div className="text-sm text-destructive">
                        Search failed: {error.message}
                    </div>
                ) : results.length === 0 && !isFetching ? (
                    <div className="text-muted-foreground text-sm">No results found.</div>
                ) : (
                    <div className="flex flex-col divide-y rounded-md border">
                        {results.map((result) => (
                            <button
                                key={`${result.conversationId}}`}
                                type="button"
                                onClick={() =>
                                    navigate(
                                        `/conversation/${encodeURIComponent(result.conversationId)}/html`,
                                    )
                                }
                                className="hover:bg-muted/50 flex w-full flex-col gap-1 p-3 text-left"
                            >
                                <div className="font-medium">{result.conversationTitle}</div>
                                {
                                    result.messages.map((message) => (
                                        <div
                                            key={message.messageId}
                                            className="text-muted-foreground text-sm py-1"
                                            dangerouslySetInnerHTML={{ __html: message.snippet }}
                                        />
                                    ))
                                }
                            </button>
                        ))}
                    </div>
                )}
            </div>
        </div>
    )
}