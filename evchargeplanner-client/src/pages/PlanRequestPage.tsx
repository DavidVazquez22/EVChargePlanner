import { useEffect, useState } from 'react';
import type { Car } from '../types/Car';
import type { CarChargingPlan } from '../types/ChargingPlan';
import { getCars } from '../services/carService';
import { confirmChargingPlan, requestChargingPlan } from '../services/chargingPlanService';
import Navbar from '../components/Navbar';
import axios from 'axios';
import { getPriceAvailability } from '../services/priceService';

interface CarInputState {
  selected: boolean;
  currentBatteryPercentage: string;
  targetBatteryPercentage: string;
  arrivalTime: string;
  departureTime: string;
}

const PlanRequestPage = () => {
  const [cars, setCars] = useState<Car[]>([]);
  const [inputs, setInputs] = useState<Record<number, CarInputState>>({});
  const [plans, setPlans] = useState<CarChargingPlan[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [confirming, setConfirming] = useState(false);
  const [confirmMessage, setConfirmMessage] = useState('');
  const [priceAvailableUntil, setPriceAvailableUntil] = useState<Date | null>(null);

  useEffect(() => {
    getPriceAvailability('NO1').then(setPriceAvailableUntil);
    const interval = setInterval(() => {
        getPriceAvailability('NO1').then(setPriceAvailableUntil);
    }, 5 * 60 * 1000);

    return () => clearInterval(interval);
    }, []);
  
  useEffect(() => {
    getPriceAvailability('NO1').then(setPriceAvailableUntil);
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
            arrivalTime: '',
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
        arrivalTime: input.arrivalTime ? new Date(input.arrivalTime).toISOString() : null,
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
    } catch (err) {
    if (axios.isAxiosError(err) && typeof err.response?.data === 'string') {
        setError(err.response.data);
    } else {
        setError('Could not calculate the charging plan. Price data may not be available yet.');
    }
    } finally {
    setSubmitting(false);
    }
  };

  const formatDuration = (start: string, end: string): string => {
    const diffMs = new Date(end).getTime() - new Date(start).getTime();
    const totalMinutes = Math.round(diffMs / (1000 * 60));
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;

    if (hours === 0) return `${minutes}m`;
    if (minutes === 0) return `${hours}h`;
    return `${hours}h ${minutes}m`;
 };


    const handleConfirm = async () => {
    setConfirming(true);
    setConfirmMessage('');

    const sessions = plans
        .filter((plan) => plan.window !== null)
        .map((plan) => ({
        carId: plan.car.id,
        chargerId: plan.window!.chargerId,
        startTime: plan.window!.startTime,
        endTime: plan.window!.endTime,
        estimatedCost: plan.car.maxChargingPowerKW * plan.window!.totalPricePerKWh,
        }));

    try {
        await confirmChargingPlan(sessions);
        setConfirmMessage('Plan confirmed and reserved!');
    } catch {
        setConfirmMessage('Could not confirm the plan.');
    } finally {
        setConfirming(false);
    }
    };

  if (loading) return <p>Loading...</p>;

  return (
  <div>
    <Navbar />
    <h1>Request Charging Plan</h1>
        {priceAvailableUntil && (
            <p style={{ textAlign: 'center', color: '#94a3b8' }}>
                Price data available until {priceAvailableUntil.toLocaleString()}
            </p>
            )}
    {error && <p style={{ color: 'red', textAlign: 'center' }}>{error}</p>}

    <div className="plan-container">
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
              <div className="car-details">
                <div className="car-details-row">
                    <div>
                    <label>Current</label>
                    <input
                        type="number"
                        min="0"
                        max="100"
                        value={input.currentBatteryPercentage}
                        onChange={(e) => updateInput(car.id, 'currentBatteryPercentage', e.target.value)}
                    />
                    </div>

                    <div>
                    <label>Target</label>
                    <input
                        type="number"
                        min="0"
                        max="100"
                        value={input.targetBatteryPercentage}
                        onChange={(e) => updateInput(car.id, 'targetBatteryPercentage', e.target.value)}
                    />
                    </div>
                </div>

                <div className="car-details-row">
                    <div>
                    <label>Arrival</label>
                    <input
                        type="datetime-local"
                        value={input.arrivalTime}
                        onChange={(e) => updateInput(car.id, 'arrivalTime', e.target.value)}
                    />
                    </div>

                    <div>
                    <label>Departure</label>
                    <input
                        type="datetime-local"
                        value={input.departureTime}
                        onChange={(e) => updateInput(car.id, 'departureTime', e.target.value)}
                    />
                    </div>
                </div>
                </div>
            )}
          </div>
        );
      })}

      <div className="calculate-button-wrapper">
        <button className="calculate-button" onClick={handleSubmit} disabled={submitting}>
          {submitting ? 'Calculating...' : 'Calculate Plan'}
        </button>
      </div>
    </div>

    <div className="plan-results">
      {plans.map((plan) => (
        
        <div key={plan.car.id}>
          <h3>{plan.car.name}</h3>
          {plan.window ? (
            <p>
              {plan.window.isPartialCharge && (
                <span style={{ color: 'orange' }}>
                  ⚠ {plan.window.limitedByDataEnd
                    ? `Only enough price data available to reach ${plan.window.achievedBatteryPercentage}% before the plan ends`
                    : `Not enough time for a full charge — will reach ${plan.window.achievedBatteryPercentage}%`}
                  <br />
                </span>
              )}
              Best window: {new Date(plan.window.startTime).toLocaleTimeString()} –{' '}
              {new Date(plan.window.endTime).toLocaleTimeString()}
              <br />
              Duration: {formatDuration(plan.window.startTime, plan.window.endTime)}
              <br />
              Estimated cost: {(plan.car.maxChargingPowerKW * plan.window.totalPricePerKWh).toFixed(2)} NOK
              <br />
              Charger: {plan.window.chargerName}
              <br />  
            </p>
          ) : (
            <p>No charging is possible before the deadline.</p>
          )}
        </div>
      ))}
      {plans.length > 0 && (
        <div className="calculate-button-wrapper">
            <button className="calculate-button" onClick={handleConfirm} disabled={confirming}>
            {confirming ? 'Confirming...' : 'Confirm this plan'}
            </button>
        </div>
        )}

        {confirmMessage && <p style={{ textAlign: 'center', color: '#4ade80' }}>{confirmMessage}</p>}
    </div>
  </div>
)};

export default PlanRequestPage;