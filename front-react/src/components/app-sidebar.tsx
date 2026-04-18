import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
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
import { ChevronRight, Search, Settings } from "lucide-react";
import { Switch } from "@/components/ui/switch";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { useWrapPreference } from "@/hooks/use-wrap-preference";

export function AppSidebar() {
  const { id, format } = useParams();
  const location = useLocation();
  const { setOpenMobile } = useSidebar();
  const { isWrapped, setIsWrapped } = useWrapPreference();
  const isTextFormat = format === 'markdown' || format === 'json';

  const {
    data: conversations = []
  } = useConversations();

  useEffect(() => {
    setOpenMobile(false);
  }, [location.pathname, setOpenMobile]);

  const toggleWrap = (checked: boolean) => {
    setIsWrapped(checked);
  };

  return (
    <Sidebar className="h-full">
      <SidebarHeader>
        <SidebarMenu>
          <SidebarGroupLabel className="text-base my-0.5">ChatGPT archive</SidebarGroupLabel>
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

      <SidebarFooter>
        <SidebarMenu>
          <SidebarMenuItem>
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <SidebarMenuButton isActive={location.pathname === '/admin' || isTextFormat}>
                  <Settings />
                  <span>Settings</span>
                  <ChevronRight className="ml-auto" />
                </SidebarMenuButton>
              </DropdownMenuTrigger>
              <DropdownMenuContent side="right" align="start" className="w-48">
                <DropdownMenuItem asChild>
                  <Link to="/admin">Admin</Link>
                </DropdownMenuItem>
                {isTextFormat && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      onSelect={(event) => {
                        event.preventDefault();
                        toggleWrap(!isWrapped);
                      }}
                      className="flex items-center justify-between"
                    >
                      <span>Word wrap</span>
                      <Switch
                        checked={isWrapped}
                        onCheckedChange={toggleWrap}
                        onClick={(event) => event.stopPropagation()}
                        className="pointer-events-none"
                        aria-label="Toggle word wrap"
                      />
                    </DropdownMenuItem>
                  </>
                )}
              </DropdownMenuContent>
            </DropdownMenu>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarFooter>
    </Sidebar>
  )
}

