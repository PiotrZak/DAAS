using DAAS.Application.DTOs;
using DAAS.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DAAS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Get all documents
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetDocuments()
    {
        try
        {
            var documents = await _mediator.Send(new GetAllDocumentsQuery());
            return Ok(documents);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get document by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DocumentDto>> GetDocument(int id)
    {
        try
        {
            var document = await _mediator.Send(new GetDocumentByIdQuery(id));
            if (document == null)
            {
                return NotFound(new { message = "Document not found" });
            }
            return Ok(document);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}