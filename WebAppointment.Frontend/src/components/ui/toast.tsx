"use client";

import { useEffect, useState } from "react";
import { cn } from "@/lib/cn";

export type ToastType = "success" | "error" | "warning" | "info";

export interface Toast {
  id: string;
  message: string;
  type: ToastType;
  duration?: number;
}

interface ToastItemProps {
  toast: Toast;
  onClose: (id: string) => void;
}

function ToastItem({ toast, onClose }: ToastItemProps) {
  const [isExiting, setIsExiting] = useState(false);

  useEffect(() => {
    if (!toast.duration) return;
    
    const timer = setTimeout(() => {
      setIsExiting(true);
      setTimeout(() => onClose(toast.id), 300);
    }, toast.duration);

    return () => clearTimeout(timer);
  }, [toast.duration, toast.id, onClose]);

  const colorConfig = {
    success: {
      bg: "bg-emerald-50 dark:bg-emerald-950",
      border: "border-emerald-200 dark:border-emerald-800",
      text: "text-emerald-800 dark:text-emerald-200",
      icon: "text-emerald-600 dark:text-emerald-400",
      indicator: "bg-emerald-500",
    },
    error: {
      bg: "bg-red-50 dark:bg-red-950",
      border: "border-red-200 dark:border-red-800",
      text: "text-red-800 dark:text-red-200",
      icon: "text-red-600 dark:text-red-400",
      indicator: "bg-red-500",
    },
    warning: {
      bg: "bg-amber-50 dark:bg-amber-950",
      border: "border-amber-200 dark:border-amber-800",
      text: "text-amber-800 dark:text-amber-200",
      icon: "text-amber-600 dark:text-amber-400",
      indicator: "bg-amber-500",
    },
    info: {
      bg: "bg-blue-50 dark:bg-blue-950",
      border: "border-blue-200 dark:border-blue-800",
      text: "text-blue-800 dark:text-blue-200",
      icon: "text-blue-600 dark:text-blue-400",
      indicator: "bg-blue-500",
    },
  };

  const config = colorConfig[toast.type];

  const icons = {
    success: "✓",
    error: "✕",
    warning: "!",
    info: "i",
  };

  return (
    <div
      className={cn(
        "transform transition-all duration-300",
        isExiting ? "translate-x-96 opacity-0" : "translate-x-0 opacity-100"
      )}
    >
      <div
        className={cn(
          "flex items-start gap-3 rounded-lg border px-4 py-3 shadow-lg",
          config.bg,
          config.border
        )}
      >
        <div className={cn("mt-0.5 flex h-6 w-6 items-center justify-center rounded-full font-bold text-white text-sm", config.indicator)}>
          {icons[toast.type]}
        </div>
        <div className="flex-1 min-w-0">
          <p className={cn("text-sm font-medium", config.text)}>{toast.message}</p>
        </div>
        <button
          onClick={() => {
            setIsExiting(true);
            setTimeout(() => onClose(toast.id), 300);
          }}
          className={cn("ml-2 flex-shrink-0 text-sm font-medium hover:underline", config.text)}
        >
          Kapat
        </button>
      </div>
    </div>
  );
}

interface ToastContainerProps {
  toasts: Toast[];
  onClose: (id: string) => void;
}

export function ToastContainer({ toasts, onClose }: ToastContainerProps) {
  return (
    <div className="fixed bottom-6 right-6 z-50 flex flex-col gap-2 pointer-events-auto max-w-sm">
      {toasts.map((toast) => (
        <ToastItem key={toast.id} toast={toast} onClose={onClose} />
      ))}
    </div>
  );
}
