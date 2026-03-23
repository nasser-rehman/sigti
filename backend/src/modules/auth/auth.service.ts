import { prisma } from '../../config/prisma';
import bcrypt from 'bcrypt';
import jwt from 'jsonwebtoken';
import { env } from '../../config/env';

interface LoginDTO {
    email: string;
    password: string;
}

export async function loginService({email, password}   : LoginDTO) { 
    const user = await prisma.user.findUnique(
        {
            where: { email }
        }
    );

    if (!user) {
        throw new Error('Invalid credentials');
    }

    const passwordMatch = await bcrypt.compare(password, user.password);

    if (!passwordMatch) {
        throw new Error('Invalid credentials');
    }

    const token = jwt.sign({ sub: user.id, role: user.role }, env.jwtSecret);

    return {
        token,
        user: {
            id: user?.id,
            name: user?.name,
            email: user?.email,
            role: user?.role
        }
    }

}