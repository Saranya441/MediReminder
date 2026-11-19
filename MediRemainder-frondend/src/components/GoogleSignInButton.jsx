import { useEffect, useRef, useState } from "react";

const scriptUrl = "https://accounts.google.com/gsi/client";
let scriptPromise;

function loadGoogleIdentityServices() {
  if (window.google?.accounts?.id) return Promise.resolve();
  if (!scriptPromise) {
    scriptPromise = new Promise((resolve, reject) => {
      let script = document.querySelector(`script[src="${scriptUrl}"]`);
      if (!script) {
        script = document.createElement("script");
        script.src = scriptUrl;
        script.async = true;
        script.defer = true;
        document.head.appendChild(script);
      }
      script.addEventListener("load", resolve, { once: true });
      script.addEventListener("error", reject, { once: true });
    });
  }
  return scriptPromise;
}

export default function GoogleSignInButton({ onCredential, onError }) {
  const containerRef = useRef(null);
  const onCredentialRef = useRef(onCredential);
  const onErrorRef = useRef(onError);
  const [message, setMessage] = useState("");
  const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID?.trim();

  useEffect(() => { onCredentialRef.current = onCredential; }, [onCredential]);
  useEffect(() => { onErrorRef.current = onError; }, [onError]);

  useEffect(() => {
    let cancelled = false;
    if (!clientId) {
      return undefined;
    }

    loadGoogleIdentityServices().then(() => {
      if (cancelled || !containerRef.current) return;
      setMessage("");
      window.google.accounts.id.initialize({
        client_id: clientId,
        callback: (response) => {
          if (response.credential) onCredentialRef.current(response.credential);
          else onErrorRef.current("Google did not return an identity token.");
        },
        error_callback: () => onErrorRef.current("Google sign-in was closed or could not start."),
        ux_mode: "popup",
      });
      containerRef.current.replaceChildren();
      window.google.accounts.id.renderButton(containerRef.current, {
        type: "standard",
        theme: "outline",
        size: "large",
        text: "continue_with",
        shape: "rectangular",
        width: Math.min(containerRef.current.clientWidth || 360, 400),
        logo_alignment: "center",
      });
    }).catch(() => {
      if (!cancelled) setMessage("Could not load Google sign-in. Check your internet connection and try again.");
    });

    return () => { cancelled = true; };
  }, [clientId]);

  if (!clientId) {
    return (
      <div className="google-signin-wrap">
        <button
          type="button"
          className="google-signin-fallback"
          onClick={() => setMessage("To enable Google sign-in, add your Google OAuth client ID to the frontend .env.local file and the backend Google:ClientId setting, then restart both apps.")}
        >
          <span className="google-signin-g" aria-hidden="true">G</span>
          Continue with Google
        </button>
        {message && <p className="google-signin-message" role="status">{message}</p>}
      </div>
    );
  }

  return (
    <div className="google-signin-wrap">
      <div ref={containerRef} className="google-signin-container" />
      {message && <p className="google-signin-message" role="status">{message}</p>}
    </div>
  );
}
