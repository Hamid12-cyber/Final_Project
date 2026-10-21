import { useState, useEffect, useMemo } from 'react';
import { Link } from 'react-router-dom';
import apiClient from '../api/client.js';
import { STATUS_LABELS } from '../utils/orderStatus.js';

const STATUS_COLORS = {
  Pending: '#ffc107',
  Confirmed: '#2196f3',
  Shipped: '#ba68c8',
  Delivered: '#4caf50',
  Cancelled: '#e53935',
};

const TYPE_LABELS = { motorcycle: 'Motosiklet', part: 'Ehtiyat hissəsi', accessory: 'Aksesuar' };

const fmt = (n) => Number(n ?? 0).toLocaleString('az-AZ', { maximumFractionDigits: 2 });

export default function AdminDashboardPage() {
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    apiClient.get('/admin/stats')
      .then((res) => setStats(res.data))
      .catch((err) => setError(err.response?.data?.message ?? 'Statistika yüklənmədi.'))
      .finally(() => setLoading(false));
  }, []);

  // Sifariş olmayan günlər də 0 kimi görünsün deyə son 30 günü tam qururuq
  const days = useMemo(() => {
    if (!stats) return [];
    const byDate = new Map(stats.last30Days.map((d) => [String(d.date).slice(0, 10), d]));
    return Array.from({ length: 30 }, (_, i) => {
      const date = new Date(Date.now() - (29 - i) * 86400000).toISOString().slice(0, 10);
      const d = byDate.get(date);
      return { date, orders: d?.orders ?? 0, revenue: d?.revenue ?? 0 };
    });
  }, [stats]);

  if (loading) return <div className="state-msg">Yüklənir...</div>;
  if (error) return <div className="state-msg">Xəta: {error}</div>;
  if (!stats) return null;

  const maxRevenue = Math.max(...days.map((d) => d.revenue), 1);
  const totalStatusCount = Math.max(stats.ordersByStatus.reduce((s, x) => s + x.count, 0), 1);

  const cards = [
    { label: 'Ümumi gəlir', value: `${fmt(stats.totalRevenue)} AZN` },
    { label: 'Son 30 gün gəlir', value: `${fmt(stats.revenueLast30Days)} AZN` },
    { label: 'Ümumi sifariş', value: fmt(stats.totalOrders) },
    { label: 'Gözləyən sifariş', value: fmt(stats.pendingOrders) },
    { label: 'İstifadəçilər', value: fmt(stats.totalUsers) },
    { label: 'Təsdiq gözləyən elan', value: fmt(stats.pendingListings) },
  ];

  return (
    <div className="page-wrap">
      <Link to="/admin" className="see-all">← Admin panelinə qayıt</Link>
      <h1 className="page-title" style={{ marginTop: 16 }}>Statistika</h1>

      <div className="stats-grid">
        {cards.map((c) => (
          <div className="stat-card" key={c.label}>
            <p className="stat-label">{c.label}</p>
            <p className="stat-value">{c.value}</p>
          </div>
        ))}
      </div>

      <div className="dash-grid">
        <div className="chart-card">
          <h2 className="chart-title">Son 30 gün gəlir (AZN)</h2>
          <div className="bar-chart">
            {days.map((d) => (
              <div className="bar-col" key={d.date} title={`${d.date}: ${fmt(d.revenue)} AZN, ${d.orders} sifariş`}>
                <div
                  className={`bar ${d.revenue === 0 ? 'empty' : ''}`}
                  style={{ height: d.revenue === 0 ? '2px' : `${Math.max(2, (d.revenue / maxRevenue) * 100)}%` }}
                />
              </div>
            ))}
          </div>
          <div className="bar-axis">
            <span>{days[0].date}</span>
            <span>{days[days.length - 1].date}</span>
          </div>
        </div>

        <div className="chart-card">
          <h2 className="chart-title">Sifarişlər statusa görə</h2>
          {stats.ordersByStatus.length === 0 ? (
            <p className="cart-item-type">Hələ sifariş yoxdur.</p>
          ) : (
            stats.ordersByStatus.map((s) => (
              <div className="status-row" key={s.status}>
                <div className="status-row-head">
                  <span>{STATUS_LABELS[s.status] ?? s.status}</span>
                  <strong>{s.count}</strong>
                </div>
                <div className="status-track">
                  <div
                    className="status-fill"
                    style={{ width: `${(s.count / totalStatusCount) * 100}%`, background: STATUS_COLORS[s.status] ?? '#888' }}
                  />
                </div>
              </div>
            ))
          )}
        </div>
      </div>

      <div className="chart-card">
        <h2 className="chart-title">Ən çox satılanlar</h2>
        {stats.topProducts.length === 0 ? (
          <p className="cart-item-type">Hələ satış yoxdur.</p>
        ) : (
          stats.topProducts.map((p, i) => (
            <div className="top-row" key={`${p.type}-${p.id}`}>
              <span className="top-rank">{i + 1}</span>
              <div className="top-info">
                <p className="cart-item-name">{p.name}</p>
                <p className="cart-item-type">{TYPE_LABELS[p.type] ?? p.type} · {p.soldQty} ədəd</p>
              </div>
              <strong>{fmt(p.revenue)} AZN</strong>
            </div>
          ))
        )}
      </div>
    </div>
  );
}