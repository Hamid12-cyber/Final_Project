export default function RentalBanner() {
  return (
    <section className="banner rental-banner">
      <p className="banner-eyebrow">KİRAYƏ</p>
      <h3 className="banner-title">Motosiklet kirayəsi</h3>
      <p className="banner-desc">
        Günlük, həftəlik və aylıq kirayə imkanları ilə macəranı sən də yaşa!
      </p>
      <button className="banner-btn">Kataloqa bax →</button>

      <div className="rental-prices">
        <div>
          <p className="price-label">Günlük</p>
          <p className="price-value">50 AZN</p>
        </div>
        <div>
          <p className="price-label">Həftəlik</p>
          <p className="price-value">300 AZN</p>
        </div>
        <div>
          <p className="price-label">Aylıq</p>
          <p className="price-value">1000 AZN</p>
        </div>
      </div>
    </section>
  );
}
