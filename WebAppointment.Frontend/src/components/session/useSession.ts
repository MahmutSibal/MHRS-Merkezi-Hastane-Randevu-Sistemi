"use client";

import { useContext } from "react";
import { SessionContext } from "@/components/session/SessionProvider";

export function useSession() {
  return useContext(SessionContext);
}
