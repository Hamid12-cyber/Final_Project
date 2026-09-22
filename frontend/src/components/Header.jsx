export default function Header() {
  return (
    <header className="header">
      <div className="logo">
        <div className="logo-badge">HM</div>
        <span className="logo-text">
          Moto<span className="accent">HM</span>
        </span>
      </div>

      <nav className="nav">
        <a href="#" className="nav-link active">Ana səhifə</a>
        <a href="#" className="nav-link">Motosikletlər</a>
        <a href="#" className="nav-link">Ehtiyat hissələri</a>
        <a href="#" className="nav-link">Kirayə</a>
        <a href="#" className="nav-link">Servis</a>
        <a href="#" className="nav-link">Aksesuarlar</a>
      </nav>

      <div className="header-actions">
        <button className="icon-btn" aria-label="Axtar">🔍</button>
        <button className="icon-btn cart-btn" aria-label="Səbət">
          🛒<span className="cart-count">0</span>
        </button>
        <button className="login-btn">👤 Giriş / Qeydiyyat</button>
      </div>
    </header>
  );
}
