import { useState } from 'react';
import type { Charger } from '../types/Charger';

interface ChargerFormProps {
  onSubmit: (charger: Omit<Charger, 'id'>) => Promise<void>;
}

const ChargerForm = ({ onSubmit }: ChargerFormProps) => {
  const [name, setName] = useState('');
  const [maxPowerKW, setMaxPowerKW] = useState('');
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSubmitting(true);

    try {
      await onSubmit({ name, maxPowerKW: Number(maxPowerKW) });
      setName('');
      setMaxPowerKW('');
    } catch {
      setError('Could not save the charger');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <div className="form-message">
        {error && <p style={{ color: 'red', margin: 0 }}>{error}</p>}
      </div>

      <div>
        <label>Name</label>
        <input value={name} onChange={(e) => setName(e.target.value)} required />
      </div>

      <div>
        <label>Max Power (kW)</label>
        <input type="number" step="0.1" value={maxPowerKW} onChange={(e) => setMaxPowerKW(e.target.value)} required />
      </div>

      <button type="submit" disabled={submitting}>
        {submitting ? 'Saving...' : 'Add Charger'}
      </button>
    </form>
  );
};

export default ChargerForm;