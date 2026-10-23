import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';
import { useCart } from '../context/CartContext.jsx';
import { useTranslation } from 'react-i18next';
import LanguageSwitcher from './LanguageSwitcher.jsx';

function SearchIcon() {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <circle cx="11" cy="11" r="8" />
      <line x1="21" y1="21" x2="16.65" y2="16.65" />
    </svg>
  );
}

function CartIcon() {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <circle cx="9" cy="21" r="1" />
      <circle cx="20" cy="21" r="1" />
      <path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6" />
    </svg>
  );
}

export default function Header() {
  const { t } = useTranslation();
  const { user, logout } = useAuth();
  const { count } = useCart();
  const location = useLocation();
  const navigate = useNavigate();
  const [searchOpen, setSearchOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');

  const isActive = (path) => location.pathname === path;

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  const handleSearchSubmit = (e) => {
    e.preventDefault();
    const q = searchQuery.trim();
    if (!q) return;
    navigate(`/motorcycles?search=${encodeURIComponent(q)}`);
    setSearchOpen(false);
    setSearchQuery('');
  };

  return (
    <header className="header">
      <div className="logo">
        <div className="logo-badge">HM</div>
        <span className="logo-text">
          Moto<span className="accent">HM</span>
        </span>
      </div>

      <nav className="nav">
        <Link to="/" className={`nav-link ${isActive('/') ? 'active' : ''}`}>{t('nav.home')}</Link>
        <Link to="/motorcycles" className={`nav-link ${isActive('/motorcycles') ? 'active' : ''}`}>{t('nav.motorcycles')}</Link>
        <Link to="/parts" className={`nav-link ${isActive('/parts') ? 'active' : ''}`}>{t('nav.parts')}</Link>
        <Link to="/rentals" className={`nav-link ${isActive('/rentals') ? 'active' : ''}`}>{t('nav.rentals')}</Link>
        <Link to="/service" className={`nav-link ${isActive('/service') ? 'active' : ''}`}>{t('nav.service')}</Link>
        <Link to="/accessories" className={`nav-link ${isActive('/accessories') ? 'active' : ''}`}>{t('nav.accessories')}</Link>
      </nav>

      <div className="header-actions">
        <LanguageSwitcher />
        {searchOpen ? (
          <form className="search-box" onSubmit={handleSearchSubmit}>
            <span className="search-icon"><SearchIcon /></span>
            <input
              type="text"
              autoFocus
              placeholder={t('header.searchPlaceholder')}
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              onBlur={() => { if (!searchQuery) setSearchOpen(false); }}
            />
          </form>
        ) : (
          <button className="icon-btn" aria-label={t('header.searchAria')} onClick={() => setSearchOpen(true)}>
            <SearchIcon />
          </button>
        )}
        <Link to="/cart" className="icon-btn cart-btn" aria-label={t('header.cartAria')}>
          <CartIcon />
          <span className="cart-count">{count}</span>
        </Link>

        {user ? (
          <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
            {user.role === 'Admin' && <Link to="/admin" className="nav-link">{t('header.adminPanel')}</Link>}
            <Link to="/seller" className="nav-link">{t('header.myListings')}</Link>
            <Link to="/sell" className="nav-link">{t('header.createListing')}</Link>
            <Link to="/favorites" className="nav-link">{t('header.favorites')}</Link>
            <Link to="/orders" className="nav-link">{t('header.myOrders')}</Link>
            <span style={{ color: '#c8c8cc', fontSize: 14 }}>👤 {user.fullName}</span>
            <button className="login-btn" onClick={handleLogout}>{t('header.logout')}</button>
          </div>
        ) : (
          <Link to="/login" className="login-btn">👤 {t('header.loginRegister')}</Link>
        )}
      </div>
    </header>
  );
}