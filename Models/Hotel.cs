using System.ComponentModel.DataAnnotations;

namespace booking_api.Models;

public class Hotel
{
    [Key] public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Location { get; set; } = "";
}
