import { useTranslation } from 'react-i18next';

const categories = [
  { id: 'bikes', title: 'categories.bikesTitle', subtitle: 'categories.bikesSub' },
  { id: 'parts', title: 'categories.partsTitle', subtitle: 'categories.partsSub' },
  { id: 'accessories', title: 'categories.accessoriesTitle', subtitle: 'categories.accessoriesSub' },
  { id: 'rental', title: 'categories.rentalTitle', subtitle: 'categories.rentalSub' }
];

export default function Categories({ onSelect }) {
  const { t } = useTranslation();

  return (
    <section className="categories">
      {categories.map((c) => (
        <button
          key={c.id}
          className="category-card"
          style={{ '--cat-img': `url('/images/categories/${c.id}.jpg')` }}
          onClick={() => onSelect && onSelect(c.id)}
        >
          <p className="category-title">{t(c.title)}</p>
          <p className="category-subtitle">{t(c.subtitle)}</p>
        </button>
      ))}
    </section>
  );
}