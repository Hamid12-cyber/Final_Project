import { createContext, useContext, useState } from 'react';
import apiClient from '../api/client.js';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const stored = localStorage.getItem('user');
    return stored ? JSON.parse(stored) : null;
  });

  const saveUser = (next) => {
    localStorage.setItem('user', JSON.stringify(next));
    setUser(next);
  };

  const login = async (email, password) => {
    const res = await apiClient.post('/auth/login', { email, password });
    const { token, fullName, role, avatarUrl } = res.data;

    localStorage.setItem('token', token);
    saveUser({ fullName, role, avatarUrl: avatarUrl ?? null });
  };

  // Profil şəklini dəyişir. Boş / null göndərilsə şəkil silinir və default avatar qayıdır.
  const updateAvatar = async (avatarUrl) => {
    const res = await apiClient.put('/users/me/avatar', { avatarUrl: avatarUrl || null });
    const saved = res.data?.avatarUrl ?? null;
    saveUser({ ...user, avatarUrl: saved });
    return saved;
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, login, logout, updateAvatar }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}