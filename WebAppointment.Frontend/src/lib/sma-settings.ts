import { getBackendOrigin } from "@/lib/backend";

/** Server-side check of the superadmin-controlled SMA feature toggle. Fails open (true)
 *  if the backend can't be reached, so a transient outage never looks like an intentional
 *  shutdown of the feature. */
export async function getIsSmaEnabled(): Promise<boolean> {
  try {
    const res = await fetch(`${getBackendOrigin()}/api/sma/settings`, { cache: "no-store" });
    if (!res.ok) return true;
    const data = (await res.json()) as { isSmaEnabled: boolean };
    return data.isSmaEnabled;
  } catch {
    return true;
  }
}
