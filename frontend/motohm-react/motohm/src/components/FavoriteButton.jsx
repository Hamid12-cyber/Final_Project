import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';
import { useFavorites } from '../context/FavoritesContext.jsx';

function HeartIcon({ filled }) {
  return (
    <svg
      width="20"
      height="20"
      viewBox="0 0 24 24"
      fill={filled ? 'currentColor' : 'none'}
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z" />
    </svg>
  );
}

// props: motorcycleId, className ('inline' = səbət düyməsinin yanında)
// Yalnız ürək ikonu: basanda sevimlilərə əlavə olunur, yenə basanda çıxarılır
export default function FavoriteButton({ motorcycleId, className = '' }) {
  const { user } = useAuth();
  const { isFavorite, toggleFavorite } = useFavorites();
  const navigate = useNavigate();
  const [busy, setBusy] = useState(false);

  const active = !!user && isFavorite(motorcycleId);
  const label = active ? 'Sevimlilərdən çıxar' : 'Sevimlilərə əlavə et';

  const handleClick = async () => {
    if (!user) {
      navigate('/login');
      return;
    }
    setBusy(true);
    try {
      await toggleFavorite(motorcycleId);
    } catch {
      // səssiz: vəziyyət dəyişmir, istifadəçi yenidən cəhd edə bilər
    } finally {
      setBusy(false);
    }
  };

  return (
    <button
      type="button"
      className={`fav-btn ${className} ${active ? 'active' : ''}`}
      onClick={handleClick}
      disabled={busy}
      aria-pressed={active}
      aria-label={label}
      title={label}
    >
      <HeartIcon filled={active} />
    </button>
  );
}