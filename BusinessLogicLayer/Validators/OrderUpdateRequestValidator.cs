using BusinessLogicLayer.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Validators
{
    public class OrderUpdateRequestValidator : AbstractValidator<OrderUpdateRequest>
    {
        public OrderUpdateRequestValidator()
        {
            RuleFor(x => x.OrderID)
                .NotEmpty().WithMessage("OrderID is required.");
            
            RuleFor(x => x.UserID)
                .NotEmpty().WithMessage("UserID is required.");

            RuleFor(x => x.OrderDate)
                .NotEmpty().WithMessage("OrderDate is required.");

            RuleFor(x => x.OrderItems)
                .NotEmpty().WithMessage("OrderItems are required.")
                .Must(items => items != null && items.Count > 0).WithMessage("OrderItems must contain at least one item.");
        }
    }
}
