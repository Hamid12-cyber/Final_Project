import { useState } from 'react';

export default function Hero() {
  const [filters, setFilters] = useState({
    brand: '',
    model: '',
    year: '',
    priceRange: ''
  });

  const handleChange = (e) => {
    setFilters({ ...filters, [e.target.name]: e.target.value });
  };

  const handleSearch = () => {
    // Backend API hazır olanda burada axtarış sorğusu göndəriləcək
    console.log('Axtarış filtrləri:', filters);
  };

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
        </select>
        <select name="model" value={filters.model} onChange={handleChange}>
          <option value="">Model</option>
        </select>
        <select name="year" value={filters.year} onChange={handleChange}>
          <option value="">İl</option>
        </select>
        <select name="priceRange" value={filters.priceRange} onChange={handleChange}>
          <option value="">Qiymət aralığı</option>
        </select>
        <button className="search-btn" onClick={handleSearch}>🔍 Axtar</button>
      </div>
    </section>
  );
}
