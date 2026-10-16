export const PRICE_RANGES = [
  { value: '0-5000', label: '5 000 AZN-ə qədər', min: 0, max: 5000 },
  { value: '5000-10000', label: '5 000 – 10 000 AZN', min: 5000, max: 10000 },
  { value: '10000-20000', label: '10 000 – 20 000 AZN', min: 10000, max: 20000 },
  { value: '20000-40000', label: '20 000 – 40 000 AZN', min: 20000, max: 40000 },
  { value: '40000-', label: '40 000 AZN-dən yuxarı', min: 40000, max: Infinity },
];

export const norm = (s) => (s ?? '').toString().trim().toLowerCase();

// filters: { brand, model, year, price } - boş string = "fərqi yoxdur"
export function matchesFilters(m, f) {
  if (f.brand && norm(m.brand) !== norm(f.brand)) return false;
  if (f.model && norm(m.model) !== norm(f.model)) return false;
  if (f.year && Number(m.year) !== Number(f.year)) return false;
  if (f.price) {
    const r = PRICE_RANGES.find((p) => p.value === f.price);
    if (r && (m.price < r.min || m.price >= r.max)) return false;
  }
  return true;
}