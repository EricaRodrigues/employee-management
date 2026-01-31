import { useContext } from 'react';
import {AuthContext} from "./AuthContext.tsx";

// Custom hook to access authentication context
export function useAuth() {
    return useContext(AuthContext);
}