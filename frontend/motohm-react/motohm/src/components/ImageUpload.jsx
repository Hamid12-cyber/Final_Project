import { useRef, useState } from 'react';
import apiClient from '../api/client.js';

const ALLOWED = ['image/jpeg', 'image/png', 'image/webp'];
const MAX_SIZE = 5 * 1024 * 1024; // backend limiti ilə eyni: 5 MB

// props: value (şəkil URL-i), onChange(url)
export default function ImageUpload({ value, onChange }) {
  const inputRef = useRef(null);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState(null);

  const handleFile = async (e) => {
    const file = e.target.files?.[0];
    e.target.value = ''; // eyni faylı yenidən seçmək mümkün olsun
    if (!file) return;

    if (!ALLOWED.includes(file.type)) {
      setError('Yalnız JPG, PNG və ya WebP şəkilləri qəbul olunur.');
      return;
    }
    if (file.size > MAX_SIZE) {
      setError('Şəkil 5 MB-dan böyük ola bilməz.');
      return;
    }

    setError(null);
    setUploading(true);
    try {
      const form = new FormData();
      form.append('file', file);
      const res = await apiClient.post('/uploads/images', form);
      onChange(res.data.url);
    } catch (err) {
      setError(err.response?.data?.message ?? 'Şəkil yüklənmədi.');
    } finally {
      setUploading(false);
    }
  };

  return (
    <div className="img-upload">
      <span className="img-upload-label">Şəkil</span>
      <div className="img-upload-row">
        <div className="img-upload-preview">
          {value ? <img src={value} alt="Önizləmə" /> : <span>🏍️</span>}
        </div>
        <div className="img-upload-actions">
          <button type="button" className="seller-btn" onClick={() => inputRef.current?.click()} disabled={uploading}>
            {uploading ? 'Yüklənir...' : value ? 'Şəkli dəyiş' : 'Şəkil seç'}
          </button>
          {value && !uploading && (
            <button type="button" className="seller-btn seller-btn-danger" onClick={() => onChange('')}>
              Sil
            </button>
          )}
        </div>
        <input ref={inputRef} type="file" accept="image/jpeg,image/png,image/webp" hidden onChange={handleFile} />
      </div>
      {error && <p className="form-error">{error}</p>}
    </div>
  );
}