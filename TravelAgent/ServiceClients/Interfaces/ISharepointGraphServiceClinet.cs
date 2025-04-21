using Microsoft.Graph.Models;
using TravelAgent.Models.DTOs;

namespace TravelAgent.ServiceClients.Interfaces
{
    public interface ISharepointGraphServiceClinet
    {
        Task<List> CreateListAsync(string siteId, string listName);
        Task<IEnumerable<List>> GetListsAsync(string siteId);
        Task DeleteListAsync(string siteId, string listId);
        Task<ListItem> CreateListItemAsync(string siteId, string listId, Dictionary<string, object> fields);
        Task<IEnumerable<TravelListItemDto>> GetListItemsAsync(string siteId, string listId);
        Task DeleteListItemAsync(string siteId, string listId, string itemId);

    }
}
