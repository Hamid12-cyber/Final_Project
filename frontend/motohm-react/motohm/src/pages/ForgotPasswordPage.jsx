import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import apiClient from '../api/client.js';

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await apiClient.post('/auth/forgot-password', { email });
      navigate(`/reset-password?email=${encodeURIComponent(email)}`);
    } catch (err) {
      setError(err.response?.data?.message ?? 'Xəta baş verdi.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1 className="auth-title">Şifrəni unutmusan?</h1>

        <p className="cart-item-type" style={{ textAlign: 'center', lineHeight: 1.6 }}>
          Emailinizi yazın, sizə 6 rəqəmli təsdiq kodu göndərək.
        </p>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
          <label className="form-field">
            <span>Email</span>
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
          </label>

          {error && <p className="form-error">{error}</p>}

          <button type="submit" className="checkout-btn" disabled={submitting}>
            {submitting ? 'Göndərilir...' : 'Kod göndər'}
          </button>
        </form>

        <p className="auth-footer">
          <Link to="/login" className="accent">← Girişə qayıt</Link>
        </p>
      </div>
    </div>
  );
}