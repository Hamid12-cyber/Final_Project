import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import apiClient from '../api/client.js';
import { LOCALES } from '../i18n/index.js';

const MIN_CARDS = 8; // lent boş qalmasın deyə ən azı bu qədər kart göstərilir
const SECONDS_PER_CARD = 6; // fırlanma sürəti: kart başına saniyə
const IMAGE_SWITCH_MS = 900; // üstündə olanda şəkillərin dəyişmə sürəti

// Motosikletin bütün şəkilləri: images / imageUrls siyahısı varsa onu, yoxsa tək imageUrl-u götürür
function getImages(bike) {
  const raw = bike.images ?? bike.imageUrls ?? bike.photos ?? [];
  const list = raw.map((x) => (typeof x === 'string' ? x : x?.url)).filter(Boolean);
  if (bike.imageUrl && !list.includes(bike.imageUrl)) list.unshift(bike.imageUrl);
  return list;
}

function SliderCard({ bike }) {
  const { t, i18n } = useTranslation();
  const images = getImages(bike);
  const [hover, setHover] = useState(false);
  const [idx, setIdx] = useState(0);

  // Siçan kartın üstündə olanda şəkillər öz-özünə dəyişir (kliklə yox)
  useEffect(() => {
    if (!hover || images.length < 2) return;
    const id = setInterval(() => setIdx((i) => (i + 1) % images.length), IMAGE_SWITCH_MS);
    return () => clearInterval(id);
  }, [hover, images.length]);

  const handleEnter = () => {
    setHover(true);
    if (images.length > 1) setIdx(1); // dərhal ikinci şəkli göstər
  };
  const handleLeave = () => {
    setHover(false);
    setIdx(0);
  };

  return (
    <Link
      to={`/motorcycles/${bike.id}`}
      className="slider-card"
      onMouseEnter={handleEnter}
      onMouseLeave={handleLeave}
      onFocus={handleEnter}
      onBlur={handleLeave}
    >
      <div className="slider-card-image">
        {images.map((src, i) => (
          <img
            key={src}
            src={src}
            alt={i === 0 ? bike.name : ''}
            className={i === idx ? 'active' : ''}
            loading="lazy"
          />
        ))}
        {images.length > 1 && (
          <div className="slider-dots" aria-hidden="true">
            {images.map((src, i) => (
              <span key={src} className={`slider-dot ${i === idx ? 'active' : ''}`} />
            ))}
          </div>
        )}
      </div>
      <div className="slider-card-info">
        <p className="slider-card-name">{bike.name}</p>
        <p className="slider-card-meta">{bike.brand} · {bike.year}</p>
        <p className="slider-card-price">
          {bike.price.toLocaleString(LOCALES[i18n.resolvedLanguage])} {t('currency.azn')}
        </p>
      </div>
    </Link>
  );
}

export default function BikeSlider() {
  const { t } = useTranslation();
  const [bikes, setBikes] = useState([]);

  useEffect(() => {
    apiClient.get('/motorcycles')
      .then((res) => {
        const list = res.data.items ?? res.data;
        setBikes(list.slice(0, 4)); // hazırda ilk 4-i göstəririk; "seçilmiş" bayrağı backend-ə əlavə olunanda dəyişəcək
      })
      .catch(() => setBikes([]));
  }, []);

  if (bikes.length === 0) return null;

  // Az motosiklet olsa da lent dolu görünsün; sonra sonsuz döngü üçün iki dəfə təkrarlanır
  const repeats = Math.ceil(MIN_CARDS / bikes.length);
  const base = Array.from({ length: repeats }, () => bikes).flat();
  const loop = [...base, ...base];

  return (
    <section className="bike-slider-section">
      <div className="section-header">
        <h2>{t('home.sliderTitle')}</h2>
      </div>

      <div className="slider-track">
        <div className="slider-rail" style={{ '--slider-duration': `${base.length * SECONDS_PER_CARD}s` }}>
          {loop.map((bike, i) => (
            <SliderCard bike={bike} key={`${bike.id}-${i}`} />
          ))}
        </div>
      </div>
    </section>
  );
}