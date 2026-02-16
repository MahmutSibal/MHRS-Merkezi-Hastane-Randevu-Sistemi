import { NextRequest } from "next/server";
import { GoogleGenerativeAI } from "@google/generative-ai";

export const dynamic = "force-dynamic";
export const revalidate = 0;
export const runtime = "nodejs";

function stripCodeFence(text: string): string {
  // ```json\n...\n``` veya ```\n...\n``` kalıplarını içeriğe indirger
  const fence = text.match(/```[a-zA-Z]*\s*([\s\S]*?)```/);
  if (fence && fence[1]) return fence[1].trim();
  return text.trim();
}

function extractJsonCandidate(text: string): string | null {
  const cleaned = stripCodeFence(text);
  // Metnin tamamını dene
  try { JSON.parse(cleaned); return cleaned; } catch {}
  // İlk { ... } bloğunu yakalamayı dene
  const brace = cleaned.match(/\{[\s\S]*\}/);
  if (brace && brace[0]) {
    const candidate = brace[0];
    try { JSON.parse(candidate); return candidate; } catch {}
  }
  return null;
}

function buildPrompt(answers: any) {
  const a = answers || {};
  const lines = [
    "Hasta Hızlı Tanı Anketi (Türkçe):",
    `Yaş: ${a.age ?? ""}`,
    `Cinsiyet: ${a.gender ?? ""}`,
    `Belirtiler: ${(a.symptoms ?? []).join(", ")}`,
    `Süre: ${a.duration ?? ""}`,
    `Şiddet: ${a.severity ?? ""}`,
    `Ateş: ${a.fever === true ? "Var" : a.fever === false ? "Yok" : ""}`,
    `Ağrı yeri: ${a.painLocation ?? ""}`,
    `Kronik: ${(a.chronic ?? []).join(", ")}`,
    `İlaçlar: ${(a.meds ?? []).join(", ")}`,
    `Hamilelik: ${a.pregnant === true ? "Evet" : a.pregnant === false ? "Hayır" : ""}`,
    `Notlar: ${a.notes ?? ""}`,
    "\nGörev: Türkiye’de hastane poliklinik isimlerine göre tek bir bölüm öner (ör: Dahiliye, Kardiyoloji, Nöroloji, Kulak Burun Boğaz, Göz, Dermatoloji, Ortopedi, Kadın Doğum, Çocuk Sağlığı vb.).",
    "Çıktıyı JSON olarak ver: { departmentName: string, specialtyHint?: string, reasons: string[], urgency: 'Düşük'|'Orta'|'Yüksek' }",
    "Tıbbi tavsiye vermeden, yönlendirme amaçlı kısa gerekçeler ekle."
  ];
  return lines.join("\n");
}

export async function POST(req: NextRequest) {
  try {
    const { answers } = await req.json();
    const apiKey = process.env.GEMINI_API_KEY;
    if (!apiKey) {
      return new Response(JSON.stringify({ rawText: "Sunucu yapılandırması eksik: GEMINI_API_KEY ayarlı değil." }), { status: 500 });
    }

    const genAI = new GoogleGenerativeAI(apiKey);
    const model = genAI.getGenerativeModel({ model: "gemini-2.5-flash" });
    const prompt = buildPrompt(answers);
    const result = await model.generateContent(prompt);
    const text = result.response.text();

    // JSON gelirse kullanıcı dostu metne çevir, değilse olduğu gibi sonuna uyarı ekle
    let displayText: string;
    try {
      const candidate = extractJsonCandidate(text);
      const parsed = JSON.parse(candidate ?? text) as { departmentName?: string; specialtyHint?: string; reasons?: string[]; urgency?: string };
      const dept = parsed.departmentName?.trim();
      const urg = parsed.urgency?.trim();
      const reasons = Array.isArray(parsed.reasons) ? parsed.reasons.filter(r => typeof r === "string" && r.trim().length > 0) : [];
      const reasonsText = reasons.length ? ` Başlıca gerekçeler: ${reasons.map((r, i) => `${i + 1}. ${r}`).join(" ")}` : "";
      const deptText = dept ? `${dept}${parsed.specialtyHint ? ` (${parsed.specialtyHint})` : ""}` : "uygun bir bölüm";
      const urgencyText = urg ? ` Öncelik: ${urg}.` : "";
      displayText = `Değerlendirme: Belirtileriniz ve verdiğiniz yanıtlar doğrultusunda en uygun bölüm ${deptText} olarak görünmektedir.${urgencyText}${reasonsText} Bu sonuçlar bilgilendirme amaçlıdır; acil durumda 112’yi arayın.`;
    } catch {
      const cleaned = stripCodeFence(text);
      displayText = `${cleaned}\n\nBu sonuçlar bilgilendirme amaçlıdır; acil durumda 112’yi arayın.`;
    }
    return new Response(displayText, { status: 200, headers: { "Content-Type": "text/plain; charset=utf-8" } });
  } catch (e: any) {
    const msg = typeof e?.message === "string" ? e.message : "İşlem başarısız";
    return new Response(msg, { status: 500, headers: { "Content-Type": "text/plain; charset=utf-8" } });
  }
}

export async function GET() {
  return new Response("Tanı için POST isteği gönderin.", {
    status: 405,
    headers: { "Content-Type": "text/plain; charset=utf-8" },
  });
}
