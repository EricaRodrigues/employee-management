import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
    createEmployee, type Employee,
    getEmployeeById, getAllEmployees,
    updateEmployee,
} from "../services/employeeService";
import { getApiErrorMessage } from "../api/apiError";
import { useAuth } from "../context/useAuth";
import {ROLE} from "../types/employee.ts";

export default function EmployeeForm() {
    const { id } = useParams();
    const isEdit = !!id;

    const navigate = useNavigate();
    const { user } = useAuth();
    const [loading, setLoading] = useState(false);
    const [initialLoading, setInitialLoading] = useState(isEdit);
    const [error, setError] = useState<string | null>(null);
    const [errors, setErrors] = useState<Record<string, string>>({});
    const [submitted, setSubmitted] = useState(false);
    const [managers, setManagers] = useState<Employee[]>([]);
    const [showPassword, setShowPassword] = useState(false);
    const maxRoleAllowed = user?.role ?? ROLE.Employee;

    const [form, setForm] = useState({
        firstName: "",
        lastName: "",
        email: "",
        docNumber: "",
        birthDate: "",
        role: 1,
        managerId: "",
        password: "",
        phones: [""],
    });

    // Any employee can be a manager, (except themselves)
    const availableManagers = managers.filter(m => {
        if (m.id === id) return false; // never himself

        // Director can have another Director as Manager
        if (form.role === ROLE.Director && m.role === ROLE.Director) return true;

        // Manager needs to have a higher role
        return m.role > form.role;
    });

    // Load employee data when editing
    useEffect(() => {
        if (!isEdit) return;

        async function loadEmployee() {
            try {
                const data = await getEmployeeById(id!);
                setForm({
                    firstName: data.firstName ?? "",
                    lastName: data.lastName ?? "",
                    email: data.email ?? "",
                    docNumber: data.docNumber ?? "",
                    birthDate: data.birthDate
                        ? data.birthDate.slice(0, 10)
                        : "",
                    role: data.role,
                    managerId: data.managerId ?? "",
                    password: "",
                    phones: Array.isArray(data.phones) && data.phones.length > 0
                        ? data.phones
                        : [""],
                });
            } catch (err) {
                setError(getApiErrorMessage(err));
            } finally {
                setInitialLoading(false);
            }
        }

        loadEmployee();
    }, [id, isEdit]);

    // Load possible managers
    useEffect(() => {
        async function loadManagers() {
            try {
                const data = await getAllEmployees();
                setManagers(data);
            } catch (err) {
                console.error("Failed to load managers", err);
            }
        }

        loadManagers();
    }, []);

    // Basic role protection (UI level) -- Employee can only edit themselves
    if (user?.role === ROLE.Employee && (!isEdit || id !== user.id)) {
        navigate("/employees");
        return null;
    }

    function canEditSensitiveFields(user: any, targetRole: number) {
        if (!user) return false;

        // Can never change own role/manager
        if (user.id === id) return false;

        // Leader -> only Employee
        if (user.role === ROLE.Leader) return targetRole === ROLE.Employee;

        // Director -> any
        if (user.role === ROLE.Director) return true;

        return false;
    }

    const canEditSensitive = canEditSensitiveFields(user, form.role);
    
    // Update form state and clear field error while typing
    function handlePhoneChange(index: number, value: string) {
        setForm(prev => {
            const phones = [...prev.phones];
            phones[index] = value;
            return { ...prev, phones };
        });

        // Clear phone validation error while typing
        if (errors[`phones.${index}`]) {
            setErrors(prev => {
                const copy = { ...prev };
                delete copy[`phones.${index}`];
                return copy;
            });
        }
    }

    function addPhone() {
        setForm(prev => ({
            ...prev,
            phones: [...prev.phones, ""],
        }));
    }

    function removePhone(index: number) {
        setForm(prev => ({
            ...prev,
            phones: prev.phones.filter((_, i) => i !== index),
        }));
    }

    function isValidPhone(phone: string) {
        const digits = phone.replace(/\D/g, "");
        return digits.length >= 10 && digits.length <= 15;
    }

    // Update form state and clear field error while typing
    function handleChange(
        e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
    ) {
        const { name, value } = e.target;

        setForm(prev => {
            if (name === "role") {
                return {
                    ...prev,
                    role: Number(value),
                    managerId: "",
                };
            }

            return {
                ...prev,
                [name]: value,
            };
        });

        // Clear error as soon as user starts fixing the field
        if (errors[name]) {
            setErrors(prev => {
                const copy = { ...prev };
                delete copy[name];
                return copy;
            });
        }
    }


    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    // Frontend validation before submit
    function validateForm(form: any) {
        const errors: Record<string, string> = {};

        if (!form.firstName.trim()) {
            errors.firstName = "First name is required";
        }

        if (!form.lastName.trim()) {
            errors.lastName = "Last name is required";
        }

        if (!form.email.trim()) {
            errors.email = "Email is required";
        } else if (!emailRegex.test(form.email)) {
            errors.email = "Invalid email format";
        }

        if (!form.docNumber.trim()) {
            errors.docNumber = "Doc number is required";
        }

        if (!form.birthDate) {
            errors.birthDate = "Birth date is required";
        }

        form.phones.forEach((phone: string, index: number) => {
            // Phone is optional, but if provided it must be valid
            if (phone.trim() && !isValidPhone(phone)) {
                errors[`phones.${index}`] = "Invalid phone number";
            }
        });

        if (!isEdit && !form.password.trim()) {
            errors.password = "Password is required";
        }

        return errors;
    }

    // Helper to check if a field should show error
    function hasError(field: string) {
        return submitted && !!errors[field];
    }

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        setLoading(true);
        setError(null);
        setSubmitted(true);

        const validationErrors = validateForm(form);
        if (Object.keys(validationErrors).length > 0) {
            setErrors(validationErrors);
            setLoading(false);
            return;
        }
        
        try {
            const payload = {
                ...form,
                role: Number(form.role),
                phones: form.phones
                    .map(p => p.trim())
                    .filter(p => p.length > 0),
                managerId: form.managerId || null,
            };

            if (isEdit) {
                const { password, ...updateData } = payload;
                await updateEmployee(id!, updateData);
            } else {
                await createEmployee(payload);
            }

            navigate("/employees");
        } catch (err) {
            setError(getApiErrorMessage(err));
        } finally {
            setLoading(false);
        }
    }

    if (initialLoading) {
        return <p className="p-6">Loading...</p>;
    }

    return (
        <div className="min-h-screen bg-[#0b0f1a] flex items-center justify-center p-6">
            <div className="w-full max-w-xl bg-[#161b2c] border border-slate-800 rounded-3xl p-10 shadow-2xl space-y-8">

                <button
                    type="button"
                    onClick={() => navigate("/employees")}
                    className="text-sm text-slate-400 hover:underline"
                >
                    ← Back
                </button>
                
                {/* HEADER */}
                <div className="text-center">
                    <div className="w-16 h-16 mx-auto mb-4 rounded-2xl bg-indigo-500/20 flex items-center justify-center text-3xl">
                        👤
                    </div>
                    <h1 className="text-3xl font-black text-white">
                        {isEdit ? "Edit Employee" : "Create Employee"}
                    </h1>
                    <p className="text-slate-400 text-sm mt-2">
                        {isEdit
                            ? "Update employee information"
                            : "Add a new team member"}
                    </p>
                </div>
                
                <form onSubmit={handleSubmit} className="space-y-6">

                    {/*First Name*/}
                    <div className="flex flex-col gap-1.5">
                        <label className="text-[10px] uppercase font-black text-slate-500 tracking-widest ml-1"> First Name </label>
                        <input
                            type="text"
                            name="firstName"
                            value={form.firstName}
                            onChange={handleChange}
                            placeholder="First Name"
                            className={`w-full  bg-[#0b0f1a] border rounded-2xl px-4 py-3 text-white outline-none transition-all ${
                                hasError("firstName") ? "border-rose-500 focus:border-rose-500" : 'border-slate-700 focus:border-indigo-500'
                            }`}
                        />
                        {hasError("firstName") && (
                            <p className="text-xs text-rose-500 mt-1">
                                {errors.firstName}
                            </p>
                        )}
                    </div>
                    

                    {/*Last Name*/}
                    <div className="flex flex-col gap-1.5">
                        <label className="text-[10px] uppercase font-black text-slate-500 tracking-widest ml-1"> Last Name </label>
                        <input
                            type="text"
                            name="lastName"
                            value={form.lastName}
                            onChange={handleChange}
                            placeholder="Last Name"
                            className={`w-full bg-[#0b0f1a] border rounded-2xl px-4 py-3 text-white outline-none transition-all ${
                                hasError("lastName") ? "border-rose-500 focus:border-rose-500" : 'border-slate-700 focus:border-indigo-500'
                            }`}
                        />
                        {hasError("lastName") && (
                            <p className="text-xs text-rose-500 mt-1">
                                {errors.lastName}
                            </p>
                        )}
                    </div>

                    {/*Email*/}
                    <div className="flex flex-col gap-1.5">
                        <label className="text-[10px] uppercase font-black text-slate-500 tracking-widest ml-1"> Email </label>
                        <input
                            type="text"
                            name="email"
                            value={form.email}
                            onChange={handleChange}
                            placeholder="Email"
                            className={`w-full bg-[#0b0f1a] border rounded-2xl px-4 py-3 text-white outline-none transition-all ${
                                hasError("email") ? "border-rose-500 focus:border-rose-500" : 'border-slate-700 focus:border-indigo-500'
                            }`}
                        />
                        {hasError("email") && (
                            <p className="text-xs text-rose-500 mt-1">
                                {errors.email}
                            </p>
                        )}
                    </div>

                    {/*Doc Number*/}
                    <div className="flex flex-col gap-1.5">
                        <label className="text-[10px] uppercase font-black text-slate-500 tracking-widest ml-1"> Doc Number </label>
                        <input
                            type="text"
                            name="docNumber"
                            value={form.docNumber}
                            onChange={handleChange}
                            placeholder="Doc Number"
                            className={`w-full bg-[#0b0f1a] border rounded-2xl px-4 py-3 text-white outline-none transition-all ${
                                hasError("docNumber") ? "border-rose-500 focus:border-rose-500" : 'border-slate-700 focus:border-indigo-500'
                            }`}
                        />
                        {hasError("docNumber") && (
                            <p className="text-xs text-rose-500 mt-1">
                                {errors.docNumber}
                            </p>
                        )}
                    </div>

                    {/*Birth Date*/}
                    <div className="flex flex-col gap-1.5">
                        <label className="text-[10px] uppercase font-black text-slate-500 tracking-widest ml-1"> Birth Date </label>
                        <input
                            type="date"
                            name="birthDate"
                            value={form.birthDate}
                            onChange={handleChange}
                            placeholder="Birth Date"
                            className={`w-full bg-[#0b0f1a] border rounded-2xl px-4 py-3 text-white outline-none transition-all scheme-dark ${
                                hasError("birthDate") ? "border-rose-500 focus:border-rose-500" : 'border-slate-700 focus:border-indigo-500'
                            }`}
                        />
                        {hasError("birthDate") && (
                            <p className="text-xs text-rose-500 mt-1">
                                {errors.birthDate}
                            </p>
                        )}
                    </div>

                    {/*Password*/}
                    {!isEdit && (
                        <div className="flex flex-col gap-1.5">
                            <label className="text-[10px] uppercase font-black text-slate-500 tracking-widest ml-1">
                                Password
                            </label>

                            <div className="relative">
                                <input
                                    type={showPassword ? "text" : "password"}
                                    name="password"
                                    value={form.password}
                                    onChange={handleChange}
                                    placeholder="••••••••"
                                    className={`w-full bg-[#0b0f1a] border rounded-2xl px-4 py-3 text-white outline-none transition-all pr-12  ${
                                        hasError("password") ? "border-rose-500 focus:border-rose-500" : 'border-slate-700 focus:border-indigo-500'
                                    }`}
                                />

                                {/* Toggle password visibility */}
                                <button
                                    type="button"
                                    onClick={() => setShowPassword(prev => !prev)}
                                    className="absolute right-4 top-1/2 -translate-y-1/2 text-slate-400 hover:text-white text-sm font-bold"
                                >
                                    {showPassword ? "Hide" : "Show"}
                                </button>
                            </div>
                            
                            {hasError("password") && (
                                <p className="text-xs text-rose-500 mt-1">
                                    {errors.password}
                                </p>
                            )}
                        </div>
                    )}

                    {/*Phones*/}
                    <div className="flex flex-col gap-3">
                        <label className="text-[10px] uppercase font-black text-slate-500 tracking-widest ml-1">
                            Phones
                        </label>

                        {form.phones.map((phone, index) => (
                            <div key={index} className="flex gap-2 items-start">
                                <input
                                    type="text"
                                    value={phone}
                                    onChange={(e) => handlePhoneChange(index, e.target.value)}
                                    placeholder="(11) 99999-9999"
                                    className={`w-full flex-1 bg-[#0b0f1a] border rounded-2xl px-4 py-3 text-white outline-none transition-all ${
                                        hasError(`phones.${index}`)
                                            ? "border-rose-500 focus:border-rose-500"
                                            : "border-slate-700 focus:border-indigo-500"
                                    }`}
                                />

                                {form.phones.length > 1 && (
                                    <button
                                        type="button"
                                        onClick={() => removePhone(index)}
                                        className="text-rose-500 font-bold px-3"
                                    >
                                        ✕
                                    </button>
                                )}
                            </div>
                        ))}

                        {Object.keys(errors)
                            .filter(key => key.startsWith("phones."))
                            .map(key => (
                                <p key={key} className="text-xs text-rose-500">
                                    {errors[key]}
                                </p>
                            ))}

                        <button
                            type="button"
                            onClick={addPhone}
                            className="text-indigo-400 text-sm font-bold hover:underline w-fit"
                        >
                            + Add phone
                        </button>
                    </div>
                    
                    {/* ROLE */}
                    <div className="flex flex-col gap-1.5">
                        <label className="text-[10px] uppercase font-black text-slate-500 tracking-widest ml-1">
                            Role
                        </label>
                        <select
                            name="role"
                            value={form.role}
                            onChange={handleChange}
                            className={`w-full bg-[#0b0f1a] border rounded-2xl px-4 py-3 text-sm text-white outline-none
                                ${isEdit && !canEditSensitive ? "opacity-50 cursor-not-allowed pointer-events-none" : "focus:border-indigo-500"}
                            `}>
                            {maxRoleAllowed >= ROLE.Employee && <option value={ROLE.Employee}>Employee</option>}
                            {maxRoleAllowed >= ROLE.Leader && <option value={ROLE.Leader}>Leader</option>}
                            {maxRoleAllowed >= ROLE.Director && <option value={ROLE.Director}>Director</option>}
                        </select>
                    </div>

                    {/* Manager selection */}
                    <div className="flex flex-col gap-1.5">
                        <label className="text-[10px] uppercase font-black text-slate-500 tracking-widest ml-1">
                            Manager
                        </label>
                        {availableManagers.length === 0 && (
                            <p className="text-[10px] text-slate-500 mt-1 ml-1">
                                Managers must have a higher role than the employee
                            </p>
                        )}
                        <select
                            name="managerId"
                            value={form.managerId}
                            onChange={handleChange}
                            className={`w-full bg-[#0b0f1a] border rounded-2xl px-4 py-3 text-sm text-white outline-none
                                ${isEdit && !canEditSensitive ? "opacity-50 cursor-not-allowed pointer-events-none" : "focus:border-indigo-500"}
                            `}
                        >
                            <option value="">
                                {availableManagers.length === 0
                                    ? "Select role first"
                                    : "No manager"}
                            </option>

                            {availableManagers.map(manager => (
                                <option key={manager.id} value={manager.id}>
                                    {manager.firstName} {manager.lastName}
                                </option>
                            ))}
                        </select>
                    </div>
                    

                    {error && (
                        <div className="bg-rose-500/10 border border-rose-500/20 text-rose-400 p-3 rounded-xl text-sm">
                            {error}
                        </div>
                    )}

                    <button
                        type="submit"
                        disabled={loading}
                        className="w-full bg-white text-[#0b0f1a] py-4 rounded-2xl font-black text-lg hover:bg-slate-200 transition-all disabled:opacity-50"
                    >
                        {loading ? "Saving..." : "Save Employee"}
                    </button>
                </form>
            </div>
        </div>
    );
}