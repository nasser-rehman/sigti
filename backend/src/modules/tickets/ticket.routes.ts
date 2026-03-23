import { Router } from 'express';
import { createTicketController, listTicketsController, assignTicketController, updateStatusController } from './ticket.controller';
import { authMiddleware } from '../../middlewares/auth.middleware';
import { roleMiddleware } from '../../middlewares/role.middleware';
import { Roles } from '../../utils/roles';

export const ticketRoutes = Router();

ticketRoutes.post(
    '/',
    authMiddleware,
    createTicketController
)

ticketRoutes.get(
    '/',
    authMiddleware,
    listTicketsController
)

ticketRoutes.patch(
    '/:id/assign',
    authMiddleware,
    roleMiddleware([Roles.ADMIN, Roles.AGENT]),
    assignTicketController
)

ticketRoutes.patch(
    '/:id/status',
    authMiddleware,
    roleMiddleware([Roles.ADMIN, Roles.AGENT]),
    updateStatusController
)
