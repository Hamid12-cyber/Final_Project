import { createContext, useContext, useState, useEffect, useCallback } from 'react';
import apiClient from '../api/client.js';
import { useAuth } from './AuthContext.jsx';

const CartContext = createContext(null);

export function CartProvider({ children }) {
  const { user } = useAuth();
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);

  const refreshCart = useCallback(() => {
    if (!user) {
      setItems([]);
      return;
    }
    setLoading(true);
    apiClient.get('/cart')
      .then((res) => setItems(res.data))
      .catch(() => setItems([]))
      .finally(() => setLoading(false));
  }, [user]);

  // İstifadəçi login/logout olanda səbəti yenilə
  useEffect(() => {
    refreshCart();
  }, [refreshCart]);

  const addToCart = async ({ motorcycleId = null, partId = null, quantity = 1 }) => {
    await apiClient.post('/cart/items', { motorcycleId, partId, quantity });
    await refreshCart();
  };

  const updateQuantity = async (cartItemId, quantity) => {
    await apiClient.put(`/cart/items/${cartItemId}`, { quantity });
    await refreshCart();
  };

  const removeItem = async (cartItemId) => {
    await apiClient.delete(`/cart/items/${cartItemId}`);
    await refreshCart();
  };

  const count = items.reduce((sum, i) => sum + i.quantity, 0);
  const total = items.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0);

  return (
    <CartContext.Provider
      value={{ items, loading, count, total, addToCart, updateQuantity, removeItem, refreshCart }}
    >
      {children}
    </CartContext.Provider>
  );
}

export function useCart() {
  return useContext(CartContext);
}