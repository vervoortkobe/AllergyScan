using AllergyScan.BLL.DTO;
using AllergyScan.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AllergyScan.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AllergiesController : ControllerBase
    {
        private readonly IAllergyService _allergyService;

        public AllergiesController(IAllergyService allergyService)
        {
            _allergyService = allergyService;
        }

        /// <summary>Returns all allergies of the user.</summary>
        [HttpGet]
        public async Task<ActionResult<List<AllergyDTO>>> GetAll(CancellationToken cancellationToken)
        {
            return Ok(await _allergyService.GetAllAsync(cancellationToken));
        }

        /// <summary>Returns one allergy.</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AllergyDTO>> GetById(int id, CancellationToken cancellationToken)
        {
            return Ok(await _allergyService.GetByIdAsync(id, cancellationToken));
        }

        /// <summary>Adds an allergy to the user's list.</summary>
        [HttpPost]
        public async Task<ActionResult<AllergyDTO>> Create(
            [FromBody] CreateAllergyRequest request,
            CancellationToken cancellationToken)
        {
            var allergy = await _allergyService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = allergy.Id }, allergy);
        }

        /// <summary>Renames an allergy.</summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<AllergyDTO>> Update(
            int id,
            [FromBody] UpdateAllergyRequest request,
            CancellationToken cancellationToken)
        {
            return Ok(await _allergyService.UpdateAsync(id, request, cancellationToken));
        }

        /// <summary>Removes an allergy from the user's list.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _allergyService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
