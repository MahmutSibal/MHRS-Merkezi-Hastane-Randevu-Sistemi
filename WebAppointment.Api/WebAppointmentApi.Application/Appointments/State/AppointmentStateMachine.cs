using WebAppointmentApi.Application.Common.Exceptions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.Application.Appointments.State;

public sealed class AppointmentStateMachine : IAppointmentStateMachine
{
    private readonly IReadOnlyDictionary<AppointmentStatus, IAppointmentState> _states;

    public AppointmentStateMachine()
    {
        _states = new Dictionary<AppointmentStatus, IAppointmentState>
        {
            [AppointmentStatus.Pending] = new PendingState(),
            [AppointmentStatus.Approved] = new ApprovedState(),
            [AppointmentStatus.Cancelled] = new TerminalState(AppointmentStatus.Cancelled),
            [AppointmentStatus.Completed] = new TerminalState(AppointmentStatus.Completed),
        };
    }

    public void Transition(Appointment appointment, AppointmentStatus targetStatus, DateTimeOffset nowUtc)
    {
        if (!_states.TryGetValue(appointment.Status, out var state))
        {
            throw new ConflictException("Appointment status is not supported.");
        }

        if (!state.CanTransitionTo(appointment, targetStatus, nowUtc, out var reason))
        {
            throw new ConflictException(reason);
        }

        appointment.Status = targetStatus;
        appointment.UpdatedAtUtc = nowUtc;
    }

    private interface IAppointmentState
    {
        bool CanTransitionTo(Appointment appointment, AppointmentStatus targetStatus, DateTimeOffset nowUtc, out string reason);
    }

    private sealed class TerminalState : IAppointmentState
    {
        private readonly AppointmentStatus _status;

        public TerminalState(AppointmentStatus status) => _status = status;

        public bool CanTransitionTo(Appointment appointment, AppointmentStatus targetStatus, DateTimeOffset nowUtc, out string reason)
        {
            reason = $"Appointment cannot be changed from {_status}.";
            return false;
        }
    }

    private sealed class PendingState : IAppointmentState
    {
        public bool CanTransitionTo(Appointment appointment, AppointmentStatus targetStatus, DateTimeOffset nowUtc, out string reason)
        {
            if (targetStatus == AppointmentStatus.Approved)
            {
                if (appointment.StartAt <= nowUtc)
                {
                    reason = "Past appointments cannot be approved.";
                    return false;
                }

                reason = string.Empty;
                return true;
            }

            if (targetStatus == AppointmentStatus.Cancelled)
            {
                return CanCancel(appointment, nowUtc, out reason);
            }

            reason = "Invalid status transition.";
            return false;
        }
    }

    private sealed class ApprovedState : IAppointmentState
    {
        public bool CanTransitionTo(Appointment appointment, AppointmentStatus targetStatus, DateTimeOffset nowUtc, out string reason)
        {
            if (targetStatus == AppointmentStatus.Completed)
            {
                if (appointment.EndAt > nowUtc)
                {
                    reason = "Appointment can only be completed after it ends.";
                    return false;
                }

                reason = string.Empty;
                return true;
            }

            if (targetStatus == AppointmentStatus.Cancelled)
            {
                return CanCancel(appointment, nowUtc, out reason);
            }

            reason = "Invalid status transition.";
            return false;
        }
    }

    private static bool CanCancel(Appointment appointment, DateTimeOffset nowUtc, out string reason)
    {
        if (appointment.StartAt <= nowUtc)
        {
            reason = "Past appointments cannot be cancelled.";
            return false;
        }

        if ((appointment.StartAt - nowUtc).TotalMinutes <= 15)
        {
            reason = "Appointment cannot be cancelled within 15 minutes of start time.";
            return false;
        }

        reason = string.Empty;
        return true;
    }
}
