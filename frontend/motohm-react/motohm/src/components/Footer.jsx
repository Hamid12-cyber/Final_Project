import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

export default function Footer() {
  const { t } = useTranslation();
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
          <p className="footer-tagline">{t('footer.tagline')}</p>
        </div>

        <div>
          <p className="footer-heading">{t('footer.menu')}</p>
          <Link to="/" className="footer-link">{t('nav.home')}</Link>
          <Link to="/motorcycles" className="footer-link">{t('nav.motorcycles')}</Link>
          <Link to="/parts" className="footer-link">{t('nav.parts')}</Link>
          <Link to="/rentals" className="footer-link">{t('nav.rentals')}</Link>
          <Link to="/service" className="footer-link">{t('nav.service')}</Link>
        </div>

        <div>
          <p className="footer-heading">{t('footer.info')}</p>
          <Link to="/about" className="footer-link">{t('footer.about')}</Link>
          <Link to="/delivery" className="footer-link">{t('footer.delivery')}</Link>
          <Link to="/returns" className="footer-link">{t('footer.returns')}</Link>
          <Link to="/terms" className="footer-link">{t('footer.terms')}</Link>
        </div>

        <div>
          <p className="footer-heading">{t('footer.contact')}</p>
          <p className="footer-contact-item">📞 +994-70-791-17-07</p>
          <p className="footer-contact-item">✉️ infomotohm@gmail.com</p>
          <p className="footer-contact-item">📍 {t('footer.address')}</p>
        </div>
      </div>

      <p className="footer-bottom">© {new Date().getFullYear()} MotoHM. {t('footer.rights')}</p>
    </footer>
  );
}