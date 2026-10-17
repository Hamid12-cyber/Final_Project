import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import apiClient from '../api/client.js';
import ImageUpload from '../components/ImageUpload.jsx';

const TYPE_LABELS = { motorcycle: 'Motosiklet', part: 'Ehtiyat hissəsi', accessory: 'Aksesuar' };
const TYPE_ENDPOINT = { motorcycle: 'motorcycles', part: 'parts', accessory: 'accessories' };

const STATUS_META = {
  Pending: { label: 'Gözləyir', cls: 'status-pending' },
  Approved: { label: 'Təsdiqlənib', cls: 'status-delivered' },
  Rejected: { label: 'Rədd edilib', cls: 'status-cancelled' },
};

const TYPE_FILTERS = [
  { key: 'all', label: 'Hamısı' },
  { key: 'motorcycle', label: 'Motosikletlər' },
  { key: 'part', label: 'Hissələr' },
  { key: 'accessory', label: 'Aksesuarlar' },
];

const STATUS_FILTERS = [
  { key: 'all', label: 'Bütün statuslar' },
  { key: 'Pending', label: 'Gözləyir' },
  { key: 'Approved', label: 'Təsdiqlənib' },
  { key: 'Rejected', label: 'Rədd edilib' },
];

const itemKey = (i) => `${i.type}-${i.id}`;

