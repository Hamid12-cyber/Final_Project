// Ulduz reytinqi.
//  - Göstərmə rejimi:  <StarRating value={4.5} count={12} />
//  - Seçim rejimi:     <StarRating value={rating} onChange={setRating} />
export default function StarRating({ value = 0, count, onChange, showText = true }) {
  if (onChange) {
    return (
      <div className="star-input" role="radiogroup" aria-label="Reytinq">
        {[1, 2, 3, 4, 5].map((n) => (
          <button
            key={n}
            type="button"
            className={`star-btn ${n <= value ? 'on' : ''}`}
            onClick={() => onChange(n)}
            aria-label={`${n} ulduz`}
            aria-checked={n === value}
            role="radio"
          >
            ★
          </button>
        ))}
      </div>
    );
  }

  const pct = Math.max(0, Math.min(100, (value / 5) * 100));

  return (
    <span className="stars" title={`${value} / 5`}>
      <span className="stars-glyphs" aria-hidden="true">
        <span className="stars-bg">★★★★★</span>
        <span className="stars-fg" style={{ width: `${pct}%` }}>★★★★★</span>
      </span>
      {showText && (
        <span className="stars-text">
          {count === undefined ? value.toFixed(1) : `${value.toFixed(1)} (${count})`}
        </span>
      )}
    </span>
  );
}