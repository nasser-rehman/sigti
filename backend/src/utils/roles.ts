export const Roles = {
    ADMIN: 'ADMIN',
    AGENT: 'AGENT',
    USER: 'USER',
} as const;

export type Role = typeof Roles[keyof typeof Roles];