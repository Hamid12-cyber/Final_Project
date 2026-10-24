import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import apiClient from '../api/client.js';

export default function PartsCategories() {
  const { t } = useTranslation();
  const [categories, setCategories] = useState([]);

  useEffect(() => {
    apiClient.get('/part-categories')
      .then((res) => setCategories(res.data))
      .catch(() => setCategories([])); // ana səhifədə xəta banner-i şişirtmirik, sadəcə boş qalır
  }, []);

  if (categories.length === 0) return null;

  return (
    <section className="section">
      <div className="section-header">
        <h2>{t('home.partsTitle')}</h2>
        <Link to="/parts" className="see-all">{t('home.partsSeeAll')}</Link>
      </div>

      <div className="parts-grid">
        {categories.map((c) => (
          <Link to={`/parts?category=${c.id}`} className="part-card" key={c.id}>
            <span className="part-icon">{c.icon}</span>
            <p className="part-name">{c.name}</p>
          </Link>
        ))}
      </div>
    </section>
  );
}