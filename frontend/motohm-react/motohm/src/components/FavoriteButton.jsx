import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';
import { useFavorites } from '../context/FavoritesContext.jsx';

// props: motorcycleId, className ('inline' = səbət düyməsinin yanında)
export default function FavoriteButton({ motorcycleId, className = '' }) {
  const { user } = useAuth();
  const { isFavorite, toggleFavorite } = useFavorites();
  const navigate = useNavigate();
  const [busy, setBusy] = useState(false);

  const active = !!user && isFavorite(motorcycleId);

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
    >
      <span aria-hidden="true">{active ? '♥' : '♡'}</span>{' '}
      {active ? 'Sevimlilərdə' : 'Sevimlilərə əlavə et'}
    </button>
  );
}