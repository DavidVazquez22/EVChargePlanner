import api from './api';
import type { PriceRecord } from '../types/PriceRecord';

export const getTodayPrices = async (zone: string = 'NO1'): Promise<PriceRecord[]> => {
  const response = await api.get<PriceRecord[]>('/prices/today', {
    params: { zone },
  });
  return response.data;
};