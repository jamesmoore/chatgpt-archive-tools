import { useParams, useNavigate } from "react-router-dom";
import { useState } from "react";
import { Button } from "./components/ui/button";
import { ButtonGroup } from "./components/ui/button-group";
import { SidebarTrigger } from "./components/ui/sidebar";
import { WrapText } from "lucide-react";
import { getWrapStatus, setWrapStatus } from "./getWrapStatus";

export function TopBar() {
  const { id, format } = useParams();
  const navigate = useNavigate();

  const isTextFormat = format === 'markdown' || format === 'json';
  const [isWrapped, setIsWrapped] = useState(() => {
    return getWrapStatus();
  });

  const toggleWrap = () => {
    const newValue = !isWrapped;
    setIsWrapped(newValue);
    setWrapStatus(newValue);
    window.dispatchEvent(new Event('storage'));
  };

  const buttonsDisabled = !id || !format;

  return (
    <div className="flex items-center gap-2">
      <SidebarTrigger />

      <ButtonGroup className="ml-auto">
        <Button variant={format === 'html' ? 'default' : 'outline'} onClick={() => navigate(`/conversation/${id}/html`)} disabled={buttonsDisabled}>Html</Button>
        <Button variant={format === 'markdown' ? 'default' : 'outline'} onClick={() => navigate(`/conversation/${id}/markdown`)} disabled={buttonsDisabled}>Markdown</Button>
        <Button variant={format === 'json' ? 'default' : 'outline'} onClick={() => navigate(`/conversation/${id}/json`)} disabled={buttonsDisabled}>JSON</Button>
      </ButtonGroup>

      <Button
        variant={isWrapped ? 'default' : 'outline'}
        size="icon"
        onClick={toggleWrap}
        title="Toggle word wrap"
        disabled={!isTextFormat}
        aria-label="Toggle word wrap"
      >
        <WrapText className="h-4 w-4" />
      </Button>
    </div>
  );
}




