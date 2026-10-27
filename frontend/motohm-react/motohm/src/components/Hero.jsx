import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import apiClient from '../api/client.js';

const EMPTY_FILTERS = { brand: '', model: '', year: '', price: '' };
const EMPTY_OPTIONS = { brands: [], models: [], years: [], priceRanges: [] };

function SearchIcon() {
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
        <select name="model" aria-label={t('hero.model')} value={filters.model} onChange={handleChange} disabled={!filters.brand}>
          <option value="">{t('hero.model')}</option>
          {loaded && renderOptions(options.models)}
        </select>
        <select name="year" aria-label={t('hero.year')} value={filters.year} onChange={handleChange}>
          <option value="">{t('hero.year')}</option>
          {loaded && renderOptions(options.years)}
        </select>
        <select name="price" aria-label={t('hero.price')} value={filters.price} onChange={handleChange}>
          <option value="">{t('hero.price')}</option>
          {loaded && renderOptions(options.priceRanges)}
        </select>
        <button type="button" className="search-btn" onClick={handleSearch}>
          <SearchIcon />
          {t('hero.search')}
        </button>
      </div>
    </section>
  );
}