import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';
import apiClient from '../api/client.js';

const TABS = [
  { key: 'motorcycles', label: 'Motosikletlər' },
  { key: 'parts', label: 'Ehtiyat hissələri' },
  { key: 'accessories', label: 'Aksesuarlar' },
];

export default function AdminPage() {
  const { user } = useAuth();
  const [tab, setTab] = useState('motorcycles');
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [busyId, setBusyId] = useState(null);
  const [resyncing, setResyncing] = useState(false);
  const [resyncReport, setResyncReport] = useState(null);
  const [rejecting, setRejecting] = useState(null); // rədd edilən elan
  const [reason, setReason] = useState('');
  const [rejectError, setRejectError] = useState(null);

  const load = useCallback(() => {
    setLoading(true);
    apiClient.get(`/admin/${tab}/pending`)
      .then((res) => {
        setItems(res.data);
        setLoading(false);
      })
      .catch((err) => {
        setError(err.response?.data?.message ?? err.message);
        setLoading(false);
      });
  }, [tab]);

  useEffect(() => {
    setError(null);
    load();
  }, [load]);

  if (!user || user.role !== 'Admin') {
    return (
      <div className="page-wrap">
        <h1 className="page-title">Admin panel</h1>
        <div className="state-msg">Bu səhifəyə giriş icazəniz yoxdur.</div>
      </div>
    );
  }

  const handleDecision = async (id, decision) => {
    setBusyId(id);
    try {
      await apiClient.put(`/admin/${tab}/${id}/${decision}`);
      setItems((prev) => prev.filter((i) => i.id !== id));
    } catch (err) {
      alert(err.response?.data?.message ?? 'Əməliyyat uğursuz oldu.');
    } finally {
      setBusyId(null);
    }
  };

  const openReject = (item) => {
    setRejecting(item);
    setReason('');
    setRejectError(null);
  };

  const handleReject = async (e) => {
    e.preventDefault();
    if (!reason.trim()) {
      setRejectError('Rədd səbəbi yazılmalıdır.');
      return;
    }
    setBusyId(rejecting.id);
    setRejectError(null);
    try {
      await apiClient.put(`/admin/${tab}/${rejecting.id}/reject`, { reason: reason.trim() });
      setItems((prev) => prev.filter((i) => i.id !== rejecting.id));
      setRejecting(null);
    } catch (err) {
      const errors = err.response?.data?.errors;
      const firstValidation = errors ? Object.values(errors).flat()[0] : null;
      setRejectError(firstValidation ?? err.response?.data?.message ?? 'Əməliyyat uğursuz oldu.');
    } finally {
      setBusyId(null);
    }
  };

  const handleResync = async () => {
    setResyncing(true);
    setResyncReport(null);
    try {
      const res = await apiClient.post('/admin/backup/resync');
      setResyncReport(res.data);
    } catch (err) {
      alert(err.response?.data?.message ?? 'Backup sinxronizasiyası uğursuz oldu.');
    } finally {
      setResyncing(false);
    }
  };

  return (
    <div className="page-wrap">
      <h1 className="page-title">Admin panel</h1>

            <p style={{ marginBottom: 20, display: 'flex', gap: 20, flexWrap: 'wrap' }}>
        <Link to="/admin/dashboard" className="see-all">Statistika →</Link>
        <Link to="/admin/orders" className="see-all">Bütün sifarişlərə bax →</Link>
      </p>

      <div className="category-filters">
        {TABS.map((t) => (
          <button
            key={t.key}
            className={`filter-chip ${tab === t.key ? 'active' : ''}`}
            onClick={() => setTab(t.key)}
          >
            {t.label}
          </button>
        ))}
      </div>

      {loading ? (
        <div className="state-msg">Yüklənir...</div>
      ) : error ? (
        <div className="state-msg">Xəta: {error}</div>
      ) : items.length === 0 ? (
        <div className="state-msg">Təsdiq gözləyən elan yoxdur.</div>
      ) : (
        <div className="cart-list" style={{ marginBottom: 32 }}>
          {items.map((item) => (
            <div className="cart-row" key={item.id}>
              <div className="cart-item-info">
                <p className="cart-item-name">{item.name}</p>
                <p className="cart-item-type">{item.brand} · Satıcı: {item.sellerName}</p>
              </div>
              <p className="cart-item-price">{item.price.toLocaleString('az-AZ')} AZN</p>
              <div style={{ display: 'flex', gap: 8 }}>
                <button
                  className="admin-approve-btn"
                  disabled={busyId === item.id}
                  onClick={() => handleDecision(item.id, 'approve')}
                >
                  Təsdiqlə
                </button>
                <button
                  className="admin-reject-btn"
                  disabled={busyId === item.id}
                  onClick={() => openReject(item)}
                >
                  Rədd et
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      <div className="cart-summary" style={{ flexDirection: 'column', alignItems: 'flex-start', gap: 12 }}>
        <div>
          <p className="cart-item-name">Backup bazasını sinxronlaşdır</p>
          <p className="cart-item-type">Əsas bazadan (SQL Server) ehtiyat bazasına (PostgreSQL) tam köçürmə edir.</p>
        </div>
        <button className="checkout-btn" onClick={handleResync} disabled={resyncing}>
          {resyncing ? 'Sinxronlaşdırılır...' : 'Resync et'}
        </button>
        {resyncReport && (
          <div style={{ fontSize: 13, color: 'var(--muted)' }}>
            {Object.entries(resyncReport).map(([table, count]) => (
              <span key={table} style={{ marginRight: 14 }}>{table}: {count}</span>
            ))}
          </div>
        )}
      </div>

      {rejecting && (
        <div className="modal-backdrop" onMouseDown={(e) => { if (e.target === e.currentTarget && busyId === null) setRejecting(null); }}>
          <form className="modal-card" onSubmit={handleReject}>
            <h2 className="modal-title">Elanı rədd et</h2>
            <p className="cart-item-type">{rejecting.name} · Satıcı: {rejecting.sellerName}</p>
            <label className="form-field">
              <span>Rədd səbəbi (satıcı görəcək)</span>
              <textarea
                className="reject-textarea"
                value={reason}
                onChange={(e) => setReason(e.target.value)}
                maxLength={500}
                rows={4}
                autoFocus
                placeholder="Məs.: Şəkil keyfiyyətsizdir, qiymət səhvdir..."
              />
              <span style={{ alignSelf: 'flex-end', fontSize: 12 }}>{reason.length}/500</span>
            </label>
            {rejectError && <p className="form-error">{rejectError}</p>}
            <div className="seller-modal-buttons">
              <button type="button" className="seller-btn" onClick={() => setRejecting(null)} disabled={busyId !== null}>Ləğv et</button>
              <button type="submit" className="admin-reject-btn" disabled={busyId !== null}>Rədd et</button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
}