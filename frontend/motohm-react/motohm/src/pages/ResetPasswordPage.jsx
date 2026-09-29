import { useState } from 'react';
import { useSearchParams, Link, useNavigate } from 'react-router-dom';
import apiClient from '../api/client.js';

export default function ResetPasswordPage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);
  const [done, setDone] = useState(false);
  const navigate = useNavigate();

  if (!token) {
    return (
      <div className="auth-page">
        <div className="auth-card">
          <h1 className="auth-title">Etibarsız link</h1>
          <p className="cart-item-type" style={{ textAlign: 'center' }}>
            Bu link etibarsızdır. Yenidən <Link to="/forgot-password" className="accent">şifrə bərpası</Link> tələb edin.
          </p>
        </div>
      </div>
    );
  }

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    if (newPassword !== confirmPassword) {
      setError('Şifrələr uyğun gəlmir.');
      return;
    }
    setSubmitting(true);
    try {
      await apiClient.post('/auth/reset-password', { token, newPassword });
      setDone(true);
      setTimeout(() => navigate('/login'), 2000);
    } catch (err) {
      setError(err.response?.data?.message ?? 'Şifrə yenilənmədi.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1 className="auth-title">Yeni şifrə təyin et</h1>

        {done ? (
          <p className="cart-item-type" style={{ textAlign: 'center' }}>
            Şifrəniz yeniləndi ✅ Giriş səhifəsinə yönləndirilirsiniz...
          </p>
        ) : (
          <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
            <label className="form-field">
              <span>Yeni şifrə</span>
              <input type="password" value={newPassword} onChange={(e) => setNewPassword(e.target.value)} required minLength={6} />
            </label>

            <label className="form-field">
              <span>Yeni şifrə (təkrar)</span>
              <input type="password" value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} required minLength={6} />
            </label>

            {error && <p className="form-error">{error}</p>}

            <button type="submit" className="checkout-btn" disabled={submitting}>
              {submitting ? 'Göndərilir...' : 'Şifrəni yenilə'}
            </button>
          </form>
        )}
      </div>
    </div>
  );
}