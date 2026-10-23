import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import az from './locales/az.json';
import en from './locales/en.json';
import ru from './locales/ru.json';

export const LANGUAGES = ['az', 'en', 'ru'];
export const DEFAULT_LANGUAGE = 'az';

// Rəqəm / tarix formatı üçün hər dilin locale kodu
export const LOCALES = { az: 'az-AZ', en: 'en-US', ru: 'ru-RU' };

function getInitialLanguage() {
  try {
    const saved = localStorage.getItem('lang');
    if (saved && LANGUAGES.includes(saved)) return saved;
  } catch {
    // localStorage əlçatan deyilsə default dil
  }
  return DEFAULT_LANGUAGE;
}

i18n.use(initReactI18next).init({
  resources: {
    az: { translation: az },
    en: { translation: en },
    ru: { translation: ru },
  },
  lng: getInitialLanguage(),
  fallbackLng: DEFAULT_LANGUAGE,
  interpolation: { escapeValue: false }, // React özü qoruyur
});

// Dil dəyişəndə: yadda saxla + <html lang="..."> yenilə
i18n.on('languageChanged', (lng) => {
  try {
    localStorage.setItem('lang', lng);
  } catch {
    // ignore
  }
  document.documentElement.lang = lng;
});
document.documentElement.lang = i18n.language;

export default i18n;