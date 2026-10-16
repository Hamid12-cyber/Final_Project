import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import apiClient from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';
import { useCart } from '../context/CartContext.jsx';

export default function AccessoriesPage() {
  const [accessories, setAccessories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [addingId, setAddingId] = useState(null);

  const { user } = useAuth();
  const { addToCart } = useCart();
  const navigate = useNavigate();

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

  const handleAddToCart = async (accessoryId) => {
    if (!user) {
      navigate('/login');
      return;
    }
    setAddingId(accessoryId);
    try {
      await addToCart({ accessoryId, quantity: 1 });
    } catch (err) {
      alert(err.response?.data?.message ?? 'Səbətə əlavə etmək mümkün olmadı.');
    } finally {
      setAddingId(null);
    }
  };

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
              <div className="product-image">
                {a.imageUrl && <img src={a.imageUrl} alt={a.name} />}
              </div>
              <div className="product-info">
                <p className="product-name">{a.name}</p>
                <p className="product-meta">{a.brand}</p>
                <p className="product-price">{a.price.toLocaleString('az-AZ')} AZN</p>
                <p className={`product-stock ${a.stockQty === 0 ? 'low' : ''}`}>
                  {a.stockQty > 0 ? `Stokda: ${a.stockQty} ədəd` : 'Stokda yoxdur'}
                </p>
                <button
                  className="details-btn"
                  disabled={a.stockQty === 0 || addingId === a.id}
                  onClick={() => handleAddToCart(a.id)}
                >
                  {addingId === a.id ? 'Əlavə olunur...' : 'Səbətə at'}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}