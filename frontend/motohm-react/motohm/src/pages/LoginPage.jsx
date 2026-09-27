import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';

export default function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await login(email, password);
      navigate('/');
    } catch (err) {
      setError(err.response?.data?.message ?? 'Giriş uğursuz oldu.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1 className="auth-title">Giriş</h1>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
          <label className="form-field">
            <span>Email</span>
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
          </label>

          <label className="form-field">
            <span>Şifrə</span>
            <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
          </label>

          <Link to="/forgot-password" className="accent" style={{ fontSize: 13, alignSelf: 'flex-end' }}>
            Şifrəni unutmusan?
          </Link>

          {error && <p className="form-error">{error}</p>}

          <button type="submit" className="checkout-btn" disabled={submitting}>
            {submitting ? 'Göndərilir...' : 'Giriş et'}
          </button>
        </form>

        <p className="auth-footer">
          Hesabın yoxdur? <Link to="/register" className="accent">Qeydiyyatdan keç</Link>
        </p>
      </div>
    </div>
  );
}