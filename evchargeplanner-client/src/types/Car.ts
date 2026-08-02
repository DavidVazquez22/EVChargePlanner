export interface Car {
  id: number;
  name: string;
  batteryCapacityKWh: number;
  maxChargingPowerKW: number;
  chargingSessions: unknown[];
}