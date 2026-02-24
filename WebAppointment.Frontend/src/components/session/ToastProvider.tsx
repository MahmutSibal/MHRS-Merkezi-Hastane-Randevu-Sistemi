"use client";

import { createContext, useCallback, useContext, useMemo, useState } from "react";
import { Toast, ToastType, ToastContainer } from "@/components/ui/toast";

interface ToastContextType {
  show: (message: string, type: ToastType, duration?: number) => void;
  success: (message: string, duration?: number) => void;
  error: (message: string, duration?: number) => void;
  warning: (message: string, duration?: number) => void;
  info: (message: string, duration?: number) => void;
}

const ToastContext = createContext<ToastContextType | undefined>(undefined);

export function ToastProvider({ children }: { children: React.ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([]);

  const show = useCallback((message: string, type: ToastType, duration: number = 4000) => {
    const id = Math.random().toString(36).substr(2, 9);
    const newToast: Toast = { id, message, type, duration };
    setToasts((prev) => [...prev, newToast]);
  }, []);

  const onClose = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const success = useCallback((message: string, duration?: number) => show(message, "success", duration), [show]);
  const error = useCallback((message: string, duration?: number) => show(message, "error", duration), [show]);
  const warning = useCallback((message: string, duration?: number) => show(message, "warning", duration), [show]);
  const info = useCallback((message: string, duration?: number) => show(message, "info", duration), [show]);

  const value = useMemo(() => ({ show, success, error, warning, info }), [show, success, error, warning, info]);

  return (
    <ToastContext.Provider value={value}>
      {children}
      <ToastContainer toasts={toasts} onClose={onClose} />
    </ToastContext.Provider>
  );
}

export function useToast() {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error("useToast must be used within ToastProvider");
  }
  return context;
}
