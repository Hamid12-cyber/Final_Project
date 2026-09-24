import Header from '../components/Header.jsx';
import Hero from '../components/Hero.jsx';
import Features from '../components/Features.jsx';
import Categories from '../components/Categories.jsx';
import PopularBikes from '../components/PopularBikes.jsx';
import PartsCategories from '../components/PartsCategories.jsx';
import RentalBanner from '../components/RentalBanner.jsx';
import ServiceBanner from '../components/ServiceBanner.jsx';
import Testimonials from '../components/Testimonials.jsx';
import Footer from '../components/Footer.jsx';
<h1>Motorcycles Page</h1>

export default function HomePage() {
  return (
    <>
      <Header />
      <Hero />
      <Features />
      <Categories />
      <PopularBikes />
      <PartsCategories />
      <RentalBanner />
      <ServiceBanner />
      <Testimonials />
      <Footer />
    </>
  );
}