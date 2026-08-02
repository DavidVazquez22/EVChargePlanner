import { Link, useNavigate } from 'react-router-dom';
import { logout } from '../services/authService';

const Navbar = () => {
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <nav>
      <div className="nav-links">
        <span>EVChargePlanner</span>
        <Link to="/dashboard">Dashboard</Link>
        <Link to="/cars">Cars</Link>
        <Link to="/planRequest">PlanRequest</Link>
      </div>
      <button onClick={handleLogout}>Logout</button>
    </nav>
  );
};

export default Navbar;