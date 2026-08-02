import { Link, useNavigate } from 'react-router-dom';
import { logout } from '../services/authService';

const LightningIcon = () => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path
      d="M13 2L4.5 13.5H11L10 22L19.5 10H13L13 2Z"
      fill="#22c55e"
    />
  </svg>
);

const Navbar = () => {
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
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
      </div>
      <button onClick={handleLogout}>Logout</button>
    </nav>
  );
};

export default Navbar;