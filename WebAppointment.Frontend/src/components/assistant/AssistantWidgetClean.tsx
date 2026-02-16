"use client";

import { useContext, useEffect, useMemo, useRef, useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card } from "@/components/ui/card";
import { cn } from "@/lib/cn";
import { LogoMark } from "@/components/ui/logo";
import { apiJson } from "@/lib/api-client";
import { SessionContext } from "@/components/session/SessionProvider";

type Message = { id: string; role: "assistant" | "user"; text: string };
type Intent = "none" | "register" | "appointment" | "diagnosis";
type DepartmentDto = { id: number; name: string };
type DoctorDto = { id: number; name: string; departmentId: number; departmentName: string };
type HospitalDto = { id: number; name: string };
type DoctorDailySlotPublicDto = {
  startTime: string; // HH:mm
  endTime: string; // HH:mm
  isAvailable: boolean;
  unavailableReason?: string | null;
};
type Step =
  | "start"
  | "complete"
  | "regFirstName" | "regLastName" | "regTC" | "regPhone" | "regEmail" | "regPassword" | "regConfirm"
  | "email" | "password" | "confirmLogin" | "hospital" | "department" | "doctor" | "date" | "time" | "finalConfirm"
  | "diagSymptoms" | "diagDuration" | "diagSeverity" | "diagFever" | "diagPain" | "diagChronic" | "diagMeds" | "diagPregnant" | "diagRun";

function uid() { return Math.random().toString(36).slice(2); }

