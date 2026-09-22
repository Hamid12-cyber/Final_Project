import { partCategories } from '../data/mockData.js';

export default function PartsCategories() {
  return (
    <section className="section">
      <div className="section-header">
        <h2>Ehtiyat hissələri</h2>
        <a href="#" className="see-all">Bütün hissələrə bax →</a>
      </div>

      <div className="parts-grid">
        {partCategories.map((p) => (
          <button className="part-card" key={p.id}>
            <span className="part-icon">{p.icon}</span>
            <p className="part-name">{p.name}</p>
          </button>
        ))}
      </div>
    </section>
  );
}
