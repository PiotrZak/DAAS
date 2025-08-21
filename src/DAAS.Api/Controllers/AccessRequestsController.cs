using DAAS.Application.Commands;
using DAAS.Application.DTOs;
using DAAS.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DAAS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccessRequestsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Get all access requests or pending requests for approvers
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccessRequestDto>>> GetAccessRequests(
        [FromQuery] int? userId = null, 
        [FromQuery] bool pendingOnly = false)
    {
        try
        {
            IEnumerable<AccessRequestDto> requests;

            if (pendingOnly)
            {
                requests = await _mediator.Send(new GetPendingAccessRequestsQuery());
            }
            else if (userId.HasValue)
            {
                requests = await _mediator.Send(new GetUserAccessRequestsQuery(userId.Value));
            }
            else
            {
                requests = await _mediator.Send(new GetAllAccessRequestsQuery());
            }

            return Ok(requests);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create a new access request
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AccessRequestDto>> CreateAccessRequest(
        [FromQuery] int userId,
        [FromBody] CreateAccessRequestDto createDto)
    {
        try
        {
            var command = new CreateAccessRequestCommand(userId, createDto.DocumentId, createDto.Reason, createDto.AccessType);
            var request = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAccessRequests), new { id = request.Id }, request);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Make a decision on an access request (approve/reject)
    /// </summary>
    [HttpPut("{id}/decision")]
    public async Task<ActionResult<AccessRequestDto>> MakeDecision(
        int id,
        [FromQuery] int approverId,
        [FromBody] CreateDecisionDto decisionDto)
    {
        try
        {
            var command = new ApproveAccessRequestCommand(id, approverId, decisionDto.IsApproved, decisionDto.Comment);
            var updatedRequest = await _mediator.Send(command);
            return Ok(updatedRequest);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}