import { useEffect, useState } from 'react'
import { Search as SearchIcon } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { InputGroup, InputGroupAddon, InputGroupInput } from './components/ui/input-group'
import { useSearch } from './hooks/use-search'

export function Search() {
    const navigate = useNavigate()
    const [query, setQuery] = useState('')
    const [debouncedQuery, setDebouncedQuery] = useState('')

    useEffect(() => {
        const timeoutId = window.setTimeout(() => {
            setDebouncedQuery(query)
        }, 350)

        return () => window.clearTimeout(timeoutId)
    }, [query])

    const normalizedQuery = debouncedQuery.trim()
    const { data, isFetching, isError, error } = useSearch(normalizedQuery)
    const results = data ?? []

    const resultCountLabel = isFetching
        ? 'Searching...'
        : `${results.length} ${results.length === 1 ? 'result' : 'results'}`

    return (
        <>
            <div className="container mx-auto flex max-w-xl flex-col gap-4 py-8">
                <InputGroup>
                    <InputGroupInput
                        placeholder="Search..."
                        value={query}
                        onChange={(event) => setQuery(event.target.value)}
                    />
                    <InputGroupAddon>
                        <SearchIcon />
                    </InputGroupAddon>
                    <InputGroupAddon align="inline-end">{resultCountLabel}</InputGroupAddon>
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
                                key={`${result.conversationId}-${result.messageId}`}
                                type="button"
                                onClick={() =>
                                    navigate(
                                        `/conversation/${encodeURIComponent(result.conversationId)}/html`,
                                    )
                                }
                                className="hover:bg-muted/50 flex w-full flex-col gap-1 p-3 text-left"
                            >
                                <div className="font-medium">{result.conversationTitle}</div>
                                <div
                                    className="text-muted-foreground text-sm"
                                    dangerouslySetInnerHTML={{ __html: result.snippet }}
                                />
                            </button>
                        ))}
                    </div>
                )}
            </div>
        </>
    )
}