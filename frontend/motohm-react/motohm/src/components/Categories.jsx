import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

const categories = [
  { id: 'bikes', to: '/motorcycles', title: 'categories.bikesTitle', subtitle: 'categories.bikesSub' },
  { id: 'parts', to: '/parts', title: 'categories.partsTitle', subtitle: 'categories.partsSub' },
  { id: 'accessories', to: '/accessories', title: 'categories.accessoriesTitle', subtitle: 'categories.accessoriesSub' },
  { id: 'rental', to: '/rentals', title: 'categories.rentalTitle', subtitle: 'categories.rentalSub' }
];

export default function Categories() {
  const { t } = useTranslation();

  return (
    <section className="categories">
      {categories.map((c) => (
        <Link
          key={c.id}
          to={c.to}
          className="category-card"
          style={{ '--cat-art': `var(--art-${c.id})` }}
        >
          <p className="category-title">{t(c.title)}</p>
          <p className="category-subtitle">{t(c.subtitle)}</p>
        </Link>
      ))}
    </section>
  );
}