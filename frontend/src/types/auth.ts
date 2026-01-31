export interface LoginRequest {
    email: string;
    password: string;
}

export interface AuthUser {
    id: string;
    role: number;
}