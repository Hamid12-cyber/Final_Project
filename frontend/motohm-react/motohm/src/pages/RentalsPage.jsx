import { useState, useEffect } from 'react';
import apiClient from '../api/client.js';
import RentalCalendar from '../components/RentalCalendar.jsx';
import { diffDays, estimatePrice, fromKey } from '../utils/dateUtils.js';

const PERIOD_OPTIONS = [
  { value: 'Daily', label: 'Günlük' },
  { value: 'Weekly', label: 'Həftəlik' },
  { value: 'Monthly', label: 'Aylıq' },
];

export default function RentalsPage() {
  const [motorcycles, setMotorcycles] = useState([]);
  const [motorcycleId, setMotorcycleId] = useState('');
  const [period, setPeriod] = useState('Daily');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [customerName, setCustomerName] = useState('');
  const [customerPhone, setCustomerPhone] = useState('');

  const [bookedRanges, setBookedRanges] = useState([]);
  const [loadingBooked, setLoadingBooked] = useState(false);

  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState(null);
  const [confirmation, setConfirmation] = useState(null);

  useEffect(() => {
    apiClient.get('/motorcycles').then((res) => {
      const list = (res.data.items ?? res.data).filter((m) => m.isForRent);
      setMotorcycles(list);
      if (list.length > 0) setMotorcycleId(String(list[0].id));
    });
  }, []);

  // Seçilmiş motosiklet üçün dolu tarixləri yüklə.
  // Gözlənilən endpoint: GET /rentals/booked/{motorcycleId} -> [{ startDate, endDate }]
  // Endpoint hələ yoxdursa (404), təqvim sadəcə boş görünür və səhv göstərilmir.
  const loadBooked = (id) => {
    if (!id) return;
    setLoadingBooked(true);
    apiClient.get(`/rentals/booked/${id}`)
      .then((res) => setBookedRanges(res.data ?? []))
      .catch(() => setBookedRanges([]))
      .finally(() => setLoadingBooked(false));
  };

  useEffect(() => {
    setStartDate('');
    setEndDate('');
    loadBooked(motorcycleId);
  }, [motorcycleId]);

  const days = startDate && endDate ? Math.max(1, diffDays(startDate, endDate)) : 0;
  const estimate = estimatePrice(startDate, endDate, period);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setFormError(null);

    if (!startDate || !endDate) {
      setFormError('Təqvimdən başlanğıc və bitmə tarixini seç.');
      return;
    }

    setSubmitting(true);
    try {
      const res = await apiClient.post('/rentals', {
        motorcycleId: Number(motorcycleId),
        startDate,
        endDate,
        period,
        customerName,
        customerPhone,
      });
      setConfirmation(res.data);
      loadBooked(motorcycleId);
    } catch (err) {
      setFormError(err.response?.data?.message ?? 'Kirayə sorğusu göndərilmədi.');
    } finally {
      setSubmitting(false);
    }
  };

  const resetForm = () => {
    setConfirmation(null);
    setStartDate('');
    setEndDate('');
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
        <button className="checkout-btn" style={{ marginTop: 20 }} onClick={resetForm}>
          Yeni sorğu göndər
        </button>
      </div>
    );
  }

  return (
    <div className="page-wrap">
      <h1 className="page-title">Motosiklet kirayəsi</h1>

      {motorcycles.length === 0 ? (
        <div className="state-msg">Hazırda kirayə üçün əlçatan motosiklet yoxdur.</div>
      ) : (
        <form className="checkout-form rental-layout" onSubmit={handleSubmit}>
          <div className="rental-col">
            <label className="form-field">
              <span>Motosiklet</span>
              <select value={motorcycleId} onChange={(e) => setMotorcycleId(e.target.value)} required>
                {motorcycles.map((m) => (
                  <option key={m.id} value={m.id}>{m.name} — {m.brand}</option>
                ))}
              </select>
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
              <span>Ad, Soyad</span>
              <input type="text" value={customerName} onChange={(e) => setCustomerName(e.target.value)} required maxLength={100} />
            </label>

            <label className="form-field">
              <span>Telefon</span>
              <input type="text" value={customerPhone} onChange={(e) => setCustomerPhone(e.target.value)} required maxLength={30} placeholder="+994 55 123 45 67" />
            </label>
          </div>

          <div className="rental-col">
            <span className="rental-cal-label">Tarixləri seç {loadingBooked && '(yüklənir...)'}</span>
            <RentalCalendar
              bookedRanges={bookedRanges}
              startDate={startDate}
              endDate={endDate}
              onChange={(s, e) => { setStartDate(s); setEndDate(e); }}
            />

            {days > 0 && (
              <div className="cart-summary" style={{ flexDirection: 'column', alignItems: 'flex-start', gap: 4 }}>
                <p className="cart-item-type">
                  {fromKey(startDate).toLocaleDateString('az-AZ')} — {fromKey(endDate).toLocaleDateString('az-AZ')} · {days} gün
                </p>
                <p className="cart-total-value">≈ {estimate.toLocaleString('az-AZ')} AZN</p>
                <p className="cart-item-type" style={{ fontSize: 12 }}>Yekun məbləğ təsdiqdən sonra serverdə hesablanır.</p>
              </div>
            )}

            {formError && <p className="form-error">{formError}</p>}

            <button className="checkout-btn" type="submit" disabled={submitting || !motorcycleId || !startDate || !endDate}>
              {submitting ? 'Göndərilir...' : 'Kirayə sorğusu göndər'}
            </button>
          </div>
        </form>
      )}
    </div>
  );
}