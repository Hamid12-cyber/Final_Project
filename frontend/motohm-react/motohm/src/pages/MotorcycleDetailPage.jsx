import { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import apiClient from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';
import { useCart } from '../context/CartContext.jsx';

export default function MotorcycleDetailPage() {
  const { id } = useParams();
  const [bike, setBike] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [adding, setAdding] = useState(false);

  const { user } = useAuth();
  const { addToCart } = useCart();
  const navigate = useNavigate();

  useEffect(() => {
    apiClient.get(`/motorcycles/${id}`)
      .then((res) => {
        setBike(res.data);
        setLoading(false);
      })
      .catch((err) => {
        setError(err.response?.data?.message ?? err.message);
        setLoading(false);
      });
  }, [id]);

  const handleAddToCart = async () => {
    if (!user) {
      navigate('/login');
      return;
    }
    setAdding(true);
    try {
      await addToCart({ motorcycleId: bike.id, quantity: 1 });
    } catch (err) {
      alert(err.response?.data?.message ?? 'Səbətə əlavə etmək mümkün olmadı.');
    } finally {
      setAdding(false);
    }
  };

  if (loading) return <div className="state-msg">Yüklənir...</div>;
  if (error) return <div className="state-msg">Xəta: {error}</div>;
  if (!bike) return null;

  return (
    <div className="page-wrap">
      <Link to="/motorcycles" className="see-all">← Motosikletlərə qayıt</Link>

      <div className="detail-layout">
        <div className="detail-image">
          {bike.imageUrl && <img src={bike.imageUrl} alt={bike.name} />}
        </div>

        <div className="detail-info">
          <h1 className="page-title" style={{ marginBottom: 8 }}>{bike.name}</h1>
          <p className="cart-item-type" style={{ marginBottom: 16 }}>{bike.brand} · {bike.model} · {bike.year}</p>

          <div className="detail-badges">
            {bike.isForSale && <span className="status-badge status-delivered">Satışda</span>}
            {bike.isForRent && <span className="status-badge status-confirmed">Kirayə üçün əlçatandır</span>}
          </div>

          <table className="detail-specs">
            <tbody>
              <tr><td>Marka</td><td>{bike.brand}</td></tr>
              <tr><td>Model</td><td>{bike.model}</td></tr>
              <tr><td>Həcm</td><td>{bike.cc} cc</td></tr>
              <tr><td>İl</td><td>{bike.year}</td></tr>
            </tbody>
          </table>

          <p className="cart-total-value" style={{ margin: '20px 0' }}>
            {bike.price.toLocaleString('az-AZ')} AZN
          </p>

          <div style={{ display: 'flex', gap: 12 }}>
            {bike.isForSale && (
              <button className="checkout-btn" disabled={adding} onClick={handleAddToCart}>
                {adding ? 'Əlavə olunur...' : 'Səbətə at'}
              </button>
            )}
            {bike.isForRent && (
              <Link to="/rentals" className="checkout-btn" style={{ textDecoration: 'none', display: 'inline-block' }}>
                Kirayə et
              </Link>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}