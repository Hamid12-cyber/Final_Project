import { useMemo, useState } from 'react';
import { toKey, fromKey, addDays, todayKey, eachNight } from '../utils/dateUtils.js';

const MONTHS = [
  'Yanvar', 'Fevral', 'Mart', 'Aprel', 'May', 'İyun',
  'İyul', 'Avqust', 'Sentyabr', 'Oktyabr', 'Noyabr', 'Dekabr',
];
const WEEKDAYS = ['B.e', 'Ç.a', 'Çər', 'C.a', 'Cüm', 'Şən', 'Baz'];

/**
 * Kirayə təqvimi (aralıq seçimi).
 *
 * props:
 *  - bookedRanges: [{ startDate, endDate }]  (dolu aralıqlar; endDate = təhvil günü, boş sayılır)
 *  - startDate, endDate: 'YYYY-MM-DD' | ''
 *  - onChange(startDate, endDate)
 */
export default function RentalCalendar({ bookedRanges = [], startDate, endDate, onChange }) {
  const today = todayKey();
  const [cursor, setCursor] = useState(() => {
    const d = startDate ? fromKey(startDate) : new Date();
    return new Date(d.getFullYear(), d.getMonth(), 1);
  });
  const [hover, setHover] = useState(null);
  const [notice, setNotice] = useState(null);

  // Dolu gecələrin set-i
  const bookedNights = useMemo(() => {
    const set = new Set();
    bookedRanges.forEach((r) => {
      eachNight(String(r.startDate).slice(0, 10), String(r.endDate).slice(0, 10))
        .forEach((k) => set.add(k));
    });
    return set;
  }, [bookedRanges]);

  const rangeHasBooked = (a, b) => eachNight(a, b).some((k) => bookedNights.has(k));

  const year = cursor.getFullYear();
  const month = cursor.getMonth();

  // Həftə bazar ertəsi başlayır
  const firstWeekday = (new Date(year, month, 1).getDay() + 6) % 7;
  const daysInMonth = new Date(year, month + 1, 0).getDate();

  const cells = [];
  for (let i = 0; i < firstWeekday; i++) cells.push(null);
  for (let d = 1; d <= daysInMonth; d++) cells.push(toKey(new Date(year, month, d)));

  const canGoPrev = toKey(new Date(year, month, 1)) > toKey(new Date(new Date().getFullYear(), new Date().getMonth(), 1));

  const handlePick = (key) => {
    setNotice(null);

    // Yeni seçim başlayır
    if (!startDate || (startDate && endDate)) {
      onChange(key, '');
      return;
    }

    // Başlanğıcdan əvvəlki günə klik -> başlanğıc kimi yenilə
    if (key <= startDate) {
      onChange(key, '');
      return;
    }

    if (rangeHasBooked(startDate, key)) {
      setNotice('Seçdiyin aralıqda artıq dolu günlər var. Başqa tarix seç.');
      onChange(key, '');
      return;
    }

    onChange(startDate, key);
  };

  const previewEnd = startDate && !endDate && hover && hover > startDate && !rangeHasBooked(startDate, hover)
    ? hover
    : null;
  const rangeEnd = endDate || previewEnd;

  const cellClass = (key) => {
    const classes = ['cal-day'];
    const isPast = key < today;
    const isBooked = bookedNights.has(key);

    if (isPast) classes.push('past');
    if (isBooked) classes.push('booked');
    if (key === today) classes.push('today');
    if (startDate && key === startDate) classes.push('range-start');
    if (endDate && key === endDate) classes.push('range-end');
    if (startDate && rangeEnd && key > startDate && key < rangeEnd) classes.push('in-range');
    if (!endDate && previewEnd && key === previewEnd) classes.push('range-preview-end');
    return classes.join(' ');
  };

  // Gün seçilə bilərmi? Keçmiş günlər seçilmir. Dolu gün başlanğıc ola bilməz,
  // amma bitmə günü (təhvil) kimi seçilə bilər, çünki həmin gün boşdur.
  const isDisabled = (key) => {
    if (key < today) return true;
    const picksStart = !startDate || (startDate && endDate) || key <= startDate;
    if (picksStart) return bookedNights.has(key);
    return false;
  };

  return (
    <div className="cal">
      <div className="cal-head">
        <button
          type="button"
          className="cal-nav"
          onClick={() => setCursor(new Date(year, month - 1, 1))}
          disabled={!canGoPrev}
          aria-label="Əvvəlki ay"
        >
          ‹
        </button>
        <strong>{MONTHS[month]} {year}</strong>
        <button
          type="button"
          className="cal-nav"
          onClick={() => setCursor(new Date(year, month + 1, 1))}
          aria-label="Növbəti ay"
        >
          ›
        </button>
      </div>

      <div className="cal-grid cal-weekdays">
        {WEEKDAYS.map((w) => <span key={w}>{w}</span>)}
      </div>

      <div className="cal-grid" onMouseLeave={() => setHover(null)}>
        {cells.map((key, i) =>
          key === null ? (
            <span key={`e${i}`} />
          ) : (
            <button
              key={key}
              type="button"
              className={cellClass(key)}
              disabled={isDisabled(key)}
              onMouseEnter={() => setHover(key)}
              onClick={() => handlePick(key)}
              title={bookedNights.has(key) ? 'Dolu' : undefined}
            >
              {Number(key.slice(8))}
            </button>
          )
        )}
      </div>

      <div className="cal-legend">
        <span><i className="dot dot-sel" /> Seçilmiş</span>
        <span><i className="dot dot-booked" /> Dolu</span>
        <span><i className="dot dot-today" /> Bu gün</span>
      </div>

      {notice && <p className="form-error">{notice}</p>}

      <p className="cart-item-type" style={{ marginTop: 8 }}>
        {!startDate && 'Başlanğıc tarixini seç.'}
        {startDate && !endDate && 'İndi bitmə (təhvil) tarixini seç.'}
        {startDate && endDate && (
          <>
            {fromKey(startDate).toLocaleDateString('az-AZ')} — {fromKey(endDate).toLocaleDateString('az-AZ')}
            {' '}
            <button type="button" className="cal-clear" onClick={() => onChange('', '')}>Təmizlə</button>
          </>
        )}
      </p>
    </div>
  );
}