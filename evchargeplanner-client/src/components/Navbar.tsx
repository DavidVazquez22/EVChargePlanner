import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { getUserRole, logout } from '../services/authService';
import { getSelectedZone, setSelectedZone } from '../services/zoneService';

const LightningIcon = () => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path d="M13 2L4.5 13.5H11L10 22L19.5 10H13L13 2Z" fill="#22c55e" />
  </svg>
);

const Navbar = () => {
  const navigate = useNavigate();
  const isAdmin = getUserRole() === 'Admin';
  const [zone, setZone] = useState(getSelectedZone());

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const handleZoneChange = (newZone: string) => {
    setSelectedZone(newZone);
    setZone(newZone);
    window.location.reload();
  };

  return (
    <nav>
      <div className="nav-links">
        <span style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
          <LightningIcon /> EV Charge Planner
        </span>
        <Link to="/dashboard">Dashboard</Link>
        <Link to="/cars">Cars</Link>
        <Link to="/planRequest">Planner</Link>
        {isAdmin && <Link to="/chargers">Chargers</Link>}
      </div>
      <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
        <select value={zone} onChange={(e) => handleZoneChange(e.target.value)}>
          <option value="NO1">🇳🇴 Norway</option>
          <option value="ES">🇪🇸 Spain</option>
        </select>
        <button onClick={handleLogout}>Logout</button>
      </div>
    </nav>
  );
};

export default Navbar;