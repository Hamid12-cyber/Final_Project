import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import apiClient from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';
import { STATUS_LABELS } from './OrdersPage.jsx';

const TYPE_LABELS = { motorcycle: 'Motosiklet', part: 'Ehtiyat hissəsi', accessory: 'Aksesuar' };
const STATUS_COLORS = {
  Pending: '#ffc107',
  Confirmed: '#2196f3',
  Shipped: '#ba68c8',
  Delivered: '#4caf50',
  Cancelled: '#e53935',
};

const money = (n) => `${Number(n).toLocaleString('az-AZ', { maximumFractionDigits: 2 })} AZN`;
const shortDate = (iso) => {
  const [, m, d] = iso.split('-');
  return `${d}.${m}`;
};

export default function AdminDashboardPage() {
  const { user } = useAuth();
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    apiClient.get('/admin/stats')
      .then((res) => setStats(res.data))
      .catch((err) => setError(err.response?.data?.message ?? err.message))
      .finally(() => setLoading(false));
  }, []);

  if (!user || user.role !== 'Admin') {
    return (
      <div className="page-wrap">
        <h1 className="page-title">Statistika</h1>
        <div className="state-msg">Bu səhifəyə giriş icazəniz yoxdur.</div>
      </div>
    );
  }

  if (loading) return <div className="state-msg">Yüklənir...</div>;
  if (error) return <div className="state-msg">Xəta: {error}</div>;
  if (!stats) return null;

  const maxRevenue = Math.max(...stats.last30Days.map((d) => d.revenue), 0);
  const totalByStatus = stats.ordersByStatus.reduce((sum, s) => sum + s.count, 0);
  const days = stats.last30Days;

  const cards = [
    { label: 'Ümumi gəlir', value: money(stats.totalRevenue), accent: true },
    { label: 'Son 30 gün gəliri', value: money(stats.revenueLast30Days) },
    { label: 'Ümumi sifariş', value: stats.totalOrders },
    { label: 'Gözləyən sifariş', value: stats.pendingOrders },
    { label: 'İstifadəçilər', value: stats.totalUsers },
    { label: 'Təsdiq gözləyən elan', value: stats.pendingListings },
  ];

  return (
    <div className="page-wrap">
      <Link to="/admin" className="see-all">← Admin panelə qayıt</Link>
      <h1 className="page-title" style={{ marginTop: 16 }}>Statistika</h1>

      <div className="stats-grid">
        {cards.map((c) => (
          <div className="stat-card" key={c.label}>
            <p className="stat-label">{c.label}</p>
            <p className={`stat-value ${c.accent ? 'accent' : ''}`}>{c.value}</p>
          </div>
        ))}
      </div>

      <div className="dash-grid">
        <div className="chart-card">
          <h2 className="chart-title">Son 30 gün: gəlir</h2>
          <div className="bar-chart">
            {days.map((d) => (
              <div
                className="bar-col"
                key={d.date}
                title={`${shortDate(d.date)}: ${money(d.revenue)} · ${d.orders} sifariş`}
              >
                <div
                  className={`bar ${d.revenue === 0 ? 'empty' : ''}`}
                  style={{ height: maxRevenue > 0 ? `${Math.max((d.revenue / maxRevenue) * 100, 2)}%` : '2px' }}
                />
              </div>
            ))}
          </div>
          <div className="bar-axis">
            <span>{shortDate(days[0].date)}</span>
            <span>{shortDate(days[Math.floor(days.length / 2)].date)}</span>
            <span>{shortDate(days[days.length - 1].date)}</span>
          </div>
        </div>

        <div className="chart-card">
          <h2 className="chart-title">Sifarişlər statuslara görə</h2>
          {stats.ordersByStatus.map((s) => (
            <div className="status-row" key={s.status}>
              <div className="status-row-head">
                <span>{STATUS_LABELS[s.status] ?? s.status}</span>
                <span>{s.count}</span>
              </div>
              <div className="status-track">
                <div
                  className="status-fill"
                  style={{
                    width: totalByStatus > 0 ? `${(s.count / totalByStatus) * 100}%` : '0%',
                    background: STATUS_COLORS[s.status],
                  }}
                />
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="chart-card">
        <h2 className="chart-title">Ən çox satılanlar</h2>
        {stats.topProducts.length === 0 ? (
          <p className="stat-label">Hələ satış yoxdur.</p>
        ) : (
          stats.topProducts.map((p, idx) => (
            <div className="top-row" key={`${p.type}-${p.id}`}>
              <span className="top-rank">{idx + 1}</span>
              <div className="top-info">
                <p className="cart-item-name">{p.name}</p>
                <p className="cart-item-type">{TYPE_LABELS[p.type] ?? p.type} · {p.soldQty} ədəd satılıb</p>
              </div>
              <p className="cart-item-price">{money(p.revenue)}</p>
            </div>
          ))
        )}
      </div>
    </div>
  );
}