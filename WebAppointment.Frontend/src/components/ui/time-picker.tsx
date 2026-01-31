"use client";

import { cn } from "@/lib/cn";

interface TimePickerProps {
  value?: string; // HH:MM format
  onChange?: (value: string) => void;
  label?: string;
  error?: string;
  hint?: string;
}

export function TimePicker({ value = "09:00", onChange, label, error, hint }: TimePickerProps) {
  const [hour, minute] = value.split(":").length === 2 ? value.split(":") : ["09", "00"];

  // Çalışma saatleri: 09:00 - 17:00
  const hours = Array.from({ length: 9 }, (_, i) => String(i + 9).padStart(2, "0"));
  const minutes = ["00", "30"];

  const handleHourChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const newHour = e.target.value;
    onChange?.(`${newHour}:${minute}`);
  };

  const handleMinuteChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const newMinute = e.target.value;
    onChange?.(`${hour}:${newMinute}`);
  };

  const describedById = label ? `time-picker-help` : undefined;

  return (
    <label className="block">
      {label ? (
        <span className="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">{label}</span>
      ) : null}

      <div className="flex gap-2">
        <div className="flex-1">
          <select
            value={hour}
            onChange={handleHourChange}
            aria-describedby={describedById}
            aria-invalid={error ? "true" : "false"}
            className={cn(
              "h-10 w-full rounded-lg border-2 border-slate-200 bg-white px-3 py-2 text-sm outline-none transition-all duration-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100 dark:focus:border-blue-400 dark:focus:ring-blue-900",
              error ? "border-red-500 focus:border-red-500 focus:ring-red-200 dark:focus:ring-red-900" : "",
            )}
          >
            {hours.map((h) => (
              <option key={h} value={h}>
                {h}
              </option>
            ))}
          </select>
          <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">Saat</p>
        </div>

        <div className="flex items-center text-2xl font-bold text-slate-400 dark:text-slate-600">:</div>

        <div className="flex-1">
          <select
            value={minute}
            onChange={handleMinuteChange}
            aria-describedby={describedById}
            aria-invalid={error ? "true" : "false"}
            className={cn(
              "h-10 w-full rounded-lg border-2 border-slate-200 bg-white px-3 py-2 text-sm outline-none transition-all duration-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100 dark:focus:border-blue-400 dark:focus:ring-blue-900",
              error ? "border-red-500 focus:border-red-500 focus:ring-red-200 dark:focus:ring-red-900" : "",
            )}
          >
            {minutes.map((m) => (
              <option key={m} value={m}>
                {m}
              </option>
            ))}
          </select>
          <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">Dakika</p>
        </div>
      </div>

      {hint || error ? (
        <span id={describedById} className={cn("mt-1.5 block text-xs font-medium", error ? "text-red-600 dark:text-red-400" : "text-slate-500 dark:text-slate-400")}>
          {error ?? hint}
        </span>
      ) : null}
    </label>
  );
}
