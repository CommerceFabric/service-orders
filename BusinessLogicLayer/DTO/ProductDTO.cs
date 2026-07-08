using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.DTO
{
    /// <summary>
    /// Represents the response DTO for a product, containing essential product information.
    /// </summary>
    /// <param name="ProductID"></param>
    /// <param name="ProductName"></param>
    /// <param name="Category"></param>
    /// <param name="UnitPrice"></param>
    /// <param name="QuantityInStock"></param>
    public record ProductDTO
    (
        Guid ProductID,
        string? ProductName,
        string? Category,
        double? UnitPrice,
        int? QuantityInStock
    );
}
