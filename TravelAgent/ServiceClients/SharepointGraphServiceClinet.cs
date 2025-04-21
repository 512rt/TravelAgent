using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Net.Http.Headers;
using TravelAgent.Models.DTOs;
using TravelAgent.ServiceClients.Interfaces;
using TravelAgent.Services.Interfaces;

namespace TravelAgent.ServiceClients
{
    public class SharepointGraphServiceClinet : ISharepointGraphServiceClinet
    {
        private readonly GraphServiceClient _graphClient;

        public SharepointGraphServiceClinet(IGraphAuthProvider authProvider)
        {
            _graphClient = new GraphServiceClient(authProvider.GetTokenCredential());
        }

        #region Lists CRUD
        
        public async Task<List?> CreateListAsync(string siteId, string listName)
        {
            var list = new List
            {
                DisplayName = listName,
                Columns = new List<ColumnDefinition>
                {
                    new ColumnDefinition { Name = "Title", Text = new TextColumn() }
                }
            };

            return await _graphClient.Sites[siteId].Lists.PostAsync(list);
        }

        public async Task<IEnumerable<List>?> GetListsAsync(string siteId)
        {
            var lists = await _graphClient.Sites[siteId].Lists.GetAsync();
         
            return lists?.Value;
        }

        public async Task DeleteListAsync(string siteId, string listId)
        {
            await _graphClient.Sites[siteId].Lists[listId].DeleteAsync();
        }

        public async Task<ListItem?> CreateListItemAsync(string siteId, string listId, Dictionary<string, object> fields)
        {
            var item = new ListItem
            {
                Fields = new FieldValueSet
                {
                    AdditionalData = fields
                }
            };

            return await _graphClient.Sites[siteId].Lists[listId].Items.PostAsync(item);
        }

        public async Task<IEnumerable<TravelListItemDto>> GetListItemsAsync(string siteId, string listId)
        {
            var items = await _graphClient.Sites[siteId].Lists[listId].Items.GetAsync(requestConfig =>
            {
                requestConfig.QueryParameters.Expand = new[] { "fields" };
            });

            var result = new List<TravelListItemDto>();

            if (items?.Value != null)
            {
                foreach (var item in items.Value)
                {
                    var fields = item.Fields?.AdditionalData;

                    if (fields != null)
                    {
                        result.Add(new TravelListItemDto
                        {
                            LocationName = fields.ContainsKey("LocationName") ? fields["LocationName"]?.ToString() : null,
                            Country = fields.ContainsKey("Country") ? fields["Country"]?.ToString() : null,
                            Status = fields.ContainsKey("Status") ? fields["Status"]?.ToString() : null
                        });
                    }
                }
            }

            return result;
        }

        public async Task DeleteListItemAsync(string siteId, string listId, string itemId)
        {
            await _graphClient.Sites[siteId].Lists[listId].Items[itemId].DeleteAsync();
        }


        #endregion

        
    }
}

