export const PHONE_PREFIX = '+994 ';
const PHONE_GROUPS = [2, 3, 2, 2]; // +994 55-123-45-67

// input-un pattern atributu üçün
export const PHONE_PATTERN = '\\+994 \\d{2}-\\d{3}-\\d{2}-\\d{2}';

// Yazılanı +994 55-123-45-67 formatına salır.
// deleting=true olanda sondakı tire qoyulmur (silmək mümkün olsun).
export function formatPhone(raw, deleting = false) {
  let digits;
  if (raw.startsWith('+994')) {
    digits = raw.slice(4).replace(/\D/g, '');
  } else {
    digits = raw.replace(/\D/g, '');
    if (digits.startsWith('994')) digits = digits.slice(3);
    else if (digits.startsWith('0')) digits = digits.slice(1);
  }
  digits = digits.slice(0, 9);
  if (!digits) return '';

  const parts = [];
  let i = 0;
  for (const size of PHONE_GROUPS) {
    if (i >= digits.length) break;
    parts.push(digits.slice(i, i + size));
    i += size;
  }
  let out = PHONE_PREFIX + parts.join('-');
  const idx = parts.length - 1;
  if (!deleting && parts[idx].length === PHONE_GROUPS[idx] && parts.length < PHONE_GROUPS.length) {
    out += '-';
  }
  return out;
}

// Serverə boşluq və tiresiz göndərilir: +994551234567
export function phoneToE164(formatted) {
  return '+994' + formatted.slice(PHONE_PREFIX.length).replace(/\D/g, '');
}