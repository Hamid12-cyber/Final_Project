// Bütün tarixlər 'YYYY-MM-DD' string kimi saxlanır (timezone problemi olmasın deyə).

export function toKey(date) {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

export function fromKey(key) {
  const [y, m, d] = key.split('-').map(Number);
  return new Date(y, m - 1, d);
}

// Backend-dən gələn ISO tarixi ('2026-10-12T00:00:00') -> 'YYYY-MM-DD'
export function isoToKey(iso) {
  return String(iso).slice(0, 10);
}

export function addDays(key, n) {
  const d = fromKey(key);
  d.setDate(d.getDate() + n);
  return toKey(d);
}

export function diffDays(startKey, endKey) {
  const ms = fromKey(endKey) - fromKey(startKey);
  return Math.round(ms / 86400000);
}

export function todayKey() {
  return toKey(new Date());
}

// Verilən aralıqdakı bütün günlər: start daxil, end daxil DEYİL (end = təhvil günü).
export function eachNight(startKey, endKey) {
  const out = [];
  for (let k = startKey; k < endKey; k = addDays(k, 1)) out.push(k);
  return out;
}

// Backend-dəki CalculatePrice ilə eyni düstur (yalnız təxmini göstərmək üçün).
export function estimatePrice(startKey, endKey, period) {
  if (!startKey || !endKey) return 0;
  const days = Math.max(1, diffDays(startKey, endKey));
  if (period === 'Weekly') return Math.round((days / 7) * 300 * 100) / 100;
  if (period === 'Monthly') return Math.round((days / 30) * 1000 * 100) / 100;
  return days * 50;
}