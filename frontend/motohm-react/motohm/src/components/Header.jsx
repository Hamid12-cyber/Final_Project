import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';
import { useCart } from '../context/CartContext.jsx';

export default function Header() {
  const { user, logout } = useAuth();
  const { count } = useCart();
  const location = useLocation();
  const navigate = useNavigate();

  const isActive = (path) => location.pathname === path;

  const handleLogout = () => {
    logout();
    navigate('/');
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
        <button className="icon-btn" aria-label="Axtar">🔍</button>
        <Link to="/cart" className="icon-btn cart-btn" aria-label="Səbət">
          🛒<span className="cart-count">{count}</span>
        </Link>

        {user ? (
          <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
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