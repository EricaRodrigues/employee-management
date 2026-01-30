import AppRoutes from "./routes/AppRoutes";
import {AuthProvider} from "./context/AuthContext.tsx";

export default function App() {
    return (
        <AuthProvider>
            <AppRoutes />
        </AuthProvider>
    );
}