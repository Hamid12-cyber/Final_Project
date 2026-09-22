const features = [
  { icon: '🚚', title: 'Sürətli çatdırılma', subtitle: 'Bakı daxilində' },
  { icon: '🛡️', title: 'Zəmanətli məhsullar', subtitle: 'Rəsmi distribyutor' },
  { icon: '🎧', title: 'Peşəkar dəstək', subtitle: '24/7' },
  { icon: '🔒', title: 'Təhlükəsiz alış-veriş', subtitle: 'SSL qorunması' }
];

export default function Features() {
  return (
    <section className="features">
      {features.map((f, i) => (
        <div className="feature-item" key={i}>
          <span className="feature-icon">{f.icon}</span>
          <div>
            <p className="feature-title">{f.title}</p>
            <p className="feature-subtitle">{f.subtitle}</p>
          </div>
        </div>
      ))}
    </section>
  );
}
