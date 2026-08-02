import type { Car } from "./Car";

export interface ChargingWindow {
  startTime: string;
  endTime: string;
  totalPricePerKWh: number;
}

export interface CarChargingPlan {
  car: Car;
  window: ChargingWindow | null;
}