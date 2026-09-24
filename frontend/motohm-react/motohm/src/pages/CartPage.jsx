import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';
import { useCart } from '../context/CartContext.jsx';

export default function CartPage() {
  const { user } = useAuth();
  const { items, loading, total, updateQuantity, removeItem } = useCart();
  const [busyId, setBusyId] = useState(null);

  if (!user) {
    return (
      <div className="page-wrap">
        <h1 className="page-title">Səbət</h1>
        <div className="state-msg">
          Səbətinizi görmək üçün <Link to="/login" className="accent">daxil olun</Link>.
        </div>
      </div>
    );
  }

  if (loading) return <div className="state-msg">Yüklənir...</div>;

  const handleQty = async (item, delta) => {
    const next = item.quantity + delta;
    if (next < 1) return;
    setBusyId(item.id);
    try {
      await updateQuantity(item.id, next);
    } catch (err) {
      alert(err.response?.data?.message ?? 'Miqdar yenilənmədi.');
    } finally {
      setBusyId(null);
    }
  };

  const handleRemove = async (item) => {
    setBusyId(item.id);
    try {
      await removeItem(item.id);
    } catch (err) {
      alert(err.response?.data?.message ?? 'Silinmədi.');
    } finally {
      setBusyId(null);
    }
  };

  return (
    <div className="page-wrap">
      <h1 className="page-title">Səbət</h1>

      {items.length === 0 ? (
        <div className="state-msg">Səbətiniz boşdur.</div>
      ) : (
        <>
          <div className="cart-list">
            {items.map((item) => (
              <div className="cart-row" key={item.id}>
                <div className="cart-item-info">
                  <p className="cart-item-name">{item.motorcycleName ?? item.partName}</p>
                  <p className="cart-item-type">{item.motorcycleId ? 'Motosiklet' : 'Ehtiyat hissəsi'}</p>
                </div>

                <div className="qty-control">
                  <button
                    className="qty-btn"
                    disabled={busyId === item.id || item.quantity <= 1}
                    onClick={() => handleQty(item, -1)}
                  >
                    −
                  </button>
                  <span className="qty-value">{item.quantity}</span>
                  <button
                    className="qty-btn"
                    disabled={busyId === item.id}
                    onClick={() => handleQty(item, 1)}
                  >
                    +
                  </button>
                </div>

                <p className="cart-item-price">
                  {(item.unitPrice * item.quantity).toLocaleString('az-AZ')} AZN
                </p>

                <button className="remove-btn" disabled={busyId === item.id} onClick={() => handleRemove(item)}>
                  Sil
                </button>
              </div>
            ))}
          </div>

          <div className="cart-summary">
            <div>
              <p className="cart-item-type">Cəmi</p>
              <p className="cart-total-value">{total.toLocaleString('az-AZ')} AZN</p>
            </div>
            {/* Orders modulu hazır olanda POST /orders buradan çağırılacaq */}
            <button className="checkout-btn" disabled title="Sifariş modulu tezliklə əlavə olunacaq">
              Sifariş ver (tezliklə)
            </button>
          </div>
        </>
      )}
    </div>
  );
}