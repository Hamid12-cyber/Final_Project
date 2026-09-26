import { useState, useEffect } from 'react';
import apiClient from '../api/client.js';

const SERVICE_TYPES = [
  { value: 1, label: 'Yağ dəyişimi' },
  { value: 2, label: 'Təmir' },
  { value: 3, label: 'Diaqnostika' },
  { value: 4, label: 'Tənzimləmə' },
];

// Backend Type enum-unu DB-də string kimi saxlayır (HasConversion<string>),
// ona görə GetServiceBookingById "OilChange" kimi ingiliscə ad qaytarır.
const TYPE_NAME_LABELS = {
  OilChange: 'Yağ dəyişimi',
  Repair: 'Təmir',
  Diagnostics: 'Diaqnostika',
  Tuning: 'Tənzimləmə',
};

function todayIso() {
  return new Date().toISOString().slice(0, 10);
}

export default function ServicePage() {
  const [motorcycles, setMotorcycles] = useState([]);
  const [motorcycleId, setMotorcycleId] = useState('');
  const [type, setType] = useState(1);
  const [date, setDate] = useState(todayIso());
  const [slots, setSlots] = useState([]);
  const [slotsLoading, setSlotsLoading] = useState(false);
  const [selectedSlot, setSelectedSlot] = useState(null);
  const [customerName, setCustomerName] = useState('');
  const [customerPhone, setCustomerPhone] = useState('');
  const [notes, setNotes] = useState('');

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

  useEffect(() => {
    if (!date) return;
    setSlotsLoading(true);
    setSelectedSlot(null);
    apiClient.get(`/service-bookings/available-slots?date=${date}`)
      .then((res) => setSlots(res.data))
      .catch(() => setSlots([]))
      .finally(() => setSlotsLoading(false));
  }, [date]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!selectedSlot) {
      setFormError('Zəhmət olmasa bir vaxt seçin.');
      return;
    }
    setFormError(null);
    setSubmitting(true);
    try {
      const res = await apiClient.post('/service-bookings', {
        motorcycleId: Number(motorcycleId),
        type: Number(type),
        scheduledDate: selectedSlot,
        customerName,
        customerPhone,
        notes: notes || null,
      });
      const detail = await apiClient.get(`/service-bookings/${res.data.id}`);
      setConfirmation(detail.data);
    } catch (err) {
      setFormError(err.response?.data?.message ?? 'Bron göndərilmədi.');
    } finally {
      setSubmitting(false);
    }
  };

  if (confirmation) {
    return (
      <div className="page-wrap">
        <h1 className="page-title">Servis bronu təsdiqləndi ✅</h1>
        <div className="cart-summary" style={{ flexDirection: 'column', alignItems: 'flex-start', gap: 10 }}>
          <p><strong>{confirmation.motorcycleName}</strong></p>
          <p className="cart-item-type">{TYPE_NAME_LABELS[confirmation.type] ?? confirmation.type}</p>
          <p className="cart-item-name">
            {new Date(confirmation.scheduledDate).toLocaleString('az-AZ', { dateStyle: 'medium', timeStyle: 'short' })}
          </p>
        </div>
        <button className="checkout-btn" style={{ marginTop: 20 }} onClick={() => setConfirmation(null)}>
          Yeni bron et
        </button>
      </div>
    );
  }

  return (
    <div className="page-wrap">
      <h1 className="page-title">Servis bronu</h1>

      <form className="checkout-form" style={{ maxWidth: 480 }} onSubmit={handleSubmit}>
        <label className="form-field">
          <span>Motosiklet</span>
          <select value={motorcycleId} onChange={(e) => setMotorcycleId(e.target.value)} required>
            {motorcycles.map((m) => (
              <option key={m.id} value={m.id}>{m.name} — {m.brand}</option>
            ))}
          </select>
        </label>

        <label className="form-field">
          <span>Xidmət növü</span>
          <select value={type} onChange={(e) => setType(e.target.value)}>
            {SERVICE_TYPES.map((t) => (
              <option key={t.value} value={t.value}>{t.label}</option>
            ))}
          </select>
        </label>

        <label className="form-field">
          <span>Tarix</span>
          <input type="date" value={date} min={todayIso()} onChange={(e) => setDate(e.target.value)} required />
        </label>

        <div className="form-field">
          <span>Boş vaxtlar (09:00–19:00)</span>
          {slotsLoading ? (
            <p className="cart-item-type">Yüklənir...</p>
          ) : (
            <div className="slot-grid">
              {slots.map((s) => (
                <button
                  type="button"
                  key={s.time}
                  disabled={!s.isAvailable}
                  className={`slot-btn ${!s.isAvailable ? 'taken' : ''} ${selectedSlot === s.time ? 'selected' : ''}`}
                  onClick={() => setSelectedSlot(s.time)}
                >
                  {new Date(s.time).toLocaleTimeString('az-AZ', { hour: '2-digit', minute: '2-digit' })}
                </button>
              ))}
            </div>
          )}
        </div>

        <label className="form-field">
          <span>Ad, Soyad</span>
          <input type="text" value={customerName} onChange={(e) => setCustomerName(e.target.value)} required maxLength={100} />
        </label>

        <label className="form-field">
          <span>Telefon</span>
          <input type="text" value={customerPhone} onChange={(e) => setCustomerPhone(e.target.value)} required maxLength={30} placeholder="+994 55 123 45 67" />
        </label>

        <label className="form-field">
          <span>Qeyd (istəyə görə)</span>
          <input type="text" value={notes} onChange={(e) => setNotes(e.target.value)} maxLength={500} />
        </label>

        {formError && <p className="form-error">{formError}</p>}

        <button className="checkout-btn" type="submit" disabled={submitting || !motorcycleId}>
          {submitting ? 'Göndərilir...' : 'Bron et'}
        </button>
      </form>
    </div>
  );
}