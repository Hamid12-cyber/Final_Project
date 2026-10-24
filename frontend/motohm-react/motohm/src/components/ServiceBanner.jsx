import { useTranslation } from 'react-i18next';

const services = [
  { icon: '📅', name: 'serviceBanner.oil' },
  { icon: '🛠️', name: 'serviceBanner.repair' },
  { icon: '📟', name: 'serviceBanner.diagnostics' },
  { icon: '⚙️', name: 'serviceBanner.tuning' }
];

export default function ServiceBanner() {
  const { t } = useTranslation();

  return (
    <section className="banner service-banner">
      <div className="service-text">
        <p className="banner-eyebrow">{t('serviceBanner.eyebrow')}</p>
        <h3 className="banner-title">{t('serviceBanner.title')}</h3>
        <p className="banner-desc">{t('serviceBanner.desc')}</p>
        <button className="banner-btn">{t('serviceBanner.btn')}</button>
      </div>

      <div className="service-list">
        {services.map((s, i) => (
          <div className="service-item" key={i}>
            <span>{s.icon}</span> {t(s.name)}
          </div>
        ))}
      </div>
    </section>
  );
}