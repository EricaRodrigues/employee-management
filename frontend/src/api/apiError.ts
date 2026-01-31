import axios from "axios";

// Centralized helper to extract an error message from API responses
export function getApiErrorMessage(error: unknown): string {
    if (axios.isAxiosError(error)) {
        const status = error.response?.status;
        const data = error.response?.data as {
            error?: string;
            message?: string;
            errors?: string[];
        };

        if (status === 403) {
            return "You do not have permission to perform this action.";
        }

        if (status === 401) {
            return "Your session has expired. Please login again.";
        }

        if (data) {
            if (typeof (data as any).error === "string") {
                return (data as any).error;
            }

            if (typeof (data as any).message === "string") {
                return (data as any).message;
            }

            if (Array.isArray((data as any).errors)) {
                return (data as any).errors.join(", ");
            }
        }

        return "Request failed. Please try again.";
    }

    return "Unexpected error.";
}