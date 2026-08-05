import api from './api';
import type { Charger } from '../types/Charger';

export const getChargers = async (): Promise<Charger[]> => {
  const response = await api.get<Charger[]>('/chargers');
  return response.data;
};

export const createCharger = async (charger: Omit<Charger, 'id'>): Promise<Charger> => {
  const response = await api.post<Charger>('/chargers', charger);
  return response.data;
};

export const deleteCharger = async (id: number): Promise<void> => {
  await api.delete(`/chargers/${id}`);
};