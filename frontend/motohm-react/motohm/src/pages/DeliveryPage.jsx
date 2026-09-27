export default function DeliveryPage() {
  return (
    <div className="page-wrap" style={{ maxWidth: 720 }}>
      <h1 className="page-title">Çatdırılma</h1>
      <p className="cart-item-type" style={{ lineHeight: 1.8, marginBottom: 16 }}>
        Bakı daxilində sifarişlər adətən 1-3 iş günü ərzində çatdırılır.
        Ehtiyat hissələri və aksesuarlar üçün çatdırılma haqqı sifarişin
        məbləğinə və çəkisinə görə hesablanır.
      </p>
      <p className="cart-item-type" style={{ lineHeight: 1.8 }}>
        Motosiklet alışlarında çatdırılma və ya mağazadan götürmə seçimləri
        sifariş təsdiqləndikdən sonra əlaqə saxlanılaraq razılaşdırılır.
      </p>
    </div>
  );
}