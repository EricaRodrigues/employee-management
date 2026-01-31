
// Base employee model returned by API
import apiClient from "../api/apiClient.ts";
import type {Role} from "../types/employee.ts";

export type Employee = {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    docNumber: string;
    birthDate: string;
    role: Role;
    managerId: string | null;
    phones: string[];
};

// Detailed employee model (used on edit screen)
export type EmployeeDetails = Employee & {
    docNumber: string;
    birthDate: string;
    managerId?: string | null;
    phones: string[];
};

// Payload to create a new employee
export type CreateEmployeeRequest = {
    firstName: string;
    lastName: string;
    email: string;
    docNumber: string;
    birthDate: string;
    role: number;
    managerId?: string | null;
    password: string;
    phones: string[];
};

// Payload to update an employee (password not allowed)
export type UpdateEmployeeRequest = Omit<CreateEmployeeRequest, "password">;

export async function getAllEmployees(): Promise<Employee[]> {
    const response = await apiClient.get<Employee[]>("/employees");
    return response.data;
}

export async function getEmployeeById(id: string): Promise<EmployeeDetails> {
    const response = await apiClient.get<EmployeeDetails>(`/employees/${id}`);
    return response.data;
}

export async function createEmployee(data: CreateEmployeeRequest) {
    await apiClient.post("/employees", data);
}

export async function updateEmployee(id: string, data: UpdateEmployeeRequest) {
    await apiClient.put(`/employees/${id}`, data);
}

export async function deleteEmployee(id: string) {
    await apiClient.delete(`/employees/${id}`);
}