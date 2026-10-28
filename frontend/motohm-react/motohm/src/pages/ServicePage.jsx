import { useState, useEffect } from 'react';
import apiClient from '../api/client.js';
import Header from '../components/Header.jsx';
import Footer from '../components/Footer.jsx';
import { PHONE_PREFIX, PHONE_PATTERN, formatPhone, phoneToE164 } from '../utils/phone.js';

const SERVICE_TYPES = [
  { value: 'OilChange', label: 'Yağ dəyişimi' },
  { value: 'Repair', label: 'Təmir' },
  { value: 'Diagnostics', label: 'Diaqnostika' },
  { value: 'Tuning', label: 'Tənzimləmə' },
];

// Backend indi enum-ları string kimi qaytarır, ona görə əlavə map-ə ehtiyac qalmadı,
// amma "type" dəyərini birbaşa label kimi göstərmək istəsək deyə saxlayırıq.
const TYPE_NAME_LABELS = {
  OilChange: 'Yağ dəyişimi',
  Repair: 'Təmir',
  Diagnostics: 'Diaqnostika',
  Tuning: 'Tənzimləmə',
};

// Siyahıda olmayan motosiklet üçün "Digər" seçimi
const OTHER = '__other__';

// Backend saatlıq vaxtları qaytarır (09:00, 10:00 ...). Hər saatın arxasına yarım saatlıq (09:30, 10:30 ...) əlavə edirik.
// Yarım saatlıq vaxt yalnız qonşu saatlıq vaxtların hər ikisi boşdursa seçilə bilir (üst-üstə düşməsin deyə).
function withHalfHours(list) {
  if (!Array.isArray(list)) return [];
  const out = [];
  list.forEach((s, i) => {
    out.push(s);
    if (typeof s.time !== 'string') return;
    const half = s.time.replace(/T(\d{2}):00/, 'T$1:30');
    if (half === s.time) return;
    const next = list[i + 1];
    out.push({ time: half, isAvailable: s.isAvailable && (!next || next.isAvailable) });
  });
  return out;
}

function todayIso() {
  return new Date().toISOString().slice(0, 10);
}

export default function ServicePage() {
  const [motorcycles, setMotorcycles] = useState([]);
  const [motorcycleId, setMotorcycleId] = useState('');
  const [type, setType] = useState('OilChange');
  const [date, setDate] = useState(todayIso());
  const [slots, setSlots] = useState([]);
  const [slotsLoading, setSlotsLoading] = useState(false);
  const [selectedSlot, setSelectedSlot] = useState(null);
  const [customerName, setCustomerName] = useState('');
  const [customerPhone, setCustomerPhone] = useState('');
  const [notes, setNotes] = useState('');
  const [customMotorcycle, setCustomMotorcycle] = useState('');

  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState(null);
  const [confirmation, setConfirmation] = useState(null);

  useEffect(() => {
    apiClient.get('/motorcycles').then((res) => {
      const list = res.data.items ?? res.data;
      setMotorcycles(list);
      setMotorcycleId(list.length > 0 ? String(list[0].id) : OTHER);
    });
  }, []);

  useEffect(() => {
    if (!date) return;
    setSlotsLoading(true);
    setSelectedSlot(null);
    apiClient.get(`/service-bookings/available-slots?date=${date}`)
      .then((res) => setSlots(withHalfHours(res.data)))
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
      const isOther = motorcycleId === OTHER;
      const otherName = customMotorcycle.trim();
      const payload = {
        motorcycleId: isOther ? null : Number(motorcycleId),
        type,
        scheduledDate: selectedSlot,
        customerName,
        customerPhone: phoneToE164(customerPhone),
        notes: notes || null,
      };
      if (isOther) {
        payload.motorcycleName = otherName;
        // Ad itməsin deyə qeydə də əlavə olunur
        payload.notes = `Motosiklet: ${otherName}${notes ? ` | ${notes}` : ''}`;
      }
      const res = await apiClient.post('/service-bookings', payload);
      setConfirmation(res.data);
    } catch (err) {
      setFormError(err.response?.data?.message ?? 'Bron göndərilmədi.');
    } finally {
      setSubmitting(false);
    }
  };

  if (confirmation) {
    return (
      <>
      <Header />
      <div className="page-wrap">
        <h1 className="page-title">Servis bronu təsdiqləndi ✅</h1>
        <div className="cart-summary" style={{ flexDirection: 'column', alignItems: 'flex-start', gap: 10 }}>
          <p><strong>{confirmation.motorcycleName ?? customMotorcycle}</strong></p>
          <p className="cart-item-type">{TYPE_NAME_LABELS[confirmation.type] ?? confirmation.type}</p>
          <p className="cart-item-name">
            {new Date(confirmation.scheduledDate).toLocaleString('az-AZ', { dateStyle: 'medium', timeStyle: 'short' })}
          </p>
        </div>
        <button className="checkout-btn" style={{ marginTop: 20 }} onClick={() => setConfirmation(null)}>
          Yeni bron et
        </button>
      </div>
      <Footer />
      </>
    );
  }

  return (
    <>
    <Header />
    <div className="page-wrap">
      <div className="listing-wrap listing-split">
      <h1 className="page-title">Servis bronu</h1>

      <form className="checkout-form" style={{ maxWidth: 480 }} onSubmit={handleSubmit}>
        <label className="form-field">
          <span>Motosiklet</span>
          <select value={motorcycleId} onChange={(e) => setMotorcycleId(e.target.value)} required>
            {motorcycles.map((m) => (
              <option key={m.id} value={m.id}>{m.name} — {m.brand}</option>
            ))}
            <option value={OTHER}>Digər (özüm yazacağam)</option>
          </select>
        </label>

        {motorcycleId === OTHER && (
          <label className="form-field">
            <span>Motosikletin adı</span>
            <input
              type="text"
              value={customMotorcycle}
              onChange={(e) => setCustomMotorcycle(e.target.value)}
              required
              maxLength={100}
              placeholder="məs. Yamaha R6"
              autoFocus
            />
          </label>
        )}

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
          <input
            type="tel"
            inputMode="tel"
            value={customerPhone}
            onChange={(e) => setCustomerPhone(formatPhone(e.target.value, e.target.value.length < customerPhone.length))}
            onFocus={() => { if (!customerPhone) setCustomerPhone(PHONE_PREFIX); }}
            onBlur={() => setCustomerPhone(formatPhone(customerPhone, true))}
            required
            pattern={PHONE_PATTERN}
            title="Nömrəni tam yazın: +994 55-123-45-67"
            placeholder="+994 55-123-45-67"
          />
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
    </div>
    <Footer />
    </>
  );
}