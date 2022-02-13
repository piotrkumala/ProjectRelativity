using System.Collections.Generic;

namespace ProjectRelativity.ClientObjects;

public record Order(string UserId, List<OrderItem> OrderItems);