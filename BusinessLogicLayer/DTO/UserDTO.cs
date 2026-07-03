using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.DTO
{
    /// <summary>
    /// Define the UserDTO record which is used to encapsulate the user data, including user ID, email, person name
    /// </summary>
    /// <param name="UserID"></param>
    /// <param name="Email"></param>
    /// <param name="PersonName"></param>
    /// <param name="Gender"></param>
    public record UserDTO(
        Guid UserID,
        string? Email,
        string? PersonName,
        string? Gender
    );
}
