using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebAppointmentApi.Application.Common.Abstractions;

namespace WebAppointmentApi.Infrastructure.BackgroundJobs;

/// <summary>
/// Runs every 15 minutes and sends WhatsApp reminders for appointments
/// starting within the next 24 hours that haven't been reminded yet.
/// </summary>
public sealed class AppointmentReminderService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ReminderWindow = TimeSpan.FromHours(24);
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

        var upcoming = await appointments.ListUpcomingUnremindedAsync(fromUtc, toUtc, ct);

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

                var message = string.Format(TrCulture,
                    "Hatirlatma: Yarin {0} tarihinde {1} hastanesinde {2} bolumu {3} doktorundan randevunuz bulunmaktadir. Lutfen zamaninda gelmeyi unutmayin.",
                    dateStr, hospitalName, deptName, doctorName);

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
}
