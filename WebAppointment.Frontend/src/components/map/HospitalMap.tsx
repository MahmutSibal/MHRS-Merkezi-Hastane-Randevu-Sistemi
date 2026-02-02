"use client";

import { GoogleMap, Marker, LoadScript } from "@react-google-maps/api";
import { useMemo } from "react";

export type HospitalMarker = {
  id: number;
  name: string;
  latitude: number | null;
  longitude: number | null;
};

type Props = {
  hospitals: HospitalMarker[];
  center?: { lat: number; lng: number } | null;
  height?: number;
};

export default function HospitalMap({ hospitals, center, height = 300 }: Props) {
  const apiKey = process.env.NEXT_PUBLIC_GOOGLE_MAPS_API_KEY ?? "";
  const markers = useMemo(() => hospitals.filter(h => h.latitude != null && h.longitude != null), [hospitals]);

  const mapCenter = center ?? (markers.length > 0 ? { lat: markers[0].latitude!, lng: markers[0].longitude! } : { lat: 41.015137, lng: 28.979530 });

  return (
    <div style={{ height }}>
      <LoadScript googleMapsApiKey={apiKey} loadingElement={<div>Harita yükleniyor…</div>}>
        <GoogleMap
          mapContainerStyle={{ width: "100%", height: "100%" }}
          center={mapCenter}
          zoom={12}
          options={{ disableDefaultUI: false }}
        >
          {markers.map(h => (
            <Marker key={h.id} position={{ lat: h.latitude!, lng: h.longitude! }} title={h.name} />
          ))}
        </GoogleMap>
      </LoadScript>
    </div>
  );
}
