import { useState, useEffect } from 'react';
import apiClient from '../api/client.js';

export default function MotorcyclesPage() {
  const [motorcycles, setMotorcycles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    apiClient.get('/motorcycles')
      .then((res) => {
        setMotorcycles(res.data.items ?? res.data);
        setLoading(false);
      })
      .catch((err) => {
        setError(err.message);
        setLoading(false);
      });
  }, []);

  if (loading) return <div style={{ padding: 40, color: '#fff' }}>Yüklənir...</div>;
  if (error) return <div style={{ padding: 40, color: 'red' }}>Xəta: {error}</div>;

  return (
    <div style={{ padding: 40, color: '#fff' }}>
      <h1>Motosikletlər</h1>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 20, marginTop: 20 }}>
        {motorcycles.map((m) => (
          <div key={m.id} style={{ background: '#151519', border: '1px solid #232329', borderRadius: 14, padding: 16 }}>
            <p style={{ fontWeight: 700 }}>{m.name}</p>
            <p style={{ color: '#8a8a92', fontSize: 14 }}>{m.brand} · {m.year}</p>
            <p style={{ fontWeight: 700, marginTop: 8 }}>{m.price} AZN</p>
          </div>
        ))}
      </div>
    </div>
  );
}