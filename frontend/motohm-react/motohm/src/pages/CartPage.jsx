import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';
import { useCart } from '../context/CartContext.jsx';

export default function CartPage() {
  const { user } = useAuth();
  const { items, loading, total, updateQuantity, removeItem, checkout } = useCart();
  const [busyId, setBusyId] = useState(null);
  const [showCheckout, setShowCheckout] = useState(false);
  const [shippingAddress, setShippingAddress] = useState('');
  const [contactPhone, setContactPhone] = useState('');
  const [checkoutError, setCheckoutError] = useState(null);
  const [placing, setPlacing] = useState(false);
  const navigate = useNavigate();

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

  const handlePlaceOrder = async (e) => {
    e.preventDefault();
    setCheckoutError(null);
    setPlacing(true);
    try {
      const orderId = await checkout({ shippingAddress, contactPhone });
      navigate(`/orders/${orderId}`);
    } catch (err) {
      setCheckoutError(err.response?.data?.message ?? 'Sifariş yaradıla bilmədi.');
    } finally {
      setPlacing(false);
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
            {!showCheckout && (
              <button className="checkout-btn" onClick={() => setShowCheckout(true)}>
                Sifariş ver
              </button>
            )}
          </div>

          {showCheckout && (
            <form className="checkout-form" onSubmit={handlePlaceOrder}>
              <h2 className="checkout-form-title">Çatdırılma məlumatları</h2>

              <label className="form-field">
                <span>Ünvan</span>
                <input
                  type="text"
                  value={shippingAddress}
                  onChange={(e) => setShippingAddress(e.target.value)}
                  placeholder="Bakı, Nərimanov r., ..."
                  maxLength={300}
                  required
                />
              </label>

              <label className="form-field">
                <span>Əlaqə nömrəsi</span>
                <input
                  type="text"
                  value={contactPhone}
                  onChange={(e) => setContactPhone(e.target.value)}
                  placeholder="+994 55 123 45 67"
                  maxLength={30}
                  required
                />
              </label>

              {checkoutError && <p className="form-error">{checkoutError}</p>}

              <button className="checkout-btn" type="submit" disabled={placing}>
                {placing ? 'Göndərilir...' : 'Sifarişi təsdiqlə'}
              </button>
            </form>
          )}
        </>
      )}
    </div>
  );
}