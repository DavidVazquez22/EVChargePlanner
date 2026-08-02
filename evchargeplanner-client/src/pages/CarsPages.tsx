import { useEffect, useState } from 'react';
import type { Car } from '../types/Car';
import { getCars, createCar, updateCar, deleteCar } from '../services/carService';
import CarForm from '../components/CarForm';
import Navbar from '../components/Navbar';
import ConfirmDialog from '../components/ConfirmDialog';

const CarsPage = () => {
  const [cars, setCars] = useState<Car[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [editingCar, setEditingCar] = useState<Car | null>(null);
  const [carToDelete, setCarToDelete] = useState<number | null>(null);

  const loadCars = async () => {
    try {
      const data = await getCars();
      setCars(data);
    } catch {
      setError('Could not load cars');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadCars();
  }, []);

  const handleSave = async (car: Omit<Car, 'id' | 'chargingSessions'>) => {
    if (editingCar) {
      await updateCar(editingCar.id, car);
      setEditingCar(null);
    } else {
      await createCar(car);
    }
    await loadCars();
  };

  const confirmDelete = async () => {
    if (carToDelete === null) return;
    await deleteCar(carToDelete);
    setCarToDelete(null);
    await loadCars();
  };

  if (loading) return <p>Loading...</p>;
  if (error) return <p style={{ color: 'red' }}>{error}</p>;

  return (
    <div>
      <Navbar />
      <h1>Cars</h1>

      <CarForm
        onSubmit={handleSave}
        initialData={editingCar ?? undefined}
        onCancel={editingCar ? () => setEditingCar(null) : undefined}
      />

      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Battery</th>
            <th>Power</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {cars.map((car) => (
            <tr key={car.id}>
              <td>{car.name}</td>
              <td>{car.batteryCapacityKWh} kWh</td>
              <td>{car.maxChargingPowerKW} kW</td>
              <td>
                <div className="action-buttons">
                    <button onClick={() => setEditingCar(car)}>Edit</button>
                    <button onClick={() => setCarToDelete(car.id)}>Delete</button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {carToDelete !== null && (
        <ConfirmDialog
          message="Are you sure you want to delete this car?"
          onConfirm={confirmDelete}
          onCancel={() => setCarToDelete(null)}
        />
      )}
    </div>
  );
};

export default CarsPage;
