// value: 0-5 arası orta reytinq, count: rəy sayı (verilməsə yazı göstərilmir)
export default function StarRating({ value = 0, count }) {
  const percent = Math.max(0, Math.min(100, (value / 5) * 100));

  let text = null;
  if (count !== undefined) {
    text = count > 0 ? `${value.toFixed(1)} (${count})` : 'Rəy yoxdur';
  }

  return (
    <span className="stars">
      <span className="stars-glyphs" aria-hidden="true">
        <span className="stars-bg">★★★★★</span>
        <span className="stars-fg" style={{ width: `${percent}%` }}>★★★★★</span>
      </span>
      {text && <span className="stars-text">{text}</span>}
    </span>
  );
}