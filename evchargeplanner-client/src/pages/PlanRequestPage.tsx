import { useEffect, useState } from 'react';
import type { Car } from '../types/Car';
import type { CarChargingPlan } from '../types/ChargingPlan';
import { getCars } from '../services/carService';
import { requestChargingPlan } from '../services/chargingPlanService';
import Navbar from '../components/Navbar';

interface CarInputState {
  selected: boolean;
  currentBatteryPercentage: string;
  targetBatteryPercentage: string;
  departureTime: string;
}

const PlanRequestPage = () => {
  const [cars, setCars] = useState<Car[]>([]);
  const [inputs, setInputs] = useState<Record<number, CarInputState>>({});
  const [plans, setPlans] = useState<CarChargingPlan[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    const loadCars = async () => {
      try {
        const data = await getCars();
        setCars(data);

        const initialInputs: Record<number, CarInputState> = {};
        data.forEach((car) => {
          initialInputs[car.id] = {
            selected: false,
            currentBatteryPercentage: '',
            targetBatteryPercentage: '',
            departureTime: '',
          };
        });
        setInputs(initialInputs);
      } catch {
        setError('Could not load cars');
      } finally {
        setLoading(false);
      }
    };

    loadCars();
  }, []);

  const updateInput = (carId: number, field: keyof CarInputState, value: string | boolean) => {
    setInputs((prev) => ({
      ...prev,
      [carId]: { ...prev[carId], [field]: value },
    }));
  };

  const handleSubmit = async () => {
    setError('');
    setSubmitting(true);

    const selectedCars = Object.entries(inputs)
      .filter(([, input]) => input.selected)
      .map(([carId, input]) => ({
        carId: Number(carId),
        currentBatteryPercentage: Number(input.currentBatteryPercentage),
        targetBatteryPercentage: Number(input.targetBatteryPercentage),
        departureTime: input.departureTime ? new Date(input.departureTime).toISOString() : null,
      }));

    if (selectedCars.length === 0) {
      setError('Select at least one car');
      setSubmitting(false);
      return;
    }

    try {
      const result = await requestChargingPlan(selectedCars, 'NO1');
      setPlans(result);
    } catch {
      setError('Could not calculate the charging plan. Price data may not be available yet.');
    } finally {
      setSubmitting(false);
    }
  };

  const getDurationHours = (start: string, end: string): number => {
    const diffMs = new Date(end).getTime() - new Date(start).getTime();
    return diffMs / (1000 * 60 * 60);
  };

  if (loading) return <p>Loading...</p>;

  return (
    <div>
      <Navbar />
      <h1>Request Charging Plan</h1>
      {error && <p style={{ color: 'red' }}>{error}</p>}

      {cars.map((car) => {
        const input = inputs[car.id];
        if (!input) return null;

        return (
          <div key={car.id} className="car-input-row">
            <label>
              <input
                type="checkbox"
                checked={input.selected}
                onChange={(e) => updateInput(car.id, 'selected', e.target.checked)}
              />
              {car.name}
            </label>

            {input.selected && (
              <div>
                <label>Current %</label>
                <input
                  type="number"
                  min="0"
                  max="100"
                  value={input.currentBatteryPercentage}
                  onChange={(e) => updateInput(car.id, 'currentBatteryPercentage', e.target.value)}
                />

                <label>Target %</label>
                <input
                  type="number"
                  min="0"
                  max="100"
                  value={input.targetBatteryPercentage}
                  onChange={(e) => updateInput(car.id, 'targetBatteryPercentage', e.target.value)}
                />

                <label>Departure (optional)</label>
                <input
                  type="datetime-local"
                  value={input.departureTime}
                  onChange={(e) => updateInput(car.id, 'departureTime', e.target.value)}
                />
              </div>
            )}
          </div>
        );
      })}

      <button onClick={handleSubmit} disabled={submitting}>
        {submitting ? 'Calculating...' : 'Calculate Plan'}
      </button>

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
              Estimated cost: {(plan.car.maxChargingPowerKW * plan.window.totalPricePerKWh).toFixed(2)} NOK
            </p>
          ) : (
            <p>No available slot found before the deadline.</p>
          )}
        </div>
      ))}
    </div>
  );
};

export default PlanRequestPage;