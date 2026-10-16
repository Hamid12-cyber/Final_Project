import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';
import { useFavorites } from '../context/FavoritesContext.jsx';

// className="inline" verilsə düymə kartın üstündə yox, öz yerində durur
export default function FavoriteButton({ motorcycleId, className = '' }) {
  const { user } = useAuth();
  const { isFavorite, toggleFavorite } = useFavorites();
  const navigate = useNavigate();
  const [busy, setBusy] = useState(false);

  const active = isFavorite(motorcycleId);

  const handleClick = async (e) => {
    // Düymə link içində ola bilər, səhifəyə keçməsin
    e.preventDefault();
    e.stopPropagation();

    if (!user) {
      navigate('/login');
      return;
    }

    setBusy(true);
    try {
      await toggleFavorite(motorcycleId);
    } catch (err) {
      alert(err.response?.data?.message ?? 'Əməliyyat alınmadı.');
    } finally {
      setBusy(false);
    }
  };

  const label = active ? 'Favorilərdən çıxar' : 'Favorilərə əlavə et';

  return (
    <button
      type="button"
      className={`fav-btn ${active ? 'active' : ''} ${className}`}
      disabled={busy}
      onClick={handleClick}
      aria-label={label}
      title={label}
    >
      <svg width="18" height="18" viewBox="0 0 24 24" fill={active ? 'currentColor' : 'none'}
        stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z" />
      </svg>
    </button>
  );
}