import { createContext, useContext, useState, useEffect, useCallback, useMemo } from 'react';
import apiClient from '../api/client.js';
import { useAuth } from './AuthContext.jsx';

const FavoritesContext = createContext(null);

export function FavoritesProvider({ children }) {
  const { user } = useAuth();
  const [favorites, setFavorites] = useState([]);
  const [loading, setLoading] = useState(false);

  const refreshFavorites = useCallback(() => {
    if (!user) {
      setFavorites([]);
      return Promise.resolve();
    }
    setLoading(true);
    return apiClient.get('/favorites')
      .then((res) => setFavorites(res.data))
      .catch(() => setFavorites([]))
      .finally(() => setLoading(false));
  }, [user]);

  // İstifadəçi login/logout olanda favorilər yenilənir
  useEffect(() => {
    refreshFavorites();
  }, [refreshFavorites]);

  const ids = useMemo(() => new Set(favorites.map((f) => f.id)), [favorites]);

  const isFavorite = (motorcycleId) => ids.has(motorcycleId);

  const toggleFavorite = async (motorcycleId) => {
    if (ids.has(motorcycleId)) {
      await apiClient.delete(`/favorites/${motorcycleId}`);
      setFavorites((prev) => prev.filter((f) => f.id !== motorcycleId));
    } else {
      await apiClient.put(`/favorites/${motorcycleId}`);
      await refreshFavorites();
    }
  };

  return (
    <FavoritesContext.Provider
      value={{ favorites, loading, count: favorites.length, isFavorite, toggleFavorite, refreshFavorites }}
    >
      {children}
    </FavoritesContext.Provider>
  );
}

export function useFavorites() {
  return useContext(FavoritesContext);
}