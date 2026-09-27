import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import apiClient from '../api/client.js';

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
      await apiClient.post('/auth/register', { fullName, email, phone, password, role });
      navigate('/login');
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
              type="text"
              value={phone}
              onChange={(e) => setPhone(e.target.value)}
              required
              maxLength={30}
              placeholder="+994 55 123 45 67"
            />
          </label>

          <label className="form-field">
            <span>Şifrə</span>
            <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required minLength={6} />
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