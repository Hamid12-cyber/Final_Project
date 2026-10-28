import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { LANGUAGES } from '../i18n/index.js';

export default function LanguageSwitcher() {
  const { i18n, t } = useTranslation();
  const [open, setOpen] = useState(false);
  const current = i18n.resolvedLanguage || '';

  return (
    <div
      className={`lang-switch ${open ? 'open' : ''}`}
      role="group"
      aria-label={t('lang.label')}
      onMouseLeave={() => setOpen(false)}
      onBlur={(e) => {
        if (!e.currentTarget.contains(e.relatedTarget)) setOpen(false);
      }}
    >
      {/* Həmişə görünən: yalnız seçilmiş dil */}
      <button
        type="button"
        className="lang-current"
        aria-haspopup="true"
        aria-expanded={open}
        onClick={() => setOpen((o) => !o)}
      >
        <span className="lang-btn active">{current.toUpperCase()}</span>
      </button>

      {/* Üstünə gələndə açılır: AZ / EN / RU yan-yana */}
      <div className="lang-options">
        {LANGUAGES.map((lng) => (
          <button
            key={lng}
            type="button"
            className={`lang-btn ${current === lng ? 'active' : ''}`}
            onClick={(e) => {
              i18n.changeLanguage(lng);
              setOpen(false);
              e.currentTarget.blur();
            }}
          >
            {lng.toUpperCase()}
          </button>
        ))}
      </div>
    </div>
  );
}