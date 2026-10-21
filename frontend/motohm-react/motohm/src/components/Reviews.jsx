import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import apiClient from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';
import StarRating from './StarRating.jsx';

// Yalnız birini ötür: motorcycleId | partId | accessoryId
export default function Reviews({ motorcycleId, partId, accessoryId }) {
  const { user } = useAuth();
  const [data, setData] = useState({ averageRating: 0, count: 0, items: [] });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const [rating, setRating] = useState(0);
  const [comment, setComment] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState(null);
  const [deleting, setDeleting] = useState(false);

  const mine = data.items.find((r) => r.isMine);

  const load = useCallback(() => {
    setError(null);
    return apiClient.get('/reviews', { params: { motorcycleId, partId, accessoryId } })
      .then((res) => setData(res.data))
      .catch((err) => setError(err.response?.data?.message ?? 'Rəylər yüklənmədi.'))
      .finally(() => setLoading(false));
  }, [motorcycleId, partId, accessoryId]);

  // user dəyişəndə (login/logout) "isMine" yenilənsin
  useEffect(() => { load(); }, [load, user]);

  // İstifadəçinin öz rəyi varsa, formu onunla doldur
  useEffect(() => {
    setRating(mine?.rating ?? 0);
    setComment(mine?.comment ?? '');
  }, [mine?.id, mine?.rating, mine?.comment]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (rating < 1) {
      setFormError('Ulduz seç (1-5).');
      return;
    }
    setFormError(null);
    setSubmitting(true);
    try {
      await apiClient.post('/reviews', {
        motorcycleId: motorcycleId ?? null,
        partId: partId ?? null,
        accessoryId: accessoryId ?? null,
        rating,
        comment: comment.trim() || null,
      });
      await load();
    } catch (err) {
      const errors = err.response?.data?.errors;
      const firstValidation = errors ? Object.values(errors).flat()[0] : null;
      setFormError(firstValidation ?? err.response?.data?.message ?? 'Rəy göndərilmədi.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!mine || !window.confirm('Rəyini silmək istəyirsən?')) return;
    setDeleting(true);
    try {
      await apiClient.delete(`/reviews/${mine.id}`);
      setRating(0);
      setComment('');
      await load();
    } catch (err) {
      setFormError(err.response?.data?.message ?? 'Rəy silinmədi.');
    } finally {
      setDeleting(false);
    }
  };

  return (
    <section className="reviews">
      <h2 className="reviews-title">Rəylər</h2>

      {loading ? (
        <p className="cart-item-type">Yüklənir...</p>
      ) : error ? (
        <p className="form-error">{error}</p>
      ) : (
        <>
          <div className="reviews-summary">
            <span className="reviews-avg">{data.count > 0 ? data.averageRating.toFixed(1) : '–'}</span>
            <StarRating value={data.averageRating} showText={false} />
            <span className="cart-item-type">
              {data.count > 0 ? `${data.count} rəy` : 'Hələ rəy yoxdur'}
            </span>
          </div>

          {user ? (
            <form className="review-form" onSubmit={handleSubmit}>
              <strong>{mine ? 'Rəyini yenilə' : 'Rəy yaz'}</strong>
              <StarRating value={rating} onChange={setRating} />
              <textarea
                rows={3}
                maxLength={1000}
                placeholder="Fikirlərini yaz (istəyə bağlı)"
                value={comment}
                onChange={(e) => setComment(e.target.value)}
              />
              {formError && <p className="form-error">{formError}</p>}
              <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap' }}>
                <button type="submit" className="checkout-btn" disabled={submitting}>
                  {submitting ? 'Göndərilir...' : mine ? 'Yenilə' : 'Göndər'}
                </button>
                {mine && (
                  <button type="button" className="seller-btn seller-btn-danger" onClick={handleDelete} disabled={deleting}>
                    {deleting ? 'Silinir...' : 'Rəyimi sil'}
                  </button>
                )}
              </div>
            </form>
          ) : (
            <p className="cart-item-type" style={{ marginBottom: 24 }}>
              Rəy yazmaq üçün <Link to="/login" className="see-all">daxil ol</Link>.
            </p>
          )}

          <div className="review-list">
            {data.items.map((r) => (
              <article className="review-item" key={r.id}>
                <div className="review-head">
                  <strong>{r.userName}{r.isMine && ' (sən)'}</strong>
                  <StarRating value={r.rating} showText={false} />
                  <span className="cart-item-type">{new Date(r.createdAt).toLocaleDateString('az-AZ')}</span>
                </div>
                {r.comment && <p className="review-comment">{r.comment}</p>}
              </article>
            ))}
          </div>
        </>
      )}
    </section>
  );
}