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
import OrdersPage from './pages/OrdersPage.jsx';
import OrderDetailPage from './pages/OrderDetailPage.jsx';
import AdminPage from './pages/AdminPage.jsx';
import MotorcycleDetailPage from './pages/MotorcycleDetailPage.jsx';
import CreateListingPage from './pages/CreateListingPage.jsx';

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/motorcycles" element={<MotorcyclesPage />} />
      <Route path="/motorcycles/:id" element={<MotorcycleDetailPage />} />
      <Route path="/parts" element={<PartsPage />} />
      <Route path="/rentals" element={<RentalsPage />} />
      <Route path="/service" element={<ServicePage />} />
      <Route path="/accessories" element={<AccessoriesPage />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/cart" element={<CartPage />} />
      <Route path="/orders" element={<OrdersPage />} />
      <Route path="/orders/:id" element={<OrderDetailPage />} />
      <Route path="/admin" element={<AdminPage />} />
      <Route path="/sell" element={<CreateListingPage />} />
    </Routes>
  );
}