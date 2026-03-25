import bcrypt from 'bcrypt';
import { prisma } from '../../config/prisma';
import { Role } from '../../utils/roles';
import { AppError } from '../../errors/AppError';

interface CreateUserDTO {
  name: string;
  email: string;
  password: string;
  role: Role;
}

interface UpdateUserDTO {
  id: string;
  name?: string;
  email?: string;
  password?: string;
  role?: Role;
}

export async function createUserService({ name, email, password, role }: CreateUserDTO) {
  const existing = await prisma.user.findUnique({ where: { email } });

  if (existing) {
    throw new AppError('Email already in use', 409);
  }

  const hashedPassword = await bcrypt.hash(password, 10);

  const user = await prisma.user.create({
    data: {
      name,
      email,
      password: hashedPassword,
      role: role || 'USER',
    },
  });

  return user;
}

export async function updateUserService({ id, name, email, password, role }: Partial<UpdateUserDTO>) {
  const data: any = {};

  if (id) data.id = id;
  if (name) data.name = name;
  if (email) data.email = email;
  if (password) data.password = await bcrypt.hash(password, 10);
  if (role) data.role = role;

  const user = await prisma.user.update({
    where: { id: id },
    data,
  });

  return user;
}

export async function getAllUsersService() {
  const users = await prisma.user.findMany();
  return users;
}
