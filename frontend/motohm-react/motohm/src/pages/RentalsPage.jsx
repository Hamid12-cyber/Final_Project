import { useState, useEffect } from 'react';
import apiClient from '../api/client.js';

const PERIOD_OPTIONS = [
  { value: 1, label: 'Günlük' },
  { value: 2, label: 'Həftəlik' },
  { value: 3, label: 'Aylıq' },
];

export default function RentalsPage() {
  const [motorcycles, setMotorcycles] = useState([]);
  const [motorcycleId, setMotorcycleId] = useState('');
  const [period, setPeriod] = useState(1);
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [customerName, setCustomerName] = useState('');
  const [customerPhone, setCustomerPhone] = useState('');

  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState(null);
  const [confirmation, setConfirmation] = useState(null);

  useEffect(() => {
    apiClient.get('/motorcycles').then((res) => {
      const list = res.data.items ?? res.data;
      setMotorcycles(list);
      if (list.length > 0) setMotorcycleId(String(list[0].id));
    });
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setFormError(null);
    setSubmitting(true);
    try {
      const res = await apiClient.post('/rentals', {
        motorcycleId: Number(motorcycleId),
        startDate,
        endDate,
        period: Number(period),
        customerName,
        customerPhone,
      });
      const detail = await apiClient.get(`/rentals/${res.data.id}`);
      setConfirmation(detail.data);
    } catch (err) {
      setFormError(err.response?.data?.message ?? 'Kirayə sorğusu göndərilmədi.');
    } finally {
      setSubmitting(false);
    }
  };

  if (confirmation) {
    return (
      <div className="page-wrap">
        <h1 className="page-title">Kirayə təsdiqləndi ✅</h1>
        <div className="cart-summary" style={{ flexDirection: 'column', alignItems: 'flex-start', gap: 10 }}>
          <p><strong>{confirmation.motorcycleName}</strong></p>
          <p className="cart-item-type">
            {new Date(confirmation.startDate).toLocaleDateString('az-AZ')} — {new Date(confirmation.endDate).toLocaleDateString('az-AZ')}
          </p>
          <p className="cart-total-value">{confirmation.totalPrice.toLocaleString('az-AZ')} AZN</p>
        </div>
        <button className="checkout-btn" style={{ marginTop: 20 }} onClick={() => setConfirmation(null)}>
          Yeni sorğu göndər
        </button>
      </div>
    );
  }

  return (
    <div className="page-wrap">
      <h1 className="page-title">Motosiklet kirayəsi</h1>

      <form className="checkout-form" onSubmit={handleSubmit}>
        <label className="form-field">
          <span>Motosiklet</span>
          <select value={motorcycleId} onChange={(e) => setMotorcycleId(e.target.value)} required>
            {motorcycles.map((m) => (
              <option key={m.id} value={m.id}>{m.name} — {m.brand}</option>
            ))}
          </select>
          {/* Backend /motorcycles endpoint-i IsForRent sahəsini qaytarmır, ona görə burda filtr edə bilmirik.
              Kirayə üçün olmayan model seçilsə, backend "Bu motosiklet kirayə üçün deyil." xətası qaytarır. */}
          <span style={{ fontSize: 11, color: 'var(--muted)' }}>
            Qeyd: seçilən model kirayə üçün deyilsə, göndərəndə xəbərdarlıq görəcəksiniz.
          </span>
        </label>

        <label className="form-field">
          <span>Müddət</span>
          <select value={period} onChange={(e) => setPeriod(e.target.value)}>
            {PERIOD_OPTIONS.map((p) => (
              <option key={p.value} value={p.value}>{p.label}</option>
            ))}
          </select>
        </label>

        <label className="form-field">
          <span>Başlanğıc tarixi</span>
          <input type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} required />
        </label>

        <label className="form-field">
          <span>Bitmə tarixi</span>
          <input type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)} required />
        </label>

        <label className="form-field">
          <span>Ad, Soyad</span>
          <input type="text" value={customerName} onChange={(e) => setCustomerName(e.target.value)} required maxLength={100} />
        </label>

        <label className="form-field">
          <span>Telefon</span>
          <input type="text" value={customerPhone} onChange={(e) => setCustomerPhone(e.target.value)} required maxLength={30} placeholder="+994 55 123 45 67" />
        </label>

        {formError && <p className="form-error">{formError}</p>}

        <button className="checkout-btn" type="submit" disabled={submitting || !motorcycleId}>
          {submitting ? 'Göndərilir...' : 'Kirayə sorğusu göndər'}
        </button>
      </form>
    </div>
  );
}