import { Link } from 'react-router-dom';

export default function ForgotPasswordPage() {
  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1 className="auth-title">Şifrəni unutmusan?</h1>
        <p className="cart-item-type" style={{ textAlign: 'center', lineHeight: 1.6 }}>
          Hazırda şifrə bərpası funksiyası mövcud deyil — bu, backend tərəfdə
          email göndərmə infrastrukturu tələb edir və hələ qurulmayıb.
          Zəhmət olmasa dəstək komandası ilə əlaqə saxla.
        </p>
        <p className="auth-footer">
          <Link to="/login" className="accent">← Girişə qayıt</Link>
        </p>
      </div>
    </div>
  );
}