using Microsoft.Extensions.Logging;
using WebAppointmentApi.Application.Appointments.Abstractions;
using WebAppointmentApi.Application.Common;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Common.Exceptions;
using WebAppointmentApi.Application.Patients.Services;
using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Appointments.Services;

public sealed class WhatsAppReplyService : IWhatsAppReplyService
{
    private static readonly string[] ConfirmKeywords =
    {
        "1", "evet", "onay", "onaylıyorum", "onayliyorum", "katılacağım", "katilacagim", "geliyorum"
    };

    private static readonly string[] CancelKeywords =
    {
        "2", "hayır", "hayir", "iptal", "gelemeyeceğim", "gelemeyecegim", "gelmeyeceğim", "gelmeyecegim"
    };

    private readonly IPatientRepository _patients;
    private readonly IAppointmentRepository _appointments;
    private readonly IAppointmentService _appointmentService;
    private readonly IWhatsAppMessageSender _whatsapp;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<WhatsAppReplyService> _logger;

    public WhatsAppReplyService(
        IPatientRepository patients,
        IAppointmentRepository appointments,
        IAppointmentService appointmentService,
        IWhatsAppMessageSender whatsapp,
        IDateTimeProvider clock,
        ILogger<WhatsAppReplyService> logger)
    {
        _patients = patients;
        _appointments = appointments;
        _appointmentService = appointmentService;
        _whatsapp = whatsapp;
        _clock = clock;
        _logger = logger;
    }

    public async Task HandleInboundReplyAsync(string rawPhone, string text, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(rawPhone) || string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var last10 = PhoneNumberNormalizer.Last10Digits(rawPhone);
        var patient = await _patients.FindByPhoneSuffixAsync(last10, ct);
        if (patient is null)
        {
            _logger.LogInformation("WhatsApp yanıtı yok sayıldı: telefona ait hasta bulunamadı.");
            return;
        }

        var nowUtc = _clock.UtcNow;
        var appt = await _appointments.FindActiveReminderByUserAsync(patient.UserId, nowUtc, ct);
        if (appt is null)
        {
            _logger.LogInformation("WhatsApp yanıtı yok sayıldı: hasta {PatientId} için bekleyen hatırlatma yok.", patient.Id);
            return;
        }

        var normalized = text.Trim().ToLowerInvariant();

        if (ConfirmKeywords.Any(k => normalized == k || normalized.Contains(k)))
        {
            await ConfirmAsync(patient, appt, nowUtc, ct);
            return;
        }

        if (CancelKeywords.Any(k => normalized == k || normalized.Contains(k)))
        {
            await CancelAsync(patient, appt, ct);
            return;
        }

        _logger.LogInformation("WhatsApp yanıtı anlaşılamadı, işlem yapılmadı. Hasta {PatientId}.", patient.Id);
    }

    private async Task ConfirmAsync(Patient patient, Appointment appt, DateTimeOffset nowUtc, CancellationToken ct)
    {
        await _appointments.MarkReminderConfirmedAsync(appt.Id, nowUtc, ct);
        await AdjustNoShowScoreAsync(patient, NoShowScoring.ReminderConfirmedBonus, ct);

        await TrySendAsync(patient.Phone, "Randevunuz icin tesekkur ederiz, katiliminizi onayladik.", ct);
    }

    private async Task CancelAsync(Patient patient, Appointment appt, CancellationToken ct)
    {
        try
        {
            await _appointmentService.ForceCancelAsync(appt.Id, "Hasta WhatsApp uzerinden iptal etti.", ct);
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning("WhatsApp iptal talebi kabul edilemedi (appt {AppointmentId}): {Reason}", appt.Id, ex.Message);
            await TrySendAsync(patient.Phone,
                "Randevunuza cok az kaldigi icin WhatsApp uzerinden iptal edemiyoruz. Lutfen hastaneyi arayin.", ct);
            return;
        }

        await AdjustNoShowScoreAsync(patient, NoShowScoring.NotifiedCancelPenalty, ct);
        await TrySendAsync(patient.Phone, "Iptaliniz alinmistir, gecmis olsun.", ct);
    }

    private async Task AdjustNoShowScoreAsync(Patient patient, int delta, CancellationToken ct)
    {
        patient.NoShowScore = NoShowScoring.Apply(patient.NoShowScore, delta);
        await _patients.SaveChangesAsync(ct);
    }

    private async Task TrySendAsync(string phone, string message, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return;
        }

        try
        {
            await _whatsapp.SendMessageAsync(phone, message, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "WhatsApp onay/iptal bilgilendirme mesajı gönderilemedi.");
        }
    }
}
