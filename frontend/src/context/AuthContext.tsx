import { login } from '../services/authService';
import {createContext, type ReactNode, useEffect, useState} from "react";
import type {AuthUser, LoginRequest} from "../types/auth.ts";
import { jwtDecode } from "jwt-decode";
import {mapRoleToNumber} from "../types/employee.ts";

// JWT payload based on backend token structure
interface JwtPayload {
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": string;
    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": string;
    exp: number;
    iss: string;
    aud: string;
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

        // Normalize backend JWT claims to frontend user model
        const authUser: AuthUser = {
            id: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"],
            role: mapRoleToNumber(
                decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]
            ),
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