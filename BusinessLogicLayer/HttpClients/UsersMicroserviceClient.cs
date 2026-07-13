using BusinessLogicLayer.DTO;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace BusinessLogicLayer.HttpClients
{
    public class UsersMicroserviceClient
    {
        #region dependencies
        private readonly HttpClient _httpClient;
        private readonly ILogger<UsersMicroserviceClient> _logger;
        #endregion

        public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<UserDTO> GetUserByUserID(Guid userID)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/users/{userID}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null; // if the user is not found, return null
                                                                                            //else if (!response.IsSuccessStatusCode) throw new HttpRequestException($"Error retrieving user with ID {userID}: {response.ReasonPhrase}"); // if any other error occurs, throw an exception
                else if (!response.IsSuccessStatusCode)
                {
                    // if any other error occurs, return Fault Data instead of an exception so that the application can continue to run and display an error message to the user
                    return new UserDTO
                    (
                        Guid.Empty,
                        "Error retrieving user",
                        "Error retrieving user",
                        "Error retrieving user"
                    );
                }

                var user = await response.Content.ReadFromJsonAsync<UserDTO>();
                return user;
            }
            catch (BrokenCircuitException)
            {
                _logger.LogError($"Circuit breaker is open. Unable to retrieve user with ID {userID}.");
                return new UserDTO
                (
                    Guid.Empty,
                    "Error retrieving user",
                    "Error retrieving user",
                    "Error retrieving user"
                );
            }
        }
    }
}
