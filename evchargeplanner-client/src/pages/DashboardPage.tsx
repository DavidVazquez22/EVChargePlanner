import { useEffect, useState } from 'react';
import type { CarChargingPlan } from '../types/ChargingPlan';
import type { PriceRecord } from '../types/PriceRecord';
import type { Car } from '../types/Car';
import { getChargingPlan } from '../services/chargingPlanService';
import { getTodayPrices } from '../services/priceService';
import Navbar from '../components/Navbar';
import PriceChart from '../components/PriceChart';

const getDurationHours = (start: string, end: string): number => {
  const diffMs = new Date(end).getTime() - new Date(start).getTime();
  return diffMs / (1000 * 60 * 60);
};

const getEstimatedCost = (car: Car, totalPricePerKWh: number): number => {
  return car.maxChargingPowerKW * totalPricePerKWh;
};

const DashboardPage = () => {
  const [plans, setPlans] = useState<CarChargingPlan[]>([]);
  const [prices, setPrices] = useState<PriceRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const loadData = async () => {
      try {
        const [planData, priceData] = await Promise.all([
          getChargingPlan('NO1'),
          getTodayPrices('NO1'),
        ]);
        setPlans(planData);
        setPrices(priceData);
      } catch {
        setError('Could not load data. The price data may not be available yet.');
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
      {prices.length > 0 && <PriceChart prices={prices} />}

      <h2>Charging Plan</h2>
      {error && <p style={{ color: 'red' }}>{error}</p>}

      {plans.map((plan) => (
        <div key={plan.car.id}>
          <h3>{plan.car.name}</h3>
          {plan.window ? (
            <p>
              Best window: {new Date(plan.window.startTime).toLocaleTimeString()} –{' '}
              {new Date(plan.window.endTime).toLocaleTimeString()}
              <br />
              Duration: {getDurationHours(plan.window.startTime, plan.window.endTime)} hours
              <br />
              Estimated cost: {getEstimatedCost(plan.car, plan.window.totalPricePerKWh).toFixed(2)}{' '}
              {prices[0]?.currency ?? 'NOK'}
            </p>
          ) : (
            <p>No available slot found before the deadline.</p>
          )}
        </div>
      ))}
    </div>
  );
};

export default DashboardPage;