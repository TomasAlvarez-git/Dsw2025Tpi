using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
    public record OrderItemModel
    {
        public record Request(
        Guid ProductId,
        int Quantity,
        string Name,
        string Description,
        decimal CurrentUnitPrice
        );

        public record Response(
        Guid ProductId,
        string? Name,
        string? Description,
        decimal? UnitPrice,
        int? Quantity,
        decimal? Subtotal
        );
    }
}
