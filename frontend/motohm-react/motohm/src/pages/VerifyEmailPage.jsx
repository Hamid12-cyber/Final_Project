import { useEffect, useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import apiClient from '../api/client.js';

const STORAGE_KEY = 'pendingRegistration';

export default function VerifyEmailPage() {
  const navigate = useNavigate();
  const [pending, setPending] = useState(() => {
    try {
      return JSON.parse(sessionStorage.getItem(STORAGE_KEY));
    } catch {
      return null;
    }
  });
  const [code, setCode] = useState('');
  const [error, setError] = useState(null);
  const [info, setInfo] = useState(null);
  const [submitting, setSubmitting] = useState(false);
  const [resending, setResending] = useState(false);
  const [cooldown, setCooldown] = useState(60);
  const [done, setDone] = useState(false);

  useEffect(() => {
    if (cooldown <= 0) return undefined;
    const t = setTimeout(() => setCooldown((c) => c - 1), 1000);
    return () => clearTimeout(t);
  }, [cooldown]);

  if (!pending?.verificationToken) {
    return (
      <div className="auth-page">
        <div className="auth-card">
          <h1 className="auth-title">Təsdiq sessiyası tapılmadı</h1>
          <p className="cart-item-type" style={{ textAlign: 'center' }}>
            Zəhmət olmasa <Link to="/register" className="accent">yenidən qeydiyyatdan keçin</Link>.
          </p>
        </div>
      </div>
    );
  }

  const handleVerify = async (e) => {
    e.preventDefault();
    setError(null);
    setInfo(null);
    setSubmitting(true);
    try {
      await apiClient.post('/auth/verify-registration', {
        verificationToken: pending.verificationToken,
        code,
      });
      sessionStorage.removeItem(STORAGE_KEY);
      setDone(true);
      setTimeout(() => navigate('/login'), 2000);
    } catch (err) {
      setError(err.response?.data?.message ?? 'Təsdiq uğursuz oldu.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleResend = async () => {
    setError(null);
    setInfo(null);
    setResending(true);
    try {
      const res = await apiClient.post('/auth/resend-registration-code', {
        verificationToken: pending.verificationToken,
      });
      const next = { verificationToken: res.data.verificationToken, email: res.data.email };
      sessionStorage.setItem(STORAGE_KEY, JSON.stringify(next));
      setPending(next);
      setCode('');
      setCooldown(60);
      setInfo('Yeni kod emailinizə göndərildi.');
    } catch (err) {
      setError(err.response?.data?.message ?? 'Kod göndərilmədi.');
    } finally {
      setResending(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1 className="auth-title">Email təsdiqi</h1>

        {done ? (
          <p className="cart-item-type" style={{ textAlign: 'center' }}>
            Hesabınız yaradıldı ✅ Giriş səhifəsinə yönləndirilirsiniz...
          </p>
        ) : (
          <>
            <p className="cart-item-type" style={{ textAlign: 'center', lineHeight: 1.6 }}>
              <strong>{pending.email}</strong> ünvanına 6 rəqəmli kod göndərildi.
            </p>

            <form onSubmit={handleVerify} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
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

              {error && <p className="form-error">{error}</p>}
              {info && <p className="cart-item-type" style={{ textAlign: 'center' }}>{info}</p>}

              <button type="submit" className="checkout-btn" disabled={submitting || code.length !== 6}>
                {submitting ? 'Yoxlanılır...' : 'Təsdiq et'}
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
                  disabled={resending}
                >
                  {resending ? 'Göndərilir...' : 'Kodu yenidən göndər'}
                </button>
              )}
            </p>
          </>
        )}

        <p className="auth-footer">
          <Link to="/register" className="accent">← Qeydiyyata qayıt</Link>
        </p>
      </div>
    </div>
  );
}