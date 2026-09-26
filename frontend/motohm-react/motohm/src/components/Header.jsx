import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';
import { useCart } from '../context/CartContext.jsx';

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
        <Link to="/" className={`nav-link ${isActive('/') ? 'active' : ''}`}>Ana səhifə</Link>
        <Link to="/motorcycles" className={`nav-link ${isActive('/motorcycles') ? 'active' : ''}`}>Motosikletlər</Link>
        <Link to="/parts" className={`nav-link ${isActive('/parts') ? 'active' : ''}`}>Ehtiyat hissələri</Link>
        <Link to="/rentals" className={`nav-link ${isActive('/rentals') ? 'active' : ''}`}>Kirayə</Link>
        <Link to="/service" className={`nav-link ${isActive('/service') ? 'active' : ''}`}>Servis</Link>
        <Link to="/accessories" className={`nav-link ${isActive('/accessories') ? 'active' : ''}`}>Aksesuarlar</Link>
      </nav>

      <div className="header-actions">
        {searchOpen ? (
          <form className="search-box" onSubmit={handleSearchSubmit}>
            <span className="search-icon"><SearchIcon /></span>
            <input
              type="text"
              autoFocus
              placeholder="Motosiklet axtar..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              onBlur={() => { if (!searchQuery) setSearchOpen(false); }}
            />
          </form>
        ) : (
          <button className="icon-btn" aria-label="Axtar" onClick={() => setSearchOpen(true)}>
            <SearchIcon />
          </button>
        )}
        <Link to="/cart" className="icon-btn cart-btn" aria-label="Səbət">
          <CartIcon />
          <span className="cart-count">{count}</span>
        </Link>

        {user ? (
          <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
            {user.role === 'Admin' && <Link to="/admin" className="nav-link">Admin panel</Link>}
            <Link to="/sell" className="nav-link">Elan yerləşdir</Link>
            <Link to="/orders" className="nav-link">Sifarişlərim</Link>
            <span style={{ color: '#c8c8cc', fontSize: 14 }}>👤 {user.fullName}</span>
            <button className="login-btn" onClick={handleLogout}>Çıxış</button>
          </div>
        ) : (
          <Link to="/login" className="login-btn">👤 Giriş / Qeydiyyat</Link>
        )}
      </div>
    </header>
  );
}