export function AssistantWidget({ className }: { className?: string }) {
  const { session } = useContext(SessionContext);
  const [open, setOpen] = useState(false);
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState("");
  const [busy, setBusy] = useState(false);
  const [intent, setIntent] = useState<Intent>("none");
  const [step, setStep] = useState<Step>("start");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [tcKimlikNo, setTcKimlikNo] = useState("");
  const [phone, setPhone] = useState("");
  const [departments, setDepartments] = useState<DepartmentDto[]>([]);
  const [departmentId, setDepartmentId] = useState<number>(0);
  const [doctors, setDoctors] = useState<DoctorDto[]>([]);
  const [doctorId, setDoctorId] = useState<number>(0);
  const [hospitals, setHospitals] = useState<HospitalDto[]>([]);
  const [hospitalId, setHospitalId] = useState<number>(0);
  const [date, setDate] = useState<string>("");
  const [time, setTime] = useState<string>("09:00");
  const [dailySlots, setDailySlots] = useState<DoctorDailySlotPublicDto[]>([]);
  const [answers, setAnswers] = useState<{
    symptoms: string[];
    duration?: string;
    severity?: "Hafif" | "Orta" | "Şiddetli";
    fever?: boolean;
    painLocation?: string;
    chronic?: string[];
    meds?: string[];
    pregnant?: boolean;
  }>({ symptoms: [], chronic: [], meds: [] });

  const selectedDepartment = useMemo(() => departments.find(d => d.id === departmentId) ?? null, [departments, departmentId]);
  const selectedDoctor = useMemo(() => doctors.find(d => d.id === doctorId) ?? null, [doctors, doctorId]);

  const listEndRef = useRef<HTMLDivElement>(null);
  const scrollToBottom = () => listEndRef.current?.scrollIntoView({ behavior: "smooth" });
  useEffect(scrollToBottom, [messages, open]);

  useEffect(() => {
    if (open && messages.length === 0) {
      setMessages([{ id: uid(), role: "assistant", text: "Merhaba! Lütfen bir işlem seçin: 'Hasta Kaydı' veya 'Randevu Alma'." }]);
      setIntent("none");
      setStep("start");
    }
  }, [open, messages.length]);

  // Reset chat on session changes (login/logout or switch accounts)
  useEffect(() => {
    setMessages([]);
    setIntent("none");
    setStep("start");
    setAnswers({ symptoms: [], chronic: [], meds: [] });
  }, [session?.userId]);

  // Hızlı Tanı: 'Şimdi değerlendirme yapıyorum' sonrası otomatik çalıştır
  useEffect(() => {
    async function runDiagnosis() {
      try {
        const res = await fetch("/api/assistant/diagnosis", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ answers }) });
        const text = await res.text();
        if (!res.ok) throw new Error(text);
        addAssistant(text);

        // Bölüm adını metinden sezgisel olarak çıkarıp doktorları listelemeyi deneyelim
        try {
          const deptMatch = text.match(/en uygun bölüm\s+(.+?)\s+olarak/i);
          const deptName = deptMatch?.[1]?.trim();
          if (deptName) {
            const deps = await apiJson<DepartmentDto[]>("/backend/catalog/departments");
            const match = deps.find(d => d.name.toLowerCase() === deptName.toLowerCase());
            if (match) {
              const docs = await apiJson<DoctorDto[]>(`/backend/catalog/doctors?departmentId=${match.id}`);
              if (docs.length) {
                const list = docs.map(d => `- ${d.name}`).join("\n");
                addAssistant(`Uygun doktorlar:\n${list}\nRandevu almak için 'Randevu Alma' tuşunu kullanın.`);
              }
            }
          }
        } catch {}
      } catch (e) {
        const msg = e instanceof Error ? e.message : "Değerlendirme başarısız.";
        addAssistant(`Hata: ${msg}`);
      } finally {
        setIntent("none");
        setStep("start");
      }
    }
    if (intent === "diagnosis" && step === "diagRun") {
      void runDiagnosis();
    }
  }, [intent, step, answers]);

  function addAssistant(text: string) { setMessages(m => [...m, { id: uid(), role: "assistant", text }]); }
  function addUser(text: string) { setMessages(m => [...m, { id: uid(), role: "user", text }]); }

  function availableTimesText(slots: DoctorDailySlotPublicDto[]) {
    const times = slots.filter(s => s.isAvailable).map(s => s.startTime);
    if (times.length === 0) return "";
    return times.map(t => `- ${t}`).join("\n");
  }

  async function loadDoctorDailySlots(doctorId: number, ymd: string) {
    const data = await apiJson<DoctorDailySlotPublicDto[]>(
      `/backend/catalog/doctors/${doctorId}/daily-slots?date=${encodeURIComponent(ymd)}`
    );
    setDailySlots(data);
    return data;
  }

  async function onSend() {
    const content = input.trim();
    if (!content || busy) return;
    setBusy(true);
    setInput("");
    addUser(content);

    try {
    // Kayıt akışı
    if (intent === "register") {
      if (step === "regFirstName") {
        if (!content) addAssistant("Lütfen adınızı girin.");
        else { setFirstName(content); addAssistant("Teşekkürler. Soyadınızı girin."); setStep("regLastName"); }
        return;
      }
      if (step === "regLastName") {
        if (!content) addAssistant("Lütfen soyadınızı girin.");
        else { setLastName(content); addAssistant("TC Kimlik Numaranızı girin (11 haneli). "); setStep("regTC"); }
        return;
      }
      if (step === "regTC") {
        const ok = /^\d{11}$/.test(content);
        if (!ok) addAssistant("Lütfen 11 haneli TC Kimlik No girin.");
        else { setTcKimlikNo(content); addAssistant("Telefon numaranızı girin (örn. 5551234567). "); setStep("regPhone"); }
        return;
      }
      if (step === "regPhone") {
        const ok = /^\d{10,11}$/.test(content);
        if (!ok) addAssistant("Lütfen geçerli bir telefon numarası girin.");
        else { setPhone(content); addAssistant("E-posta adresinizi girin."); setStep("regEmail"); }
        return;
      }
      if (step === "regEmail") {
        const ok = /.+@.+\..+/.test(content);
        if (!ok) addAssistant("Geçerli bir e-posta adresi girin.");
        else { setEmail(content); addAssistant("Şifrenizi girin (en az 6 karakter)."); setStep("regPassword"); }
        return;
      }
      if (step === "regPassword") {
        if (content.length < 6) addAssistant("Lütfen en az 6 karakterli bir şifre girin.");
        else {
          setPassword(content);
          const summary = `Ad: ${firstName}\nSoyad: ${lastName}\nTC: ${tcKimlikNo}\nTelefon: ${phone}\nE-posta: ${email}`;
          addAssistant(`${summary}\nKayıt işlemini onaylıyorsanız 'onayliyorum' yazın.`);
          setStep("regConfirm");
        }
        return;
      }
      if (step === "regConfirm") {
        if (content.toLowerCase() !== "onayliyorum") addAssistant("Onaylamak için 'onayliyorum' yazın veya bilgileri değiştirin.");
        else {
          try {
            await apiJson("/api/session/register", {
              method: "POST",
              body: JSON.stringify({ email, password, tcKimlikNo, firstName, lastName, phone }),
            });
            addAssistant("Kayıt başarıyla tamamlandı. Hoş geldiniz!");
            setStep("complete");
          } catch (e) {
            const msg = e instanceof Error ? e.message : "Kayıt başarısız.";
            addAssistant(msg);
          }
        }
        return;
      }
      // Intent seçildi ama adım yoksa başlangıç
      addAssistant("Lütfen adınızı yazın."); setStep("regFirstName"); return;
    }

    // Randevu akışı
    if (intent === "appointment") {
      if (step === "email") {
        const ok = /.+@.+\..+/.test(content);
        if (!ok) addAssistant("Geçerli bir e-posta adresi girin.");
        else { setEmail(content); addAssistant("Teşekkürler. Şifrenizi girin."); setStep("password"); }
        return;
      }
      if (step === "password") {
        setPassword(content);
        addAssistant(`E-posta: ${email}. Giriş yapmamı onaylıyorsanız 'onayliyorum' yazın.`);
        setStep("confirmLogin"); return;
      }
      if (step === "confirmLogin") {
        if (content.toLowerCase() !== "onayliyorum") addAssistant("Girişe devam etmek için lütfen 'onayliyorum' yazın.");
        else {
          try {
            const res = await fetch("/api/session/login", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ email, password }) });
            if (!res.ok) {
              addAssistant("Giriş başarısız. Lütfen e-postayı tekrar yazın.");
              setStep("email");
            } else {
              const hosps = await apiJson<HospitalDto[]>("/backend/catalog/hospitals");
              setHospitals(hosps);
              const list = hosps.map(h => `- ${h.name}`).join("\n");
              addAssistant(`Giriş başarılı. Mevcut hastaneler:\n${list}\nHangi hastaneyi istersiniz?`);
              setStep("hospital");
            }
          } catch (e) {
            const msg = e instanceof Error ? e.message : "Giriş/hastane listesi alınamadı.";
            addAssistant(`Hata: ${msg}`);
            addAssistant("Lütfen tekrar e-postanızı yazın.");
            setStep("email");
          }
        }
        return;
      }
      if (step === "hospital") {
        const match = hospitals.find(h => h.name.toLowerCase() === content.toLowerCase());
        if (!match) addAssistant("Listelediğim hastane adlarından birini aynen yazın.");
        else {
          setHospitalId(match.id);
          try {
            const deps = await apiJson<DepartmentDto[]>(`/backend/catalog/departments?hospitalId=${match.id}`);
            setDepartments(deps);
            if (deps.length === 0) addAssistant("Bu hastanede bölüm bulunamadı. Başka bir hastane seçiniz.");
            else {
              const list = deps.map(d => `- ${d.name}`).join("\n");
              addAssistant(`Seçilen hastane: ${match.name}. Mevcut bölümler:\n${list}\nHangi bölümü istersiniz?`);
              setStep("department");
            }
          } catch (e) {
            const msg = e instanceof Error ? e.message : "Bölümler getirilemedi.";
            addAssistant(`Hata: ${msg}`);
            addAssistant("Lütfen tekrar hastane seçin.");
            setStep("hospital");
          }
        }
        return;
      }
      if (step === "department") {
        const match = departments.find(d => d.name.toLowerCase() === content.toLowerCase());
        if (!match) addAssistant("Listelediğim bölüm adlarından birini aynen yazın.");
        else {
          setDepartmentId(match.id);
          try {
            const docs = await apiJson<DoctorDto[]>(`/backend/catalog/doctors?departmentId=${match.id}`);
            setDoctors(docs);
            if (docs.length === 0) addAssistant("Bu bölümde doktor bulunamadı. Başka bir bölüm seçiniz.");
            else {
              const list = docs.map(d => `- ${d.name}`).join("\n");
              addAssistant(`Seçilen bölüm: ${match.name}. Uygun doktorlar:\n${list}\nHangi doktoru istersiniz?`);
              setStep("doctor");
            }
          } catch (e) {
            const msg = e instanceof Error ? e.message : "Doktorlar getirilemedi.";
            addAssistant(`Hata: ${msg}`);
            addAssistant("Lütfen tekrar bölüm seçin.");
            setStep("department");
          }
        }
        return;
      }
      if (step === "doctor") {
        const match = doctors.find(d => d.name.toLowerCase() === content.toLowerCase());
        if (!match) addAssistant("Lütfen listeden bir doktor adını olduğu gibi yazın.");
        else {
          setDoctorId(match.id);
          setDailySlots([]);
          setDate("");
          setTime("09:00");
          addAssistant(`Doktor: ${match.name}. Tarih 'yyyy-aa-gg' formatında yazın (ör. 2026-02-10).`);
          setStep("date");
        }
        return;
      }
      if (step === "date") {
        const ok = /^\d{4}-\d{2}-\d{2}$/.test(content);
        if (!ok) {
          addAssistant("Lütfen 'yyyy-aa-gg' formatında tarih yazın (ör. 2026-02-10).");
          return;
        }

        const day = new Date(`${content}T00:00:00`).getDay();
        if (day === 0 || day === 6) {
          addAssistant("Hafta sonu randevu alınamaz. Lütfen hafta içi bir tarih yazın.");
          return;
        }

        setDate(content);
        try {
          const slots = await loadDoctorDailySlots(doctorId, content);
          const availableText = availableTimesText(slots);
          if (!availableText) {
            addAssistant("Bu tarih için uygun randevu saati bulunmuyor. Lütfen başka bir tarih yazın.");
            setStep("date");
            return;
          }
          addAssistant(`Bu tarih için uygun saatler:\n${availableText}\nLütfen saat seçin (HH:mm).`);
          setStep("time");
        } catch (e) {
          const msg = e instanceof Error ? e.message : "Uygun saatler getirilemedi.";
          addAssistant(`Hata: ${msg}`);
          addAssistant("Lütfen başka bir tarih deneyin.");
          setStep("date");
        }
        return;
      }
      if (step === "time") {
        const ok = /^\d{2}:\d{2}$/.test(content);
        if (!ok) {
          addAssistant("Lütfen 'HH:mm' formatında saat yazın (ör. 09:00).");
          return;
        }

        const normalized = content;
        const isAvailable = dailySlots.some(s => s.startTime === normalized && s.isAvailable);
        if (!isAvailable) {
          const availableText = availableTimesText(dailySlots);
          if (availableText) {
            addAssistant(`Bu saat uygun değil. Lütfen listeden bir saat seçin:\n${availableText}`);
          } else {
            addAssistant("Bu tarih için uygun saat bulunmuyor. Lütfen başka bir tarih yazın.");
            setStep("date");
          }
          return;
        }

        setTime(normalized);
        const selectedHospital = hospitals.find(h => h.id === hospitalId) ?? null;
        const summary = `E-posta: ${email}\nHastane: ${selectedHospital?.name}\nBölüm: ${selectedDepartment?.name}\nDoktor: ${selectedDoctor?.name}\nTarih: ${date}\nSaat: ${normalized}`;
        addAssistant(`${summary}\nOnaylıyorsanız 'onayliyorum' yazın.`);
        setStep("finalConfirm");
        return;
      }
      if (step === "finalConfirm") {
        if (content.toLowerCase() !== "onayliyorum") addAssistant("Onaylamak için 'onayliyorum' yazın veya bilgileri değiştirin.");
        else {
          try {
            const appointmentDate = `${date}T${time}:00+03:00`;
            const res = await apiJson("/backend/appointments", { method: "POST", body: JSON.stringify({ doctorId, appointmentDate }) });
            void res; // apiJson returns any
            addAssistant("Randevunuz başarıyla oluşturuldu. Geçmiş olsun!");
            setStep("complete");
          } catch (e) {
            const msg = e instanceof Error ? e.message : "Randevu oluşturma başarısız.";
            addAssistant(`Hata: ${msg}`);

            // Hata sonrası kilitlenmeyi önlemek için kullanıcıyı tekrar seçime yönlendir.
            // Slotlar değişmiş olabilir: tekrar yükleyip saat seçimini tekrar iste.
            try {
              const slots = await loadDoctorDailySlots(doctorId, date);
              const availableText = availableTimesText(slots);
              if (availableText) {
                addAssistant(`Lütfen başka bir saat seçin:\n${availableText}`);
                setStep("time");
              } else {
                addAssistant("Bu tarih için uygun saat kalmamış. Lütfen başka bir tarih yazın.");
                setStep("date");
              }
            } catch {
              addAssistant("Lütfen saat seçimini tekrar yapın (HH:mm)." );
              setStep("time");
            }
          }
        }
        return;
      }
      // Intent seçildi ama adım yoksa başlangıç
      addAssistant("Lütfen e-postanızı yazın."); setStep("email"); return;
    }

    // Hızlı Tanı akışı
    if (intent === "diagnosis") {
      if (step === "diagSymptoms") {
        const list = content.split(",").map(x => x.trim()).filter(Boolean);
        if (list.length === 0) { addAssistant("Belirtilerinizi virgülle yazar mısınız? Ör. baş ağrısı, ateş."); return; }
        setAnswers(a => ({ ...a, symptoms: list }));
        addAssistant("Belirtiler ne zamandır mevcut? Ör. 3 gündür, 2 haftadır.");
        setStep("diagDuration");
        return;
      }
      if (step === "diagDuration") {
        setAnswers(a => ({ ...a, duration: content }));
        addAssistant("Şiddetini seçin: 'Hafif', 'Orta' veya 'Şiddetli'.");
        setStep("diagSeverity");
        return;
      }
      if (step === "diagSeverity") {
        const v = content.trim();
        if (!["Hafif","Orta","Şiddetli"].includes(v)) { addAssistant("Lütfen 'Hafif', 'Orta' veya 'Şiddetli' yazın."); return; }
        setAnswers(a => ({ ...a, severity: v as any }));
        addAssistant("Ateş var mı? 'var' veya 'yok' yazın.");
        setStep("diagFever");
        return;
      }
      if (step === "diagFever") {
        const v = content.toLowerCase();
        if (!["var","yok"].includes(v)) { addAssistant("Lütfen 'var' veya 'yok' yazın."); return; }
        setAnswers(a => ({ ...a, fever: v === "var" }));
        addAssistant("Ağrı yeri (ör. göğüs, karın, baş, bel) yazın veya 'yok'.");
        setStep("diagPain");
        return;
      }
      if (step === "diagPain") {
        setAnswers(a => ({ ...a, painLocation: content.toLowerCase() === "yok" ? undefined : content }));
        addAssistant("Kronik hastalıklarınızı virgülle yazın veya 'yok'.");
        setStep("diagChronic");
        return;
      }
      if (step === "diagChronic") {
        const list = content.toLowerCase() === "yok" ? [] : content.split(",").map(x => x.trim()).filter(Boolean);
        setAnswers(a => ({ ...a, chronic: list }));
        addAssistant("Kullandığınız ilaçları virgülle yazın veya 'yok'.");
        setStep("diagMeds");
        return;
      }
      if (step === "diagMeds") {
        const list = content.toLowerCase() === "yok" ? [] : content.split(",").map(x => x.trim()).filter(Boolean);
        setAnswers(a => ({ ...a, meds: list }));
        addAssistant("Hamilelik durumu? 'evet' veya 'hayır'.");
        setStep("diagPregnant");
        return;
      }
      if (step === "diagPregnant") {
        const v = content.toLowerCase();
        if (!["evet","hayır"].includes(v)) { addAssistant("Lütfen 'evet' veya 'hayır' yazın."); return; }
        setAnswers(a => ({ ...a, pregnant: v === "evet" }));
        addAssistant("Teşekkürler. Şimdi değerlendirme yapıyorum...");
        setStep("diagRun"); // Otomatik olarak useEffect ile çalışacak
        return;
      }
      // Intent seçildi ama adım yoksa başlangıç
      addAssistant("Belirtilerinizi virgülle yazın (ör. baş ağrısı, ateş)."); setStep("diagSymptoms"); return;
    }

    // Henüz intent seçilmemişse basit yönlendirme
    addAssistant("Lütfen önce bir işlem seçin: Hasta Kaydı veya Randevu Alma.");
    } catch (e) {
      const msg = e instanceof Error ? e.message : "Beklenmeyen bir hata oluştu.";
      addAssistant(`Hata: ${msg}`);
    } finally {
      setBusy(false);
    }
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
                <div key={m.id} className={m.role === "assistant" ? "text-slate-800 dark:text-slate-100" : "text-blue-700 dark:text-blue-300"}>
                  <div className="rounded-2xl border border-slate-200 bg-white p-3 soft-shadow inline-block max-w-[85%] dark:border-slate-700 dark:bg-slate-800/90">
                    <pre className="whitespace-pre-wrap text-sm leading-6">{m.text}</pre>
                  </div>
                </div>
              ))}
              <div ref={listEndRef} />
            </div>
            <div className="mt-2 flex flex-wrap gap-2">
              <Button
                variant={intent === "register" ? "primary" : "outline"}
                size="sm"
                onClick={() => {
                  setIntent("register");
                  setStep("regFirstName");
                  addAssistant("Hasta Kaydı seçildi. Lütfen adınızı girin.");
                }}
              >
                Hasta Kaydı
              </Button>
              <Button
                variant={intent === "appointment" ? "primary" : "outline"}
                size="sm"
                onClick={() => {
                  setIntent("appointment");
                  setStep("email");
                  addAssistant("Randevu Alma seçildi. Lütfen e-posta adresinizi girin.");
                }}
              >
                Randevu Alma
              </Button>
              <Button
                variant={intent === "diagnosis" ? "primary" : "outline"}
                size="sm"
                onClick={() => {
                  setIntent("diagnosis");
                  setStep("diagSymptoms");
                  addAssistant("Hızlı Tanı seçildi. Belirtilerinizi virgülle yazın (ör. baş ağrısı, ateş).");
                }}
              >
                Hızlı Tanı
              </Button>
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
