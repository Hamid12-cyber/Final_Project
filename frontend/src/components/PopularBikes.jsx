import { popularBikes } from '../data/mockData.js';

export default function PopularBikes() {
  return (
    <section className="section">
      <div className="section-header">
        <h2>Populyar motosikletlər</h2>
        <a href="#" className="see-all">Hamısına bax →</a>
      </div>

      <div className="bike-grid">
        {popularBikes.map((bike) => (
          <div className="bike-card" key={bike.id}>
            <div className="bike-image" />
            <div className="bike-info">
              <p className="bike-name">{bike.name}</p>
              <p className="bike-meta">{bike.cc} · {bike.year}</p>
              <p className="bike-price">{bike.price}</p>
              <button className="details-btn">Ətraflı</button>
            </div>
          </div>
        ))}
      </div>
    </section>
  );
}
