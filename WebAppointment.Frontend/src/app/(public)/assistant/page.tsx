"use client";

import { useContext, useEffect, useMemo, useRef, useState } from "react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { PageHeader } from "@/components/ui/page-header";
import { Input } from "@/components/ui/input";
import { apiJson } from "@/lib/api-client";
import { SessionContext } from "@/components/session/SessionProvider";

 type DepartmentDto = { id: number; name: string };
 type DoctorDto = { id: number; name: string; departmentId: number; departmentName: string; title?: string };

 type Message = { id: string; role: "assistant" | "user"; text: string };
 type Step =
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

export default function AssistantPage() {
  const { session } = useContext(SessionContext);
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState("");
  const [busy, setBusy] = useState(false);
  const [step, setStep] = useState<Step>("email");
  const [input, setInput] = useState<string>("");
  const [password, setPassword] = useState("");
  const [departments, setDepartments] = useState<DepartmentDto[]>([]);
  const [departmentId, setDepartmentId] = useState<number>(0);
  const [doctors, setDoctors] = useState<DoctorDto[]>([]);
  const [doctorId, setDoctorId] = useState<number>(0);
  const [date, setDate] = useState<string>("");
  const [time, setTime] = useState<string>("09:00");

  const listEndRef = useRef<HTMLDivElement>(null);
  const scrollToBottom = () => listEndRef.current?.scrollIntoView({ behavior: "smooth" });
  useEffect(scrollToBottom, [messages]);

  useEffect(() => {
    // İlk karşılama
    setMessages([
      { id: uid(), role: "assistant", text: "Merhaba! E-posta adresinizi yazar mısınız?" },
    ]);
  }, []);

  // Oturum değiştiğinde (login/logout veya kullanıcı switch) sohbeti temizle
  const lastUserRef = useRef<string | null>(null);
  useEffect(() => {
    const currentUser = session?.userId ?? null;
    if (currentUser !== lastUserRef.current) {
      lastUserRef.current = currentUser;
      setMessages([{ id: uid(), role: "assistant", text: "Merhaba! E-posta adresinizi yazar mısınız?" }]);
      setStep("email");
      setEmail("");
      setPassword("");
      setDepartments([]);
      setDepartmentId(0);
      setDoctors([]);
      setDoctorId(0);
      setDate("");
      setTime("09:00");
    }
  }, [session]);

  const selectedDepartment = useMemo(() => departments.find(d => d.id === departmentId) ?? null, [departments, departmentId]);
  const selectedDoctor = useMemo(() => doctors.find(d => d.id === doctorId) ?? null, [doctors, doctorId]);

  async function replyWithGemini(systemPrompt: string) {
    try {
      const res = await fetch("/api/assistant/chat", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ prompt: systemPrompt }),
      });
      if (!res.ok) throw new Error(await res.text());
      const data = await res.json() as { text: string };
      return data.text;
    } catch {
      return systemPrompt; // Yedek olarak düz metni dön
    }
  }

  function addAssistant(text: string) {
    setMessages(m => [...m, { id: uid(), role: "assistant", text }]);
  }
  function addUser(text: string) {
    setMessages(m => [...m, { id: uid(), role: "user", text }]);
  }

  async function onSend() {
    const content = input.trim();
    if (!content || busy) return;
    addUser(content);
    setInput("");

    setBusy(true);
    try {
      if (step === "email") {
        const ok = /.+@.+\..+/.test(content);
        if (!ok) {
          addAssistant("Geçerli bir e-posta adresi girer misiniz?");
        } else {
          setEmail(content);
          const text = await replyWithGemini("Teşekkürler. Şifrenizi yazar mısınız?");
          addAssistant(text);
          setStep("password");
        }
      } else if (step === "password") {
        setPassword(content);
        const text = await replyWithGemini(`E-posta: ${email}. Şifreyi aldım. Giriş yapmamı onaylıyorsanız 'onayliyorum' yazın.`);
        addAssistant(text);
        setStep("confirmLogin");
      } else if (step === "confirmLogin") {
        if (content.toLowerCase() !== "onayliyorum") {
          addAssistant("Girişe devam etmek için lütfen 'onayliyorum' yazın.");
        } else {
          // Login
          const res = await fetch("/api/session/login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email, password }),
          });
          if (!res.ok) {
            const t = await res.text();
            addAssistant(`Giriş başarısız: ${t || "Hata"}. Lütfen tekrar deneyin, e-postanızı yazın.`);
            setStep("email");
          } else {
            // Bölümler
            const deps = await apiJson<DepartmentDto[]>("/backend/catalog/departments");
            setDepartments(deps);
            const list = deps.map(d => `- ${d.name}`).join("\n");
            const text = await replyWithGemini(`Giriş başarılı. Mevcut bölümler:\n${list}\nHangi bölümü istersiniz? Lütfen bölüm adını yazın.`);
            addAssistant(text);
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
            const list = docs.map(d => `- ${d.name}${d.title ? ` – ${d.title}` : ""}`).join("\n");
            const text = await replyWithGemini(`Seçilen bölüm: ${match.name}. Uygun doktorlar:\n${list}\nHangi doktoru istersiniz? Lütfen doktor adını yazın.`);
            addAssistant(text);
            setStep("doctor");
          }
        }
      } else if (step === "doctor") {
        const match = doctors.find(d => d.name.toLowerCase() === content.toLowerCase());
        if (!match) {
          addAssistant("Lütfen listeden bir doktor adını olduğu gibi yazın.");
        } else {
          setDoctorId(match.id);
          const text = await replyWithGemini(`Doktor: ${match.name}. Randevu günü için 'yyyy-aa-gg' formatında tarih yazar mısınız?`);
          addAssistant(text);
          setStep("date");
        }
      } else if (step === "date") {
        const ok = /^\d{4}-\d{2}-\d{2}$/.test(content);
        if (!ok) {
          addAssistant("Lütfen 'yyyy-aa-gg' (ör. 2026-02-10) formatında tarih yazın.");
        } else {
          setDate(content);
          const text = await replyWithGemini("Teşekkürler. Saat için 'SS:dd' (ör. 09:00 veya 13:30) yazın.");
          addAssistant(text);
          setStep("time");
        }
      } else if (step === "time") {
        const ok = /^\d{2}:\d{2}$/.test(content);
        if (!ok) {
          addAssistant("Lütfen 'SS:dd' formatında saat yazın (ör. 09:00).");
        } else {
          setTime(content);
          const summary = `E-posta: ${email}\nBölüm: ${selectedDepartment?.name}\nDoktor: ${selectedDoctor?.name}\nTarih: ${date}\nSaat: ${content}`;
          const text = await replyWithGemini(`${summary}\nYukarıdaki bilgilerle randevu oluşturacağım. Onaylıyorsanız 'onayliyorum' yazın.`);
          addAssistant(text);
          setStep("finalConfirm");
        }
      } else if (step === "finalConfirm") {
        if (content.toLowerCase() !== "onayliyorum") {
          addAssistant("Onaylamak için 'onayliyorum' yazın veya bilgileri değiştirin.");
        } else {
          const appointmentDate = withTurkeyOffset(date, time);
          try {
            await apiJson("/backend/appointments", {
              method: "POST",
              body: JSON.stringify({ doctorId, appointmentDate }),
            });
            addAssistant("Randevunuz başarıyla oluşturuldu. Geçmiş olsun!");
            setStep("complete");
          } catch (e) {
            const msg = e instanceof Error ? e.message : "Randevu oluşturma başarısız.";
            addAssistant(`Hata: ${msg}`);
          }
        }
      }
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="grid gap-6">
      <PageHeader title="Asistan ile Randevu" subtitle="Giriş ve seçimleri sohbetle tamamlayın." />
      <Card>
        <div className="h-[60vh] overflow-y-auto space-y-3 pr-1">
      <PageHeader title="Asistan" subtitle="Randevu sohbeti ve Hızlı Tanı." />
      <div className="grid md:grid-cols-2 gap-6">
        <div>
          {/* Hızlı Tanı butonu ve anketi */}
          <Card className="mb-4 p-4">
            <div className="flex items-center justify-between">
              <div>
                <h3 className="font-semibold">Hızlı Tanı</h3>
                <p className="text-sm text-slate-600 dark:text-slate-300">Belirtilerinize göre bölüm önerisi alın.</p>
              </div>
              <Button onClick={() => setMessages(m => [...m, { id: uid(), role: "assistant", text: "Hızlı Tanı başlatıldı. Aşağıdaki anketi doldurun." }])}>Başlat</Button>
            </div>
          </Card>
          {/* Quick Diagnosis form */}
          {/* eslint-disable-next-line @typescript-eslint/no-var-requires */}
          {require("./QuickDiagnosisClient").default && require("./QuickDiagnosisClient").default()}
        </div>
        <div>
          <Card>
            <div className="h-[60vh] overflow-y-auto space-y-3 pr-1">
              {messages.map(m => (
                <div
                  key={m.id}
                  className={
                    m.role === "assistant"
                      ? "text-slate-800 dark:text-slate-100"
                      : "text-blue-700 dark:text-blue-300"
                  }
                >
                  <div className="rounded-2xl border border-slate-200 bg-white p-3 soft-shadow inline-block max-w-[80%] dark:border-slate-700 dark:bg-slate-800/90">
                    <pre className="whitespace-pre-wrap text-sm leading-6">{m.text}</pre>
                  </div>
                </div>
              ))}
              <div ref={listEndRef} />
            </div>
            <div className="mt-4 flex gap-2">
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
      </div>
            <div
              key={m.id}
              className={
                m.role === "assistant"
                  ? "text-slate-800 dark:text-slate-100"
                  : "text-blue-700 dark:text-blue-300"
              }
            >
              <div className="rounded-2xl border border-slate-200 bg-white p-3 soft-shadow inline-block max-w-[80%] dark:border-slate-700 dark:bg-slate-800/90">
                <pre className="whitespace-pre-wrap text-sm leading-6">{m.text}</pre>
              </div>
            </div>
          ))}
          <div ref={listEndRef} />
        </div>
        <div className="mt-4 flex gap-2">
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
  );
}
