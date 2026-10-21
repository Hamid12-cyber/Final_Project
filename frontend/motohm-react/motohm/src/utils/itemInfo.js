export const STATUS_LABELS = {
  Pending: 'Gözləmədə',
  Confirmed: 'Təsdiqləndi',
  Shipped: 'Göndərildi',
  Delivered: 'Çatdırıldı',
  Cancelled: 'Ləğv edildi',
};

export const STATUS_CLASS = {
  Pending: 'status-pending',
  Confirmed: 'status-confirmed',
  Shipped: 'status-shipped',
  Delivered: 'status-delivered',
  Cancelled: 'status-cancelled',
};

// İzləmə xəttindəki addımlar (ləğv bura daxil deyil)
export const TRACK_STEPS = ['Pending', 'Confirmed', 'Shipped', 'Delivered'];

// Admin üçün icazə verilən növbəti statuslar (backend OrderRules ilə eyni olmalıdır)
export const NEXT_ACTIONS = {
  Pending: [
    { status: 'Confirmed', label: 'Təsdiqlə' },
    { status: 'Cancelled', label: 'Ləğv et', danger: true },
  ],
  Confirmed: [
    { status: 'Shipped', label: 'Göndərildi' },
    { status: 'Cancelled', label: 'Ləğv et', danger: true },
  ],
  Shipped: [{ status: 'Delivered', label: 'Çatdırıldı' }],
  Delivered: [],
  Cancelled: [],
};
// Sifariş elementinin (OrderItemDto) adı və tipi

export function itemName(item) {
  return item.motorcycleName ?? item.partName ?? item.accessoryName ?? 'Silinmiş məhsul';
}

export function itemType(item) {
  if (item.motorcycleId != null) return 'Motosiklet';
  if (item.partId != null) return 'Ehtiyat hissəsi';
  if (item.accessoryId != null) return 'Aksesuar';
  return 'Məhsul';
}