import { useNavigate } from 'react-router-dom';
import { logout } from '../services/authService';

const Navbar = () => {
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <nav>
      <span>FleetManager</span>
      <button onClick={handleLogout}>Logout</button>
    </nav>
  );
};

export default Navbar;