import { useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import apiClient from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';
import Avatar from '../components/Avatar.jsx';
import Header from '../components/Header.jsx';
import Footer from '../components/Footer.jsx';

const ALLOWED = ['image/jpeg', 'image/png', 'image/webp'];
const MAX_SIZE = 5 * 1024 * 1024; // backend limiti ilə eyni: 5 MB

export default function ProfilePage() {
  const { t } = useTranslation();
  const { user, updateAvatar } = useAuth();
  const inputRef = useRef(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState(null);
  const [message, setMessage] = useState(null);

  const handleFile = async (e) => {
    const file = e.target.files?.[0];
    e.target.value = ''; // eyni faylı yenidən seçmək mümkün olsun
    if (!file) return;

    setMessage(null);
    if (!ALLOWED.includes(file.type)) {
      setError(t('profile.typeError'));
      return;
    }
    if (file.size > MAX_SIZE) {
      setError(t('profile.sizeError'));
      return;
    }

    setError(null);
    setBusy(true);
    try {
      const form = new FormData();
      form.append('file', file);
      const upload = await apiClient.post('/uploads/images', form);
      await updateAvatar(upload.data.url);
      setMessage(t('profile.saved'));
    } catch (err) {
      setError(err.response?.data?.message ?? t('profile.error'));
    } finally {
      setBusy(false);
    }
  };

  const handleRemove = async () => {
    setError(null);
    setMessage(null);
    setBusy(true);
    try {
      await updateAvatar(null);
      setMessage(t('profile.removed'));
    } catch (err) {
      setError(err.response?.data?.message ?? t('profile.error'));
    } finally {
      setBusy(false);
    }
  };

  return (
    <>
      <Header />
      <div className="page-wrap">
        <div className="listing-wrap">
          <h1 className="page-title">{t('profile.title')}</h1>

          <div className="profile-card">
            <Avatar name={user.fullName} src={user.avatarUrl} size={120} />

            <div className="profile-info">
              <div className="profile-name">{user.fullName}</div>
              <p className="profile-hint">{t('profile.hint')}</p>

              <div className="img-upload-actions">
                <button type="button" className="seller-btn" onClick={() => inputRef.current?.click()} disabled={busy}>
                  {busy ? t('profile.uploading') : user.avatarUrl ? t('profile.change') : t('profile.upload')}
                </button>
                {user.avatarUrl && !busy && (
                  <button type="button" className="seller-btn seller-btn-danger" onClick={handleRemove}>
                    {t('profile.remove')}
                  </button>
                )}
              </div>

              <input ref={inputRef} type="file" accept="image/jpeg,image/png,image/webp" hidden onChange={handleFile} />
              {error && <p className="form-error">{error}</p>}
              {message && !error && <p className="profile-ok">{message}</p>}
            </div>
          </div>
        </div>
      </div>
      <Footer />
    </>
  );
}