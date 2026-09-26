import { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import apiClient from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';
import { useCart } from '../context/CartContext.jsx';

export default function PartsPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [parts, setParts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [addingId, setAddingId] = useState(null);

  const { user } = useAuth();
  const { addToCart } = useCart();
  const navigate = useNavigate();

  const activeCategory = searchParams.get('category');

  useEffect(() => {
    Promise.all([
      apiClient.get('/parts'),
      apiClient.get('/part-categories'),
    ])
      .then(([partsRes, categoriesRes]) => {
        setParts(partsRes.data);
        setCategories(categoriesRes.data);
        setLoading(false);
      })
      .catch((err) => {
        setError(err.response?.data?.message ?? err.message);
        setLoading(false);
      });
  }, []);

  if (loading) return <div className="state-msg">Yüklənir...</div>;
  if (error) return <div className="state-msg">Xəta: {error}</div>;

  const filtered = activeCategory
    ? parts.filter((p) => String(p.partCategoryId) === activeCategory)
    : parts;

  const handleAddToCart = async (partId) => {
    if (!user) {
      navigate('/login');
      return;
    }
    setAddingId(partId);
    try {
      await addToCart({ partId, quantity: 1 });
    } catch (err) {
      alert(err.response?.data?.message ?? 'Səbətə əlavə etmək mümkün olmadı.');
    } finally {
      setAddingId(null);
    }
  };

  return (
    <div className="page-wrap">
      <h1 className="page-title">Ehtiyat hissələri</h1>

      <div className="category-filters">
        <button
          className={`filter-chip ${!activeCategory ? 'active' : ''}`}
          onClick={() => setSearchParams({})}
        >
          Hamısı
        </button>
        {categories.map((c) => (
          <button
            key={c.id}
            className={`filter-chip ${activeCategory === String(c.id) ? 'active' : ''}`}
            onClick={() => setSearchParams({ category: String(c.id) })}
          >
            {c.icon} {c.name}
          </button>
        ))}
      </div>

      {filtered.length === 0 ? (
        <div className="state-msg">Bu kateqoriyada təsdiqlənmiş hissə tapılmadı.</div>
      ) : (
        <div className="product-grid">
          {filtered.map((p) => (
            <div className="product-card" key={p.id}>
              <div className="product-image">
                {p.imageUrl && <img src={p.imageUrl} alt={p.name} />}
              </div>
              <div className="product-info">
                <p className="product-name">{p.name}</p>
                <p className="product-meta">{p.brand} · {p.partCategoryName}</p>
                <p className="product-price">{p.price.toLocaleString('az-AZ')} AZN</p>
                <p className={`product-stock ${p.stockQty === 0 ? 'low' : ''}`}>
                  {p.stockQty > 0 ? `Stokda: ${p.stockQty} ədəd` : 'Stokda yoxdur'}
                </p>
                <button
                  className="details-btn"
                  disabled={p.stockQty === 0 || addingId === p.id}
                  onClick={() => handleAddToCart(p.id)}
                >
                  {p.stockQty === 0 ? 'Stokda yoxdur' : addingId === p.id ? 'Əlavə olunur...' : 'Səbətə at'}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}