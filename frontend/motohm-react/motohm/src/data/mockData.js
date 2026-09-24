// Bu fayl müvəqqəti test datasıdır.
// API hazır olduqda bu massivləri backend-dən gələn fetch/axios sorğuları ilə əvəz et.

export const popularBikes = [
  { id: 1, name: 'Honda CB500F', cc: '500cc', year: 2021, price: '9 500 AZN' },
  { id: 2, name: 'Yamaha MT-07', cc: '700cc', year: 2020, price: '12 000 AZN' },
  { id: 3, name: 'KTM Duke 390', cc: '390cc', year: 2022, price: '10 500 AZN' },
  { id: 4, name: 'Kawasaki Ninja 400', cc: '400cc', year: 2021, price: '11 000 AZN' },
  { id: 5, name: 'Suzuki GSX-S750', cc: '750cc', year: 2019, price: '9 000 AZN' }
];

export const partCategories = [
  { id: 1, icon: '🛞', name: 'Təkərlər' },
  { id: 2, icon: '⚙️', name: 'Əyləc sistemi' },
  { id: 3, icon: '🔧', name: 'Mühərrik hissələri' },
  { id: 4, icon: '🌀', name: 'Filtrlər' },
  { id: 5, icon: '🔋', name: 'Akkumulyator' },
  { id: 6, icon: '💡', name: 'Elektronika' },
  { id: 7, icon: '🔦', name: 'İşıqlandırma' },
  { id: 8, icon: '⛓️', name: 'Zəncir & Dişlilər' }
];

export const testimonials = [
  {
    id: 1,
    name: 'Rəşad Məmmədov',
    rating: 5,
    text: 'Çox keyfiyyətli məhsullar və sürətli çatdırılma. Mütləq tövsiyə edirəm!',
    date: '12.05.2025'
  },
  {
    id: 2,
    name: 'Nurlan Əliyev',
    rating: 5,
    text: 'Kirayə motosiklet xidməti super idi. Motosiklet təmiz və problemsiz idi.',
    date: '28.04.2025'
  },
  {
    id: 3,
    name: 'Aysel Kazımova',
    rating: 5,
    text: 'Ehtiyat hissələri orijinaldır. Qiymətlər münasibdir. Məmnun qaldım.',
    date: '15.04.2025'
  }
];
