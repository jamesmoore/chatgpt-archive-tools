import { useCallback, useEffect, useState } from "react";
import { getWrapStatus, setWrapStatus, WRAP_STATUS_EVENT, WRAP_STORAGE_KEY } from "../getWrapStatus";

export function useWrapPreference() {
    const [isWrapped, setIsWrappedState] = useState(() => getWrapStatus());

    useEffect(() => {
        const syncWrapStatus = () => {
            setIsWrappedState(getWrapStatus());
        };

        const handleStorageChange = (event: StorageEvent) => {
            if (event.key && event.key !== WRAP_STORAGE_KEY) {
                return;
            }

            syncWrapStatus();
        };

        const handleWrapStatusChange = (event: Event) => {
            const { detail } = event as CustomEvent<boolean>;

            if (typeof detail === "boolean") {
                setIsWrappedState(detail);
                return;
            }

            syncWrapStatus();
        };

        window.addEventListener("storage", handleStorageChange);
        window.addEventListener(WRAP_STATUS_EVENT, handleWrapStatusChange);

        return () => {
            window.removeEventListener("storage", handleStorageChange);
            window.removeEventListener(WRAP_STATUS_EVENT, handleWrapStatusChange);
        };
    }, []);

    const updateWrapPreference = useCallback((nextValue: boolean) => {
        setWrapStatus(nextValue);
    }, []);

    return {
        isWrapped,
        setIsWrapped: updateWrapPreference,
    };
}
