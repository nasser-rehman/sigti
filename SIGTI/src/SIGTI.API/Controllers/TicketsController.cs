using MediatR;
using Microsoft.AspNetCore.Mvc;
using SIGTI.Application.Common.Exceptions;
using SIGTI.Application.Features.Tickets.Commands.AddComment;
using SIGTI.Application.Features.Tickets.Commands.CloseTicket;
using SIGTI.Application.Features.Tickets.Commands.CreateTicket;
using SIGTI.Application.Features.Tickets.Commands.DispatchTicket;
using SIGTI.Application.Features.Tickets.Commands.ResolveTicket;
using SIGTI.Application.Features.Tickets.Commands.StartTicketService;
using SIGTI.Application.Features.Tickets.Queries.GetTicketById;
using SIGTI.Application.Features.Tickets.Queries.ListTickets;

namespace SIGTI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ISender _sender;

        public TicketsController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateTicketCommand command
        )
        {
            var result = await _sender.Send(command);
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result
            );
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response = await _sender.Send(new GetTicketByIdQuery(id));
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> List(
            [FromQuery] ListTicketsQuery query,
            CancellationToken cancellationToken
        )
        {
            var response = await _sender.Send(query, cancellationToken);

            return Ok(response);
        }

        [HttpPatch("{id:guid}/start")]
        public async Task<IActionResult> Start(
            [FromRoute] Guid id,
            CancellationToken cancellationToken
        )
        {
            await _sender.Send(
                new StartTicketServiceCommand(id),
                cancellationToken
            );
            return NoContent();
        }

        [HttpPatch("{id:guid}/dispatch")]
        public async Task<IActionResult> Dispatch(
            [FromRoute] Guid id,
            [FromBody] DispatchTicketRequest request,
            CancellationToken cancellationToken
        )
        {
            var command = new DispatchTicketCommand(
                id,
                request.TechnicianId,
                request.AssignedById,
                request.Reason
            );

            var response = await _sender.Send(command, cancellationToken);

            return Ok(response);
        }

        [HttpPatch("{id:guid}/resolve")]
        public async Task<IActionResult> Resolve(
            [FromRoute] Guid id,
            CancellationToken cancellationToken
        )
        {
            var response = await _sender.Send(
                new ResolveTicketCommand(id),
                cancellationToken
            );

            return Ok(response);
        }

        [HttpPatch("{id:guid}/close")]
        public async Task<IActionResult> Close(
            [FromRoute] Guid id,
            CancellationToken cancellationToken
        )
        {
            var response = await _sender.Send(
                new CloseTicketCommand(id),
                cancellationToken
            );

            return Ok(response);
        }

        [HttpPost("{id:guid}/comments")]
        public async Task<IActionResult> AddComment(
            [FromRoute] Guid id,
            [FromBody] AddCommentRequest request,
            CancellationToken cancellationToken
        )
        {
            var command = new AddCommentCommand(
                id,
                request.CreatedById,
                request.Content
            );

            var response = await _sender.Send(command, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = response.TicketId },
                response
            );
        }
    }
}
