import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext.jsx';
import { CartProvider } from './context/CartContext.jsx';
import { FavoritesProvider } from './context/FavoritesContext.jsx';
import App from './App.jsx';
import { installArtwork } from './artwork.js';
import './i18n/index.js';
import './index.css';

// Hero və kateqoriya illüstrasiyalarını CSS dəyişənlərinə yazır (şəkil faylı lazım deyil)
installArtwork();

// Yüklənməyən şəkil (sınıq link): sınıq-ikon və alt mətn əvəzinə placeholder göstər
document.addEventListener(
  'error',
  (e) => {
    const el = e.target;
    if (el instanceof HTMLImageElement) {
      el.closest('.product-image, .slider-card-image, .bike-image, .detail-image')?.classList.add('img-broken');
    }
  },
  true
);

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <BrowserRouter>
      <AuthProvider>
        <CartProvider>
          <FavoritesProvider>
            <App />
          </FavoritesProvider>
        </CartProvider>
      </AuthProvider>
    </BrowserRouter>
  </React.StrictMode>
);