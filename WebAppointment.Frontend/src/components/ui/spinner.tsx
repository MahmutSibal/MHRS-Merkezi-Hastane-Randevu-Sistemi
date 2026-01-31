export function Spinner({ className }: { className?: string }) {
  return (
    <span
      aria-hidden="true"
      className={
        [
          "inline-block size-4 animate-spin rounded-full border-2 border-current border-r-transparent align-[-2px]",
          className,
        ]
          .filter(Boolean)
          .join(" ")
      }
    />
  );
}
