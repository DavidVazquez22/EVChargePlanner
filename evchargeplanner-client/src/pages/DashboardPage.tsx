import { useEffect, useState } from 'react';
import type { PriceRecord } from '../types/PriceRecord';
import { getUpcomingPrices } from '../services/priceService';
import { deleteSession, getTodaySessions, type TodaySession } from '../services/chargingPlanService';
import Navbar from '../components/Navbar';
import PriceChart from '../components/PriceChart';
import ConfirmDialog from '../components/ConfirmDialog';

const DashboardPage = () => {
  const [prices, setPrices] = useState<PriceRecord[]>([]);
  const [sessions, setSessions] = useState<TodaySession[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [sessionToDelete, setSessionToDelete] = useState<number | null>(null);

  useEffect(() => {
    const loadData = async () => {
      try {
        const [priceData, sessionData] = await Promise.all([
          getUpcomingPrices('NO1'),
          getTodaySessions(),
        ]);
        setPrices(priceData);
        setSessions(sessionData);
      } catch {
        setError('Could not load price data. It may not be available yet.');
      } finally {
        setLoading(false);
      }
    };

    loadData();
    const interval = setInterval(loadData, 5 * 60 * 1000);
    return () => clearInterval(interval);
  }, []);

  const confirmDeleteSession = async () => {
    if (sessionToDelete === null) return;
    await deleteSession(sessionToDelete);
    const updated = await getTodaySessions();
    setSessions(updated);
    setSessionToDelete(null);
  };

  if (loading) return <p>Loading...</p>;

  return (
    <div>
      <Navbar />
      <h1>Electricity Prices</h1>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      {prices.length > 0 && <PriceChart prices={prices} />}

      <h2>Reserved Charging Sessions</h2>
      {sessions.length === 0 ? (
        <p style={{ textAlign: 'center', color: '#94a3b8' }}>No sessions reserved yet today.</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Car</th>
              <th>Charger</th>
              <th>Start</th>
              <th>End</th>
              <th>Price</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {sessions
              .sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime())
              .map((s) => (
                <tr key={s.id}>
                  <td>{s.carName}</td>
                  <td>{s.chargerName}</td>
                  <td>{new Date(s.startTime).toLocaleTimeString()}</td>
                  <td>{new Date(s.endTime).toLocaleTimeString()}</td>
                  <td>{s.estimatedCost.toFixed(2)} NOK</td>
                  <td>
                    <button onClick={() => setSessionToDelete(s.id)}>Delete</button>
                  </td>
                </tr>
              ))}
          </tbody>
        </table>
      )}

      {sessionToDelete !== null && (
        <ConfirmDialog
          message="Are you sure you want to delete this charging session?"
          onConfirm={confirmDeleteSession}
          onCancel={() => setSessionToDelete(null)}
        />
      )}
    </div>
  );
};

export default DashboardPage;