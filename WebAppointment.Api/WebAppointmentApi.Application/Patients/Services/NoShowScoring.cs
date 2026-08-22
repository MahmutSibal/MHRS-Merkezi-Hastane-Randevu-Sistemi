namespace WebAppointmentApi.Application.Patients.Services;

/// <summary>
/// Central place for the constants that drive Patient.NoShowScore. Score is 0-100:
/// higher means more likely to not show up without notice.
/// </summary>
public static class NoShowScoring
{
    public const int NoShowPenalty = 15;
    public const int SilentCancelPenalty = 10;
    public const int NotifiedCancelPenalty = 3;
    public const int AttendedOrConfirmedBonus = -10;
    public const int ReminderConfirmedBonus = -5;

    /// <summary>
    /// Threshold at/above which an unconfirmed reminder is auto-cancelled to free the slot
    /// for the waitlist instead of just being flagged for staff.
    /// </summary>
    public const int AutoCancelThreshold = 60;

    public static int Apply(int currentScore, int delta) => Math.Clamp(currentScore + delta, 0, 100);
}
