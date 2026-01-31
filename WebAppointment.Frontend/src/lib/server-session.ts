import { cookies } from "next/headers";
import { decodeJwtPayload } from "@/lib/jwt";

const ACCESS_COOKIE = "mhrs_at";
const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

export type ServerSession = {
  userId: string;
  email: string;
  role: string;
};

export async function getServerSession(): Promise<ServerSession | null> {
  const cookieStore = await cookies();
  const accessToken = cookieStore.get(ACCESS_COOKIE)?.value;
  if (!accessToken) return null;

  const payload = decodeJwtPayload(accessToken);
  if (!payload) return null;

  const roleClaim = payload[ROLE_CLAIM] ?? payload.role ?? payload.roles;
  const role = Array.isArray(roleClaim) ? String(roleClaim[0] ?? "") : String(roleClaim ?? "");

  return {
    userId: String(payload.sub ?? ""),
    email: String(payload.email ?? ""),
    role,
  };
}
