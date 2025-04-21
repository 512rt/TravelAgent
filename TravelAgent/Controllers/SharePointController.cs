using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelAgent.Models.DTOs;
using TravelAgent.ServiceClients.Interfaces;

namespace TravelAgent.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/sharepoint")]
    public class SharePointController : ControllerBase
    {
        private readonly ISharepointGraphServiceClinet _sharepointGraphService;

        public SharePointController(ISharepointGraphServiceClinet sharepointGraphService)
        {
            _sharepointGraphService = sharepointGraphService;
        }

        [HttpGet("{siteId}/lists")]
        public async Task<IActionResult> GetLists(string siteId)
        {
            var lists = await _sharepointGraphService.GetListsAsync(siteId);
            return Ok(lists);
        }

        [HttpGet("{siteId}/lists/{listId}")]
        public async Task<ActionResult<IEnumerable<TravelListItemDto>>> GetLists(string siteId, string listId)
        {
            var listItems = await _sharepointGraphService.GetListItemsAsync(siteId, listId);

            return Ok(listItems);
        }

        [HttpPost("{siteId}/lists/{listId}/items")]
        public async Task<IActionResult> CreateListItem(string siteId, string listId, [FromBody] CreateTravelListItemDto dto)
        {
            var fields = new Dictionary<string, object>
            {
                { "LocationName", dto.LocationName },
                { "Country", dto.Country },
                { "Status", dto.Status }
            };

            var result = await _sharepointGraphService.CreateListItemAsync(siteId, listId, fields);

            if (result == null)
                return StatusCode(500, "Failed to create list item");

            return Ok(result);
        }
    }
}
