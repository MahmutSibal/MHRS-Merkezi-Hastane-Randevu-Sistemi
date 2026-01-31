"use client";

import { createContext, useEffect, useMemo, useState } from "react";

export type Session = {
  userId: string;
  email: string;
  role: string;
};

type SessionState = {
  session: Session | null;
  isLoading: boolean;
  refresh: () => Promise<void>;
};

export const SessionContext = createContext<SessionState>({
  session: null,
  isLoading: true,
  refresh: async () => {},
});

export function SessionProvider({ children }: { children: React.ReactNode }) {
  const [session, setSession] = useState<Session | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  async function refresh() {
    setIsLoading(true);
    try {
      const res = await fetch("/api/session/me", { cache: "no-store" });
      if (!res.ok) {
        setSession(null);
        return;
      }
      const data = (await res.json()) as Session;
      setSession(data);
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void refresh();
  }, []);

  const value = useMemo(() => ({ session, isLoading, refresh }), [session, isLoading]);
  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>;
}
