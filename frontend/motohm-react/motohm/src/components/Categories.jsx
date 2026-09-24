const categories = [
  { id: 'bikes', title: 'Motosikletlər', subtitle: 'Yeni və ikinci əl' },
  { id: 'parts', title: 'Ehtiyat hissələri', subtitle: 'Hər markaya uyğun' },
  { id: 'accessories', title: 'Aksesuarlar', subtitle: 'Dəstək və qorunma' },
  { id: 'rental', title: 'Kirayə', subtitle: 'Günlük / Həftəlik / Aylıq' }
];

export default function Categories({ onSelect }) {
  return (
    <section className="categories">
      {categories.map((c) => (
        <button
          key={c.id}
          className="category-card"
          onClick={() => onSelect && onSelect(c.id)}
        >
          <p className="category-title">{c.title}</p>
          <p className="category-subtitle">{c.subtitle}</p>
        </button>
      ))}
    </section>
  );
}
