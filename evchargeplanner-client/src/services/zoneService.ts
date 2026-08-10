export const getSelectedZone = (): string => {
  return localStorage.getItem('selectedZone') || 'NO1';
};

export const setSelectedZone = (zone: string) => {
  localStorage.setItem('selectedZone', zone);
};