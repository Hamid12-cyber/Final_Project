import { useRef, useState } from 'react';
import apiClient from '../api/client.js';

const ALLOWED = ['image/jpeg', 'image/png', 'image/webp'];
const MAX_SIZE = 5 * 1024 * 1024; // backend limiti ilə eyni: 5 MB
export const MAX_IMAGES = 5;

// props: value (şəkil URL-lərinin massivi), onChange(massiv), max (default 5)
export default function MultiImageUpload({ value = [], onChange, max = MAX_IMAGES }) {
  const inputRef = useRef(null);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState(null);

  const remaining = max - value.length;

  const handleFiles = async (e) => {
    const files = Array.from(e.target.files ?? []);
    e.target.value = '';
    if (files.length === 0) return;

    const problems = [];
    let picked = files;
    if (picked.length > remaining) {
      problems.push(`Ən çox ${max} şəkil yükləmək olar. İlk ${remaining} şəkil götürüldü.`);
      picked = picked.slice(0, remaining);
    }

    const valid = picked.filter((file) => {
      if (!ALLOWED.includes(file.type)) {
        problems.push(`${file.name}: yalnız JPG, PNG və ya WebP qəbul olunur.`);
        return false;
      }
      if (file.size > MAX_SIZE) {
        problems.push(`${file.name}: 5 MB-dan böyükdür.`);
        return false;
      }
      return true;
    });

    setError(problems.length ? problems.join(' ') : null);
    if (valid.length === 0) return;

    setUploading(true);
    const uploaded = [];
    try {
      for (const file of valid) {
        const form = new FormData();
        form.append('file', file);
        const res = await apiClient.post('/uploads/images', form);
        uploaded.push(res.data.url);
      }
    } catch (err) {
      setError(err.response?.data?.message ?? 'Şəkil yüklənmədi.');
    } finally {
      setUploading(false);
      if (uploaded.length) onChange([...value, ...uploaded]);
    }
  };

  const removeAt = (i) => onChange(value.filter((_, idx) => idx !== i));

  const makeCover = (i) => {
    const next = [...value];
    const [item] = next.splice(i, 1);
    onChange([item, ...next]);
  };

  return (
    <div className="img-upload">
      <span className="img-upload-label">Şəkillər ({value.length}/{max})</span>

      <div className="multi-img-grid">
        {value.map((url, i) => (
          <div className="multi-img-item" key={url}>
            <img src={url} alt={`Şəkil ${i + 1}`} />
            {i === 0 && <span className="multi-img-cover">Əsas</span>}
            <div className="multi-img-tools">
              {i !== 0 && (
                <button type="button" title="Əsas şəkil et" onClick={() => makeCover(i)} disabled={uploading}>★</button>
              )}
              <button type="button" title="Sil" onClick={() => removeAt(i)} disabled={uploading}>✕</button>
            </div>
          </div>
        ))}

        {remaining > 0 && (
          <button
            type="button"
            className="multi-img-add"
            onClick={() => inputRef.current?.click()}
            disabled={uploading}
          >
            {uploading ? 'Yüklənir...' : <><span>＋</span><small>Şəkil əlavə et</small></>}
          </button>
        )}
      </div>

      <input
        ref={inputRef}
        type="file"
        multiple
        accept="image/jpeg,image/png,image/webp"
        hidden
        onChange={handleFiles}
      />
      {error && <p className="form-error">{error}</p>}
    </div>
  );
}