# MHRS API (Merkezi Hastane Randevu Sistemi)

Bu proje Merkezi Hastane Randevu Sistemi'nin backend API servislerini içermektedir.

**Geliştirici:** Mahmut Sibal  
**E-posta:** mahmutsibal9@gmail.com

## Teknolojiler

- **.NET 8** (ASP.NET Core Web API)
- **Entity Framework Core**
- **Symmetric Encryption (JWT)**
- **Serilog**
- **FluentValidation**
- **Swagger / OpenAPI**

## Kurulum ve Çalıştırma

1. **Gereksinimler:**
   - .NET SDK (8.0 veya üzeri)
   - SQL Server (veya `appsettings.json` içinde yapılandırılmış veritabanı)

2. **Bağımlılıkları Yükleme:**
   Terminali `WebAppointmentApi` klasöründe açın ve komutu çalıştırın:
   ```bash
   dotnet restore
   ```

3. **Veritabanı Oluşturma (Migration):**
   Veritabanını güncellemek için:
   ```bash
   dotnet ef database update --project WebAppointmentApi.Infrastructure --startup-project WebAppointmentApi.WebApi
   ```

4. **Projeyi Başlatma:**
   ```bash
   dotnet run --project WebAppointmentApi.WebApi
   ```

Proje varsayılan olarak `http://localhost:5000` (veya benzeri bir port) üzerinde çalışacaktır.
API dokümantasyonuna erişmek için tarayıcınızda `/swagger` yoluna gidin (Örn: `http://localhost:5000/swagger`).