export default function SellerPage() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const [typeFilter, setTypeFilter] = useState('all');
  const [statusFilter, setStatusFilter] = useState('all');

  const [editing, setEditing] = useState(null);   // redaktə olunan elanın form halı
  const [categories, setCategories] = useState([]);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState(null);

  const [deletingKey, setDeletingKey] = useState(null);
  const [notice, setNotice] = useState(null);

  const load = () => {
    setLoading(true);
    setError(null);
    apiClient.get('/seller/listings')
      .then((res) => setItems(res.data))
      .catch((err) => setError(err.response?.data?.message ?? 'Elanlar yüklənmədi.'))
      .finally(() => setLoading(false));
  };

  useEffect(load, []);

  const counts = useMemo(() => {
    const c = { Pending: 0, Approved: 0, Rejected: 0 };
    items.forEach((i) => { if (c[i.status] !== undefined) c[i.status] += 1; });
    return c;
  }, [items]);

  const visible = items.filter((i) =>
    (typeFilter === 'all' || i.type === typeFilter) &&
    (statusFilter === 'all' || i.status === statusFilter));

  // ---------- Redaktə ----------
  const openEdit = (item) => {
    setFormError(null);
    setEditing({
      key: itemKey(item),
      type: item.type,
      id: item.id,
      name: item.name,
      brand: item.brand,
      price: String(item.price),
      imageUrl: item.imageUrl ?? '',
      stockQty: String(item.stockQty ?? 0),
      model: item.model ?? '',
      cc: String(item.cc ?? ''),
      year: String(item.year ?? ''),
      isForRent: !!item.isForRent,
      isForSale: item.isForSale ?? true,
      partCategoryId: String(item.partCategoryId ?? ''),
    });
    if (item.type === 'part' && categories.length === 0) {
      apiClient.get('/part-categories').then((res) => setCategories(res.data)).catch(() => setCategories([]));
    }
  };

  const setField = (field, value) => setEditing((prev) => ({ ...prev, [field]: value }));

  const buildBody = (f) => {
    const common = { name: f.name.trim(), brand: f.brand.trim(), price: Number(f.price), imageUrl: f.imageUrl || null };
    if (f.type === 'motorcycle') {
      return { ...common, model: f.model.trim(), cc: Number(f.cc), year: Number(f.year), isForRent: f.isForRent, isForSale: f.isForSale };
    }
    if (f.type === 'part') {
      return { ...common, stockQty: Number(f.stockQty), partCategoryId: Number(f.partCategoryId) };
    }
    return { ...common, stockQty: Number(f.stockQty) };
  };

  const handleSave = async (e) => {
    e.preventDefault();
    setFormError(null);
    setSaving(true);
    try {
      const body = buildBody(editing);
      await apiClient.put(`/${TYPE_ENDPOINT[editing.type]}/${editing.id}`, body);

      setItems((prev) => prev.map((i) =>
        itemKey(i) === editing.key
          ? {
              ...i,
              name: body.name, brand: body.brand, price: body.price, imageUrl: body.imageUrl,
              ...(editing.type === 'motorcycle'
                ? { model: body.model, cc: body.cc, year: body.year, isForRent: body.isForRent, isForSale: body.isForSale }
                : { stockQty: body.stockQty }),
              ...(editing.type === 'part' ? { partCategoryId: body.partCategoryId } : {}),
            }
          : i));
      setEditing(null);
      setNotice('Elan yeniləndi.');
    } catch (err) {
      const errors = err.response?.data?.errors;
      const firstValidation = errors ? Object.values(errors).flat()[0] : null;
      setFormError(firstValidation ?? err.response?.data?.message ?? 'Elan yenilənmədi.');
    } finally {
      setSaving(false);
    }
  };

  // ---------- Silmə ----------
  const handleDelete = async (item) => {
    if (!window.confirm(`"${item.name}" elanını silmək istəyirsən?`)) return;
    setDeletingKey(itemKey(item));
    try {
      await apiClient.delete(`/${TYPE_ENDPOINT[item.type]}/${item.id}`);
      setItems((prev) => prev.filter((i) => itemKey(i) !== itemKey(item)));
      setNotice('Elan silindi.');
    } catch (err) {
      setNotice(err.response?.data?.message ?? 'Elan silinmədi.');
    } finally {
      setDeletingKey(null);
    }
  };

  return (
    <div className="page-wrap">
      <div className="seller-head">
        <h1 className="page-title" style={{ marginBottom: 0 }}>Elanlarım</h1>
        <Link to="/sell" className="checkout-btn seller-new-btn">+ Yeni elan</Link>
      </div>

      <div className="seller-stats">
        {Object.entries(STATUS_META).map(([key, meta]) => (
          <button
            key={key}
            type="button"
            className={`seller-stat ${statusFilter === key ? 'active' : ''}`}
            onClick={() => setStatusFilter(statusFilter === key ? 'all' : key)}
          >
            <span className="seller-stat-value">{counts[key]}</span>
            <span className={`status-badge ${meta.cls}`}>{meta.label}</span>
          </button>
        ))}
      </div>

      <div className="category-filters">
        {TYPE_FILTERS.map((t) => (
          <button
            key={t.key}
            type="button"
            className={`filter-chip ${typeFilter === t.key ? 'active' : ''}`}
            onClick={() => setTypeFilter(t.key)}
          >
            {t.label}
          </button>
        ))}
        <select
          className="seller-status-select"
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
        >
          {STATUS_FILTERS.map((s) => <option key={s.key} value={s.key}>{s.label}</option>)}
        </select>
      </div>

      {notice && (
        <p className="seller-notice" onClick={() => setNotice(null)}>{notice}</p>
      )}

      {loading ? (
        <div className="state-msg">Yüklənir...</div>
      ) : error ? (
        <div className="state-msg">
          Xəta: {error} <button type="button" className="seller-btn" onClick={load}>Yenidən yoxla</button>
        </div>
      ) : items.length === 0 ? (
        <div className="state-msg">
          Hələ heç bir elanın yoxdur. <Link to="/sell" className="see-all">İlk elanını yerləşdir →</Link>
        </div>
      ) : visible.length === 0 ? (
        <div className="state-msg">Bu filtrə uyğun elan yoxdur.</div>
      ) : (
        <div className="cart-list">
          {visible.map((item) => {
            const meta = STATUS_META[item.status] ?? { label: item.status, cls: '' };
            return (
              <div className="cart-row seller-row" key={itemKey(item)}>
                <div className="seller-thumb">
                  {item.imageUrl ? <img src={item.imageUrl} alt={item.name} /> : <span>🏍️</span>}
                </div>
                <div className="cart-item-info">
                  <p className="cart-item-name">{item.name}</p>
                  <p className="cart-item-type">
                    {item.brand} · {TYPE_LABELS[item.type]}
                    {item.stockQty !== null && item.stockQty !== undefined && ` · Stok: ${item.stockQty}`}
                  </p>
                </div>
                <span className={`status-badge ${meta.cls}`}>{meta.label}</span>
                <p className="cart-item-price">{item.price.toLocaleString('az-AZ')} AZN</p>
                <div className="seller-actions">
                  <button type="button" className="seller-btn" onClick={() => openEdit(item)}>Redaktə</button>
                  <button
                    type="button"
                    className="seller-btn seller-btn-danger"
                    onClick={() => handleDelete(item)}
                    disabled={deletingKey === itemKey(item)}
                  >
                    {deletingKey === itemKey(item) ? '...' : 'Sil'}
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {editing && (
        <div className="modal-backdrop" onMouseDown={(e) => { if (e.target === e.currentTarget && !saving) setEditing(null); }}>
          <form className="modal-card checkout-form" onSubmit={handleSave}>
            <h2 className="modal-title">{TYPE_LABELS[editing.type]} elanını redaktə et</h2>

            <label className="form-field">
              <span>Ad</span>
              <input type="text" value={editing.name} onChange={(e) => setField('name', e.target.value)} required maxLength={150} />
            </label>

            <label className="form-field">
              <span>Marka</span>
              <input type="text" value={editing.brand} onChange={(e) => setField('brand', e.target.value)} required maxLength={80} />
            </label>

            {editing.type === 'motorcycle' && (
              <>
                <label className="form-field">
                  <span>Model</span>
                  <input type="text" value={editing.model} onChange={(e) => setField('model', e.target.value)} required maxLength={80} />
                </label>
                <div className="seller-two-cols">
                  <label className="form-field">
                    <span>Həcm (cc)</span>
                    <input type="number" min="1" value={editing.cc} onChange={(e) => setField('cc', e.target.value)} required />
                  </label>
                  <label className="form-field">
                    <span>İl</span>
                    <input type="number" min="1980" max={new Date().getFullYear() + 1} value={editing.year} onChange={(e) => setField('year', e.target.value)} required />
                  </label>
                </div>
              </>
            )}

            {editing.type === 'part' && (
              <label className="form-field">
                <span>Kateqoriya</span>
                <select value={editing.partCategoryId} onChange={(e) => setField('partCategoryId', e.target.value)} required>
                  {categories.length === 0 && <option value={editing.partCategoryId}>Yüklənir...</option>}
                  {categories.map((c) => <option key={c.id} value={c.id}>{c.icon} {c.name}</option>)}
                </select>
              </label>
            )}

            <div className="seller-two-cols">
              <label className="form-field">
                <span>Qiymət (AZN)</span>
                <input type="number" min="0.01" step="0.01" value={editing.price} onChange={(e) => setField('price', e.target.value)} required />
              </label>
              {editing.type !== 'motorcycle' && (
                <label className="form-field">
                  <span>Stok</span>
                  <input type="number" min="0" value={editing.stockQty} onChange={(e) => setField('stockQty', e.target.value)} required />
                </label>
              )}
            </div>

            {editing.type === 'motorcycle' && (
              <div className="seller-checks">
                <label className="check-row">
                  <input type="checkbox" checked={editing.isForSale} onChange={(e) => setField('isForSale', e.target.checked)} />
                  Satışa çıxarılıb
                </label>
                <label className="check-row">
                  <input type="checkbox" checked={editing.isForRent} onChange={(e) => setField('isForRent', e.target.checked)} />
                  Kirayə verilir
                </label>
              </div>
            )}

            <ImageUpload value={editing.imageUrl} onChange={(url) => setField('imageUrl', url)} />

            {formError && <p className="form-error">{formError}</p>}

            <div className="seller-modal-buttons">
              <button type="button" className="seller-btn" onClick={() => setEditing(null)} disabled={saving}>Ləğv et</button>
              <button type="submit" className="checkout-btn" disabled={saving}>{saving ? 'Saxlanılır...' : 'Yadda saxla'}</button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
}