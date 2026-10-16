import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import apiClient from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';
import StarRating from './StarRating.jsx';

// Üçündən yalnız birini ötür: motorcycleId, partId və ya accessoryId
export default function Reviews({ motorcycleId, partId, accessoryId }) {
  const { user } = useAuth();
  const [data, setData] = useState({ averageRating: 0, count: 0, items: [] });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const [rating, setRating] = useState(0);
  const [hover, setHover] = useState(0);
  const [comment, setComment] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState(null);
  const [busyId, setBusyId] = useState(null);

  const load = useCallback(() => {
    apiClient.get('/reviews', { params: { motorcycleId, partId, accessoryId } })
      .then((res) => {
        setData(res.data);
        // İstifadəçinin öz rəyi varsa, forma onunla dolur
        const mine = res.data.items.find((r) => r.isMine);
        setRating(mine ? mine.rating : 0);
        setComment(mine?.comment ?? '');
        setError(null);
      })
      .catch((err) => setError(err.response?.data?.message ?? err.message))
      .finally(() => setLoading(false));
  }, [motorcycleId, partId, accessoryId, user]);

  useEffect(() => {
    load();
  }, [load]);

  const hasMine = data.items.some((r) => r.isMine);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setFormError(null);

    if (rating < 1) {
      setFormError('Zəhmət olmasa ulduz seçin.');
      return;
    }

    setSubmitting(true);
    try {
      await apiClient.post('/reviews', {
        motorcycleId: motorcycleId ?? null,
        partId: partId ?? null,
        accessoryId: accessoryId ?? null,
        rating,
        comment: comment.trim() || null,
      });
      load();
    } catch (err) {
      setFormError(err.response?.data?.message ?? 'Rəy göndərilmədi.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Rəyi silmək istəyirsiniz?')) return;
    setBusyId(id);
    try {
      await apiClient.delete(`/reviews/${id}`);
      load();
    } catch (err) {
      alert(err.response?.data?.message ?? 'Rəy silinmədi.');
    } finally {
      setBusyId(null);
    }
  };

  return (
    <section className="reviews">
      <h2 className="reviews-title">Rəylər</h2>

      {loading ? (
        <div className="state-msg">Yüklənir...</div>
      ) : error ? (
        <div className="state-msg">Xəta: {error}</div>
      ) : (
        <>
          <div className="reviews-summary">
            <span className="reviews-avg">{data.count > 0 ? data.averageRating.toFixed(1) : '–'}</span>
            <StarRating value={data.averageRating} />
            <span className="stat-label" style={{ margin: 0 }}>{data.count} rəy</span>
          </div>

          {user ? (
            <form className="review-form" onSubmit={handleSubmit}>
              <p className="cart-item-name">{hasMine ? 'Rəyinizi dəyişin' : 'Rəy yazın'}</p>

              <div className="star-input" onMouseLeave={() => setHover(0)}>
                {[1, 2, 3, 4, 5].map((n) => (
                  <button
                    type="button"
                    key={n}
                    className={`star-btn ${n <= (hover || rating) ? 'on' : ''}`}
                    onMouseEnter={() => setHover(n)}
                    onClick={() => setRating(n)}
                    aria-label={`${n} ulduz`}
                  >
                    ★
                  </button>
                ))}
              </div>

              <textarea
                value={comment}
                onChange={(e) => setComment(e.target.value)}
                placeholder="Təcrübənizi yazın (istəyə görə)"
                maxLength={1000}
                rows={3}
              />

              {formError && <p className="form-error">{formError}</p>}

              <button className="checkout-btn" type="submit" disabled={submitting}>
                {submitting ? 'Göndərilir...' : hasMine ? 'Rəyi yenilə' : 'Rəyi göndər'}
              </button>
            </form>
          ) : (
            <p className="stat-label">
              Rəy yazmaq üçün <Link to="/login" className="accent">daxil olun</Link>.
            </p>
          )}

          {data.items.length === 0 ? (
            <p className="stat-label">Hələ rəy yoxdur. İlk rəyi siz yazın.</p>
          ) : (
            <div className="review-list">
              {data.items.map((r) => (
                <div className="review-item" key={r.id}>
                  <div className="review-head">
                    <span className="cart-item-name">{r.userName}{r.isMine ? ' (siz)' : ''}</span>
                    <StarRating value={r.rating} />
                    <span className="cart-item-type">{new Date(r.createdAt).toLocaleDateString('az-AZ')}</span>
                    {(r.isMine || user?.role === 'Admin') && (
                      <button
                        type="button"
                        className="remove-btn"
                        disabled={busyId === r.id}
                        onClick={() => handleDelete(r.id)}
                      >
                        Sil
                      </button>
                    )}
                  </div>
                  {r.comment && <p className="review-comment">{r.comment}</p>}
                </div>
              ))}
            </div>
          )}
        </>
      )}
    </section>
  );
}