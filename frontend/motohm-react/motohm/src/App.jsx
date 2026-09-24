import { Routes, Route } from 'react-router-dom';
import HomePage from './pages/HomePage.jsx';
import MotorcyclesPage from './pages/MotorcyclesPage.jsx';
import LoginPage from './pages/LoginPage.jsx';
import RegisterPage from './pages/RegisterPage.jsx';
import CartPage from './pages/CartPage.jsx';
import PartsPage from './pages/PartsPage.jsx';
import RentalsPage from './pages/RentalsPage.jsx';
import ServicePage from './pages/ServicePage.jsx';
import AccessoriesPage from './pages/AccessoriesPage.jsx';

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/motorcycles" element={<MotorcyclesPage />} />
      <Route path="/parts" element={<PartsPage />} />
      <Route path="/rentals" element={<RentalsPage />} />
      <Route path="/service" element={<ServicePage />} />
      <Route path="/accessories" element={<AccessoriesPage />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/cart" element={<CartPage />} />
    </Routes>
  );
}