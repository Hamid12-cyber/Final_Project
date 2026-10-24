import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import apiClient from '../api/client.js';
import { LOCALES } from '../i18n/index.js';

export default function PopularBikes() {
  const { t, i18n } = useTranslation();
  const [bikes, setBikes] = useState([]);

  useEffect(() => {
    apiClient.get('/motorcycles')
      .then((res) => {
        const list = res.data.items ?? res.data;
        setBikes(list.slice(0, 5)); // ana səhifədə yalnız ilk 5-i göstəririk
      })
      .catch(() => setBikes([]));
  }, []);

  if (bikes.length === 0) return null;

  return (
    <section className="section">
      <div className="section-header">
        <h2>{t('home.popularTitle')}</h2>
        <Link to="/motorcycles" className="see-all">{t('home.popularSeeAll')}</Link>
      </div>

      <div className="bike-grid">
        {bikes.map((m) => (
          <Link to={`/motorcycles/${m.id}`} className="bike-card" key={m.id}>
            <div className="bike-image">
              {m.imageUrl && <img src={m.imageUrl} alt={m.name} />}
            </div>
            <div className="bike-info">
              <p className="bike-name">{m.name}</p>
              <p className="bike-meta">{m.brand} · {m.year}</p>
              <p className="bike-price">{m.price.toLocaleString(LOCALES[i18n.resolvedLanguage])} {t('currency.azn')}</p>
            </div>
          </Link>
        ))}
      </div>
    </section>
  );
}