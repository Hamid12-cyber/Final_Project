import { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import apiClient from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';
import { useCart } from '../context/CartContext.jsx';
import Reviews from '../components/Reviews.jsx';
import FavoriteButton from '../components/FavoriteButton.jsx';
import AdminDeleteButton from '../components/AdminDeleteButton.jsx';
import Header from '../components/Header.jsx';
import Footer from '../components/Footer.jsx';

export default function MotorcycleDetailPage() {
  const { id } = useParams();
  const [bike, setBike] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [adding, setAdding] = useState(false);
  const [activeImg, setActiveImg] = useState(0);

  const { user } = useAuth();
  const { addToCart } = useCart();
  const navigate = useNavigate();

  useEffect(() => {
    apiClient.get(`/motorcycles/${id}`)
      .then((res) => {
        setBike(res.data);
        setActiveImg(0);
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

  if (loading || error || !bike) {
    return (
      <>
        <Header />
        {loading && <div className="state-msg">Yüklənir...</div>}
        {!loading && error && <div className="state-msg">Xəta: {error}</div>}
      </>
    );
  }

  const gallery = (bike.imageUrls?.length ? bike.imageUrls : bike.imageUrl ? [bike.imageUrl] : []);

  return (
    <>
    <Header />
    <div className="page-wrap">
      {/* Birinci ekran: məhsul bölməsi tam görünür, rəylər aşağıda qalır */}
      <section className="detail-hero">
      <Link to="/motorcycles" className="back-link">
        <span aria-hidden="true">←</span> Motosikletlərə qayıt
      </Link>

      <div className="detail-layout">
        <div className="detail-gallery">
          <div className="detail-image">
            {gallery[activeImg] && <img src={gallery[activeImg]} alt={bike.name} />}
          </div>
          {gallery.length > 1 && (
            <div className="detail-thumbs">
              {gallery.map((src, i) => (
                <button
                  type="button"
                  key={src}
                  className={`detail-thumb ${i === activeImg ? 'active' : ''}`}
                  onClick={() => setActiveImg(i)}
                  aria-label={`Şəkil ${i + 1}`}
                >
                  <img src={src} alt="" />
                </button>
              ))}
            </div>
          )}
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

          <div style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
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
            <FavoriteButton motorcycleId={bike.id} className="inline" />
          </div>
          <AdminDeleteButton
            endpoint="motorcycles"
            id={bike.id}
            name={bike.name}
            onDeleted={() => navigate('/motorcycles')}
          />
        </div>
      </div>
      </section>

      <Reviews motorcycleId={bike.id} />
    </div>
    <Footer />
    </>
  );
}