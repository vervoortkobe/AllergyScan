using AllergyScan.BLL.DTO;
using AllergyScan.BLL.Interfaces;
using AllergyScan.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace AllergyScan.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ScansController : ControllerBase
    {
        private const long MaxUploadBytes = 15 * 1024 * 1024;

        private readonly IScanService _scanService;

        public ScansController(IScanService scanService)
        {
            _scanService = scanService;
        }

        /// <summary>
        /// Stores a scan that was already processed by the app (OCR, translation and
        /// allergen check). The backend does not analyze the image or text.
        /// </summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(MaxUploadBytes)]
        public async Task<ActionResult<ScanHistoryDTO>> Create(
            [FromForm] CreateScanForm form,
            CancellationToken cancellationToken)
        {
            await using var imageStream = form.Image?.OpenReadStream();

            var request = new CreateScanRequest(
                imageStream,
                form.Image?.ContentType,
                form.Image?.FileName,
                form.Text,
                form.Flag!.Value);

            var result = await _scanService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>Returns the scan history, newest first. Images are not included.</summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<ScanHistoryDTO>>> GetHistory(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            return Ok(await _scanService.GetHistoryAsync(page, pageSize, cancellationToken));
        }

        /// <summary>Returns one scan without the image bytes.</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ScanHistoryDTO>> GetById(int id, CancellationToken cancellationToken)
        {
            return Ok(await _scanService.GetByIdAsync(id, cancellationToken));
        }

        /// <summary>Returns the image of a scan.</summary>
        [HttpGet("{id:int}/image")]
        public async Task<IActionResult> GetImage(int id, CancellationToken cancellationToken)
        {
            var (data, contentType, fileName) = await _scanService.GetImageAsync(id, cancellationToken);
            return File(data, contentType, fileName, enableRangeProcessing: true);
        }

        /// <summary>Corrects the stored text or updates the flag of a scan.</summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ScanHistoryDTO>> Update(
            int id,
            [FromBody] UpdateScanRequest request,
            CancellationToken cancellationToken)
        {
            return Ok(await _scanService.UpdateAsync(id, request, cancellationToken));
        }

        /// <summary>Replaces the image of a scan.</summary>
        [HttpPut("{id:int}/image")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(MaxUploadBytes)]
        public async Task<ActionResult<ScanHistoryDTO>> UpdateImage(
            int id,
            IFormFile? image,
            CancellationToken cancellationToken)
        {
            if (image is null)
            {
                return BadRequest(new { message = "An image is required." });
            }

            await using var stream = image.OpenReadStream();
            return Ok(await _scanService.UpdateImageAsync(id, stream, image.ContentType, image.FileName, cancellationToken));
        }

        /// <summary>Deletes one scan.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _scanService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }

        /// <summary>Deletes the complete scan history.</summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteAll(CancellationToken cancellationToken)
        {
            await _scanService.DeleteAllAsync(cancellationToken);
            return NoContent();
        }
    }
}
