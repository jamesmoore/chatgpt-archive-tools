import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  useSidebar,
} from "@/components/ui/sidebar"
import { useEffect } from "react";
import { Link, useLocation, useParams } from "react-router-dom";
import { useConversations } from '../hooks/use-conversations'
import { Search } from "lucide-react";

export function AppSidebar() {
  const { id, format } = useParams();
  const location = useLocation();
  const { setOpenMobile } = useSidebar();

  const {
    data: conversations = []
  } = useConversations();

  useEffect(() => {
    setOpenMobile(false);
  }, [location.pathname, setOpenMobile]);

  return (
    <Sidebar className="h-full">
      <SidebarHeader>
        <SidebarMenu>
          <SidebarGroupLabel className="text-base my-0.5">Conversations</SidebarGroupLabel>
          <SidebarMenuItem key={'search'}>
            <SidebarMenuButton asChild isActive={false}>
              <Link to={'/'}
                onClick={() => {
                  setOpenMobile(false);
                }}
              >
                <Search /> Search chats
              </Link>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarHeader>

      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupContent>
            <SidebarMenu>
              {conversations.map((item) => (
                <SidebarMenuItem key={item.id}>
                  <SidebarMenuButton asChild isActive={id === item.id}>
                    <Link to={`/conversation/${item.id}/${format || 'html'}`}
                      onClick={() => {
                        if (id === item.id) {
                          setOpenMobile(false);
                        }
                      }}
                    >
                      <span>{item.title}</span>
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
    </Sidebar>
  )
}

