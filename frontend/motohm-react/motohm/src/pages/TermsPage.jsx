export default function TermsPage() {
  return (
    <div className="page-wrap" style={{ maxWidth: 720 }}>
      <h1 className="page-title">İstifadə şərtləri</h1>
      <p className="cart-item-type" style={{ lineHeight: 1.8, marginBottom: 16 }}>
        MotoHM platformasından istifadə edərək aşağıdakı şərtləri qəbul etmiş olursunuz:
        saytda yerləşdirilən elanların doğruluğuna görə satıcılar məsuliyyət daşıyır,
        bütün elanlar admin təsdiqindən keçir.
      </p>
      <p className="cart-item-type" style={{ lineHeight: 1.8 }}>
        Ödəniş və çatdırılma şərtləri sifariş zamanı göstərilən qaydada həyata keçirilir.
        Platforma qaydaları dəyişdirmə hüququnu özündə saxlayır.
      </p>
    </div>
  );
}