"use client";

import { useState } from "react";
import { TURKEY_PROVINCES } from "@/lib/turkey-provinces";

export function TurkeyMap({
  selectedProvinceId,
  onSelectProvince,
}: {
  selectedProvinceId: string | null;
  onSelectProvince: (provinceId: string, provinceName: string) => void;
}) {
  const [hoveredId, setHoveredId] = useState<string | null>(null);

  return (
    <svg
      viewBox="10 130 1030 470"
      className="h-auto w-full"
      role="img"
      aria-label="Türkiye il haritası"
    >
      {TURKEY_PROVINCES.map((province) => {
        const isSelected = province.id === selectedProvinceId;
        const isHovered = province.id === hoveredId;
        return (
          <path
            key={province.id}
            d={province.path}
            onClick={() => onSelectProvince(province.id, province.name)}
            onMouseEnter={() => setHoveredId(province.id)}
            onMouseLeave={() => setHoveredId((current) => (current === province.id ? null : current))}
            className="cursor-pointer transition-colors"
            style={{
              fill: isSelected ? "#2563eb" : isHovered ? "#93c5fd" : "#e2e8f0",
              stroke: "#64748b",
              strokeWidth: 0.7,
            }}
          >
            <title>{province.name}</title>
          </path>
        );
      })}
    </svg>
  );
}
