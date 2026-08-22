using System.Data.Common;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebAppointmentApi.Application.Appointments.Abstractions;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Patients.Services;

namespace WebAppointmentApi.Infrastructure.BackgroundJobs;

/// <summary>
/// Runs every 15 minutes: sends a personalized WhatsApp reminder ~24 hours out, a second
/// same-day reminder as the appointment nears its 3-hour-out cutoff, and sweeps unconfirmed
/// appointments at that cutoff — auto-cancelling the high-risk ones to free the slot for the
/// waitlist, and leaving the rest alone for staff to judge.
/// </summary>
public sealed class AppointmentReminderService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ReminderWindow = TimeSpan.FromHours(24);
    private static readonly TimeSpan ConfirmationCutoff = TimeSpan.FromHours(3);
    private static readonly CultureInfo TrCulture = CultureInfo.GetCultureInfo("tr-TR");

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AppointmentReminderService> _logger;

    public AppointmentReminderService(IServiceScopeFactory scopeFactory, ILogger<AppointmentReminderService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Appointment reminder service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendRemindersAsync(stoppingToken);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Appointment reminder cycle failed.");
            }

            try
            {
                await SendSecondRemindersAsync(stoppingToken);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Second reminder cycle failed.");
            }

            try
            {
                await SweepUnconfirmedAsync(stoppingToken);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unconfirmed-reminder sweep failed.");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException) { break; }
        }
    }

    private async Task SendRemindersAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var appointments = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
        var patients = scope.ServiceProvider.GetRequiredService<IPatientRepository>();
        var whatsapp = scope.ServiceProvider.GetRequiredService<IWhatsAppMessageSender>();
        var clock = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        var nowUtc = clock.UtcNow;
        var fromUtc = nowUtc;
        var toUtc = nowUtc.Add(ReminderWindow);

        IReadOnlyList<WebAppointmentApi.Domain.Entities.Appointment> upcoming;

        try
        {
            upcoming = await appointments.ListUpcomingUnremindedAsync(fromUtc, toUtc, ct);
        }
        catch (DbException ex)
        {
            _logger.LogWarning(ex, "Appointment reminder cycle skipped because the database is unavailable.");
            return;
        }

        if (upcoming.Count == 0) return;

        _logger.LogInformation("Found {Count} appointments to remind.", upcoming.Count);

        foreach (var appt in upcoming)
        {
            try
            {
                var patient = await patients.FindByUserIdAsync(appt.UserId, ct);
                if (patient is null || string.IsNullOrWhiteSpace(patient.Phone))
                {
                    // Still mark as sent to avoid retrying endlessly
                    await appointments.MarkReminderSentAsync(appt.Id, nowUtc, ct);
                    continue;
                }

                var turkeyTz = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
                var localTime = TimeZoneInfo.ConvertTime(appt.StartAt, turkeyTz);
                var dateStr = localTime.ToString("dd MMMM yyyy HH:mm", TrCulture);

                var doctorName = appt.Doctor?.Name ?? "Doktor";
                var deptName = appt.Doctor?.Department?.Name ?? "Bölüm";
                var hospitalName = appt.Doctor?.Department?.Hospital?.Name ?? "Hastane";

                var greeting = string.IsNullOrWhiteSpace(patient.FirstName)
                    ? "Merhaba,"
                    : string.Format(TrCulture, "Sayin {0},", patient.FirstName);

                var message = string.Format(TrCulture,
                    "{0} hatirlatma: Yarin {1} tarihinde {2} hastanesinde {3} bolumu {4} doktorundan randevunuz bulunmaktadir. " +
                    "Katilacaksaniz 1, gelemeyecekseniz 2 yazarak bize bildirebilirsiniz.",
                    greeting, dateStr, hospitalName, deptName, doctorName);

                await whatsapp.SendMessageAsync(patient.Phone, message, ct);
                await appointments.MarkReminderSentAsync(appt.Id, nowUtc, ct);

                _logger.LogInformation("Reminder sent for appointment {AppointmentId} to {Phone}.", appt.Id, patient.Phone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reminder for appointment {AppointmentId}.", appt.Id);
                // Don't mark as sent — retry next cycle
            }
        }
    }

    private async Task SendSecondRemindersAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var appointments = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
        var patients = scope.ServiceProvider.GetRequiredService<IPatientRepository>();
        var whatsapp = scope.ServiceProvider.GetRequiredService<IWhatsAppMessageSender>();
        var clock = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        var nowUtc = clock.UtcNow;
        var toUtc = nowUtc.Add(ConfirmationCutoff);

        IReadOnlyList<WebAppointmentApi.Domain.Entities.Appointment> due;

        try
        {
            due = await appointments.ListUpcomingUnremindedSecondAsync(nowUtc, toUtc, ct);
        }
        catch (DbException ex)
        {
            _logger.LogWarning(ex, "Second reminder cycle skipped because the database is unavailable.");
            return;
        }

        if (due.Count == 0) return;

        foreach (var appt in due)
        {
            try
            {
                var patient = await patients.FindByUserIdAsync(appt.UserId, ct);
                if (patient is null || string.IsNullOrWhiteSpace(patient.Phone))
                {
                    await appointments.MarkSecondReminderSentAsync(appt.Id, nowUtc, ct);
                    continue;
                }

                var turkeyTz = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
                var localTime = TimeZoneInfo.ConvertTime(appt.StartAt, turkeyTz);
                var timeStr = localTime.ToString("HH:mm", TrCulture);

                var doctorName = appt.Doctor?.Name ?? "Doktor";
                var greeting = string.IsNullOrWhiteSpace(patient.FirstName)
                    ? "Merhaba,"
                    : string.Format(TrCulture, "Sayin {0},", patient.FirstName);

                var message = appt.ReminderConfirmedAtUtc is not null
                    ? string.Format(TrCulture,
                        "{0} bugun saat {1} icin {2} doktorundan randevunuz yaklasiyor. Sizi bekliyoruz.",
                        greeting, timeStr, doctorName)
                    : string.Format(TrCulture,
                        "{0} bugun saat {1} icin {2} doktorundan randevunuz var ve henuz onaylamadiniz. " +
                        "Gelecekseniz 1, gelemeyecekseniz 2 yazarak son bir kez bildirebilirsiniz.",
                        greeting, timeStr, doctorName);

                await whatsapp.SendMessageAsync(patient.Phone, message, ct);
                await appointments.MarkSecondReminderSentAsync(appt.Id, nowUtc, ct);

                _logger.LogInformation("Second reminder sent for appointment {AppointmentId} to {Phone}.", appt.Id, patient.Phone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send second reminder for appointment {AppointmentId}.", appt.Id);
            }
        }
    }

    private async Task SweepUnconfirmedAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var appointments = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
        var patients = scope.ServiceProvider.GetRequiredService<IPatientRepository>();
        var appointmentService = scope.ServiceProvider.GetRequiredService<IAppointmentService>();
        var clock = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        var nowUtc = clock.UtcNow;
        var toUtc = nowUtc.Add(ConfirmationCutoff);

        IReadOnlyList<WebAppointmentApi.Domain.Entities.Appointment> nearing;

        try
        {
            nearing = await appointments.ListUnconfirmedNearingAsync(nowUtc, toUtc, ct);
        }
        catch (DbException ex)
        {
            _logger.LogWarning(ex, "Unconfirmed-reminder sweep skipped because the database is unavailable.");
            return;
        }

        if (nearing.Count == 0) return;

        foreach (var appt in nearing)
        {
            try
            {
                var patient = await patients.FindByUserIdAsync(appt.UserId, ct);
                if (patient is null || patient.NoShowScore < NoShowScoring.AutoCancelThreshold)
                {
                    continue;
                }

                await appointmentService.ForceCancelAsync(
                    appt.Id, "Hasta hatirlatmaya yanit vermedi (otomatik iptal).", ct);

                _logger.LogInformation(
                    "Auto-cancelled unconfirmed high-risk appointment {AppointmentId} (score {Score}).",
                    appt.Id, patient.NoShowScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to auto-cancel unconfirmed appointment {AppointmentId}.", appt.Id);
            }
        }
    }
}
