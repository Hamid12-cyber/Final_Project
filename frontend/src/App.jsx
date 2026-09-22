import { Routes, Route } from 'react-router-dom';
import HomePage from './pages/HomePage.jsx';
import MotorcyclesPage from './pages/MotorcyclesPage.jsx';
import LoginPage from './pages/LoginPage.jsx';
import CartPage from './pages/CartPage.jsx';

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/motorcycles" element={<MotorcyclesPage />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/cart" element={<CartPage />} />
    </Routes>
  );
}