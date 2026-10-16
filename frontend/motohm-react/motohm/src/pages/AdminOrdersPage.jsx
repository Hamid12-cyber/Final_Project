import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import apiClient from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';
import { STATUS_LABELS, STATUS_CLASS } from './OrdersPage.jsx';
import { itemName, itemType } from '../utils/itemInfo.js';

const STATUSES = ['Pending', 'Confirmed', 'Shipped', 'Delivered', 'Cancelled'];
const PAGE_SIZE = 20;

export default function AdminOrdersPage() {
  const { user } = useAuth();
  const [status, setStatus] = useState('');
  const [page, setPage] = useState(1);
  const [data, setData] = useState({ items: [], totalCount: 0 });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [openId, setOpenId] = useState(null);
  const [details, setDetails] = useState({});
  const [busyId, setBusyId] = useState(null);

  const load = useCallback(() => {
    setLoading(true);
    setError(null);
    apiClient.get('/admin/orders', {
      params: { status: status || undefined, page, pageSize: PAGE_SIZE },
    })
      .then((res) => setData(res.data))
      .catch((err) => setError(err.response?.data?.message ?? err.message))
      .finally(() => setLoading(false));
  }, [status, page]);

  useEffect(() => {
    load();
  }, [load]);

  if (!user || user.role !== 'Admin') {
    return (
      <div className="page-wrap">
        <h1 className="page-title">Bütün sifarişlər</h1>
        <div className="state-msg">Bu səhifəyə giriş icazəniz yoxdur.</div>
      </div>
    );
  }

  const changeFilter = (s) => {
    setStatus(s);
    setPage(1);
    setOpenId(null);
  };

  const toggleDetails = async (id) => {
    if (openId === id) {
      setOpenId(null);
      return;
    }
    setOpenId(id);
    if (details[id]) return;
    try {
      const res = await apiClient.get(`/admin/orders/${id}`);
      setDetails((prev) => ({ ...prev, [id]: res.data }));
    } catch (err) {
      alert(err.response?.data?.message ?? 'Sifariş detalları yüklənmədi.');
      setOpenId(null);
    }
  };

  const changeStatus = async (id, newStatus) => {
    setBusyId(id);
    try {
      await apiClient.put(`/admin/orders/${id}/status`, { status: newStatus });
      setDetails((prev) => (prev[id] ? { ...prev, [id]: { ...prev[id], status: newStatus } } : prev));
      load();
    } catch (err) {
      alert(err.response?.data?.message ?? 'Status dəyişmədi.');
    } finally {
      setBusyId(null);
    }
  };

  const totalPages = Math.max(1, Math.ceil(data.totalCount / PAGE_SIZE));

  return (
    <div className="page-wrap">
      <Link to="/admin" className="see-all">← Admin panelə qayıt</Link>
      <h1 className="page-title" style={{ marginTop: 16 }}>Bütün sifarişlər ({data.totalCount})</h1>

      <div className="category-filters">
        <button className={`filter-chip ${status === '' ? 'active' : ''}`} onClick={() => changeFilter('')}>
          Hamısı
        </button>
        {STATUSES.map((s) => (
          <button
            key={s}
            className={`filter-chip ${status === s ? 'active' : ''}`}
            onClick={() => changeFilter(s)}
          >
            {STATUS_LABELS[s]}
          </button>
        ))}
      </div>

      {loading ? (
        <div className="state-msg">Yüklənir...</div>
      ) : error ? (
        <div className="state-msg">Xəta: {error}</div>
      ) : data.items.length === 0 ? (
        <div className="state-msg">Sifariş tapılmadı.</div>
      ) : (
        <div className="cart-list">
          {data.items.map((o) => {
            const d = details[o.id];
            return (
              <div key={o.id}>
                <div className="cart-row">
                  <div className="cart-item-info">
                    <p className="cart-item-name">Sifariş #{o.id} · {o.customerName}</p>
                    <p className="cart-item-type">
                      {o.customerEmail} · {new Date(o.createdAt).toLocaleDateString('az-AZ')} · {o.itemCount} ədəd
                    </p>
                  </div>
                  <span className={`status-badge ${STATUS_CLASS[o.status] ?? ''}`}>
                    {STATUS_LABELS[o.status] ?? o.status}
                  </span>
                  <p className="cart-item-price">{o.totalAmount.toLocaleString('az-AZ')} AZN</p>
                  <button className="filter-chip" onClick={() => toggleDetails(o.id)}>
                    {openId === o.id ? 'Bağla' : 'Ətraflı'}
                  </button>
                </div>

                {openId === o.id && (
                  <div style={{ padding: '16px 4px 24px' }}>
                    {!d ? (
                      <div className="state-msg">Yüklənir...</div>
                    ) : (
                      <>
                        <div className="cart-summary" style={{ marginBottom: 16 }}>
                          <div>
                            <p className="cart-item-type">Ünvan</p>
                            <p className="cart-item-name">{d.shippingAddress}</p>
                          </div>
                          <div>
                            <p className="cart-item-type">Əlaqə</p>
                            <p className="cart-item-name">{d.contactPhone}</p>
                          </div>
                        </div>

                        <div className="cart-list" style={{ marginBottom: 16 }}>
                          {d.items.map((item, idx) => (
                            <div className="cart-row" key={idx}>
                              <div className="cart-item-info">
                                <p className="cart-item-name">{itemName(item)}</p>
                                <p className="cart-item-type">{itemType(item)} · {item.quantity} ədəd</p>
                              </div>
                              <p className="cart-item-price">
                                {(item.unitPriceAtOrderTime * item.quantity).toLocaleString('az-AZ')} AZN
                              </p>
                            </div>
                          ))}
                        </div>

                        <p className="cart-item-type" style={{ marginBottom: 8 }}>Statusu dəyiş:</p>
                        <div className="category-filters" style={{ marginBottom: 0 }}>
                          {STATUSES.map((s) => (
                            <button
                              key={s}
                              className={`filter-chip ${d.status === s ? 'active' : ''}`}
                              disabled={busyId === o.id || d.status === s}
                              onClick={() => changeStatus(o.id, s)}
                            >
                              {STATUS_LABELS[s]}
                            </button>
                          ))}
                        </div>
                      </>
                    )}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}

      {totalPages > 1 && (
        <div className="category-filters" style={{ marginTop: 24, alignItems: 'center' }}>
          <button className="filter-chip" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
            ← Əvvəlki
          </button>
          <span className="cart-item-type">{page} / {totalPages}</span>
          <button className="filter-chip" disabled={page >= totalPages} onClick={() => setPage((p) => p + 1)}>
            Sonrakı →
          </button>
        </div>
      )}
    </div>
  );
}