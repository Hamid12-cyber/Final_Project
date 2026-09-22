const services = [
  { icon: '📅', name: 'Yağ dəyişimi' },
  { icon: '🛠️', name: 'Təmir' },
  { icon: '📟', name: 'Diaqnostika' },
  { icon: '⚙️', name: 'Tüninq' }
];

export default function ServiceBanner() {
  return (
    <section className="banner service-banner">
      <div className="service-text">
        <p className="banner-eyebrow">SERVİS</p>
        <h3 className="banner-title">Peşəkar servis xidməti</h3>
        <p className="banner-desc">
          Təcrübəli ustalarımızla motosiklətiniz həmişə ideal vəziyyətdə olsun.
        </p>
        <button className="banner-btn">Servisə yazıl →</button>
      </div>

      <div className="service-list">
        {services.map((s, i) => (
          <div className="service-item" key={i}>
            <span>{s.icon}</span> {s.name}
          </div>
        ))}
      </div>
    </section>
  );
}
