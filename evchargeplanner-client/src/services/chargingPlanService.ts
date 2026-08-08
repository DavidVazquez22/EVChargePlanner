import api from './api';
import type { CarChargingPlan, CarChargeRequest, ConfirmSessionRequest } from '../types/ChargingPlan';

export const requestChargingPlan = async (
  cars: CarChargeRequest[],
  zone: string = 'NO1'
): Promise<CarChargingPlan[]> => {
  const response = await api.post<CarChargingPlan[]>('/charging-plan', { cars, zone });
  return response.data;
};

export const confirmChargingPlan = async (sessions: ConfirmSessionRequest[]): Promise<void> => {
  await api.post('/charging-plan/confirm', { sessions });
};

export interface TodaySession {
  id: number;
  carName: string;
  chargerName: string;
  startTime: string;
  endTime: string;
}

export const getTodaySessions = async (): Promise<TodaySession[]> => {
  const response = await api.get<TodaySession[]>('/charging-plan/today');
  return response.data;
};

export const deleteSession = async (id: number): Promise<void> => {
  await api.delete(`/charging-plan/sessions/${id}`);
};