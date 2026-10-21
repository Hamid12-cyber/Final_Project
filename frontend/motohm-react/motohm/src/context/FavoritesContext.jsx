import { createContext, useContext, useState, useEffect, useCallback, useMemo } from 'react';
import apiClient from '../api/client.js';
import { useAuth } from './AuthContext.jsx';

const FavoritesContext = createContext(null);

export function FavoritesProvider({ children }) {
  const { user } = useAuth();
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);

  const refresh = useCallback(() => {
    if (!user) {
      setItems([]);
      return Promise.resolve();
    }
    setLoading(true);
    return apiClient.get('/favorites')
      .then((res) => setItems(res.data))
      .catch(() => setItems([]))
      .finally(() => setLoading(false));
  }, [user]);

  // Login/logout olanda siyahını yenilə
  useEffect(() => { refresh(); }, [refresh]);

  const ids = useMemo(() => new Set(items.map((i) => i.id)), [items]);

  const isFavorite = (motorcycleId) => ids.has(motorcycleId);

  const toggleFavorite = async (motorcycleId) => {
    if (ids.has(motorcycleId)) {
      await apiClient.delete(`/favorites/${motorcycleId}`);
      setItems((prev) => prev.filter((i) => i.id !== motorcycleId));
    } else {
      await apiClient.put(`/favorites/${motorcycleId}`);
      await refresh(); // yeni elementin tam məlumatı (ad, şəkil) üçün
    }
  };

  return (
    <FavoritesContext.Provider value={{ items, loading, isFavorite, toggleFavorite, refresh }}>
      {children}
    </FavoritesContext.Provider>
  );
}

export function useFavorites() {
  return useContext(FavoritesContext);
}