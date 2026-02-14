import { useState } from "react";
import { useMutation } from "@tanstack/react-query";
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
import { deleteConversations, loadConversations } from "./api-client";
import { useConversations } from "./hooks/use-conversations";

export function Admin() {
  const { data: conversations = [], refetch } = useConversations();
  const [statusMessage, setStatusMessage] = useState<string | null>(null);
  const [statusType, setStatusType] = useState<"success" | "error" | null>(null);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);

  const loadMutation = useMutation({
    mutationFn: loadConversations,
  });
  const deleteMutation = useMutation({
    mutationFn: deleteConversations,
  });

  const handleLoadConversations = async () => {
    setStatusMessage(null);
    setStatusType(null);

    try {
      await loadMutation.mutateAsync();
      await refetch();
      setStatusType("success");
      setStatusMessage("Conversations loaded successfully.");
    } catch (error) {
      setStatusType("error");
      setStatusMessage(
        error instanceof Error ? error.message : "Failed to load conversations.",
      );
    }
  };

  const handleDeleteConversations = async () => {
    setStatusMessage(null);
    setStatusType(null);

    try {
      await deleteMutation.mutateAsync();
      await refetch();
      setStatusType("success");
      setStatusMessage("Conversations deleted successfully.");
      setIsDeleteDialogOpen(false);
    } catch (error) {
      setStatusType("error");
      setStatusMessage(
        error instanceof Error ? error.message : "Failed to delete conversations.",
      );
    }
  };

  return (
    <div className="p-4 space-y-4">
      <p>Conversations: {conversations.length}</p>
      <Card>
        <CardHeader>
          <CardTitle>Conversation cache management</CardTitle>
          <CardDescription>
            Load conversations from the source, or delete all cached conversations.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center justify-between gap-4">
            <p className="text-sm text-muted-foreground">
              Load and sync conversations into the cache.
            </p>
            <Button
              onClick={handleLoadConversations}
              disabled={loadMutation.isPending || deleteMutation.isPending}
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
              Remove all  conversations from the cache.
            </p>
            <Dialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
              <DialogTrigger asChild>
                <Button
                  variant="destructive"
                  disabled={
                    loadMutation.isPending ||
                    deleteMutation.isPending ||
                    conversations.length === 0
                  }
                >
                  Delete
                </Button>
              </DialogTrigger>
              <DialogContent>
                <DialogHeader>
                  <DialogTitle>Delete all cached conversations?</DialogTitle>
                  <DialogDescription>
                    All cached conversations will be deleted. Your original source conversations will not be affected, and you can load them back into the cache at any time. Are you sure you want to proceed?
                  </DialogDescription>
                </DialogHeader>
                <DialogFooter>
                  <Button
                    variant="outline"
                    onClick={() => setIsDeleteDialogOpen(false)}
                    disabled={deleteMutation.isPending}
                  >
                    Cancel
                  </Button>
                  <Button
                    variant="destructive"
                    onClick={handleDeleteConversations}
                    disabled={deleteMutation.isPending}
                  >
                    {deleteMutation.isPending ? (
                      <>
                        <Spinner className="mr-2" />
                        Deleting...
                      </>
                    ) : (
                      "Delete"
                    )}
                  </Button>
                </DialogFooter>
              </DialogContent>
            </Dialog>
          </div>
        </CardContent>
      </Card>
      {statusMessage ? (
        <p className={statusType === "error" ? "text-destructive" : "text-green-600"}>
          {statusMessage}
        </p>
      ) : null}
    </div>
  );
}
