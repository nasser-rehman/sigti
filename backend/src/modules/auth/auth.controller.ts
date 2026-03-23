import { Request, Response } from 'express';
import { loginService } from './auth.service';

export async function loginController(req: Request, res: Response) {

    try {
        const { email, password } = req.body;

        const result = await loginService({ email, password });

        return res.json(result);
    } catch (error) {
        return res.status(401).json({ message: 'Invalid credentials' });
    }
}