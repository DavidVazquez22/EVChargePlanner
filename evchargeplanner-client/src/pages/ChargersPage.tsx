import { useEffect, useState } from 'react';
import type { Charger } from '../types/Charger';
import { getChargers, createCharger, deleteCharger } from '../services/chargerService';
import ChargerForm from '../components/ChargerForm';
import Navbar from '../components/Navbar';
import ConfirmDialog from '../components/ConfirmDialog';

const ChargersPage = () => {
  const [chargers, setChargers] = useState<Charger[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [chargerToDelete, setChargerToDelete] = useState<number | null>(null);

  const loadChargers = async () => {
    try {
      const data = await getChargers();
      setChargers(data);
    } catch {
      setError('Could not load chargers');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadChargers();
  }, []);

  const handleCreate = async (charger: Omit<Charger, 'id'>) => {
    await createCharger(charger);
    await loadChargers();
  };

  const confirmDelete = async () => {
    if (chargerToDelete === null) return;
    await deleteCharger(chargerToDelete);
    setChargerToDelete(null);
    await loadChargers();
  };

  if (loading) return <p>Loading...</p>;
  if (error) return <p style={{ color: 'red' }}>{error}</p>;

  return (
    <div>
      <Navbar />
      <h1>Chargers</h1>

      <ChargerForm onSubmit={handleCreate} />

      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Max Power</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {chargers.map((charger) => (
            <tr key={charger.id}>
              <td>{charger.name}</td>
              <td>{charger.maxPowerKW} kW</td>
              <td>
                <div className="action-buttons">
                  <button onClick={() => setChargerToDelete(charger.id)}>Delete</button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {chargerToDelete !== null && (
        <ConfirmDialog
          message="Are you sure you want to delete this charger?"
          onConfirm={confirmDelete}
          onCancel={() => setChargerToDelete(null)}
        />
      )}
    </div>
  );
};

export default ChargersPage;