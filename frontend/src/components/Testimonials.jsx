import { testimonials } from '../data/mockData.js';

export default function Testimonials() {
  return (
    <section className="section">
      <div className="section-header">
        <h2>Müştəri rəyləri</h2>
        <a href="#" className="see-all">Bütün rəylər →</a>
      </div>

      <div className="testimonial-grid">
        {testimonials.map((t) => (
          <div className="testimonial-card" key={t.id}>
            <p className="testimonial-name">{t.name}</p>
            <p className="testimonial-rating">{'★'.repeat(t.rating)}</p>
            <p className="testimonial-text">{t.text}</p>
            <p className="testimonial-date">{t.date}</p>
          </div>
        ))}
      </div>
    </section>
  );
}
