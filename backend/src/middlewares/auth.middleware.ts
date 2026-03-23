import { Request, Response, NextFunction } from 'express';
import jwt from 'jsonwebtoken';
import { env } from '../config/env';

interface TokenPayload {
    sub: string;
    role: string;
}

export function authMiddleware(
    req: Request,
    res: Response,
    next: NextFunction
) {
    const authHeader = req.headers.authorization;

    if (!authHeader) {
        return res.status(401).json({ message: 'No token provided' });
    }

    const [, token] = authHeader.split(' ');

    try {
        const decoded = jwt.verify(token, env.jwtSecret) as TokenPayload;

        req.user = {
            id: decoded.sub,
            role: decoded.role
        } as Request['user'];
        
        return next();
    } catch {
        return res.status(401).json({ message: 'Invalid token' });
    }
}