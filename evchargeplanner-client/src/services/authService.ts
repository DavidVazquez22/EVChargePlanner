import api from './api';

interface LoginResponse {
  token: string;
}

export const login = async (email: string, password: string): Promise<string> => {
  const response = await api.post<LoginResponse>('/auth/login', { email, password });
  localStorage.setItem('token', response.data.token);
  return response.data.token;
};

export const logout = () => {
  localStorage.removeItem('token');
};

export const isAuthenticated = (): boolean => {
  return localStorage.getItem('token') !== null;
};

export const getUserRole = (): string | null => {
  const token = localStorage.getItem('token');
  if (!token) return null;

  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? null;
  } catch {
    return null;
  }
};

export const register = async (email: string, password: string): Promise<void> => {
  await api.post('/auth/register', { email, password });
};