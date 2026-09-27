import { Link } from 'react-router-dom';

export default function Footer() {
  return (
    <footer className="footer">
      <div className="footer-grid">
        <div>
          <div className="logo">
            <div className="logo-badge">HM</div>
            <span className="logo-text">
              Moto<span className="accent">HM</span>
            </span>
          </div>
          <p className="footer-tagline">Sürət, azadlıq, macəra!</p>
        </div>

        <div>
          <p className="footer-heading">Menyu</p>
          <Link to="/" className="footer-link">Ana səhifə</Link>
          <Link to="/motorcycles" className="footer-link">Motosikletlər</Link>
          <Link to="/parts" className="footer-link">Ehtiyat hissələri</Link>
          <Link to="/rentals" className="footer-link">Kirayə</Link>
          <Link to="/service" className="footer-link">Servis</Link>
        </div>

        <div>
          <p className="footer-heading">Məlumat</p>
          <Link to="/about" className="footer-link">Haqqımızda</Link>
          <Link to="/delivery" className="footer-link">Çatdırılma</Link>
          <Link to="/returns" className="footer-link">Qaytarma və dəyişdirmə</Link>
          <Link to="/terms" className="footer-link">İstifadə şərtləri</Link>
        </div>

        <div>
          <p className="footer-heading">Əlaqə</p>
          <p className="footer-contact-item">📞 +994 50 123 45 67</p>
          <p className="footer-contact-item">✉️ info@motohm.az</p>
          <p className="footer-contact-item">📍 Bakı, Nərimanov r., Əhməd Racabli 25</p>
        </div>
      </div>

      <p className="footer-bottom">© {new Date().getFullYear()} MotoHM. Bütün hüquqlar qorunur.</p>
    </footer>
  );
}