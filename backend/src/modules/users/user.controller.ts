import { Request, Response } from 'express';
import { createUserService, updateUserService, getAllUsersService } from './user.service';

export async function createUserController(req: Request, res: Response) {
  const { name, email, password, role } = req.body;
  const user = await createUserService({ name, email, password, role });
  return res.status(201).json(user);
}

export async function updateUserController(req: Request, res: Response) {
  const { id, name, email, password, role } = req.body;
  const user = await updateUserService({ id, name, email, password, role });
  return res.status(201).json(user);
}

export async function getAllUsersController(req: Request, res: Response) {
  const users = await getAllUsersService();
  return res.status(200).json(users);
}
