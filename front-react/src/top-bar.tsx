import { useParams, useNavigate } from "react-router-dom";
import { Button } from "./components/ui/button";
import { ButtonGroup } from "./components/ui/button-group";
import { SidebarTrigger } from "./components/ui/sidebar";

export function TopBar() {
  const { id, format } = useParams();
  const navigate = useNavigate();

  const buttonsDisabled = !id || !format;

  return (
    <div className="grid w-full grid-cols-[auto_1fr] items-center gap-2">
      <SidebarTrigger />

      <div className="flex justify-center">
        <ButtonGroup>
          <Button variant={format === 'html' ? 'default' : 'outline'} onClick={() => navigate(`/conversation/${id}/html`)} disabled={buttonsDisabled}>Html</Button>
          <Button variant={format === 'markdown' ? 'default' : 'outline'} onClick={() => navigate(`/conversation/${id}/markdown`)} disabled={buttonsDisabled}>Markdown</Button>
          <Button variant={format === 'json' ? 'default' : 'outline'} onClick={() => navigate(`/conversation/${id}/json`)} disabled={buttonsDisabled}>JSON</Button>
        </ButtonGroup>
      </div>
    </div>
  );
}




