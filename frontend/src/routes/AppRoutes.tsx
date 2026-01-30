import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import Login from "../pages/Login";
import Employees from "../pages/Employees";
import {PrivateRoute} from "./PrivateRoute.tsx";
import EmployeeForm from "../pages/EmployeeForm.tsx";

export default function AppRoutes() {
    return (
        <BrowserRouter>
            <Routes>
                {/* Public route */}
                <Route path="/login" element={<Login />} />

                {/* Protected routes */}
                <Route
                    path="/employees"
                    element={
                        <PrivateRoute>
                            <Employees />
                        </PrivateRoute>
                    }
                />

                <Route
                    path="/employees/create"
                    element={
                        <PrivateRoute>
                            <EmployeeForm />
                        </PrivateRoute>
                    }
                />

                <Route
                    path="/employees/:id/edit"
                    element={
                        <PrivateRoute>
                            <EmployeeForm />
                        </PrivateRoute>
                    }
                />

                {/* Fallback route */}
                <Route path="*" element={<Navigate to="/employees" replace />} />
            </Routes>
        </BrowserRouter>
    );
}