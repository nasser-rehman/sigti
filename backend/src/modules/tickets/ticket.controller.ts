import { Request, Response } from "express";
import { createTicketService, listTicketsService, assignTicketService, updateStatusService } from "./ticket.service";

// interface Params {
//     id: string;
// }

export async function createTicketController(req: Request, res: Response) {
    try {
        const { title, description, priority, category } = req.body;

        const userId = req.user!.id;

        const ticket = await createTicketService({
            title,
            description,
            priority,
            category,
            userId,
        });
        return res.status(201).json(ticket);
    } catch (error) {
        return res.status(500).json({ message: 'Internal server error' });
    }
}

export async function listTicketsController(req: Request, res: Response) {
    const user = req.user!;

    const tickets = await listTicketsService(user.id, user.role);

    return res.json(tickets);
}

export async function assignTicketController(req: Request, res: Response) {
    const {id} = req.params;
    const user = req.user!;

    if (!id || typeof id !== 'string') {
        return res.status(400).json({ message: 'Ticket ID is required' });
    }

    const ticket = await assignTicketService(id, user.id);

    return res.json(ticket);
}

export async function updateStatusController(req: Request, res: Response) {
    const {id} = req.params;
    const { status } = req.body;

    if (!id || typeof id !== 'string') {
        return res.status(400).json({ message: 'Ticket ID is required' });
    }

    const ticket = await updateStatusService(id, status);

    return res.json(ticket);
}