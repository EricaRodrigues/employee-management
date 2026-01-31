import {useEffect, useState} from "react";
import {
    type Employee,
    getAllEmployees,
    getEmployeeById
} from "../services/employeeService";
import {useAuth} from "../context/useAuth.ts";
import {useNavigate} from "react-router-dom";
import { deleteEmployee } from "../services/employeeService";
import { getApiErrorMessage } from "../api/apiError";
import {ROLE, ROLE_LABEL} from "../types/employee.ts";

export default function Employees() {
    const navigate = useNavigate();
    const {signOut, user} = useAuth();
    const [employees, setEmployees] = useState<Employee[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [loggedEmployee, setLoggedEmployee] = useState<Employee | null>(null);

    // Load employees on page load
    useEffect(() => {
        async function loadEmployees() {
            setLoading(true);
            setError(null);
            try {
                const data = await getAllEmployees();
                setEmployees(data);
            } catch (error) {
                console.error(error);
            } finally {
                setLoading(false);
            }
        }

        loadEmployees();
    }, []);

    useEffect(() => {
        async function loadLoggedUser() {
            setError(null);
            
            if (!user) return;

            try {
                const data = await getEmployeeById(user.id);
                setLoggedEmployee(data);
            } catch (err) {
                console.error("Failed to load logged user", err);
            }
        }

        loadLoggedUser();
    }, [user]);

    // Helpers de permissão (UI level)
    function canEdit(user: any, target: any) {
        if (!user) return false;

        // Can edit yourself
        if (user.id === target.id) return true;

        // Leader -> only Employee
        if (user.role === ROLE.Leader) return target.role === ROLE.Employee;

        // Director -> all
        if (user.role === ROLE.Director) return true;

        return false;
    }

    function canDelete(user: any, target: any) {
        if (!user) return false;
        
        // Can never delete yourself
        if (user.id === target.id) return false;

        // Employee can never delete
        if (user.role === ROLE.Employee) return false;

        // Leader -> only Employee
        if (user.role === ROLE.Leader) return target.role === ROLE.Employee;

        // Director -> can delete any other (including Director)
        if (user.role === ROLE.Director) return true;

        return false;
    }
    
    // Helper to get manager full name from managerId
    function getManagerName(managerId: string | null, employees: Employee[]) {
        if (!managerId) return "-";

        const manager = employees.find(e => e.id === managerId);
        return manager
            ? `${manager.firstName} ${manager.lastName}`
            : "-";
    }

    // Handle Delete
    async function handleDelete(e: Employee) {
        if (!confirm("Delete this employee?")) return;
        try {
            await deleteEmployee(e.id);
            setEmployees(prev =>
                prev.filter(item => item.id !== e.id)
            );
        } catch (err) {
            setError(getApiErrorMessage(err));
        }
    }

    if (loading) { // Show loading state while fetching employees
        return (
            <div className="min-h-screen bg-[#0b0f1a] p-6">
                <h1 className="text-2xl font-black text-white mb-6">Employees</h1>

                <div className="bg-[#161b2c] border border-slate-800 rounded-3xl p-10 text-center">
                    <div className="mx-auto mb-4 h-10 w-10 animate-spin rounded-full border-2 border-indigo-500 border-t-transparent" />
                    <p className="text-slate-400 text-sm">
                        Loading employees...
                    </p>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-[#0b0f1a] p-6">
            <div className="max-w-6xl mx-auto space-y-6">

                {/* HEADER */}
                <div className="flex justify-between items-center">
                    <div>
                        <h1 className="text-3xl font-black text-white">
                            Employees
                        </h1>
                        <p className="text-slate-400 text-sm mt-1">
                            Manage your team members
                        </p>
                    </div>

                    <div className="flex items-center gap-6">

                        {/* Logged user info */}
                        {loggedEmployee && (
                            <p className="text-slate-400 text-sm mt-1">
                                Welcome, {loggedEmployee.firstName} ({ROLE_LABEL[loggedEmployee.role]})
                            </p>
                        )}

                        <button
                            onClick={signOut}
                            className="text-sm text-rose-400 hover:underline"
                        >
                            Logout
                        </button>

                        {user && user.role !== ROLE.Employee && (
                            <button
                                onClick={() => navigate("/employees/create")}
                                className="bg-indigo-500 text-white px-6 py-3 rounded-2xl font-bold hover:bg-indigo-400 transition-all"
                            >
                                Create Employee
                            </button>
                        )}
                    </div>
                </div>

                {error && (
                    <div className="bg-rose-500/10 border border-rose-500/20 text-rose-400 p-4 rounded-2xl text-sm">
                        {error}
                    </div>
                )}

                {/* TABLE CARD */}
                <div className="bg-[#161b2c] border border-slate-800 rounded-3xl overflow-hidden shadow-2xl">

                    {employees.length === 0 ? ( // Empty state when no employees are returned
                        <div className="bg-[#161b2c] border border-slate-800 rounded-3xl p-12 text-center">
                            <div className="text-4xl mb-4">📂</div>

                            <p className="text-white font-bold text-lg">
                                No employees found
                            </p>

                            <p className="text-slate-400 text-sm mt-2">
                                Start by creating your first employee.
                            </p>
                            
                        </div>
                    ) : (
                        <table className="w-full">
                            <thead className="bg-[#0b0f1a]">
                            <tr>
                                <th className="px-6 py-4 text-left text-[10px] uppercase font-black tracking-widest text-slate-500">
                                    First Name
                                </th>
                                <th className="px-6 py-4 text-left text-[10px] uppercase font-black tracking-widest text-slate-500">
                                    Email
                                </th>
                                <th className="px-6 py-4 text-left text-[10px] uppercase font-black tracking-widest text-slate-500">
                                    Doc Number
                                </th>
                                <th className="px-6 py-4 text-left text-[10px] uppercase font-black tracking-widest text-slate-500">
                                    Role
                                </th>
                                <th className="px-6 py-4 text-left text-[10px] uppercase font-black tracking-widest text-slate-500">
                                    Manager
                                </th>
                                <th className="px-6 py-4 text-right text-[10px] uppercase font-black tracking-widest text-slate-500">
                                    Actions
                                </th>
                            </tr>
                            </thead>

                            <tbody>
                            {employees.map(emp => {

                                const showEdit = canEdit(user, emp);
                                const showDelete = canDelete(user, emp);

                                return (
                                    <tr
                                        key={emp.id}
                                        className="border-t border-slate-800 hover:bg-[#0b0f1a]/40 transition-all"
                                    >
                                        <td className="px-6 py-4 text-white font-medium">
                                            {emp.firstName}
                                        </td>

                                        <td className="px-6 py-4 text-slate-400 text-sm">
                                            {emp.email}
                                        </td>

                                        <td className="px-6 py-4 text-slate-400 text-sm">
                                            {emp.docNumber}
                                        </td>

                                        <td className="px-6 py-4 text-slate-300 text-sm">
                                            {ROLE_LABEL[emp.role] ?? "Unknown"}
                                        </td>

                                        <td className="p-3 text-slate-400">
                                            {getManagerName(emp.managerId, employees)}
                                        </td>

                                        <td className="px-6 py-4 text-right">
                                            <div className="flex justify-end gap-4 text-sm">

                                                {showEdit && (
                                                    <button
                                                        onClick={() => navigate(`/employees/${emp.id}/edit`)}
                                                        className="text-indigo-400 hover:underline"
                                                    >
                                                        Edit
                                                    </button>
                                                )}

                                                {showDelete && (
                                                    <button
                                                        onClick={() => handleDelete(emp)}
                                                        className="text-rose-400 hover:underline"
                                                    >
                                                        Delete
                                                    </button>
                                                )}

                                            </div>
                                        </td>
                                    </tr>
                                );
                            })}
                            </tbody>
                        </table>
                    )}
                </div>
            </div>
        </div>
    );

}