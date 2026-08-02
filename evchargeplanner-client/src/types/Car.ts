export interface Car {
  id: number;
  name: string;
  batteryCapacityKWh: number;
  maxChargingPowerKW: number;
  currentBatteryPercentage: number;
  targetBatteryPercentage: number;
  departureTime: string | null;
}