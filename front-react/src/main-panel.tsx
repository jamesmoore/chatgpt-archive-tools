import { useConversations } from "./hooks/use-conversations";
import { Search } from "./search";
import { Link } from "react-router-dom";
import { Button } from "./components/ui/button";
import {
    Empty,
    EmptyContent,
    EmptyDescription,
    EmptyHeader,
    EmptyTitle,
} from "./components/ui/empty";

export function MainPanel() {

    const {
        data: conversations = []
    } = useConversations();


    if (conversations.length === 0) {
        return (
            <Empty>
                <EmptyHeader>
                    <EmptyTitle>No chats found</EmptyTitle>
                    <EmptyDescription>Please go to the admin page to load your chats.</EmptyDescription>
                </EmptyHeader>
                <EmptyContent>
                    <Button asChild>
                        <Link to="/admin">Go to admin page</Link>
                    </Button>
                </EmptyContent>
            </Empty>
        )

    }
    else {
        return <Search />
    }


}
