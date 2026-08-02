import { useEffect, useState } from 'react';
import axios from 'axios';
import type { Car } from '../types/Car';

interface CarFormProps {
  onSubmit: (car: Omit<Car, 'id' | 'chargingSessions'>) => Promise<void>;
  initialData?: Car;
  onCancel?: () => void;
}

const CarForm = ({ onSubmit, initialData, onCancel }: CarFormProps) => {
  const [name, setName] = useState('');
  const [batteryCapacityKWh, setBatteryCapacityKWh] = useState('');
  const [maxChargingPowerKW, setMaxChargingPowerKW] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (initialData) {
      setName(initialData.name);
      setBatteryCapacityKWh(String(initialData.batteryCapacityKWh));
      setMaxChargingPowerKW(String(initialData.maxChargingPowerKW));
    } else {
      setName('');
      setBatteryCapacityKWh('');
      setMaxChargingPowerKW('');
    }
  }, [initialData]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess(false);
    setSubmitting(true);

    try {
      await onSubmit({
        name,
        batteryCapacityKWh: Number(batteryCapacityKWh),
        maxChargingPowerKW: Number(maxChargingPowerKW),
      });

      setSuccess(true);
      setTimeout(() => setSuccess(false), 3000);

      if (!initialData) {
        setName('');
        setBatteryCapacityKWh('');
        setMaxChargingPowerKW('');
      }
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data?.errors) {
        const messages = Object.values(err.response.data.errors).flat();
        setError(messages.join(' '));
      } else {
        setError('Could not save the car');
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <div className="form-message">
        {error && <p style={{ color: 'red', margin: 0 }}>{error}</p>}
        {success && <p style={{ color: 'green', margin: 0 }}>Car saved successfully!</p>}
      </div>

      <div>
        <label>Name</label>
        <input value={name} onChange={(e) => setName(e.target.value)} required />
      </div>

      <div>
        <label>Battery Capacity (kWh)</label>
        <input type="number" step="0.1" value={batteryCapacityKWh} onChange={(e) => setBatteryCapacityKWh(e.target.value)} required />
      </div>

      <div>
        <label>Max Charging Power (kW)</label>
        <input type="number" step="0.1" value={maxChargingPowerKW} onChange={(e) => setMaxChargingPowerKW(e.target.value)} required />
      </div>

      <button type="submit" disabled={submitting}>
        {submitting ? 'Saving...' : initialData ? 'Update Car' : 'Add Car'}
      </button>

      {onCancel && (
        <button type="button" className="btn-secondary" onClick={onCancel}>
          Cancel
        </button>
      )}
    </form>
  );
};

export default CarForm;