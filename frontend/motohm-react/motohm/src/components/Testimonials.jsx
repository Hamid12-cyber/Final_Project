import { useState, useEffect } from 'react';
import apiClient from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';

export default function Testimonials() {
  const { user } = useAuth();
  const [testimonials, setTestimonials] = useState([]);
  const [showForm, setShowForm] = useState(false);
  const [customerName, setCustomerName] = useState('');
  const [rating, setRating] = useState(5);
  const [text, setText] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState(null);

  const canReview = user && user.role !== 'Seller';

  const loadTestimonials = () => {
    apiClient.get('/testimonials')
      .then((res) => setTestimonials(res.data))
      .catch(() => setTestimonials([]));
  };

  useEffect(() => {
    loadTestimonials();
    if (user) setCustomerName(user.fullName);
  }, [user]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setFormError(null);
    setSubmitting(true);
    try {
      await apiClient.post('/testimonials', { customerName, rating: Number(rating), text });
      setText('');
      setRating(5);
      setShowForm(false);
      loadTestimonials();
    } catch (err) {
      setFormError(err.response?.data?.message ?? 'Rəy göndərilmədi.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <section className="section">
      <div className="section-header">
        <h2>Müştəri rəyləri</h2>
        {canReview && (
          <button className="see-all" onClick={() => setShowForm((v) => !v)}>
            {showForm ? 'Bağla ✕' : 'Rəy yaz →'}
          </button>
        )}
      </div>

      {!user && (
        <p className="cart-item-type" style={{ marginBottom: 16 }}>
          Rəy yazmaq üçün hesabınıza daxil olun.
        </p>
      )}
      {user && !canReview && (
        <p className="cart-item-type" style={{ marginBottom: 16 }}>
          Satıcı hesabları rəy yaza bilmir.
        </p>
      )}

      {showForm && canReview && (
        <form className="checkout-form" style={{ maxWidth: 480, marginBottom: 24 }} onSubmit={handleSubmit}>
          <label className="form-field">
            <span>Qiymətləndirmə</span>
            <select value={rating} onChange={(e) => setRating(e.target.value)}>
              {[5, 4, 3, 2, 1].map((r) => (
                <option key={r} value={r}>{'★'.repeat(r)} ({r})</option>
              ))}
            </select>
          </label>

          <label className="form-field">
            <span>Rəyiniz</span>
            <input type="text" value={text} onChange={(e) => setText(e.target.value)} required maxLength={1000} />
          </label>

          {formError && <p className="form-error">{formError}</p>}

          <button className="checkout-btn" type="submit" disabled={submitting}>
            {submitting ? 'Göndərilir...' : 'Rəyi göndər'}
          </button>
        </form>
      )}

      {testimonials.length === 0 ? (
        <p className="cart-item-type">Hələ rəy yoxdur.</p>
      ) : (
        <div className="testimonial-grid">
          {testimonials.map((t) => (
            <div className="testimonial-card" key={t.id}>
              <p className="testimonial-name">{t.customerName}</p>
              <p className="testimonial-rating">{'★'.repeat(t.rating)}</p>
              <p className="testimonial-text">{t.text}</p>
              <p className="testimonial-date">{new Date(t.createdAt).toLocaleDateString('az-AZ')}</p>
            </div>
          ))}
        </div>
      )}
    </section>
  );
}