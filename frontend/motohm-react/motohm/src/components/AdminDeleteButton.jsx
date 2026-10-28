import { useState } from 'react';
import apiClient from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';

// Yalnız Admin-ə görünür. Elanı backend-dən silir və uğurlu olanda onDeleted(id) çağırır.
// props: endpoint ('motorcycles' | 'parts' | 'accessories'), id, name, onDeleted(id), className
export default function AdminDeleteButton({ endpoint, id, name, onDeleted, className = '' }) {
  const { user } = useAuth();
  const [busy, setBusy] = useState(false);

  if (user?.role !== 'Admin') return null;

  const handleClick = async (e) => {
    // Kart Link-in içində ola bilər: naviqasiyanın qarşısını alırıq
    e.preventDefault();
    e.stopPropagation();
    if (!window.confirm(`"${name}" elanı həmişəlik silinsin?`)) return;
    setBusy(true);
    try {
      await apiClient.delete(`/${endpoint}/${id}`);
      onDeleted?.(id);
    } catch (err) {
      const status = err.response?.status;
      alert(
        err.response?.data?.message
          ?? (status === 403 ? 'Bu elanı silmək icazəniz yoxdur.' : 'Elan silinmədi.')
      );
    } finally {
      setBusy(false);
    }
  };

  return (
    <button
      type="button"
      className={`admin-reject-btn admin-delete-card ${className}`}
      disabled={busy}
      onClick={handleClick}
    >
      {busy ? 'Silinir...' : '🗑 Elanı sil (admin)'}
    </button>
  );
}