import { useState, useEffect } from 'react';
import apiClient from '../api/client.js';

export default function AccessoriesPage() {
  const [accessories, setAccessories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    apiClient.get('/accessories')
      .then((res) => {
        setAccessories(res.data);
        setLoading(false);
      })
      .catch((err) => {
        setError(err.response?.data?.message ?? err.message);
        setLoading(false);
      });
  }, []);

  if (loading) return <div className="state-msg">Yüklənir...</div>;
  if (error) return <div className="state-msg">Xəta: {error}</div>;

  return (
    <div className="page-wrap">
      <h1 className="page-title">Aksesuarlar</h1>

      {accessories.length === 0 ? (
        <div className="state-msg">Hələ təsdiqlənmiş aksesuar yoxdur.</div>
      ) : (
        <div className="product-grid">
          {accessories.map((a) => (
            <div className="product-card" key={a.id}>
              <div className="product-image" />
              <div className="product-info">
                <p className="product-name">{a.name}</p>
                <p className="product-meta">{a.brand}</p>
                <p className="product-price">{a.price.toLocaleString('az-AZ')} AZN</p>
                <p className={`product-stock ${a.stockQty === 0 ? 'low' : ''}`}>
                  {a.stockQty > 0 ? `Stokda: ${a.stockQty} ədəd` : 'Stokda yoxdur'}
                </p>
                {/* Backend-də CartItem yalnız MotorcycleId/PartId qəbul edir, AccessoryId sahəsi yoxdur.
                    Bu düymə backend-ə migration əlavə olunana qədər deaktivdir. */}
                <button className="details-btn" disabled title="Aksesuarlar üçün səbət dəstəyi hələ backend-də yoxdur">
                  Tezliklə səbətdə
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}