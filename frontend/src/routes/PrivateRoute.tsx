import { Navigate } from 'react-router-dom';
import type {ReactNode} from "react";
import {useAuth} from "../context/useAuth.ts";

export function PrivateRoute({ children }: { children: ReactNode }) {
    const { isAuthenticated, loadingAuth } = useAuth();

    // Wait until auth state is restored (page refresh)
    if (loadingAuth) return null;

    // Redirect unauthenticated users to login
    if (!isAuthenticated) {
        return <Navigate to="/login" replace />;
    }

    // Render protected content
    return children;
}