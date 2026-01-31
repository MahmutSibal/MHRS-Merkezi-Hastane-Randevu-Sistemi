# MHRS (Merkezi Hastane Randevu Sistemi)

Bu proje, Merkezi Hastane Randevu Sistemi'nin tam kapsamlı bir uygulamasıdır. Backend ve Frontend olmak üzere iki ana bölümden oluşur.

**Geliştirici:** Mahmut Sibal  
**E-posta:** mahmutsibal9@gmail.com

## Proje Yapısı

### 1. [Backend (API)](./WebAppointmentApi/README.md)
- **Konum:** `WebAppointmentApi`
- **Teknoloji:** .NET 8, Web API
- Veritabanı işlemleri, kimlik doğrulama (Auth) ve iş mantığı burada çalışır.

### 2. [Frontend (Arayüz)](./WebAppointment.Frontend/README.md)
- **Konum:** `WebAppointment.Frontend`
- **Teknoloji:** Next.js 16, React, Tailwind CSS
- Kullanıcıların randevu alabildiği ve doktorların işlem yapabildiği web arayüzüdür.

## Hızlı Başlangıç

Projeyi ayağa kaldırmak için her iki projeyi de ayrı terminallerde başlatmanız gerekir.

1. **Backend'i Başlatın:**
   ```bash
   cd WebAppointmentApi
   dotnet run --project WebAppointmentApi.WebApi
   ```

2. **Frontend'i Başlatın:**
   ```bash
   cd WebAppointment.Frontend
   npm run dev
   ```

Her iki servis çalıştığında, tarayıcınızdan Frontend adresine (genellikle `http://localhost:3000`) giderek uygulamayı kullanabilirsiniz.
