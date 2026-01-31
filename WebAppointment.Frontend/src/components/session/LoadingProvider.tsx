"use client";

import { createContext, useContext, useState } from "react";
import { LoadingSpinner } from "@/components/ui/loading-spinner";

interface LoadingState {
  isLoading: boolean;
  message?: string;
}

interface LoadingContextType {
  isLoading: boolean;
  message?: string;
  start: (message?: string) => void;
  stop: () => void;
}

const LoadingContext = createContext<LoadingContextType | undefined>(undefined);

export function LoadingProvider({ children }: { children: React.ReactNode }) {
  const [state, setState] = useState<LoadingState>({ isLoading: false });

  const start = (message?: string) => {
    setState({ isLoading: true, message });
  };

  const stop = () => {
    setState({ isLoading: false, message: undefined });
  };

  return (
    <LoadingContext.Provider value={{ ...state, start, stop }}>
      {children}
      {state.isLoading && (
        <LoadingSpinner fullscreen={true} message={state.message} />
      )}
    </LoadingContext.Provider>
  );
}

export function useLoading() {
  const context = useContext(LoadingContext);
  if (!context) {
    throw new Error("useLoading must be used within LoadingProvider");
  }
  return context;
}
