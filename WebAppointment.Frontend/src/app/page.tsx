import { redirect } from "next/navigation";
import PublicHomePage from "./(public)/page";
import { getServerSession } from "@/lib/server-session";

export default async function RootPage() {
  const session = await getServerSession();
  if (session) redirect("/app");
  return <PublicHomePage />;
}
