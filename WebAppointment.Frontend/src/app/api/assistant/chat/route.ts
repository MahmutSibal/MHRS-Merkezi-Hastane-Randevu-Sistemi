import { NextRequest } from "next/server";

export const runtime = "nodejs";

export async function POST(req: NextRequest) {
  try {
    const { prompt } = await req.json();
    const apiKey = process.env.GEMINI_API_KEY;
    if (!apiKey) {
      return new Response(JSON.stringify({ text: String(prompt ?? "") }), { status: 200 });
    }

    // Lazy import to keep edge compatibility optional
    const { GoogleGenerativeAI } = await import("@google/generative-ai");
    const genAI = new GoogleGenerativeAI(apiKey);
    const model = genAI.getGenerativeModel({ model: "gemini-1.5-flash" });
    const result = await model.generateContent({ contents: [{ role: "user", parts: [{ text: String(prompt ?? "") }] }] });
    const text = result.response.text();
    return new Response(JSON.stringify({ text }), { status: 200, headers: { "Content-Type": "application/json" } });
  } catch (e) {
    const msg = e instanceof Error ? e.message : "Genel hata";
    return new Response(JSON.stringify({ error: msg }), { status: 500 });
  }
}
