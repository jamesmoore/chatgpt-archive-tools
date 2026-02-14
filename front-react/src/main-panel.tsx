import { useConversations } from "./hooks/use-conversations";
import { Search } from "./search";
import { Link } from "react-router-dom";
import { Button } from "./components/ui/button";
import { Card, CardContent } from "./components/ui/card";

export function MainPanel() {

    const {
        data: conversations = []
    } = useConversations();


    if(conversations.length === 0) {
        return (
            <div className="flex h-full items-center justify-center p-4">
                <Card className="w-full max-w-xl">
                    <CardContent className="flex flex-col items-center gap-4 text-center">
                        <p>No chats found. Please go to the admin page to load your chats.</p>
                        <Button asChild>
                            <Link to="/admin">Go to admin page</Link>
                        </Button>
                    </CardContent>
                </Card>
            </div>
        )

    }
    else {
        return <Search/>
    }


}
