"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card } from "@/components/ui/card";
import { cn } from "@/lib/cn";
import { LogoMark } from "@/components/ui/logo";
import { apiJson } from "@/lib/api-client";

type DepartmentDto = { id: number; name: string };
type DoctorDto = { id: number; name: string; departmentId: number; departmentName: string };
type HospitalDto = { id: number; name: string };
type Message = { id: string; role: "assistant" | "user"; text: string };
type Step =
  | "hospital"
  | "email"
  | "password"
  | "confirmLogin"
  | "department"
  | "doctor"
  | "date"
  | "time"
  | "finalConfirm"
  | "complete";

function uid() { return Math.random().toString(36).slice(2); }
function withTurkeyOffset(date: string, time: string) {
  if (!date || !time) return "";
  const dateTimeLocal = `${date}T${time}:00`;
  return `${dateTimeLocal}+03:00`;
}

export function AssistantWidget({ className }: { className?: string }) {
  const [open, setOpen] = useState(false);
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState("");
  const [busy, setBusy] = useState(false);
  const [step, setStep] = useState<Step>("email");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [departments, setDepartments] = useState<DepartmentDto[]>([]);
  const [departmentId, setDepartmentId] = useState<number>(0);
  const [doctors, setDoctors] = useState<DoctorDto[]>([]);
  const [doctorId, setDoctorId] = useState<number>(0);
  const [hospitals, setHospitals] = useState<HospitalDto[]>([]);
  const [hospitalId, setHospitalId] = useState<number>(0);
  const [date, setDate] = useState<string>("");
  const [time, setTime] = useState<string>("09:00");

  const selectedDepartment = useMemo(() => departments.find(d => d.id === departmentId) ?? null, [departments, departmentId]);
  const selectedDoctor = useMemo(() => doctors.find(d => d.id === doctorId) ?? null, [doctors, doctorId]);

  const listEndRef = useRef<HTMLDivElement>(null);
  const scrollToBottom = () => listEndRef.current?.scrollIntoView({ behavior: "smooth" });
  useEffect(scrollToBottom, [messages, open]);

  useEffect(() => {
    if (open && messages.length === 0) {
      setMessages([{ id: uid(), role: "assistant", text: "Merhaba! E-posta adresinizi yazar mısınız?" }]);
    }
  }, [open, messages.length]);

  async function replyWithGemini(prompt: string) {
    try {
      const res = await fetch("/api/assistant/chat", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ prompt })
      });
      if (!res.ok) return prompt;
      const data = await res.json().catch(() => ({}));
      return (data?.text as string) || prompt;
    } catch {
      return prompt;
    }
  }

  function addAssistant(text: string) { setMessages(m => [...m, { id: uid(), role: "assistant", text }]); }
  function addUser(text: string) { setMessages(m => [...m, { id: uid(), role: "user", text }]); }

  async function onSend() {
    const content = input.trim();
    if (!content || busy) return;
    setBusy(true);
    setInput("");
    addUser(content);
    try {
      if (step === "email") {
        const ok = /.+@.+\..+/.test(content);
        if (!ok) {
          addAssistant("Geçerli bir e-posta adresi girer misiniz?");
        } else {
          setEmail(content);
          addAssistant(await replyWithGemini("Teşekkürler. Şifrenizi yazar mısınız?"));
          setStep("password");
        }
      } else if (step === "password") {
        setPassword(content);
        addAssistant(await replyWithGemini(`E-posta: ${email}. Şifreyi aldım. Giriş yapmamı onaylıyorsanız 'onayliyorum' yazın.`));
        setStep("confirmLogin");
      } else if (step === "confirmLogin") {
        if (content.toLowerCase() !== "onayliyorum") {
          addAssistant("Girişe devam etmek için lütfen 'onayliyorum' yazın.");
        } else {
          const res = await fetch("/api/session/login", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ email, password }) });
          if (!res.ok) {
            addAssistant("Giriş başarısız. Lütfen e-postanızı tekrar yazın.");
            setStep("email");
          } else {
            const hosps = await apiJson<HospitalDto[]>("/backend/catalog/hospitals");
            setHospitals(hosps);
            const list = hosps.map(h => `- ${h.name}`).join("\n");
            addAssistant(await replyWithGemini(`Giriş başarılı. Mevcut hastaneler:\n${list}\nHangi hastaneyi istersiniz?`));
            setStep("hospital");
          }
        }
      } else if (step === "hospital") {
        const match = hospitals.find(h => h.name.toLowerCase() === content.toLowerCase());
        if (!match) {
          addAssistant("Listelediğim hastane adlarından birini aynen yazar mısınız?");
        } else {
          setHospitalId(match.id);
          const deps = await apiJson<DepartmentDto[]>(`/backend/catalog/departments?hospitalId=${match.id}`);
          setDepartments(deps);
          if (deps.length === 0) {
            addAssistant("Bu hastanede bölüm bulunamadı. Başka bir hastane seçiniz.");
          } else {
            const list = deps.map(d => `- ${d.name}`).join("\n");
            addAssistant(await replyWithGemini(`Seçilen hastane: ${match.name}. Mevcut bölümler:\n${list}\nHangi bölümü istersiniz?`));
            setStep("department");
          }
        }
      } else if (step === "department") {
        const match = departments.find(d => d.name.toLowerCase() === content.toLowerCase());
        if (!match) {
          addAssistant("Listelediğim bölüm adlarından birini aynen yazar mısınız?");
        } else {
          setDepartmentId(match.id);
          const docs = await apiJson<DoctorDto[]>(`/backend/catalog/doctors?departmentId=${match.id}`);
          setDoctors(docs);
          if (docs.length === 0) {
            addAssistant("Bu bölümde doktor bulunamadı. Başka bir bölüm seçiniz.");
          } else {
            const list = docs.map(d => `- ${d.name}`).join("\n");
            addAssistant(await replyWithGemini(`Seçilen bölüm: ${match.name}. Uygun doktorlar:\n${list}\nHangi doktoru istersiniz?`));
            setStep("doctor");
          }
        }
      } else if (step === "doctor") {
        const match = doctors.find(d => d.name.toLowerCase() === content.toLowerCase());
        if (!match) {
          addAssistant("Lütfen listeden bir doktor adını olduğu gibi yazın.");
        } else {
          setDoctorId(match.id);
          addAssistant(await replyWithGemini(`Doktor: ${match.name}. Randevu günü için 'yyyy-aa-gg' formatında tarih yazar mısınız?`));
          setStep("date");
        }
      } else if (step === "date") {
        const ok = /^\d{4}-\d{2}-\d{2}$/.test(content);
        if (!ok) {
          addAssistant("Lütfen 'yyyy-aa-gg' formatında tarih yazın (ör. 2026-02-10).");
        } else {
          setDate(content);
          addAssistant(await replyWithGemini("Teşekkürler. Saat için 'SS:dd' (ör. 09:00) yazın."));
          setStep("time");
        }
      } else if (step === "time") {
        const ok = /^\d{2}:\d{2}$/.test(content);
        if (!ok) {
          addAssistant("Lütfen 'SS:dd' formatında saat yazın (ör. 09:00).");
        } else {
          setTime(content);
          const selectedHospital = hospitals.find(h => h.id === hospitalId) ?? null;
          const summary = `E-posta: ${email}\nHastane: ${selectedHospital?.name}\nBölüm: ${selectedDepartment?.name}\nDoktor: ${selectedDoctor?.name}\nTarih: ${date}\nSaat: ${content}`;
          addAssistant(await replyWithGemini(`${summary}\nOnaylıyorsanız 'onayliyorum' yazın.`));
          setStep("finalConfirm");
        }
      } else if (step === "finalConfirm") {
        if (content.toLowerCase() !== "onayliyorum") {
          addAssistant("Onaylamak için 'onayliyorum' yazın veya bilgileri değiştirin.");
        } else {
          const appointmentDate = withTurkeyOffset(date, time);
          try {
            await apiJson("/backend/appointments", { method: "POST", body: JSON.stringify({ doctorId, appointmentDate }) });
            addAssistant("Randevunuz başarıyla oluşturuldu. Geçmiş olsun!");
            setStep("complete");
          } catch (e) {
            const msg = e instanceof Error ? e.message : "Randevu oluşturma başarısız.";
            addAssistant(`Hata: ${msg}`);
          }
        }
      }
    } finally { setBusy(false); }
  }

  return (
    <div className={cn("fixed bottom-6 right-6 z-[60]", className)}>
      {!open ? (
        <button onClick={() => setOpen(true)} className="flex h-14 w-14 items-center justify-center rounded-full brand-gradient text-white soft-shadow hover:soft-shadow-lg active:scale-95" aria-label="Asistanı aç">
          <LogoMark className="h-6 w-6" />
        </button>
      ) : (
        <div className="w-[360px] max-w-[90vw]">
          <Card>
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-blue-600 text-white"><LogoMark className="h-4 w-4"/></div>
                <div>
                  <div className="text-sm font-semibold text-slate-900 dark:text-slate-50">Asistan</div>
                  <div className="text-[11px] text-slate-500 dark:text-slate-400">Randevu Yardımcısı</div>
                </div>
              </div>
              <Button variant="secondary" size="sm" onClick={() => setOpen(false)}>Kapat</Button>
            </div>
            <div className="mt-4 h-[380px] overflow-y-auto space-y-3 pr-1">
              {messages.map(m => (
                <div key={m.id} className={m.role === "assistant" ? "text-slate-800" : "text-blue-700"}>
                  <div className="rounded-2xl border border-slate-200 bg-white p-3 soft-shadow inline-block max-w-[85%] dark:border-slate-700 dark:bg-slate-800/90">
                    <pre className="whitespace-pre-wrap text-sm leading-6">{m.text}</pre>
                  </div>
                </div>
              ))}
              <div ref={listEndRef} />
            </div>
            <div className="mt-3 flex gap-2">
              <Input
                placeholder={busy ? "Bekleyin..." : "Mesajınızı yazın"}
                value={input}
                onChange={e => setInput(e.target.value)}
                onKeyDown={e => { if (e.key === "Enter") onSend(); }}
                disabled={busy}
                className="flex-1"
              />
              <Button onClick={onSend} disabled={busy || !input.trim()}>Gönder</Button>
            </div>
          </Card>
        </div>
      )}
    </div>
  );
}
