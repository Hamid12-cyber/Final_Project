import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import apiClient from '../api/client.js';

const STATUS_LABELS = {
  1: 'Gözləmədə',
  2: 'Təsdiqləndi',
  3: 'Göndərildi',
  4: 'Çatdırıldı',
  5: 'Ləğv edildi',
};

const STATUS_CLASS = {
  1: 'status-pending',
  2: 'status-confirmed',
  3: 'status-shipped',
  4: 'status-delivered',
  5: 'status-cancelled',
};

export default function OrdersPage() {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    apiClient.get('/orders')
      .then((res) => {
        setOrders(res.data);
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
      <h1 className="page-title">Sifarişlərim</h1>

      {orders.length === 0 ? (
        <div className="state-msg">Hələ heç bir sifarişiniz yoxdur.</div>
      ) : (
        <div className="cart-list">
          {orders.map((o) => (
            <Link to={`/orders/${o.id}`} className="cart-row order-row" key={o.id}>
              <div className="cart-item-info">
                <p className="cart-item-name">Sifariş #{o.id}</p>
                <p className="cart-item-type">{new Date(o.createdAt).toLocaleDateString('az-AZ')}</p>
              </div>
              <span className={`status-badge ${STATUS_CLASS[o.status] ?? ''}`}>
                {STATUS_LABELS[o.status] ?? o.status}
              </span>
              <p className="cart-item-price">{o.totalAmount.toLocaleString('az-AZ')} AZN</p>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}

export { STATUS_LABELS, STATUS_CLASS };