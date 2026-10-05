import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import apiClient from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';

const TYPES = [
  { key: 'motorcycle', label: 'Motosiklet' },
  { key: 'part', label: 'Ehtiyat hissəsi' },
  { key: 'accessory', label: 'Aksesuar' },
];

// Siyahıda olmayan marka/model üçün "Digər" seçimi
const OTHER = '__other__';

const byLabel = (a, b) => a.label.localeCompare(b.label, 'az');

export default function CreateListingPage() {
  const { user } = useAuth();
  const [type, setType] = useState('motorcycle');
  const [categories, setCategories] = useState([]);

  const [name, setName] = useState('');
  const [brand, setBrand] = useState('');
  const [customBrand, setCustomBrand] = useState('');
  const [price, setPrice] = useState('');
  const [imageUrl, setImageUrl] = useState('');
  const [model, setModel] = useState('');
  const [customModel, setCustomModel] = useState('');
  const [cc, setCc] = useState('');
  const [year, setYear] = useState('');
  const [stockQty, setStockQty] = useState('');
  const [partCategoryId, setPartCategoryId] = useState('');

  const [brandOptions, setBrandOptions] = useState([]);
  const [modelOptions, setModelOptions] = useState([]);

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

  // Motosiklet üçün marka siyahısı backend-dən gəlir
  useEffect(() => {
    if (type !== 'motorcycle') return;
    apiClient.get('/motorcycles/filters')
      .then((res) => setBrandOptions([...res.data.brands].sort(byLabel)))
      .catch(() => setBrandOptions([]));
  }, [type]);

  // Marka seçiləndə həmin markanın modelləri gəlir
  useEffect(() => {
    if (type !== 'motorcycle' || !brand || brand === OTHER) {
      setModelOptions([]);
      return;
    }
    apiClient.get('/motorcycles/filters', { params: { brand } })
      .then((res) =>
        setModelOptions(
          res.data.models
            .filter((m) => m.label.toLowerCase() !== 'digər')
            .sort(byLabel)
        )
      )
      .catch(() => setModelOptions([]));
  }, [type, brand]);

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

  const clearBrandModel = () => {
    setBrand(''); setCustomBrand('');
    setModel(''); setCustomModel('');
  };

  const changeType = (key) => {
    setType(key);
    clearBrandModel();
  };

  const handleBrandChange = (value) => {
    setBrand(value);
    setModel('');
    setCustomModel('');
  };

  const resetForm = () => {
    setName(''); setPrice(''); setImageUrl('');
    setCc(''); setYear(''); setStockQty('');
    clearBrandModel();
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setFormError(null);

    const finalBrand = type === 'motorcycle' && brand === OTHER ? customBrand.trim() : brand;
    const finalModel = brand === OTHER || model === OTHER ? customModel.trim() : model;

    if (type === 'motorcycle' && (!finalBrand || !finalModel)) {
      setFormError('Marka və modeli seçin (və ya "Digər" seçib yazın).');
      return;
    }

    setSubmitting(true);
    try {
      let res;
      if (type === 'motorcycle') {
        res = await apiClient.post('/motorcycles', {
          name, brand: finalBrand, model: finalModel, cc: Number(cc), year: Number(year),
          price: Number(price), imageUrl: imageUrl || null,
        });
      } else if (type === 'part') {
        res = await apiClient.post('/parts', {
          name, brand: finalBrand, price: Number(price), stockQty: Number(stockQty),
          imageUrl: imageUrl || null, partCategoryId: Number(partCategoryId),
        });
      } else {
        res = await apiClient.post('/accessories', {
          name, brand: finalBrand, price: Number(price), stockQty: Number(stockQty),
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
            onClick={() => changeType(t.key)}
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

        {type === 'motorcycle' ? (
          <>
            <label className="form-field">
              <span>Marka</span>
              <select value={brand} onChange={(e) => handleBrandChange(e.target.value)} required>
                <option value="">Marka seçin</option>
                {brandOptions.map((o) => (
                  <option key={o.value} value={o.value}>{o.label}</option>
                ))}
                <option value={OTHER}>Digər (özüm yazacağam)</option>
              </select>
            </label>

            {brand === OTHER && (
              <label className="form-field">
                <span>Markanın adı</span>
                <input type="text" value={customBrand} onChange={(e) => setCustomBrand(e.target.value)} required maxLength={80} />
              </label>
            )}

            {brand !== OTHER && (
              <label className="form-field">
                <span>Model</span>
                <select value={model} onChange={(e) => setModel(e.target.value)} required disabled={!brand}>
                  <option value="">Model seçin</option>
                  {modelOptions.map((o) => (
                    <option key={o.value} value={o.value}>{o.label}</option>
                  ))}
                  <option value={OTHER}>Digər (özüm yazacağam)</option>
                </select>
              </label>
            )}

            {(brand === OTHER || model === OTHER) && (
              <label className="form-field">
                <span>Modelin adı</span>
                <input type="text" value={customModel} onChange={(e) => setCustomModel(e.target.value)} required maxLength={80} />
              </label>
            )}

            <label className="form-field">
              <span>Həcm (cc)</span>
              <input type="number" value={cc} onChange={(e) => setCc(e.target.value)} required min={1} />
            </label>
            <label className="form-field">
              <span>İl</span>
              <input type="number" value={year} onChange={(e) => setYear(e.target.value)} required min={1980} max={new Date().getFullYear() + 1} />
            </label>
          </>
        ) : (
          <label className="form-field">
            <span>Marka</span>
            <input type="text" value={brand} onChange={(e) => setBrand(e.target.value)} required maxLength={80} />
          </label>
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