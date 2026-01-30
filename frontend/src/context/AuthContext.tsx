import { login } from '../services/authService';
import {createContext, type ReactNode, useEffect, useState} from "react";
import type {AuthUser, LoginRequest} from "../types/auth.ts";
import { jwtDecode } from "jwt-decode";

// JWT payload based on backend token structure
interface JwtPayload {
    nameId: string;
    role: string;
    expiration: number;
}

// Public contract exposed by the AuthContext
interface AuthContextData {
    token: string | null;
    user: AuthUser | null;
    isAuthenticated: boolean;
    signIn(data: LoginRequest): Promise<void>;
    signOut(): void;
    loadingAuth: boolean
}

export const AuthContext = createContext<AuthContextData>({} as AuthContextData);

export function AuthProvider({ children }: { children: ReactNode }) {
    const [token, setToken] = useState<string | null>(null);
    const [user, setUser] = useState<AuthUser | null>(null);
    const [loadingAuth, setLoadingAuth] = useState(true);
    
    const isAuthenticated = !!token; //// Simple auth flag used by protected routes

    async function signIn({ email, password }: LoginRequest) {
        const { token } = await login({ email, password });

        const decoded = jwtDecode<JwtPayload>(token);

        const authUser: AuthUser = {
            id: decoded.nameId,
            role: Number(decoded.role),
        };

        localStorage.setItem('@employee:token', token);
        localStorage.setItem('@employee:user', JSON.stringify(authUser));

        setToken(token);
        setUser(authUser);
    }

    function signOut() {
        localStorage.removeItem('@employee:token');
        localStorage.removeItem('@employee:user');

        setToken(null);
        setUser(null);
    }

    // Restore auth on refresh
    useEffect(() => {
        const storedToken = localStorage.getItem('@employee:token');
        const storedUser = localStorage.getItem('@employee:user');

        if (storedToken && storedUser) {
            setToken(storedToken);
            setUser(JSON.parse(storedUser));
        }

        setLoadingAuth(false);
    }, []);

    return (
        <AuthContext.Provider
            value={{
                token,
                user,
                isAuthenticated,
                signIn,
                signOut,
                loadingAuth
            }}
        >
            {children}
        </AuthContext.Provider>
    );
}