import { useEffect, useState } from 'react';

export function getInitials(name) {
  const parts = (name || '').trim().split(/\s+/).filter(Boolean);
  if (parts.length === 0) return '?';
  if (parts.length === 1) return parts[0][0].toUpperCase();
  return (parts[0][0] + parts[1][0]).toUpperCase();
}

// Profil şəkli varsa onu, yoxdursa (və ya şəkil yüklənmirsə) adın baş hərflərindən default avatar göstərir.
// props: name, src (şəkil URL-i, boş ola bilər), size (px)
export default function Avatar({ name, src, size = 36, className = '' }) {
  const [broken, setBroken] = useState(false);

  useEffect(() => {
    setBroken(false);
  }, [src]);

  return (
    <span
      className={`avatar ${className}`}
      style={{ width: size, height: size, fontSize: Math.round(size * 0.36) }}
      aria-hidden="true"
    >
      {src && !broken ? <img src={src} alt="" onError={() => setBroken(true)} /> : getInitials(name)}
    </span>
  );
}