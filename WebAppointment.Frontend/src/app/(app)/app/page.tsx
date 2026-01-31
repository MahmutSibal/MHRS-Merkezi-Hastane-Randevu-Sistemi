import { redirect } from "next/navigation";
import { getServerSession } from "@/lib/server-session";

export default async function AppIndexPage() {
  const session = await getServerSession();
  if (!session) redirect("/login");

  if (session.role === "Admin") redirect("/admin");
  if (session.role === "Doctor") redirect("/doctor");
  redirect("/patient");
}
