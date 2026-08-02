export interface PriceRecord {
  id: number;
  timeStart: string;
  timeEnd: string;
  pricePerKWh: number;
  currency: string;
  priceZone: string;
}