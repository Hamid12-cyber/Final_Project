import { useState, useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import apiClient from '../api/client.js';

export default function BikeSlider() {
  const [bikes, setBikes] = useState([]);
  const [index, setIndex] = useState(0);
  const timerRef = useRef(null);

  useEffect(() => {
    apiClient.get('/motorcycles')
      .then((res) => {
        const list = res.data.items ?? res.data;
        setBikes(list.slice(0, 6)); // hazırda ilk 6-nı göstəririk; "seçilmiş" bayrağı backend-ə əlavə olunanda dəyişəcək
      })
      .catch(() => setBikes([]));
  }, []);

  useEffect(() => {
    if (bikes.length <= 1) return;
    timerRef.current = setInterval(() => {
      setIndex((i) => (i + 1) % bikes.length);
    }, 4500);
    return () => clearInterval(timerRef.current);
  }, [bikes]);

  if (bikes.length === 0) return null;

  const goTo = (i) => {
    clearInterval(timerRef.current);
    setIndex(i);
  };
  const prev = () => goTo((index - 1 + bikes.length) % bikes.length);
  const next = () => goTo((index + 1) % bikes.length);

  const bike = bikes[index];

  return (
    <section className="bike-slider">
      <button className="slider-arrow slider-arrow-left" onClick={prev} aria-label="Əvvəlki">‹</button>

      <Link to={`/motorcycles/${bike.id}`} className="slider-slide">
        <div className="slider-image">
          {bike.imageUrl && <img src={bike.imageUrl} alt={bike.name} />}
        </div>
        <div className="slider-info">
          <p className="slider-eyebrow">Seçilmiş model</p>
          <h3 className="slider-name">{bike.name}</h3>
          <p className="slider-meta">{bike.brand} · {bike.year}</p>
          <p className="slider-price">{bike.price.toLocaleString('az-AZ')} AZN</p>
        </div>
      </Link>

      <button className="slider-arrow slider-arrow-right" onClick={next} aria-label="Növbəti">›</button>

      <div className="slider-dots">
        {bikes.map((b, i) => (
          <button
            key={b.id}
            className={`slider-dot ${i === index ? 'active' : ''}`}
            onClick={() => goTo(i)}
            aria-label={`Slayd ${i + 1}`}
          />
        ))}
      </div>
    </section>
  );
}