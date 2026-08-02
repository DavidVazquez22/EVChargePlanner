import api from './api';
import type { Car } from '../types/Car';

export const getCars = async (): Promise<Car[]> => {
  const response = await api.get<Car[]>('/cars');
  return response.data;
};

export const createCar = async (car: Omit<Car, 'id' | 'chargingSessions'>): Promise<Car> => {
  const response = await api.post<Car>('/cars', car);
  return response.data;
};

export const updateCar = async (id: number, car: Omit<Car, 'id' | 'chargingSessions'>): Promise<void> => {
  await api.put(`/cars/${id}`, { id, ...car });
};

export const deleteCar = async (id: number): Promise<void> => {
  await api.delete(`/cars/${id}`);
};