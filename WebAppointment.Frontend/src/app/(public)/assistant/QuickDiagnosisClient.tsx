"use client";

import { useEffect, useMemo, useState } from "react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { apiJson } from "@/lib/api-client";

type DepartmentDto = { id: number; name: string };
type DoctorDto = { id: number; name: string; departmentId: number; departmentName: string; title?: string };

type Answers = {
  age?: number;
  gender?: "Kadın" | "Erkek" | "Diğer";
  duration?: string;
  severity?: "Hafif" | "Orta" | "Şiddetli";
  symptoms?: string[];
  fever?: boolean;
  painLocation?: string;
  chronic?: string[];
  meds?: string[];
  pregnant?: boolean;
  notes?: string;
};

type DiagnosisText = string;

export default function QuickDiagnosisClient() {
  const [answers, setAnswers] = useState<Answers>({ symptoms: [], chronic: [], meds: [] });
  const [busy, setBusy] = useState(false);
  const [resultText, setResultText] = useState<DiagnosisText | "">("");
  const [departments, setDepartments] = useState<DepartmentDto[]>([]);
  const [matchedDepartment, setMatchedDepartment] = useState<DepartmentDto | null>(null);
  const [doctors, setDoctors] = useState<DoctorDto[]>([]);

  useEffect(() => {
    (async () => {
      try {
        const deps = await apiJson<DepartmentDto[]>("/backend/catalog/departments");
        setDepartments(deps);
      } catch {}
    })();
  }, []);

  useEffect(() => {
    if (!resultText || departments.length === 0) return;
    const deptMatch = resultText.match(/en uygun bölüm\s+(.+?)\s+olarak/i);
    const deptName = deptMatch?.[1]?.trim();
    const match = deptName ? departments.find(d => d.name.toLowerCase() === deptName.toLowerCase()) : undefined;
    setMatchedDepartment(match ?? null);
    (async () => {
      if (!match) return;
      try {
        const docs = await apiJson<DoctorDto[]>(`/backend/catalog/doctors?departmentId=${match.id}`);
        setDoctors(docs);
      } catch {}
    })();
  }, [resultText, departments]);

  async function runDiagnosis() {
    setBusy(true);
    setResultText("");
    try {
      const res = await fetch("/api/assistant/diagnosis", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ answers }),
      });
      const text = await res.text();
      if (!res.ok) throw new Error(text);
      setResultText(text);
    } catch (e) {
      const msg = e instanceof Error ? e.message : "Tanınlama başarısız";
      setResultText(msg);
    } finally {
      setBusy(false);
    }
  }

  const symptomInput = useMemo(() => (answers.symptoms ?? []).join(", "), [answers.symptoms]);
  const chronicInput = useMemo(() => (answers.chronic ?? []).join(", "), [answers.chronic]);
  const medsInput = useMemo(() => (answers.meds ?? []).join(", "), [answers.meds]);

  return (
    <Card className="p-4 space-y-4">
      <div className="space-y-2">
        <p className="text-sm text-slate-600 dark:text-slate-300">
          Bu özellik yalnızca yönlendirme amaçlıdır ve tıbbi tavsiye değildir. Acil durumda 112’yi arayın.
        </p>
      </div>

      <div className="grid md:grid-cols-2 gap-3">
        <div>
          <label className="block text-sm mb-1">Yaş</label>
          <Input type="number" min={0} value={answers.age ?? ""}
                 onChange={e => setAnswers(a => ({ ...a, age: Number(e.target.value || 0) }))} />
        </div>
        <div>
          <label className="block text-sm mb-1">Cinsiyet</label>
          <select className="w-full rounded-md border p-2 bg-white dark:bg-slate-900"
                  value={answers.gender ?? ""}
                  onChange={e => setAnswers(a => ({ ...a, gender: (e.target.value as Answers["gender"]) }))}>
            <option value="">Seçin</option>
            <option>Kadın</option>
            <option>Erkek</option>
            <option>Diğer</option>
          </select>
        </div>

        <div className="md:col-span-2">
          <label className="block text-sm mb-1">Belirtiler (virgülle ayırın)</label>
          <Input placeholder="Ör. baş ağrısı, ateş, öksürük"
                 value={symptomInput}
                 onChange={e => setAnswers(a => ({ ...a, symptoms: e.target.value.split(",").map(x => x.trim()).filter(Boolean) }))} />
        </div>

        <div>
          <label className="block text-sm mb-1">Süre</label>
          <Input placeholder="Ör. 3 gündür, 2 haftadır"
                 value={answers.duration ?? ""}
                 onChange={e => setAnswers(a => ({ ...a, duration: e.target.value }))} />
        </div>
        <div>
          <label className="block text-sm mb-1">Şiddet</label>
          <select className="w-full rounded-md border p-2 bg-white dark:bg-slate-900"
                  value={answers.severity ?? ""}
                  onChange={e => setAnswers(a => ({ ...a, severity: (e.target.value as Answers["severity"]) }))}>
            <option value="">Seçin</option>
            <option>Hafif</option>
            <option>Orta</option>
            <option>Şiddetli</option>
          </select>
        </div>

        <div>
          <label className="block text-sm mb-1">Ateş</label>
          <select className="w-full rounded-md border p-2 bg-white dark:bg-slate-900"
                  value={String(answers.fever ?? "")}
                  onChange={e => setAnswers(a => ({ ...a, fever: e.target.value === "true" }))}>
            <option value="">Seçin</option>
            <option value="true">Var</option>
            <option value="false">Yok</option>
          </select>
        </div>
        <div>
          <label className="block text-sm mb-1">Ağrı yeri</label>
          <Input placeholder="Ör. göğüs, karın, baş, bel"
                 value={answers.painLocation ?? ""}
                 onChange={e => setAnswers(a => ({ ...a, painLocation: e.target.value }))} />
        </div>

        <div className="md:col-span-2">
          <label className="block text-sm mb-1">Kronik hastalıklar (virgülle)</label>
          <Input placeholder="Ör. hipertansiyon, diyabet"
                 value={chronicInput}
                 onChange={e => setAnswers(a => ({ ...a, chronic: e.target.value.split(",").map(x => x.trim()).filter(Boolean) }))} />
        </div>

        <div className="md:col-span-2">
          <label className="block text-sm mb-1">Kullandığınız ilaçlar (virgülle)</label>
          <Input placeholder="Ör. ibuprofen, metformin"
                 value={medsInput}
                 onChange={e => setAnswers(a => ({ ...a, meds: e.target.value.split(",").map(x => x.trim()).filter(Boolean) }))} />
        </div>

        <div>
          <label className="block text-sm mb-1">Hamilelik</label>
          <select className="w-full rounded-md border p-2 bg-white dark:bg-slate-900"
                  value={String(answers.pregnant ?? "")}
                  onChange={e => setAnswers(a => ({ ...a, pregnant: e.target.value === "true" }))}>
            <option value="">Seçin</option>
            <option value="true">Evet</option>
            <option value="false">Hayır</option>
          </select>
        </div>
        <div>
          <label className="block text-sm mb-1">Ek notlar</label>
          <Input placeholder="Ör. yakın zamanda seyahat, alerji"
                 value={answers.notes ?? ""}
                 onChange={e => setAnswers(a => ({ ...a, notes: e.target.value }))} />
        </div>
      </div>

      <div className="flex gap-2">
        <Button onClick={runDiagnosis} disabled={busy}>Hızlı Tanı Çalıştır</Button>
      </div>

      {resultText && (
        <div className="space-y-3">
          <Card className="p-3">
            <h3 className="font-semibold">Öneri</h3>
            <p className="text-sm whitespace-pre-wrap">{resultText}</p>
          </Card>

          {matchedDepartment && (
            <Card className="p-3">
              <h3 className="font-semibold">{matchedDepartment.name} doktorları</h3>
              {doctors.length === 0 ? (
                <p className="text-sm">Bu bölümde doktor bulunamadı.</p>
              ) : (
                <ul className="text-sm space-y-1">
                  {doctors.map(d => (
                    <li key={d.name}>{d.name}{d.title ? ` – ${d.title}` : ""}</li>
                  ))}
                </ul>
              )}
            </Card>
          )}

          <p className="text-xs text-slate-500">Bu sonuçlar bilgilendirme amaçlıdır, teşhis yerine geçmez.</p>
        </div>
      )}
    </Card>
  );
}
