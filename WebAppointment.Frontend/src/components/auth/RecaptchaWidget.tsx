"use client";

import { useEffect, useRef, useState } from "react";

declare global {
  interface Window {
    grecaptcha?: {
      render: (
        container: HTMLElement,
        params: { sitekey: string; callback: (token: string) => void; "expired-callback"?: () => void }
      ) => number;
    };
    __onRecaptchaLoad?: () => void;
  }
}

const SCRIPT_ID = "recaptcha-script";

export function RecaptchaWidget({ onChange }: { onChange: (token: string | null) => void }) {
  const containerRef = useRef<HTMLDivElement>(null);
  const widgetIdRef = useRef<number | null>(null);
  const [ready, setReady] = useState(() => typeof window !== "undefined" && !!window.grecaptcha);
  const siteKey = process.env.NEXT_PUBLIC_RECAPTCHA_SITE_KEY;

  useEffect(() => {
    if (!siteKey || ready) return;

    if (document.getElementById(SCRIPT_ID)) {
      window.__onRecaptchaLoad = () => setReady(true);
      return;
    }

    window.__onRecaptchaLoad = () => setReady(true);
    const script = document.createElement("script");
    script.id = SCRIPT_ID;
    script.src = "https://www.google.com/recaptcha/api.js?onload=__onRecaptchaLoad&render=explicit";
    script.async = true;
    script.defer = true;
    document.head.appendChild(script);
  }, [siteKey, ready]);

  useEffect(() => {
    if (!ready || !siteKey || !containerRef.current || widgetIdRef.current !== null || !window.grecaptcha) {
      return;
    }

    widgetIdRef.current = window.grecaptcha.render(containerRef.current, {
      sitekey: siteKey,
      callback: (token: string) => onChange(token),
      "expired-callback": () => onChange(null),
    });
  }, [ready, siteKey, onChange]);

  if (!siteKey) {
    return null;
  }

  return <div ref={containerRef} />;
}
