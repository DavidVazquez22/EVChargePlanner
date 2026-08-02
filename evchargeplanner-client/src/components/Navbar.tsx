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
      <Link to="/dashboard">Dashboard</Link>
      <Link to="/cars">Cars</Link>
      <span>EVChagerPlanner</span>
      <button onClick={handleLogout}>Logout</button>
    </nav>
  );
};

export default Navbar;