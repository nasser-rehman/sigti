import dotenv from 'dotenv';

dotenv.config();

export const env = {
    port: process.env.PORT,
    jwtSecret: process.env.JWT_SECRET as string,
    jwtExpiresIn: process.env.JWT_EXPIRES_IN,
    databaseUrl: process.env.DATABASE_URL as string,
}