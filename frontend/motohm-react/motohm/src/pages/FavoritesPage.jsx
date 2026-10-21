import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useFavorites } from '../context/FavoritesContext.jsx';

export default function FavoritesPage() {
  const { items, loading, toggleFavorite } = useFavorites();
  const [busyId, setBusyId] = useState(null);

  const remove = async (id) => {
    setBusyId(id);
    try {
      await toggleFavorite(id);
    } finally {
      setBusyId(null);
    }
  };

  if (loading && items.length === 0) return <div className="state-msg">Yüklənir...</div>;

  return (
    <div className="page-wrap">
      <h1 className="page-title">Sevimlilər</h1>

      {items.length === 0 ? (
        <div className="state-msg">
          Hələ sevimli motosikletin yoxdur. <Link to="/motorcycles" className="see-all">Motosikletlərə bax →</Link>
        </div>
      ) : (
        <div className="product-grid">
          {items.map((m) => (
            <div className="product-card" key={m.id}>
              <Link to={`/motorcycles/${m.id}`}>
                <div className="product-image">
                  {m.imageUrl && <img src={m.imageUrl} alt={m.name} />}
                </div>
              </Link>
              <div className="product-info">
                <Link to={`/motorcycles/${m.id}`} style={{ color: 'inherit', textDecoration: 'none' }}>
                  <p className="product-name">{m.name}</p>
                </Link>
                <p className="product-meta">{m.brand} · {m.year}</p>
                <p className="product-price">{m.price.toLocaleString('az-AZ')} AZN</p>
                <button className="details-btn" disabled={busyId === m.id} onClick={() => remove(m.id)}>
                  {busyId === m.id ? '...' : 'Sevimlilərdən çıxar'}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}