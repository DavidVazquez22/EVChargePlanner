import { useEffect, useState } from 'react';
import type { PriceRecord } from '../types/PriceRecord';
import { getTodayPrices } from '../services/priceService';
import Navbar from '../components/Navbar';
import PriceChart from '../components/PriceChart';

const DashboardPage = () => {
  const [prices, setPrices] = useState<PriceRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const loadData = async () => {
      try {
        const priceData = await getTodayPrices('NO1');
        setPrices(priceData);
      } catch {
        setError('Could not load price data. It may not be available yet.');
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, []);

  if (loading) return <p>Loading...</p>;

  return (
    <div>
      <Navbar />
      <h1>Today's Electricity Prices</h1>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      {prices.length > 0 && <PriceChart prices={prices} />}
    </div>
  );
};

export default DashboardPage;