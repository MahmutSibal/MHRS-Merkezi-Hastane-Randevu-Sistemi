import { notFound } from "next/navigation";
import { getIsSmaEnabled } from "@/lib/sma-settings";

// Guards every /sma/* route: when the superadmin turns the feature off, this makes the
// whole section truly inaccessible (not just hidden from the menu) — direct URLs 404 too.
export default async function SmaLayout({ children }: { children: React.ReactNode }) {
  const isSmaEnabled = await getIsSmaEnabled();
  if (!isSmaEnabled) {
    notFound();
  }

  return <>{children}</>;
}
