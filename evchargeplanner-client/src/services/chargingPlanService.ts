import api from './api';
import type { CarChargingPlan } from '../types/ChargingPlan';

export const getChargingPlan = async (zone: string = 'NO1'): Promise<CarChargingPlan[]> => {
  const response = await api.get<CarChargingPlan[]>('/charging-plan', {
    params: { zone },
  });
  return response.data;
};