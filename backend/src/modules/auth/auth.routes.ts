import { Router } from "express";
import { loginController } from "./auth.controller";
import { authMiddleware } from "../../middlewares/auth.middleware";


export const authRoutes = Router();

authRoutes.post('/login', loginController);
