import { useTranslation } from 'react-i18next';

const features = [
  { icon: '🚚', title: 'features.deliveryTitle', subtitle: 'features.deliverySub' },
  { icon: '🛡️', title: 'features.warrantyTitle', subtitle: 'features.warrantySub' },
  { icon: '🎧', title: 'features.supportTitle', subtitle: 'features.supportSub' },
  { icon: '🔒', title: 'features.secureTitle', subtitle: 'features.secureSub' }
];

export default function Features() {
  const { t } = useTranslation();

  return (
    <section className="features">
      {features.map((f, i) => (
        <div className="feature-item" key={i}>
          <span className="feature-icon">{f.icon}</span>
          <div>
            <p className="feature-title">{t(f.title)}</p>
            <p className="feature-subtitle">{t(f.subtitle)}</p>
          </div>
        </div>
      ))}
    </section>
  );
}