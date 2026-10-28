import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import Avatar from './Avatar.jsx';

export default function UserMenu({ user, onLogout }) {
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);
  const close = () => setOpen(false);

  return (
    <div
      className={`user-menu ${open ? 'open' : ''}`}
      onMouseEnter={() => setOpen(true)}
      onMouseLeave={close}
      onBlur={(e) => {
        if (!e.currentTarget.contains(e.relatedTarget)) close();
      }}
    >
      <button
        type="button"
        className="user-menu-btn"
        aria-haspopup="true"
        aria-expanded={open}
        aria-label={user.fullName}
        onClick={() => setOpen((o) => !o)}
      >
        <Avatar name={user.fullName} src={user.avatarUrl} size={36} />
      </button>

      <div className="user-menu-dropdown">
        <div className="user-menu-panel">
          <div className="user-menu-name">{user.fullName}</div>
          <Link to="/profile" className="user-menu-item" onClick={close}>{t('header.profile')}</Link>
          {user.role === 'Admin' && (
            <Link to="/admin" className="user-menu-item" onClick={close}>{t('header.adminPanel')}</Link>
          )}
          <Link to="/seller" className="user-menu-item" onClick={close}>{t('header.myListings')}</Link>
          <Link to="/sell" className="user-menu-item" onClick={close}>{t('header.createListing')}</Link>
          <Link to="/favorites" className="user-menu-item" onClick={close}>{t('header.favorites')}</Link>
          <Link to="/orders" className="user-menu-item" onClick={close}>{t('header.myOrders')}</Link>
          <button
            type="button"
            className="user-menu-item user-menu-logout"
            onClick={() => { close(); onLogout(); }}
          >
            {t('header.logout')}
          </button>
        </div>
      </div>
    </div>
  );
}