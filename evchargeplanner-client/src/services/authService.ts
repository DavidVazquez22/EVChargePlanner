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