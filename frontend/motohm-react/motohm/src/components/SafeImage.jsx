import { useEffect, useState } from 'react';

/**
 * Şəkil yoxdursa və ya yüklənmirsə (sınıq link), sınıq-şəkil ikonu və alt mətn əvəzinə
 * səliqəli motosiklet ikonu göstərir.
 */
export default function SafeImage({ src, alt = '', ...rest }) {
  const [failed, setFailed] = useState(false);

  useEffect(() => {
    setFailed(false);
  }, [src]);

  if (!src || failed) {
    return (
      <div className="img-placeholder" role="img" aria-label={alt}>
        <svg viewBox="0 0 64 64" width="72" height="72" fill="none" stroke="currentColor"
             strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
          <circle cx="16" cy="42" r="9" />
          <circle cx="48" cy="42" r="9" />
          <path d="M16 42 L24 30 H38 L44 24 L48 42" />
          <path d="M44 24 L42 18 H47" />
          <path d="M24 30 L30 42 H38 L38 30" />
        </svg>
      </div>
    );
  }

  return <img src={src} alt={alt} onError={() => setFailed(true)} {...rest} />;
}