import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import apiClient from '../api/client.js';
import PasswordInput from '../components/PasswordInput.jsx';
import { PHONE_PREFIX, PHONE_PATTERN, formatPhone, phoneToE164 } from '../utils/phone.js';

export default function RegisterPage() {
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [phone, setPhone] = useState('');
  const [password, setPassword] = useState('');
  const [role, setRole] = useState('Customer');
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      const fullName = `${firstName.trim()} ${lastName.trim()}`.trim();
      const res = await apiClient.post('/auth/register', { fullName, email, phone: phoneToE164(phone), password, role });
      sessionStorage.setItem(
        'pendingRegistration',
        JSON.stringify({ verificationToken: res.data.verificationToken, email: res.data.email })
      );
      navigate('/verify-email');
    } catch (err) {
      setError(err.response?.data?.message ?? 'Qeydiyyat uğursuz oldu.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1 className="auth-title">Qeydiyyat</h1>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
          <div className="auth-row">
            <label className="form-field">
              <span>Ad</span>
              <input type="text" value={firstName} onChange={(e) => setFirstName(e.target.value)} required maxLength={50} />
            </label>
            <label className="form-field">
              <span>Soyad</span>
              <input type="text" value={lastName} onChange={(e) => setLastName(e.target.value)} required maxLength={50} />
            </label>
          </div>

          <label className="form-field">
            <span>Email</span>
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
          </label>

          <label className="form-field">
            <span>Telefon nömrəsi</span>
            <input
              type="tel"
              inputMode="tel"
              value={phone}
              onChange={(e) => setPhone(formatPhone(e.target.value, e.target.value.length < phone.length))}
              onFocus={() => { if (!phone) setPhone(PHONE_PREFIX); }}
              onBlur={() => setPhone(formatPhone(phone, true))}
              required
              pattern={PHONE_PATTERN}
              title="Nömrəni tam yazın: +994 55-123-45-67"
              placeholder="+994 55-123-45-67"
            />
          </label>

          <label className="form-field">
            <span>Şifrə</span>
            <PasswordInput value={password} onChange={(e) => setPassword(e.target.value)} required minLength={8} />
          </label>

          <label className="form-field">
            <span>Hesab növü</span>
            <select value={role} onChange={(e) => setRole(e.target.value)}>
              <option value="Customer">Müştəri</option>
              <option value="Seller">Satıcı</option>
            </select>
          </label>

          {error && <p className="form-error">{error}</p>}

          <button type="submit" className="checkout-btn" disabled={submitting}>
            {submitting ? 'Göndərilir...' : 'Qeydiyyatdan keç'}
          </button>
        </form>

        <p className="auth-footer">
          Artıq hesabın var? <Link to="/login" className="accent">Giriş et</Link>
        </p>
      </div>
    </div>
  );
}