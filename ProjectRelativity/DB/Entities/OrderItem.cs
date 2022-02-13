using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectRelativity.DB.Entities;

public class OrderItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int Amount { get; set; }
    
    public int ItemId { get; set; }
    public Item Item { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; }
}