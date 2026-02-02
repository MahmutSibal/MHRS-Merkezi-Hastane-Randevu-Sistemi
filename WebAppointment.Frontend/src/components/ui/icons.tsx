import type { SVGProps } from "react";

function baseIcon(props: SVGProps<SVGSVGElement>) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round" {...props} />
  );
}

export function DepartmentIcon(props: SVGProps<SVGSVGElement>) {
  return (
    baseIcon({ ...props, children: (
      <g>
        <rect x="3" y="6" width="6" height="12" rx="1.5" />
        <rect x="15" y="3" width="6" height="15" rx="1.5" />
        <path d="M3 20h18" />
      </g>
    ) })
  );
}

export function DoctorIcon(props: SVGProps<SVGSVGElement>) {
  return (
    baseIcon({ ...props, children: (
      <g>
        <circle cx="12" cy="7" r="3.2" />
        <path d="M4 20c0-3.8 3.6-6.2 8-6.2s8 2.4 8 6.2" />
        <path d="M12 12v3" />
      </g>
    ) })
  );
}

export function PatientIcon(props: SVGProps<SVGSVGElement>) {
  return (
    baseIcon({ ...props, children: (
      <g>
        <circle cx="8" cy="8" r="3" />
        <circle cx="16" cy="8" r="3" />
        <path d="M4 20c0-3.5 3-6 6-6M20 20c0-3.5-3-6-6-6" />
      </g>
    ) })
  );
}

export function AppointmentIcon(props: SVGProps<SVGSVGElement>) {
  return (
    baseIcon({ ...props, children: (
      <g>
        <rect x="3" y="5" width="18" height="16" rx="2" />
        <path d="M7 3v4M17 3v4" />
        <path d="M6 11h12M8 15h4" />
      </g>
    ) })
  );
}

export function ReportIcon(props: SVGProps<SVGSVGElement>) {
  return (
    baseIcon({ ...props, children: (
      <g>
        <path d="M6 4h9l3 3v13a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2z" />
        <path d="M9 10h6M9 14h6M9 18h3" />
      </g>
    ) })
  );
}

export function CalendarIcon(props: SVGProps<SVGSVGElement>) {
  return (
    baseIcon({ ...props, children: (
      <g>
        <rect x="3" y="5" width="18" height="16" rx="2" />
        <path d="M7 3v4M17 3v4M3 10h18" />
        <rect x="7" y="13" width="4" height="4" rx="1" />
        <rect x="13" y="13" width="4" height="4" rx="1" />
      </g>
    ) })
  );
}
