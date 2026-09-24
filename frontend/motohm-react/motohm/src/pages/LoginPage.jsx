import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext.jsx';

export default function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState(null);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    try {
      await login(email, password);
      navigate('/');
    } catch (err) {
      setError(err.response?.data?.message ?? 'Giriş uğursuz oldu.');
    }
  };

  return (
    <div style={{ padding: 40, color: '#fff', maxWidth: 400 }}>
      <h1>Giriş</h1>
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 12, marginTop: 20 }}>
        <p style={{ marginTop: 16, color: '#8a8a92' }}>
  Hesabın yoxdur? <Link to="/register" style={{ color: '#e53935' }}>Qeydiyyatdan keç</Link>
</p>
        <input
          type="email"
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          style={{ padding: 10, borderRadius: 8, border: '1px solid #232329', background: '#151519', color: '#fff' }}
        />
        <input
          type="password"
          placeholder="Şifrə"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          style={{ padding: 10, borderRadius: 8, border: '1px solid #232329', background: '#151519', color: '#fff' }}
        />
        {error && <p style={{ color: 'red' }}>{error}</p>}
        <button type="submit" style={{ padding: 12, background: '#e53935', color: '#fff', border: 'none', borderRadius: 8, fontWeight: 600 }}>
          Giriş et
        </button>
      </form>
    </div>
  );
}