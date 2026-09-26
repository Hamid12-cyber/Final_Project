import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import apiClient from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';

const TYPES = [
  { key: 'motorcycle', label: 'Motosiklet' },
  { key: 'part', label: 'Ehtiyat hissəsi' },
  { key: 'accessory', label: 'Aksesuar' },
];

export default function CreateListingPage() {
  const { user } = useAuth();
  const [type, setType] = useState('motorcycle');
  const [categories, setCategories] = useState([]);

  const [name, setName] = useState('');
  const [brand, setBrand] = useState('');
  const [price, setPrice] = useState('');
  const [imageUrl, setImageUrl] = useState('');
  const [model, setModel] = useState('');
  const [cc, setCc] = useState('');
  const [year, setYear] = useState('');
  const [stockQty, setStockQty] = useState('');
  const [partCategoryId, setPartCategoryId] = useState('');

  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState(null);
  const [confirmation, setConfirmation] = useState(null);

  useEffect(() => {
    if (type === 'part') {
      apiClient.get('/part-categories')
        .then((res) => {
          setCategories(res.data);
          if (res.data.length > 0) setPartCategoryId(String(res.data[0].id));
        })
        .catch(() => setCategories([]));
    }
  }, [type]);

  if (!user) {
    return (
      <div className="page-wrap">
        <h1 className="page-title">Elan yerləşdir</h1>
        <div className="state-msg">
          Elan yerləşdirmək üçün <Link to="/login" className="accent">daxil olun</Link>.
        </div>
      </div>
    );
  }

  const resetForm = () => {
    setName(''); setBrand(''); setPrice(''); setImageUrl('');
    setModel(''); setCc(''); setYear(''); setStockQty('');
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setFormError(null);
    setSubmitting(true);
    try {
      let res;
      if (type === 'motorcycle') {
        res = await apiClient.post('/motorcycles', {
          name, brand, model, cc: Number(cc), year: Number(year),
          price: Number(price), imageUrl: imageUrl || null,
        });
      } else if (type === 'part') {
        res = await apiClient.post('/parts', {
          name, brand, price: Number(price), stockQty: Number(stockQty),
          imageUrl: imageUrl || null, partCategoryId: Number(partCategoryId),
        });
      } else {
        res = await apiClient.post('/accessories', {
          name, brand, price: Number(price), stockQty: Number(stockQty),
          imageUrl: imageUrl || null,
        });
      }
      setConfirmation(res.data.id);
      resetForm();
    } catch (err) {
      setFormError(err.response?.data?.message ?? 'Elan göndərilmədi.');
    } finally {
      setSubmitting(false);
    }
  };

  if (confirmation) {
    return (
      <div className="page-wrap">
        <h1 className="page-title">Elan göndərildi ✅</h1>
        <div className="state-msg" style={{ textAlign: 'left' }}>
          Elanınız (ID: {confirmation}) admin təsdiqi gözləyir. Təsdiqləndikdən sonra saytda görünəcək.
        </div>
        <button className="checkout-btn" style={{ marginTop: 20 }} onClick={() => setConfirmation(null)}>
          Yeni elan yerləşdir
        </button>
      </div>
    );
  }

  return (
    <div className="page-wrap">
      <h1 className="page-title">Elan yerləşdir</h1>

      <div className="listing-type-tabs">
        {TYPES.map((t) => (
          <button
            key={t.key}
            type="button"
            className={`filter-chip ${type === t.key ? 'active' : ''}`}
            onClick={() => setType(t.key)}
          >
            {t.label}
          </button>
        ))}
      </div>

      <form className="checkout-form" style={{ maxWidth: 480 }} onSubmit={handleSubmit}>
        <label className="form-field">
          <span>Ad</span>
          <input type="text" value={name} onChange={(e) => setName(e.target.value)} required maxLength={150} />
        </label>

        <label className="form-field">
          <span>Marka</span>
          <input type="text" value={brand} onChange={(e) => setBrand(e.target.value)} required maxLength={80} />
        </label>

        {type === 'motorcycle' && (
          <>
            <label className="form-field">
              <span>Model</span>
              <input type="text" value={model} onChange={(e) => setModel(e.target.value)} required maxLength={80} />
            </label>
            <label className="form-field">
              <span>Həcm (cc)</span>
              <input type="number" value={cc} onChange={(e) => setCc(e.target.value)} required min={1} />
            </label>
            <label className="form-field">
              <span>İl</span>
              <input type="number" value={year} onChange={(e) => setYear(e.target.value)} required min={1980} max={new Date().getFullYear() + 1} />
            </label>
          </>
        )}

        {type === 'part' && (
          <>
            <label className="form-field">
              <span>Kateqoriya</span>
              <select value={partCategoryId} onChange={(e) => setPartCategoryId(e.target.value)} required>
                {categories.map((c) => (
                  <option key={c.id} value={c.id}>{c.icon} {c.name}</option>
                ))}
              </select>
            </label>
            <label className="form-field">
              <span>Stok miqdarı</span>
              <input type="number" value={stockQty} onChange={(e) => setStockQty(e.target.value)} required min={0} />
            </label>
          </>
        )}

        {type === 'accessory' && (
          <label className="form-field">
            <span>Stok miqdarı</span>
            <input type="number" value={stockQty} onChange={(e) => setStockQty(e.target.value)} required min={0} />
          </label>
        )}

        <label className="form-field">
          <span>Qiymət (AZN)</span>
          <input type="number" value={price} onChange={(e) => setPrice(e.target.value)} required min={0.01} step="0.01" />
        </label>

        <label className="form-field">
          <span>Şəkil URL-i (istəyə görə)</span>
          <input type="text" value={imageUrl} onChange={(e) => setImageUrl(e.target.value)} placeholder="https://..." />
        </label>

        {formError && <p className="form-error">{formError}</p>}

        <button className="checkout-btn" type="submit" disabled={submitting}>
          {submitting ? 'Göndərilir...' : 'Elanı göndər'}
        </button>
      </form>
    </div>
  );
}