import { useState, useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import apiClient from '../api/client.js';

export default function BikeSlider() {
  const [bikes, setBikes] = useState([]);
  const trackRef = useRef(null);
  const timerRef = useRef(null);

  useEffect(() => {
    apiClient.get('/motorcycles')
      .then((res) => {
        const list = res.data.items ?? res.data;
        setBikes(list.slice(0, 8)); // hazırda ilk 8-i göstəririk; "seçilmiş" bayrağı backend-ə əlavə olunanda dəyişəcək
      })
      .catch(() => setBikes([]));
  }, []);

  const scrollByCards = (dir) => {
    const el = trackRef.current;
    if (!el) return;
    const card = el.firstElementChild;
    const cardWidth = card ? card.offsetWidth + 20 : 260;

    const atEnd = el.scrollLeft + el.clientWidth >= el.scrollWidth - 10;
    const atStart = el.scrollLeft <= 10;

    if (dir > 0 && atEnd) {
      el.scrollTo({ left: 0, behavior: 'smooth' });
    } else if (dir < 0 && atStart) {
      el.scrollTo({ left: el.scrollWidth, behavior: 'smooth' });
    } else {
      el.scrollBy({ left: dir * cardWidth, behavior: 'smooth' });
    }
  };

  useEffect(() => {
    if (bikes.length <= 4) return;
    timerRef.current = setInterval(() => scrollByCards(1), 4000);
    return () => clearInterval(timerRef.current);
  }, [bikes]);

  if (bikes.length === 0) return null;

  return (
    <section className="bike-slider-section">
      <div className="section-header">
        <h2>Seçilmiş modellər</h2>
        <div style={{ display: 'flex', gap: 8 }}>
          <button className="slider-arrow" onClick={() => scrollByCards(-1)} aria-label="Əvvəlki">‹</button>
          <button className="slider-arrow" onClick={() => scrollByCards(1)} aria-label="Növbəti">›</button>
        </div>
      </div>

      <div className="slider-track" ref={trackRef}>
        {bikes.map((bike) => (
          <Link to={`/motorcycles/${bike.id}`} className="slider-card" key={bike.id}>
            <div className="slider-card-image">
              {bike.imageUrl && <img src={bike.imageUrl} alt={bike.name} />}
            </div>
            <div className="slider-card-info">
              <p className="slider-card-name">{bike.name}</p>
              <p className="slider-card-meta">{bike.brand} · {bike.year}</p>
              <p className="slider-card-price">{bike.price.toLocaleString('az-AZ')} AZN</p>
            </div>
          </Link>
        ))}
      </div>
    </section>
  );
}