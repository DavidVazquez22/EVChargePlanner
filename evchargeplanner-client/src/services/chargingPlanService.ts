import api from './api';
import type { CarChargingPlan, CarChargeRequest } from '../types/ChargingPlan';

export const requestChargingPlan = async (
  cars: CarChargeRequest[],
  zone: string = 'NO1'
): Promise<CarChargingPlan[]> => {
  const response = await api.post<CarChargingPlan[]>('/charging-plan', { cars, zone });
  return response.data;
};