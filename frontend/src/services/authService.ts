import { type LoginRequest } from '../types/auth';
import apiClient from "../api/apiClient.ts";

interface LoginResponse {
    token: string;
}

export async function login(data: LoginRequest): Promise<LoginResponse> {
    const response = await apiClient.post<LoginResponse>(
        '/auth/login', 
        data
    );
    
    return response.data;
}