import api from './api';
import type { PriceRecord } from '../types/PriceRecord';

export const getTodayPrices = async (zone: string = 'NO1'): Promise<PriceRecord[]> => {
  const response = await api.get<PriceRecord[]>('/prices/today', {
    params: { zone },
  });
  return response.data;
};

export const getPriceAvailability = async (zone: string = 'NO1'): Promise<Date | null> => {
  try {
    const response = await api.get<{ latestAvailable: string }>('/prices/availability', {
      params: { zone },
    });
    return new Date(response.data.latestAvailable);
  } catch {
    return null;
  }
};

export const getUpcomingPrices = async (zone: string = 'NO1'): Promise<PriceRecord[]> => {
  const response = await api.get<PriceRecord[]>('/prices/upcoming', {
    params: { zone },
  });
  return response.data;
};