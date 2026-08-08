export interface ChargingWindow {
  startTime: string;
  endTime: string;
  totalPricePerKWh: number;
  achievedBatteryPercentage: number;
  isPartialCharge: boolean;
  chargerId: number;
  chargerName: string;
  limitedByDataEnd: boolean;

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
  arrivalTime: string | null;
  departureTime: string | null;
}

export interface ConfirmSessionRequest {
  carId: number;
  chargerId: number;
  startTime: string;
  endTime: string;
  estimatedCost: number;
}