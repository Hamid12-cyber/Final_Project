import { useTranslation } from 'react-i18next';

export default function RentalBanner() {
  const { t } = useTranslation();

  return (
    <section className="banner rental-banner">
      <p className="banner-eyebrow">{t('rentalBanner.eyebrow')}</p>
      <h3 className="banner-title">{t('rentalBanner.title')}</h3>
      <p className="banner-desc">{t('rentalBanner.desc')}</p>
      <button className="banner-btn">{t('rentalBanner.btn')}</button>

      <div className="rental-prices">
        <div>
          <p className="price-label">{t('rentalBanner.daily')}</p>
          <p className="price-value">50 {t('currency.azn')}</p>
        </div>
        <div>
          <p className="price-label">{t('rentalBanner.weekly')}</p>
          <p className="price-value">300 {t('currency.azn')}</p>
        </div>
        <div>
          <p className="price-label">{t('rentalBanner.monthly')}</p>
          <p className="price-value">1000 {t('currency.azn')}</p>
        </div>
      </div>
    </section>
  );
}