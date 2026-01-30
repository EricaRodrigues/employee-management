import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/useAuth";

export default function Login() {
    const { signIn } = useAuth();
    const navigate = useNavigate();

    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [loading, setLoading] = useState(false);

    const [error, setError] = useState<string | null>(null); // API error
    const [errors, setErrors] = useState<Record<string, string>>({}); // Field errors
    const [submitted, setSubmitted] = useState(false);

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    // Simple frontend validation for better UX
    function validateForm() {
        const errors: Record<string, string> = {};

        if (!email.trim()) {
            errors.email = "Email is required";
        } else if (!emailRegex.test(email)) {
            errors.email = "Invalid email format";
        }

        if (!password.trim()) {
            errors.password = "Password is required";
        }

        return errors;
    }
    
    function hasError(field: string) {
        return submitted && !!errors[field];
    }

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        setError(null);
        setSubmitted(true);

        const validationErrors = validateForm();
        if (Object.keys(validationErrors).length > 0) {
            setErrors(validationErrors);
            return;
        }

        setLoading(true);

        try {
            await signIn({ email, password });
            navigate("/employees");
        } catch {
            // Invalid credentials returned by backend
            setError("Invalid email or password");
        } finally {
            setLoading(false);
        }
    }

    return (
        <div className="min-h-screen bg-[#0b0f1a] flex items-center justify-center p-6">
            <div className="bg-[#161b2c] border border-slate-800 rounded-4xl p-8 shadow-2xl">
                <div className="text-center mb-8">
                    <div className="w-16 h-16 mx-auto mb-4 rounded-2xl bg-indigo-500/20 flex items-center justify-center text-3xl">
                        👥
                    </div>
                    <h1 className="text-3xl font-black text-white">
                        Employee Management
                    </h1>
                    <p className="text-slate-400 text-sm mt-2">
                        Sign in to manage your team
                    </p>
                </div>

                <form onSubmit={handleSubmit} className="space-y-6">

                    {/* Email */}
                    <div className="flex flex-col gap-1.5">
                        <label className="text-[10px] uppercase font-black text-slate-500 tracking-widest ml-1">
                            Email
                        </label>
                        <input
                            name="email"
                            value={email}
                            onChange={(e) => {
                                setEmail(e.target.value);
                                if (errors.email) {
                                    setErrors(prev => {
                                        const copy = { ...prev };
                                        delete copy.email;
                                        return copy;
                                    });
                                }
                            }}
                            className={`w-full bg-[#0b0f1a] border rounded-2xl px-4 py-3 text-sm text-white outline-none transition-all ${
                                hasError("email")
                                    ? "border-rose-500 focus:border-rose-500"
                                    : "border-slate-700 focus:border-indigo-500"
                            }`}
                        />
                        {hasError("email") && (
                            <p className="text-xs text-rose-500 mt-1">
                                {errors.email}
                            </p>
                        )}
                    </div>

                    {/* Password */}
                    <div className="flex flex-col gap-1.5">
                        <label className="text-[10px] uppercase font-black text-slate-500 tracking-widest ml-1">
                            Password
                        </label>
                        <input
                            name="password"
                            value={password}
                            onChange={(e) => {
                                setPassword(e.target.value);
                                if (errors.password) {
                                    setErrors(prev => {
                                        const copy = { ...prev };
                                        delete copy.password;
                                        return copy;
                                    });
                                }
                            }}
                            className={`w-full bg-[#0b0f1a] border rounded-2xl px-4 py-3 text-sm text-white outline-none transition-all ${
                                hasError("password")
                                    ? "border-rose-500 focus:border-rose-500"
                                    : "border-slate-700 focus:border-indigo-500"
                            }`}
                        />
                        {hasError("password") && (
                            <p className="text-xs text-rose-500 mt-1">
                                {errors.password}
                            </p>
                        )}
                    </div>

                    {error && (
                        <div className="text-sm text-rose-400 bg-rose-500/10 border border-rose-500/20 p-3 rounded-xl">
                            {error}
                        </div>
                    )}

                    <button
                        type="submit"
                        disabled={loading}
                        className="w-full bg-white text-[#0b0f1a] py-4 rounded-2xl font-black text-lg hover:bg-slate-200 transition-all disabled:opacity-50"
                    >
                        {loading ? "Signing in..." : "Sign In"}
                    </button>
                </form>
            </div>
        </div>
    );
}