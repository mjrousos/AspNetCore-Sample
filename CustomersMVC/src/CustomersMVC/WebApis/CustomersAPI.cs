using CustomersMVC.Customers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CustomersMVC.CustomersAPI
{
    public class CustomersAPIService
    {
        private readonly HttpClient _client;

        public CustomersAPIService(HttpClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Returns Json from the WebAPI calls made to the CustomersAPI
        /// </summary>
        /// <param name="query">the webapi action to run</param>
        public async Task<T> GetJsonDataAsync<T>(string query)
        {
            using (var response = await _client.GetAsync(CreateFullUrl(query)))
            {
                if (!response.IsSuccessStatusCode)
                {
                    return default(T);
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(content);
                return result;
            }
        }

        /// <summary>
        /// Creates the full URL for the query to be processed by putting the ApiPort Url together with the query
        /// </summary>
        /// <param name="query">the apiportservice webapi action to be combined with the apiport URL</param>
        private Uri CreateFullUrl(string query)
        {
            var result = new Uri(_client.BaseAddress, query);
            return result;
        }

        /// <summary>
        /// Returns a list of customers
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<CustomerEntity>> GetCustomersListAsync()
        {
            string query = "/Api/Customers";
            var data = await GetJsonDataAsync<IEnumerable<CustomerEntity>>(query);

            return data.OrderBy(o => o.LastName);
        }
    }
}

