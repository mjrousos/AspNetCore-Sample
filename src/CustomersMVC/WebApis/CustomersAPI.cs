using CustomersMVC.Customers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Text;
using RequestCorrelation;

namespace CustomersMVC.CustomersAPI
{
    public class CustomersAPIService
    {
        private readonly HttpClient _client;

        // This application uses the X-Correlation-Id header to associated separate
        // HTTP requests which are part of the same logical operation. If a correlation ID
        // is known, it should be included in outgoing request headers.
        internal string CorrelationId
        { 
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _client.DefaultRequestHeaders.Remove(RequestCorrelationMiddleware.CorrelationHeaderName);
                    _client.DefaultRequestHeaders.Add(RequestCorrelationMiddleware.CorrelationHeaderName, value);
                }
            }
        }

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
            using (var response = await _client.GetAsync(query))
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
        /// Returns a list of customers
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<CustomerEntity>> GetCustomersListAsync()
        {
            string query = "/Api/Customers";
            var data = await GetJsonDataAsync<IEnumerable<CustomerEntity>>(query);

            return data.OrderBy(o => o.LastName);
        }

        /// <summary>
        /// Returns a list of customers
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> AddCustomerAsync(CustomerEntity customer)
        {
            string query = "/Api/Customers";
            StringContent content = new StringContent(JsonConvert.SerializeObject(customer), Encoding.UTF8, "application/json");

            try
            {
                var result = await _client.PostAsync(query, content);
                return new StatusCodeResult((int)result.StatusCode);
            }
            catch (HttpRequestException)
            {
                return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            }
        }

        /// <summary>
        /// Deletes customer
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> DeleteCustomerAsync(Guid customerId)
        {
            string query = $"/Api/Customers/{customerId}";

            try
            {
                var result = await _client.DeleteAsync(query);
                return new StatusCodeResult((int)result.StatusCode);
            }
            catch (HttpRequestException)
            {
                return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}

