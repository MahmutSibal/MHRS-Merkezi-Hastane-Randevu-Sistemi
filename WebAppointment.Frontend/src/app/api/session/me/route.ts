import { cookies } from "next/headers";
import { NextResponse } from "next/server";
import { decodeJwtPayload } from "@/lib/jwt";

const ACCESS_COOKIE = "mhrs_at";

export async function GET() {
  const cookieStore = await cookies();
  const accessToken = cookieStore.get(ACCESS_COOKIE)?.value;
  if (!accessToken) {
    return new NextResponse("Unauthorized", { status: 401 });
  }

  const payload = decodeJwtPayload(accessToken);
  if (!payload) {
    return new NextResponse("Unauthorized", { status: 401 });
  }

  const roleClaim =
    payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ??
    payload.role ??
    payload.roles;

  const role = Array.isArray(roleClaim) ? roleClaim[0] : roleClaim;

  return NextResponse.json({
    userId: payload.sub,
    email: payload.email,
    role: role ?? "",
    exp: payload.exp,
  });
}
