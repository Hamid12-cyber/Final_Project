import { useTranslation } from 'react-i18next';
import { LANGUAGES } from '../i18n/index.js';

export default function LanguageSwitcher() {
  const { i18n, t } = useTranslation();

  return (
    <div className="lang-switch" role="group" aria-label={t('lang.label')}>
      {LANGUAGES.map((lng) => (
        <button
          key={lng}
          type="button"
          className={`lang-btn ${i18n.resolvedLanguage === lng ? 'active' : ''}`}
          onClick={() => i18n.changeLanguage(lng)}
        >
          {lng.toUpperCase()}
        </button>
      ))}
    </div>
  );
}