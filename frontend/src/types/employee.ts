
export const ROLE = {
    Employee: 1,
    Leader: 2,
    Director: 3,
} as const;

export type Role = typeof ROLE[keyof typeof ROLE];

export const ROLE_LABEL: Record<Role, string> = {
    [ROLE.Employee]: "Employee",
    [ROLE.Leader]: "Leader",
    [ROLE.Director]: "Director",
};

// Maps role string from backend (JWT) to frontend numeric role
export function mapRoleToNumber(role: string): number {
    if (!(role in ROLE)) {
        throw new Error(`Invalid role from token: ${role}`);
    }

    return ROLE[role as keyof typeof ROLE];
}