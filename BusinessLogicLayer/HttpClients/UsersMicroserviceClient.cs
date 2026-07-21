using BusinessLogicLayer.DTO;
using DnsClient.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
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
        private readonly IDistributedCache _distributedCache;
        #endregion

        public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger, IDistributedCache distributedCache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _distributedCache = distributedCache;
        }

        public async Task<UserDTO> GetUserByUserID(Guid userID)
        {
            try
            {
                #region check redis cache for the user
                var cacheKey = $"user:{userID}";
                var cachedUser = await _distributedCache.GetStringAsync(cacheKey);

                // if cache hit, return the user from the cache
                if (!string.IsNullOrEmpty(cachedUser))
                    return System.Text.Json.JsonSerializer.Deserialize<UserDTO>(cachedUser)!;
                #endregion

                #region if cache miss, get the user from the users microservice and cache it
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

                // cache the user to redis
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30), // set the cache expiration time (after this time, the cache entry will be removed)
                    SlidingExpiration = TimeSpan.FromSeconds(10) // set the sliding expiration time (if the product is accessed again within this time, the cache expiration will be extended)
                };
                var serializedUser = System.Text.Json.JsonSerializer.Serialize(user);
                await _distributedCache.SetStringAsync(cacheKey, serializedUser, cacheOptions);

                return user!;
                #endregion
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
            catch (TimeoutRejectedException)
            {
                _logger.LogError($"Request timed out. Unable to retrieve user with ID {userID}.");
                return new UserDTO
                (
                    Guid.Empty,
                    "Error retrieving user",
                    "Error retrieving user",
                    "Error retrieving user"
                );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving user with ID {userID}.");
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
