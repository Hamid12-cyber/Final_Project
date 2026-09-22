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
          <p>Ana səhifə</p>
          <p>Motosikletlər</p>
          <p>Ehtiyat hissələri</p>
          <p>Kirayə</p>
          <p>Servis</p>
        </div>

        <div>
          <p className="footer-heading">Məlumat</p>
          <p>Haqqımızda</p>
          <p>Çatdırılma</p>
          <p>Qaytarma və dəyişdirmə</p>
          <p>İstifadə şərtləri</p>
        </div>

        <div>
          <p className="footer-heading">Əlaqə</p>
          <p>📞 +994 50 123 45 67</p>
          <p>✉️ info@motohm.az</p>
          <p>📍 Bakı, Nərimanov r., Əhməd Racabli 25</p>
        </div>
      </div>

      <p className="footer-bottom">© 2026 MotoHM. Bütün hüquqlar qorunur.</p>
    </footer>
  );
}
