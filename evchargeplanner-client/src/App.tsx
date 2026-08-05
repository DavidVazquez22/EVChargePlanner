import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import { isAuthenticated } from './services/authService';
import CarsPage from './pages/CarsPages';
import PlanRequestPage from './pages/PlanRequestPage';
import ChargersPage from './pages/ChargersPage';

const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
  return isAuthenticated() ? children : <Navigate to="/login" />;
};

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route
          path="/dashboard"
          element={
            <ProtectedRoute>
              <DashboardPage />
            </ProtectedRoute>
          }
        />
        <Route path="/" element={<Navigate to="/dashboard" />} />
        <Route
          path="/cars"
          element={
            <ProtectedRoute>
              <CarsPage />
    </ProtectedRoute>
          }
        />
        <Route
          path="/planRequest"
          element={
            <ProtectedRoute>
              <PlanRequestPage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/chargers"
          element={
            <ProtectedRoute>
              <ChargersPage />
            </ProtectedRoute>
          }
        />

      </Routes>
    </BrowserRouter>
  );
}

export default App;