import { title } from "node:process";
import { prisma } from "../../config/prisma";

interface CreateTicketDTO {
    title: string;
    description: string;
    priority: 'LOW' | 'MEDIUM' | 'HIGH';
    category: string;
    userId: string;
}

export async function createTicketService(data : CreateTicketDTO) {
    const ticket = await prisma.ticket.create({
        data: {
            title: data.title,
            description: data.description,
            priority: data.priority,
            category: data.category,
            authorId: data.userId
        }
    });

    return ticket;
}

export async function listTicketsService(userId: string, role: string) {
    if (role === 'USER') {
        return prisma.ticket.findMany({
            where: {authorId: userId},
            include: {
                author: true,
                assigned: true
            }
        })
    }

    return prisma.ticket.findMany({
        include: {
            author: true,
            assigned: true
        }
    })
}

export async function assignTicketService(ticketId: string, userId: string) {
    const ticket = await prisma.ticket.update({
        where: {id: ticketId},
        data: {
            assignedId: userId,
            status: 'IN_PROGRESS'
        }
    });

    return ticket;
}

export async function updateStatusService(
    ticketId: string,
    status: 'OPEN' | 'IN_PROGRESS' | 'RESOLVED' | 'CLOSED'
) {
    return prisma.ticket.update({
        where: {id: ticketId},
        data: { status }
    });
}