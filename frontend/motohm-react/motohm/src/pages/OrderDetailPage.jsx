import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import apiClient from '../api/client.js';
import OrderTracker from '../components/OrderTracker.jsx';
import { STATUS_LABELS, STATUS_CLASS } from '../utils/orderStatus.js';
import { itemName, itemType } from '../utils/itemInfo.js';

export default function OrderDetailPage() {
  const { id } = useParams();
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [cancelling, setCancelling] = useState(false);
  const [cancelError, setCancelError] = useState(null);

  useEffect(() => {
    apiClient.get(`/orders/${id}`)
      .then((res) => {
        setOrder(res.data);
        setLoading(false);
      })
      .catch((err) => {
        setError(err.response?.data?.message ?? err.message);
        setLoading(false);
      });
  }, [id]);

  const handleCancel = async () => {
    if (!window.confirm('Bu sifarişi ləğv etmək istəyirsən?')) return;
    setCancelling(true);
    setCancelError(null);
    try {
      await apiClient.put(`/orders/${id}/cancel`);
      setOrder((prev) => ({ ...prev, status: 'Cancelled' }));
    } catch (err) {
      setCancelError(err.response?.data?.message ?? 'Sifariş ləğv edilmədi.');
    } finally {
      setCancelling(false);
    }
  };

  if (loading) return <div className="state-msg">Yüklənir...</div>;
  if (error) return <div className="state-msg">Xəta: {error}</div>;
  if (!order) return null;

  return (
    <div className="page-wrap">
      <Link to="/orders" className="see-all">← Sifarişlərimə qayıt</Link>

      <h1 className="page-title" style={{ marginTop: 16 }}>Sifariş #{order.id}</h1>

      <div className="order-meta-row">
        <span className={`status-badge ${STATUS_CLASS[order.status] ?? ''}`}>
          {STATUS_LABELS[order.status] ?? order.status}
        </span>
        <span className="cart-item-type">{new Date(order.createdAt).toLocaleDateString('az-AZ')}</span>
      </div>

      <OrderTracker status={order.status} />

      <div className="cart-summary" style={{ marginBottom: 24 }}>
        <div>
          <p className="cart-item-type">Ünvan</p>
          <p className="cart-item-name">{order.shippingAddress}</p>
        </div>
        <div>
          <p className="cart-item-type">Əlaqə</p>
          <p className="cart-item-name">{order.contactPhone}</p>
        </div>
      </div>

      <div className="cart-list">
        {order.items.map((item, idx) => (
          <div className="cart-row" key={idx}>
            <div className="cart-item-info">
              <p className="cart-item-name">{itemName(item)}</p>
              <p className="cart-item-type">
                {itemType(item)} · {item.quantity} ədəd
              </p>
            </div>
            <p className="cart-item-price">
              {(item.unitPriceAtOrderTime * item.quantity).toLocaleString('az-AZ')} AZN
            </p>
          </div>
        ))}
      </div>

      <div className="cart-summary">
        <div>
          <p className="cart-item-type">Cəmi</p>
          <p className="cart-total-value">{order.totalAmount.toLocaleString('az-AZ')} AZN</p>
        </div>
        {order.status === 'Pending' && (
          <button type="button" className="seller-btn seller-btn-danger" onClick={handleCancel} disabled={cancelling}>
            {cancelling ? 'Ləğv edilir...' : 'Sifarişi ləğv et'}
          </button>
        )}
      </div>

      {cancelError && <p className="form-error">{cancelError}</p>}
    </div>
  );
}