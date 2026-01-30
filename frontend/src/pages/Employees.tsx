import {useEffect, useState} from "react";
import {type Employee, getAllEmployees} from "../services/employeeService";
import {useAuth} from "../context/useAuth.ts";
import {useNavigate} from "react-router-dom";
import {ROLE_LABEL} from "../utils/roles";
import { deleteEmployee } from "../services/employeeService";
import { getApiErrorMessage } from "../api/apiError";

export default function Employees() {
    const navigate = useNavigate();
    const {signOut, user} = useAuth();
    const [employees, setEmployees] = useState<Employee[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    // Load employees on page load
    useEffect(() => {
        async function loadEmployees() {
            setLoading(true);
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

    // Helper to get manager full name from managerId
    function getManagerName(
        managerId: string | null,
        employees: Employee[]
    ) {
        if (!managerId) return "-";

        const manager = employees.find(e => e.id === managerId);
        return manager
            ? `${manager.firstName} ${manager.lastName}`
            : "-";
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

                    <div className="flex items-center gap-4">
                        <button
                            onClick={signOut}
                            className="text-sm text-rose-400 hover:underline"
                        >
                            Logout
                        </button>

                        {user && user.role !== 1 && (
                            <button
                                onClick={() => navigate("/employees/create")}
                                className="mt-6 bg-indigo-500 text-white px-6 py-3 rounded-2xl font-bold hover:bg-indigo-400 transition-all"
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
                                    Name
                                </th>
                                <th className="px-6 py-4 text-left text-[10px] uppercase font-black tracking-widest text-slate-500">
                                    Email
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
                            {employees.map(emp => (
                                <tr
                                    key={emp.id}
                                    className="border-t border-slate-800 hover:bg-[#0b0f1a]/40 transition-all"
                                >
                                    <td className="px-6 py-4 text-white font-medium">
                                        {emp.firstName} {emp.lastName}
                                    </td>
                                    <td className="px-6 py-4 text-slate-400 text-sm">
                                        {emp.email}
                                    </td>
                                    <td className="px-6 py-4 text-slate-300 text-sm">
                                        {ROLE_LABEL[emp.role] ?? "Unknown"}
                                    </td>
                                    <td className="p-3 text-slate-400">
                                        {getManagerName(emp.managerId, employees)}
                                    </td>
                                    <td className="px-6 py-4 text-right">
                                        {user && user.role !== 1 && (
                                            <div className="flex justify-end gap-4 text-sm">
                                                <button
                                                    onClick={() => navigate(`/employees/${emp.id}/edit`)}
                                                    className="text-indigo-400 hover:underline"
                                                >
                                                    Edit
                                                </button>

                                                <button
                                                    onClick={async () => {
                                                        if (!confirm("Delete this employee?")) return;
                                                        try {
                                                            await deleteEmployee(emp.id);
                                                            setEmployees(prev => prev.filter(e => e.id !== emp.id));
                                                        } catch (err) {
                                                            setError(getApiErrorMessage(err));
                                                        }
                                                    }}
                                                    className="text-rose-400 hover:underline"
                                                >
                                                    Delete
                                                </button>
                                            </div>
                                        )}
                                    </td>
                                </tr>
                            ))}
                            </tbody>
                        </table>
                    )}
                </div>
            </div>
        </div>
    );

}