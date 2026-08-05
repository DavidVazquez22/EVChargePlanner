import api from './api';
import type { CarModel } from '../types/CarModel';

export const getCarModels = async (): Promise<CarModel[]> => {
  const response = await api.get<CarModel[]>('/car-models');
  return response.data;
};