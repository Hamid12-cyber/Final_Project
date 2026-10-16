import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import apiClient from '../api/client.js';

const EMPTY_FILTERS = { brand: '', model: '', year: '', price: '' };
const EMPTY_OPTIONS = { brands: [], models: [], years: [], priceRanges: [] };

function SearchIcon() {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <circle cx="11" cy="11" r="8" />
      <line x1="21" y1="21" x2="16.65" y2="16.65" />
    </svg>
  );
}

export default function Hero() {
  const navigate = useNavigate();
  const [filters, setFilters] = useState(EMPTY_FILTERS);
  const [options, setOptions] = useState(EMPTY_OPTIONS);
  const [loaded, setLoaded] = useState(false);

  // Siyahılar və saylar backend-dən gəlir; seçim dəyişdikcə sayları yenidən alırıq
  useEffect(() => {
    apiClient.get('/motorcycles/filters', {
      params: {
        brand: filters.brand || undefined,
        model: filters.model || undefined,
        year: filters.year || undefined,
      },
    })
      .then((res) => { setOptions(res.data); setLoaded(true); })
      .catch(() => setLoaded(true));
  }, [filters.brand, filters.model, filters.year]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    // Marka dəyişəndə əvvəlki modelin mənası qalmır
    setFilters((prev) => ({ ...prev, [name]: value, ...(name === 'brand' ? { model: '' } : {}) }));
  };

  const handleSearch = () => {
    const params = new URLSearchParams();
    Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v); });
    const qs = params.toString();
    navigate(`/motorcycles${qs ? `?${qs}` : ''}`);
  };

  const renderOptions = (list) =>
    list.map((o) => (
      <option key={o.value} value={o.value}>{o.label} ({o.count})</option>
    ));

  return (
    <section className="hero">
      <div className="hero-content">
        <p className="hero-eyebrow">MOTOSİKLET DÜNYASINDA</p>
        <h1 className="hero-title">
          HƏR ŞEY<br />
          <span className="accent">BİR YERDƏ</span>
        </h1>
        <p className="hero-desc">
          Motosiklet al, kirayə götür, ehtiyat hissələri sifariş et, servis
          xidmətlərimizdən yararlan — hamısı MotoHM-də.
        </p>
      </div>

      <div className="hero-shape" aria-hidden="true"></div>

      <div className="search-bar">
        <select name="brand" value={filters.brand} onChange={handleChange}>
          <option value="">Marka seçin</option>
          {loaded && renderOptions(options.brands)}
        </select>
        <select name="model" value={filters.model} onChange={handleChange} disabled={!filters.brand}>
          <option value="">Model</option>
          {loaded && renderOptions(options.models)}
        </select>
        <select name="year" value={filters.year} onChange={handleChange}>
          <option value="">İl</option>
          {loaded && renderOptions(options.years)}
        </select>
        <select name="price" value={filters.price} onChange={handleChange}>
          <option value="">Qiymət aralığı</option>
          {loaded && renderOptions(options.priceRanges)}
        </select>
        <button className="search-btn" onClick={handleSearch}>
          <SearchIcon />
          Axtar
        </button>
      </div>
    </section>
  );
}