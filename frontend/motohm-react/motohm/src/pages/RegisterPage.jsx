import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import apiClient from '../api/client.js';

export default function RegisterPage() {
  const [form, setForm] = useState({ fullName: '', email: '', password: '', role: 'Customer' });
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    try {
      await apiClient.post('/auth/register', form);
      navigate('/login');
    } catch (err) {
      setError(err.response?.data?.message ?? 'Qeydiyyat uğursuz oldu.');
    }
  };

  return (
    <div style={{ padding: 40, color: '#fff', maxWidth: 400 }}>
      <h1>Qeydiyyat</h1>
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 12, marginTop: 20 }}>
        <input
          name="fullName"
          placeholder="Ad Soyad"
          value={form.fullName}
          onChange={handleChange}
          style={inputStyle}
        />
        <input
          name="email"
          type="email"
          placeholder="Email"
          value={form.email}
          onChange={handleChange}
          style={inputStyle}
        />
        <input
          name="password"
          type="password"
          placeholder="Şifrə"
          value={form.password}
          onChange={handleChange}
          style={inputStyle}
        />
        <select name="role" value={form.role} onChange={handleChange} style={inputStyle}>
          <option value="Customer">Müştəri</option>
          <option value="Seller">Satıcı</option>
        </select>
        {error && <p style={{ color: 'red' }}>{error}</p>}
        <button type="submit" style={btnStyle}>Qeydiyyatdan keç</button>
      </form>
      <p style={{ marginTop: 16, color: '#8a8a92' }}>
        Artıq hesabın var? <Link to="/login" style={{ color: '#e53935' }}>Giriş et</Link>
      </p>
    </div>
  );
}

const inputStyle = {
  padding: 10,
  borderRadius: 8,
  border: '1px solid #232329',
  background: '#151519',
  color: '#fff',
};

const btnStyle = {
  padding: 12,
  background: '#e53935',
  color: '#fff',
  border: 'none',
  borderRadius: 8,
  fontWeight: 600,
};