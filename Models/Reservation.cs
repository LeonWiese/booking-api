using System.ComponentModel.DataAnnotations;

namespace booking_api.Models;

public class Reservation
{
    [Key] public Guid Id { get; set; }
    public virtual Hotel Hotel { get; set; }

    public Guid UserId { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    [MaxLength(1000)] public string? Comment { get; set; }
}
