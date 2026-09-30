import { useState } from 'react';
import { Link } from 'react-router-dom';
import apiClient from '../api/client.js';

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [sent, setSent] = useState(false);
  const [error, setError] = useState(null);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await apiClient.post('/auth/forgot-password', { email });
      setSent(true);
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

        {sent ? (
          <p className="cart-item-type" style={{ textAlign: 'center', lineHeight: 1.6 }}>
            Əgər bu email qeydiyyatdan keçibsə, şifrə bərpa linki göndərildi.
            Zəhmət olmasa emailinizi yoxlayın.
          </p>
        ) : (
          <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
            <label className="form-field">
              <span>Email</span>
              <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
            </label>

            {error && <p className="form-error">{error}</p>}

            <button type="submit" className="checkout-btn" disabled={submitting}>
              {submitting ? 'Göndərilir...' : 'Bərpa linki göndər'}
            </button>
          </form>
        )}

        <p className="auth-footer">
          <Link to="/login" className="accent">← Girişə qayıt</Link>
        </p>
      </div>
    </div>
  );
}