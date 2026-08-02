export interface ChargingWindow {
  startTime: string;
  endTime: string;
  totalPricePerKWh: number;
}

export interface CarChargingPlan {
  car: {
    id: number;
    name: string;
    batteryCapacityKWh: number;
    maxChargingPowerKW: number;
  };
  window: ChargingWindow | null;
}

export interface CarChargeRequest {
  carId: number;
  currentBatteryPercentage: number;
  targetBatteryPercentage: number;
  departureTime: string | null;
}