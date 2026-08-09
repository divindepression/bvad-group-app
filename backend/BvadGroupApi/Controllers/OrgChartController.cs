using BvadGroupApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BvadGroupApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Tags("🌳 Organigramme")]
    public class OrgChartController : ControllerBase
    {
        private readonly IOrgChartService _service;

        public OrgChartController(IOrgChartService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retourne l'arbre hiérarchique d'une filiale.
        /// </summary>
        [HttpGet("{companyId:guid}")]
        public async Task<IActionResult> GetOrgChart(Guid companyId)
        {
            var chart = await _service.GetOrgChartAsync(companyId);
            return Ok(chart);
        }
    }
}