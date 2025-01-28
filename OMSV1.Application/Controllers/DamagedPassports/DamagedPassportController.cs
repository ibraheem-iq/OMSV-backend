﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using OMSV1.Application.Commands.DamagedPassports;
using OMSV1.Application.Queries.DamagedPassports;
using OMSV1.Infrastructure.Extensions;
using OMSV1.Application.Helpers;
using OMSV1.Application.CQRS.DamagedPassports.Queries;
using System.Net;
using OMSV1.Application.Authorization.Attributes;
using OMSV1.Application.Handlers.DamagedPassports;
namespace OMSV1.Application.Controllers.DamagedPassports
{

    public class DamagedPassportController : BaseApiController
    {
        private readonly IMediator _mediator;

        public DamagedPassportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Get All damaged passports with Pagination
        [HttpGet]
        [RequirePermission("DPr")]

        public async Task<IActionResult> GetAllDamagedPassports([FromQuery] PaginationParams paginationParams)
        {
            try
            {
                // Send the pagination parameters to the query handler
                var damagedPassports = await _mediator.Send(new GetAllDamagedPassportsQuery(paginationParams));

                // Add pagination headers to the response
                Response.AddPaginationHeader(damagedPassports);

                // Return the paginated result
                return Ok(damagedPassports);  // Returns PagedList<DamagedPassportDto>
            }
            catch (Exception ex)
            {
                // Handle errors and return a 500 Internal Server Error with a message
                return ResponseHelper.CreateErrorResponse(HttpStatusCode.InternalServerError, 
                    "An error occurred while retrieving the damaged passports.", 
                    new[] { ex.Message });
            }
        }

        // GET method to retrieve a damaged passport by ID
        [HttpGet("{id}")]
        [RequirePermission("DPr")]

        public async Task<IActionResult> GetDamagedPassportById(Guid id)
        {
            try
            {
                var passport = await _mediator.Send(new GetDamagedPassportByIdQuery(id));
                if (passport == null) return NotFound();  // Return 404 if not found

                return Ok(passport);  // Return the DamagedPassportDto
            }
            catch (Exception ex)
            {
                // Handle errors and return a 500 Internal Server Error
                return ResponseHelper.CreateErrorResponse(HttpStatusCode.InternalServerError, 
                    "An error occurred while retrieving the damaged passport by ID.", 
                    new[] { ex.Message });
            }
        }

        // POST method to add a new damaged passport
// Controller action
[HttpPost]
[RequirePermission("DPc")]
public async Task<IActionResult> AddDamagedPassport([FromBody] AddDamagedPassportCommand command)
{
    try
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetDamagedPassportById), new { id }, id);  // 201 Created response
    }
    catch (DuplicatePassportException ex)
    {
        // Return 409 Conflict for duplicate passport numbers
        return Conflict(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        // Handle other errors and return a 500 Internal Server Error
        return StatusCode(500, $"Internal server error: {ex.Message}");
    }
}
        // PUT method to update the damaged passport
        [HttpPut("{id}")]
        [RequirePermission("DPu")]

        public async Task<IActionResult> UpdateDamagedPassport(Guid id, [FromBody] UpdateDamagedPassportCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID in URL does not match the ID in the request body.");
            }

            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound($"DamagedPassport with ID {id} not found.");
            }

            return NoContent(); // 204 No Content, as no data is returned after a successful update
        }

        // DELETE method to delete the damaged passport
        [HttpDelete("{id}")]
        [RequirePermission("DPd")]

        public async Task<IActionResult> DeleteDamagedPassport(Guid id)
        {
            try
            {
                var result = await _mediator.Send(new DeleteDamagedPassportCommand(id));

                if (!result)
                {
                    return NotFound($"DamagedPassport with ID {id} not found.");
                }

                return NoContent(); // 204 No Content, as the passport has been deleted successfully
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST method for searching damaged passports with filters
        [HttpPost("search")]
        [RequirePermission("DPr")]

        public async Task<IActionResult> GetDamagedPassports([FromBody] GetDamagedPassportQuery query)
        {
            try
            {
                var result = await _mediator.Send(query);
                Response.AddPaginationHeader(result);  // Add pagination headers
                return Ok(result);  // Return the search result
            }
            catch (Exception ex)
            {
                // Handle errors and return a 500 Internal Server Error with message details
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }

        // POST method for statistics related to damaged passports
        [HttpPost("search/statistics")]
        [RequirePermission("Sp")]

        public async Task<IActionResult> GetDamagedPassportStatistics([FromBody] SearchDamagedPassportsStatisticsQuery query)
        {
            try
            {
                var result = await _mediator.Send(query);
                return Ok(result);  // Return the statistics result
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }
    }
}
