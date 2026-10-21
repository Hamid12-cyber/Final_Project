import { useState, useEffect, useMemo } from 'react';
import { Link } from 'react-router-dom';
import apiClient from '../api/client.js';
import { STATUS_LABELS, STATUS_CLASS, NEXT_ACTIONS } from '../utils/orderStatus.js';
import { itemName, itemType } from '../utils/itemInfo.js';

const PAGE_SIZE = 20;

const FILTERS = [
  { key: '', label: 'Hamısı' },
  { key: 'Pending', label: STATUS_LABELS.Pending },
  { key: 'Confirmed', label: STATUS_LABELS.Confirmed },
  { key: 'Shipped', label: STATUS_LABELS.Shipped },
  { key: 'Delivered', label: STATUS_LABELS.Delivered },
  { key: 'Cancelled', label: STATUS_LABELS.Cancelled },
];

export default function AdminOrdersPage() {
  const [orders, setOrders] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState('');
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const [busyId, setBusyId] = useState(null);
  const [notice, setNotice] = useState(null);

  const [openId, setOpenId] = useState(null);
  const [details, setDetails] = useState({}); // id -> detail
  const [detailLoading, setDetailLoading] = useState(false);

  const load = () => {
    setLoading(true);
    setError(null);
    const params = { page, pageSize: PAGE_SIZE };
    if (statusFilter) params.status = statusFilter;
    apiClient.get('/admin/orders', { params })
      .then((res) => {
        setOrders(res.data.items);
        setTotalCount(res.data.totalCount);
      })
      .catch((err) => setError(err.response?.data?.message ?? 'Sifarişlər yüklənmədi.'))
      .finally(() => setLoading(false));
  };

  useEffect(load, [page, statusFilter]);

  const changeFilter = (key) => {
    setStatusFilter(key);
    setPage(1);
    setOpenId(null);
  };

  const visible = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return orders;
    return orders.filter((o) =>
      String(o.id) === q.replace('#', '') ||
      o.customerName.toLowerCase().includes(q) ||
      o.customerEmail.toLowerCase().includes(q));
  }, [orders, search]);

  const toggleDetails = async (order) => {
    if (openId === order.id) {
      setOpenId(null);
      return;
    }
    setOpenId(order.id);
    if (details[order.id]) return;
    setDetailLoading(true);
    try {
      const res = await apiClient.get(`/admin/orders/${order.id}`);
      setDetails((prev) => ({ ...prev, [order.id]: res.data }));
    } catch {
      setNotice('Sifariş detalları yüklənmədi.');
    } finally {
      setDetailLoading(false);
    }
  };

  const changeStatus = async (order, action) => {
    if (action.danger && !window.confirm(`Sifariş #${order.id} ləğv edilsin? Stok geri qaytarılacaq.`)) return;
    setBusyId(order.id);
    setNotice(null);
    try {
      await apiClient.put(`/admin/orders/${order.id}/status`, { status: action.status });

      // Filtr aktivdirsə və yeni status filtrə uyğun deyilsə, siyahıdan çıxar
      if (statusFilter && statusFilter !== action.status) {
        setOrders((prev) => prev.filter((o) => o.id !== order.id));
        setTotalCount((c) => Math.max(0, c - 1));
      } else {
        setOrders((prev) => prev.map((o) => (o.id === order.id ? { ...o, status: action.status } : o)));
      }
      setDetails((prev) => (prev[order.id] ? { ...prev, [order.id]: { ...prev[order.id], status: action.status } } : prev));
      setNotice(`Sifariş #${order.id}: ${STATUS_LABELS[action.status]}.`);
    } catch (err) {
      const errors = err.response?.data?.errors;
      const firstValidation = errors ? Object.values(errors).flat()[0] : null;
      setNotice(firstValidation ?? err.response?.data?.message ?? 'Status dəyişmədi.');
    } finally {
      setBusyId(null);
    }
  };

  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE));

  return (
    <div className="page-wrap">
      <Link to="/admin" className="see-all">← Admin panelinə qayıt</Link>
      <h1 className="page-title" style={{ marginTop: 16 }}>Sifarişlər</h1>

      <div className="category-filters">
        {FILTERS.map((f) => (
          <button
            key={f.key || 'all'}
            type="button"
            className={`filter-chip ${statusFilter === f.key ? 'active' : ''}`}
            onClick={() => changeFilter(f.key)}
          >
            {f.label}
          </button>
        ))}
      </div>

      <input
        className="admin-search"
        type="text"
        placeholder="Sifariş nömrəsi, ad və ya email ilə axtar..."
        value={search}
        onChange={(e) => setSearch(e.target.value)}
      />

      {notice && <p className="seller-notice" onClick={() => setNotice(null)}>{notice}</p>}

      {loading ? (
        <div className="state-msg">Yüklənir...</div>
      ) : error ? (
        <div className="state-msg">
          Xəta: {error} <button type="button" className="seller-btn" onClick={load}>Yenidən yoxla</button>
        </div>
      ) : visible.length === 0 ? (
        <div className="state-msg">Sifariş tapılmadı.</div>
      ) : (
        <div className="cart-list">
          {visible.map((o) => {
            const actions = NEXT_ACTIONS[o.status] ?? [];
            const detail = details[o.id];
            return (
              <div className="admin-order" key={o.id}>
                <div className="admin-order-head">
                  <div className="cart-item-info">
                    <p className="cart-item-name">Sifariş #{o.id}</p>
                    <p className="cart-item-type">
                      {o.customerName} · {o.customerEmail}
                    </p>
                    <p className="cart-item-type">
                      {new Date(o.createdAt).toLocaleString('az-AZ')} · {o.itemCount} ədəd
                    </p>
                  </div>
                  <span className={`status-badge ${STATUS_CLASS[o.status] ?? ''}`}>
                    {STATUS_LABELS[o.status] ?? o.status}
                  </span>
                  <p className="cart-item-price">{o.totalAmount.toLocaleString('az-AZ')} AZN</p>
                </div>

                <div className="admin-order-actions">
                  <button type="button" className="seller-btn" onClick={() => toggleDetails(o)}>
                    {openId === o.id ? 'Detalları gizlət' : 'Detallara bax'}
                  </button>
                  {actions.map((a) => (
                    <button
                      key={a.status}
                      type="button"
                      className={`seller-btn ${a.danger ? 'seller-btn-danger' : 'seller-btn-primary'}`}
                      onClick={() => changeStatus(o, a)}
                      disabled={busyId === o.id}
                    >
                      {busyId === o.id ? '...' : a.label}
                    </button>
                  ))}
                </div>

                {openId === o.id && (
                  <div className="admin-order-detail">
                    <p className="cart-item-type">Ünvan: <strong>{o.shippingAddress}</strong></p>
                    <p className="cart-item-type">Telefon: <strong>{o.contactPhone}</strong></p>
                    {detailLoading && !detail ? (
                      <p className="cart-item-type">Yüklənir...</p>
                    ) : detail ? (
                      <ul className="admin-order-items">
                        {detail.items.map((item, idx) => (
                          <li key={idx}>
                            <span>{itemName(item)} <em>({itemType(item)})</em></span>
                            <span>{item.quantity} × {item.unitPriceAtOrderTime.toLocaleString('az-AZ')} AZN</span>
                          </li>
                        ))}
                      </ul>
                    ) : null}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}

      {totalPages > 1 && (
        <div className="admin-pager">
          <button type="button" className="seller-btn" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>‹ Əvvəlki</button>
          <span className="cart-item-type">{page} / {totalPages}</span>
          <button type="button" className="seller-btn" disabled={page >= totalPages} onClick={() => setPage((p) => p + 1)}>Növbəti ›</button>
        </div>
      )}
    </div>
  );
}