import { NextResponse, type NextRequest } from "next/server";

const ACCESS_COOKIE = "mhrs_at";
const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

function decodePayload(token: string): Record<string, unknown> | null {
  const parts = token.split(".");
  if (parts.length < 2) return null;
  const base64Url = parts[1];
  const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
  const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), "=");
  try {
    const json = atob(padded);
    return JSON.parse(json) as Record<string, unknown>;
  } catch {
    return null;
  }
}

function getRole(payload: Record<string, unknown>) {
  const v = (payload[ROLE_CLAIM] ?? payload.role ?? payload.roles) as unknown;
  if (Array.isArray(v)) return String(v[0] ?? "");
  return typeof v === "string" ? v : "";
}

export function middleware(req: NextRequest) {
  const { pathname } = req.nextUrl;

  const accessToken = req.cookies.get(ACCESS_COOKIE)?.value;
  const isAuthPage = pathname.startsWith("/login") || pathname.startsWith("/register");
  const isProtected =
    pathname.startsWith("/admin") ||
    pathname.startsWith("/doctor") ||
    pathname.startsWith("/patient") ||
    pathname.startsWith("/app") ||
    pathname.startsWith("/profile");

  if (!accessToken && isProtected) {
    const url = req.nextUrl.clone();
    url.pathname = "/login";
    url.searchParams.set("next", pathname);
    return NextResponse.redirect(url);
  }

  if (accessToken && isAuthPage) {
    const url = req.nextUrl.clone();
    url.pathname = "/app";
    url.search = "";
    return NextResponse.redirect(url);
  }

  if (
    accessToken &&
    (pathname.startsWith("/admin") || pathname.startsWith("/doctor") || pathname.startsWith("/patient"))
  ) {
    const payload = decodePayload(accessToken);
    if (!payload) return NextResponse.next();
    const role = getRole(payload);

    const wantsAdmin = pathname.startsWith("/admin");
    const wantsDoctor = pathname.startsWith("/doctor");
    const wantsPatient = pathname.startsWith("/patient");

    if (wantsAdmin && role !== "Admin") return NextResponse.redirect(new URL("/app", req.url));
    if (wantsDoctor && role !== "Doctor") return NextResponse.redirect(new URL("/app", req.url));
    if (wantsPatient && role !== "Patient") return NextResponse.redirect(new URL("/app", req.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico|api/).*)"],
};
