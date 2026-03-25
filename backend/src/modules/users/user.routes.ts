import { Router } from 'express';
import { createUserController, updateUserController, getAllUsersController } from './user.controller';
import { authMiddleware } from '../../middlewares/auth.middleware';
import { roleMiddleware } from '../../middlewares/role.middleware';
import { Roles } from '../../utils/roles';

export const userRoutes = Router();

// ROTAS protegidas

// Somente ADMIN
userRoutes.post('/createUser', createUserController);
userRoutes.patch('/updateUser', authMiddleware, roleMiddleware([Roles.ADMIN]), updateUserController);

// Somente ADMIN e AGENT
userRoutes.get('/users', getAllUsersController);

// Qualquer um logado (ADMIN, AGENT, USER)
userRoutes.get('/me', authMiddleware, async (req, res) => {
  return res.json({ user: req.user });
});
