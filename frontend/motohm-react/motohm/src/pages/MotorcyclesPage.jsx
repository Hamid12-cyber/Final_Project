import { useState, useEffect } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import apiClient from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';
import { useCart } from '../context/CartContext.jsx';

export default function MotorcyclesPage() {
  const [motorcycles, setMotorcycles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [addingId, setAddingId] = useState(null);
  const [searchParams] = useSearchParams();
  const searchQuery = (searchParams.get('search') ?? '').toLowerCase();

  const { user } = useAuth();
  const { addToCart } = useCart();
  const navigate = useNavigate();

  useEffect(() => {
    apiClient.get('/motorcycles')
      .then((res) => {
        setMotorcycles(res.data.items ?? res.data);
        setLoading(false);
      })
      .catch((err) => {
        setError(err.response?.data?.message ?? err.message);
        setLoading(false);
      });
  }, []);

  const handleAddToCart = async (motorcycleId) => {
    if (!user) {
      navigate('/login');
      return;
    }
    setAddingId(motorcycleId);
    try {
      await addToCart({ motorcycleId, quantity: 1 });
    } catch (err) {
      alert(err.response?.data?.message ?? 'Səbətə əlavə etmək mümkün olmadı.');
    } finally {
      setAddingId(null);
    }
  };

  if (loading) return <div className="state-msg">Yüklənir...</div>;
  if (error) return <div className="state-msg">Xəta: {error}</div>;

  const filtered = searchQuery
    ? motorcycles.filter((m) =>
        m.name.toLowerCase().includes(searchQuery) || m.brand.toLowerCase().includes(searchQuery)
      )
    : motorcycles;

  return (
    <div className="page-wrap">
      <h1 className="page-title">
        {searchQuery ? `Axtarış: "${searchParams.get('search')}"` : 'Motosikletlər'}
      </h1>

      {filtered.length === 0 ? (
        <div className="state-msg">Nəticə tapılmadı.</div>
      ) : (
        <div className="product-grid">
          {filtered.map((m) => (
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
                <button
                  className="details-btn"
                  disabled={addingId === m.id}
                  onClick={() => handleAddToCart(m.id)}
                >
                  {addingId === m.id ? 'Əlavə olunur...' : 'Səbətə at'}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}