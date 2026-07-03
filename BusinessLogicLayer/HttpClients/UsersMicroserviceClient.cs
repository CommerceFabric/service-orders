using BusinessLogicLayer.DTO;
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
        #endregion

        public UsersMicroserviceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserDTO> GetUserByUserID(Guid userID)
        {
            var response = await _httpClient.GetAsync($"api/users/{userID}");
            
            if(response.StatusCode == System.Net.HttpStatusCode.NotFound) return null; // if the user is not found, return null
            else if (!response.IsSuccessStatusCode) throw new HttpRequestException($"Error retrieving user with ID {userID}: {response.ReasonPhrase}"); // if any other error occurs, throw an exception

            var user = await response.Content.ReadFromJsonAsync<UserDTO>();
            return user;
        }
    }
}
