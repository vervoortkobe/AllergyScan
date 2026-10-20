using AllergyScan.BLL.DTO;
using AllergyScan.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AllergyScan.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LanguagesController : ControllerBase
    {
        private readonly ILanguageService _languageService;

        public LanguagesController(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        /// <summary>Returns the languages the app supports for translation.</summary>
        [HttpGet]
        public async Task<ActionResult<List<LanguageDTO>>> GetAll(CancellationToken cancellationToken)
        {
            return Ok(await _languageService.GetAllAsync(cancellationToken));
        }

        /// <summary>Returns the currently selected language. There is always one.</summary>
        [HttpGet("current")]
        public async Task<ActionResult<LanguageDTO>> GetCurrent(CancellationToken cancellationToken)
        {
            return Ok(await _languageService.GetCurrentAsync(cancellationToken));
        }

        /// <summary>Selects the language the app uses for translation.</summary>
        [HttpPut("current")]
        public async Task<ActionResult<LanguageDTO>> SetCurrent(
            [FromBody] SetLanguageRequest request,
            CancellationToken cancellationToken)
        {
            return Ok(await _languageService.SetCurrentAsync(request, cancellationToken));
        }
    }
}
