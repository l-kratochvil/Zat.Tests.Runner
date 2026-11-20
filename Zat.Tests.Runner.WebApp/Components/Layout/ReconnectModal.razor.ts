// Blazor's reconnect UI, taken over from the framework default so the modal matches the app.
// The circuit controls live on the global Blazor object; only the two used here are declared.
declare const Blazor: {
    reconnect(): Promise<boolean>;
    resumeCircuit(): Promise<boolean>;
};

type ReconnectState = "show" | "hide" | "failed" | "rejected";

const reconnectModal = document.getElementById("components-reconnect-modal") as HTMLDialogElement;
const retryButton = document.getElementById("components-reconnect-button") as HTMLButtonElement;
const resumeButton = document.getElementById("components-resume-button") as HTMLButtonElement;

reconnectModal.addEventListener("components-reconnect-state-changed", (event: Event): void =>
    handleReconnectStateChanged(event as CustomEvent<{ state: ReconnectState }>));

retryButton.addEventListener("click", () => void retry());
resumeButton.addEventListener("click", () => void resume());

function handleReconnectStateChanged(event: CustomEvent<{ state: ReconnectState }>): void {
    if (event.detail.state === "show") {
        reconnectModal.showModal();
    } else if (event.detail.state === "hide") {
        reconnectModal.close();
    } else if (event.detail.state === "failed") {
        document.addEventListener("visibilitychange", retryWhenDocumentBecomesVisible);
    } else if (event.detail.state === "rejected") {
        location.reload();
    }
}

async function retry(): Promise<void> {
    document.removeEventListener("visibilitychange", retryWhenDocumentBecomesVisible);

    try {
        // Reconnect resolves to true on success and to false when the server was reached but
        // rejected the connection (e.g. unknown circuit ID); it throws when the server was not
        // reached at all.
        const successful = await Blazor.reconnect();

        if (!successful) {
            // The server is up but the circuit is gone. Resuming keeps the user's state; if that
            // fails too, a reload at least gets them a working app as quickly as possible.
            const resumeSuccessful = await Blazor.resumeCircuit();

            if (resumeSuccessful) {
                reconnectModal.close();
            } else {
                location.reload();
            }
        }
    } catch {
        // The server is currently unreachable - retry once the tab is looked at again.
        document.addEventListener("visibilitychange", retryWhenDocumentBecomesVisible);
    }
}

async function resume(): Promise<void> {
    try {
        const successful = await Blazor.resumeCircuit();

        if (!successful) {
            location.reload();
        }
    } catch {
        reconnectModal.classList.replace("components-reconnect-paused", "components-reconnect-resume-failed");
    }
}

// Retrying in a hidden tab is wasted work, so a failed attempt waits for the tab to come back.
async function retryWhenDocumentBecomesVisible(): Promise<void> {
    if (document.visibilityState === "visible") {
        await retry();
    }
}
