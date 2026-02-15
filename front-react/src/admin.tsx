import { Fragment, useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";
import { Button } from "./components/ui/button";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "./components/ui/card";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "./components/ui/dialog";
import { Spinner } from "./components/ui/spinner";
import { Toaster } from "./components/ui/sonner";
import { deleteConversations, loadConversations } from "./api-client";
import { useConversations } from "./hooks/use-conversations";
import { useStatus } from "./hooks/use-status";
import {
    Accordion,
    AccordionContent,
    AccordionItem,
    AccordionTrigger,
} from "./components/ui/accordion";

export function Admin() {
    const { data: conversations = [], refetch } = useConversations();
    const { data: status, isLoading: isStatusLoading, isError: isStatusError, error: statusError } = useStatus();

    const [isClearDialogOpen, setIsClearDialogOpen] = useState(false);

    const loadMutation = useMutation({
        mutationFn: loadConversations,
    });
    const clearMutation = useMutation({
        mutationFn: deleteConversations,
    });

    const handleLoadConversations = async () => {
        try {
            await loadMutation.mutateAsync();
            await refetch();
            toast.success("Chats loaded successfully.");
        } catch (error) {
            toast.error(
                error instanceof Error ? error.message : "Failed to load chats.",
            );
        }
    };

    const handleClearConversations = async () => {
        try {
            await clearMutation.mutateAsync();
            await refetch();
            toast.success("Chat cache cleared successfully.");
            setIsClearDialogOpen(false);
        } catch (error) {
            toast.error(
                error instanceof Error ? error.message : "Failed to clear chat cache.",
            );
        }
    };

    return (
        <div className="p-4 space-y-4">
            <Card>
                <CardHeader>
                    <CardTitle>Chat cache management</CardTitle>
                    <CardDescription>
                        Load chats from the source, or clear all cached chats.
                    </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                    <div className="flex items-center justify-between gap-4">
                        <span className="font-medium">Chats: {conversations.length}</span>
                    </div>
                    <div className="flex items-center justify-between gap-4">
                        <p className="text-sm text-muted-foreground">
                            Load and sync chats into the cache.
                        </p>
                        <Button
                            onClick={handleLoadConversations}
                            disabled={loadMutation.isPending || clearMutation.isPending}
                        >
                            {loadMutation.isPending ? (
                                <>
                                    <Spinner className="mr-2" />
                                    Loading...
                                </>
                            ) : (
                                "Load"
                            )}
                        </Button>
                    </div>
                    <div className="flex items-center justify-between gap-4">
                        <p className="text-sm text-muted-foreground">
                            Remove all  chats from the cache.
                        </p>
                        <Dialog open={isClearDialogOpen} onOpenChange={setIsClearDialogOpen}>
                            <DialogTrigger asChild>
                                <Button
                                    variant="destructive"
                                    disabled={
                                        loadMutation.isPending ||
                                        clearMutation.isPending ||
                                        conversations.length === 0
                                    }
                                >
                                    Clear
                                </Button>
                            </DialogTrigger>
                            <DialogContent>
                                <DialogHeader>
                                    <DialogTitle>Clear all cached chats?</DialogTitle>
                                    <DialogDescription>
                                        All cached chats will be cleared. Your original source chats will not be affected, and you can load them back into the cache at any time. Are you sure you want to proceed?
                                    </DialogDescription>
                                </DialogHeader>
                                <DialogFooter>
                                    <Button
                                        variant="outline"
                                        onClick={() => setIsClearDialogOpen(false)}
                                        disabled={clearMutation.isPending}
                                    >
                                        Cancel
                                    </Button>
                                    <Button
                                        variant="destructive"
                                        onClick={handleClearConversations}
                                        disabled={clearMutation.isPending}
                                    >
                                        {clearMutation.isPending ? (
                                            <>
                                                <Spinner className="mr-2" />
                                                Clearing...
                                            </>
                                        ) : (
                                            "Clear"
                                        )}
                                    </Button>
                                </DialogFooter>
                            </DialogContent>
                        </Dialog>
                    </div>
                </CardContent>
            </Card>
            <Toaster />

            <Card>
                <CardHeader>
                    <CardTitle>Source directories</CardTitle>
                    <CardDescription>
                        Source directories where chats are loaded from.
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    {isStatusLoading ? (
                        <div className="flex items-center text-sm text-muted-foreground">
                            <Spinner className="mr-2" />
                            Loading source status...
                        </div>
                    ) : isStatusError ? (
                        <p className="text-sm text-destructive">
                            Failed to load status: {statusError.message}
                        </p>
                    ) : !status || status.sourceDirectories.length === 0 ? (
                        <p className="text-sm text-muted-foreground">
                            No source directories configured.
                        </p>
                    ) : (
                        <div className="space-y-4">
                            <Accordion type="multiple" className="w-full">
                                {status.sourceDirectories.map((sourceDirectory) => (
                                    <Fragment key={sourceDirectory.directoryName}>
                                        <span className="font-medium">{sourceDirectory.directoryName}</span>
                                        <AccordionItem value={sourceDirectory.directoryName}>
                                            <AccordionTrigger className="text-sm">
                                                <div className="flex flex-col items-start">
                                                    <span className="text-xs text-muted-foreground">
                                                        {sourceDirectory.conversations.length}{" "}
                                                        {sourceDirectory.conversations.length === 1 ? "conversation file" : "conversation files"}
                                                    </span>
                                                </div>
                                            </AccordionTrigger>
                                            <AccordionContent>
                                                {sourceDirectory.conversations.length === 0 ? (
                                                    <p className="text-sm text-muted-foreground">
                                                        No conversations found in this source directory.
                                                    </p>
                                                ) : (
                                                    <ul className="space-y-1 overflow-auto">
                                                        {sourceDirectory.conversations.map((conversation) => (
                                                            <li
                                                                key={conversation}
                                                                className="text-sm text-muted-foreground break-all"
                                                            >
                                                                <pre>
                                                                    {conversation}
                                                                </pre>
                                                            </li>
                                                        ))}
                                                    </ul>
                                                )}
                                            </AccordionContent>
                                        </AccordionItem>
                                    </Fragment>
                                ))}
                            </Accordion>
                        </div>
                    )}
                </CardContent>
            </Card>

            <Card>
                <CardHeader>
                    <CardTitle>Data directory</CardTitle>
                    <CardDescription>
                        Directory where chat data is stored.
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    {isStatusLoading ? (
                        <div className="flex items-center text-sm text-muted-foreground">
                            <Spinner className="mr-2" />
                            Loading data directory status...
                        </div>
                    ) : isStatusError ? (
                        <p className="text-sm text-destructive">
                            Failed to load status: {statusError.message}
                        </p>
                    ) : !status ? (
                        <p className="text-sm text-muted-foreground">
                            No data directory configured.
                        </p>
                    ) : (
                        <div className="space-y-4">
                            <p className="text-sm text-muted-foreground">
                                Data directory: {status.dataDirectory}
                            </p>
                        </div>
                    )}
                </CardContent>
            </Card>
        </div>
    );
}
