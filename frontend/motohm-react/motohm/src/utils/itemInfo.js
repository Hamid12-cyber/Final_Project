// Səbət və sifariş sətirlərində məhsulun adı və növü (motosiklet / ehtiyat hissəsi / aksesuar)
export function itemName(item) {
  return item.motorcycleName ?? item.partName ?? item.accessoryName ?? 'Məhsul';
}

export function itemType(item) {
  if (item.motorcycleId) return 'Motosiklet';
  if (item.partId) return 'Ehtiyat hissəsi';
  if (item.accessoryId) return 'Aksesuar';
  return '';
}