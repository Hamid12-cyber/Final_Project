import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import apiClient from '../api/client.js';
import { priceRangeLabel } from '../utils/motorcycleFilter.js';

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
  const { t } = useTranslation();
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

  // Backend etiketləri Azərbaycan dilindədir: "Digər" və qiymət aralığını seçilmiş dildə göstəririk
  const labelOf = (o, kind) => {
    if (kind === 'price') return priceRangeLabel(o.value, t);
    return String(o.label).toLowerCase() === 'digər' ? t('common.other') : o.label;
  };
  const renderOptions = (list, kind) =>
    list.map((o) => (
      <option key={o.value} value={o.value}>{labelOf(o, kind)} ({o.count})</option>
    ));

  return (
    <section className="hero">
      <div className="hero-content">
        <p className="hero-eyebrow">{t('hero.eyebrow')}</p>
        <h1 className="hero-title">
          {t('hero.title1')}<br />
          <span className="accent">{t('hero.title2')}</span>
        </h1>
        <p className="hero-desc">
          {t('hero.desc')}
        </p>
      </div>

      <div className="hero-shape" aria-hidden="true"></div>

      {/* Axtarış paneli şəklin yol zolağında (hero-nun aşağı hissəsində) durur */}
      <div className="search-bar">
        <select name="brand" aria-label={t('hero.brand')} value={filters.brand} onChange={handleChange}>
          <option value="">{t('hero.brand')}</option>
          {loaded && renderOptions(options.brands)}
        </select>
        <select name="model" aria-label={t('hero.model')} value={filters.model} onChange={handleChange} disabled={false}>
          <option value="">{t('hero.model')}</option>
          {loaded && renderOptions(options.models)}
        </select>
        <select name="year" aria-label={t('hero.year')} value={filters.year} onChange={handleChange}>
          <option value="">{t('hero.year')}</option>
          {loaded && renderOptions(options.years)}
        </select>
        <select name="price" aria-label={t('hero.price')} value={filters.price} onChange={handleChange}>
          <option value="">{t('hero.price')}</option>
          {loaded && renderOptions(options.priceRanges, 'price')}
        </select>
        <button type="button" className="search-btn" onClick={handleSearch}>
          <SearchIcon />
          {t('hero.search')}
        </button>
      </div>
    </section>
  );
}