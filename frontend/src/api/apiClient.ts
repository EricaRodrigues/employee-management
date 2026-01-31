import axios from 'axios';

// Base API URL loaded from environment variables
const apiClient = axios.create({
    baseURL: import.meta.env.VITE_API_URL as string,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Attach JWT token to every request if it exists
apiClient.interceptors.request.use((config) => {
    const token = localStorage.getItem("@employee:token");

    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
});

// Handle global API errors
apiClient.interceptors.response.use(
    response => response,
    error => {
        // If token is invalid, force logout
        if (error.response?.status === 401) {
            localStorage.removeItem('@employee:token');
            localStorage.removeItem('@employee:user');
            window.location.href = "/login";
        }

        return Promise.reject(error);
    }
);

export default apiClient;