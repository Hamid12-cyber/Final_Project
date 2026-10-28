import { useEffect, useState } from 'react';
import { useSearchParams, Link, useNavigate } from 'react-router-dom';
import apiClient from '../api/client.js';
import PasswordInput from '../components/PasswordInput.jsx';

export default function ResetPasswordPage() {
  const [searchParams] = useSearchParams();
  const [email, setEmail] = useState(searchParams.get('email') ?? '');
  const [code, setCode] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);
  const [info, setInfo] = useState(null);
  const [done, setDone] = useState(false);
  const [cooldown, setCooldown] = useState(60);
  const navigate = useNavigate();

  useEffect(() => {
    if (cooldown <= 0) return undefined;
    const t = setTimeout(() => setCooldown((c) => c - 1), 1000);
    return () => clearTimeout(t);
  }, [cooldown]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    setInfo(null);
    if (newPassword !== confirmPassword) {
      setError('Şifrələr uyğun gəlmir.');
      return;
    }
    setSubmitting(true);
    try {
      await apiClient.post('/auth/reset-password', { email, code, newPassword });
      setDone(true);
      setTimeout(() => navigate('/login'), 2000);
    } catch (err) {
      setError(err.response?.data?.message ?? 'Şifrə yenilənmədi.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleResend = async () => {
    setError(null);
    setInfo(null);
    try {
      await apiClient.post('/auth/forgot-password', { email });
      setCode('');
      setCooldown(60);
      setInfo('Yeni kod göndərildi.');
    } catch (err) {
      setError(err.response?.data?.message ?? 'Kod göndərilmədi.');
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
          <>
            <p className="cart-item-type" style={{ textAlign: 'center', lineHeight: 1.6 }}>
              Emailinizə göndərilən 6 rəqəmli kodu və yeni şifrənizi yazın.
            </p>

            <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
              <label className="form-field">
                <span>Email</span>
                <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
              </label>

              <label className="form-field">
                <span>Təsdiq kodu</span>
                <input
                  type="text"
                  inputMode="numeric"
                  autoComplete="one-time-code"
                  maxLength={6}
                  value={code}
                  onChange={(e) => setCode(e.target.value.replace(/\D/g, ''))}
                  placeholder="123456"
                  required
                />
              </label>

              <label className="form-field">
                <span>Yeni şifrə</span>
                <PasswordInput value={newPassword} onChange={(e) => setNewPassword(e.target.value)} required minLength={8} />
              </label>

              <label className="form-field">
                <span>Yeni şifrə (təkrar)</span>
                <PasswordInput value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} required minLength={8} />
              </label>

              {error && <p className="form-error">{error}</p>}
              {info && <p className="cart-item-type" style={{ textAlign: 'center' }}>{info}</p>}

              <button type="submit" className="checkout-btn" disabled={submitting || code.length !== 6}>
                {submitting ? 'Göndərilir...' : 'Şifrəni yenilə'}
              </button>
            </form>

            <p className="auth-footer">
              {cooldown > 0 ? (
                <span>Yeni kod istəmək üçün {cooldown} san. gözləyin</span>
              ) : (
                <button
                  type="button"
                  className="accent"
                  style={{ background: 'none', border: 'none', cursor: 'pointer' }}
                  onClick={handleResend}
                  disabled={!email}
                >
                  Kodu yenidən göndər
                </button>
              )}
            </p>
          </>
        )}

        <p className="auth-footer">
          <Link to="/login" className="accent">← Girişə qayıt</Link>
        </p>
      </div>
    </div>
  );
}