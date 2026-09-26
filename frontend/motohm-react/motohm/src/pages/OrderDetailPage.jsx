import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import apiClient from '../api/client.js';
import { STATUS_LABELS, STATUS_CLASS } from './OrdersPage.jsx';

export default function OrderDetailPage() {
  const { id } = useParams();
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

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
              <p className="cart-item-name">{item.motorcycleName ?? item.partName}</p>
              <p className="cart-item-type">
                {item.motorcycleId ? 'Motosiklet' : 'Ehtiyat hissəsi'} · {item.quantity} ədəd
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
      </div>
    </div>
  );
}