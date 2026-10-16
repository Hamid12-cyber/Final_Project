import { useRef, useState } from 'react';
import apiClient from '../api/client.js';

const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp'];
const MAX_SIZE = 5 * 1024 * 1024; // 5 MB

// value: şəklin URL-i, onChange(url): yeni URL (və ya boş sətir) göndərilir
export default function ImageUpload({ value, onChange }) {
  const inputRef = useRef(null);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState(null);

  const handleFile = async (e) => {
    const file = e.target.files?.[0];
    e.target.value = ''; // eyni faylı yenidən seçmək mümkün olsun
    if (!file) return;

    setError(null);

    if (!ALLOWED_TYPES.includes(file.type)) {
      setError('Yalnız JPG, PNG və ya WebP şəkilləri qəbul olunur.');
      return;
    }
    if (file.size > MAX_SIZE) {
      setError('Şəkil 5 MB-dan böyük ola bilməz.');
      return;
    }

    const formData = new FormData();
    formData.append('file', file);

    setUploading(true);
    try {
      const res = await apiClient.post('/uploads/images', formData);
      onChange(res.data.url);
    } catch (err) {
      setError(err.response?.data?.message ?? 'Şəkil yüklənmədi.');
    } finally {
      setUploading(false);
    }
  };

  return (
    <div className="form-field">
      <span>Şəkil (istəyə görə)</span>

      <div className="image-upload">
        {value ? (
          <div className="image-upload-preview">
            <img src={value} alt="Önizləmə" />
          </div>
        ) : (
          <div className="image-upload-empty">Şəkil seçilməyib</div>
        )}

        <div className="image-upload-actions">
          <input
            ref={inputRef}
            type="file"
            accept="image/jpeg,image/png,image/webp"
            style={{ display: 'none' }}
            onChange={handleFile}
          />
          <button
            type="button"
            className="filter-chip"
            disabled={uploading}
            onClick={() => inputRef.current?.click()}
          >
            {uploading ? 'Yüklənir...' : value ? 'Şəkli dəyiş' : 'Şəkil seç'}
          </button>
          {value && (
            <button type="button" className="filter-chip" disabled={uploading} onClick={() => onChange('')}>
              Sil
            </button>
          )}
        </div>

        <input
          type="text"
          value={value}
          onChange={(e) => onChange(e.target.value)}
          placeholder="və ya şəkil URL-i yaz: https://..."
        />
      </div>

      {error && <p className="form-error">{error}</p>}
    </div>
  );
}