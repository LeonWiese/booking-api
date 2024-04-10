using System.ComponentModel.DataAnnotations;
using booking_api.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace booking_api.Controllers;

public record ReservationWithoutHotel(Guid Id, DateTime From, DateTime To, string? Comment);

[Route("/hotels/{hotelId}/reservations")]
public class ReservationsController(AppDbContext db) : Controller
{
    [HttpGet]
    public async Task<ActionResult<List<ReservationWithoutHotel>>> GetReservations(
        Guid hotelId,
        [FromHeader(Name = "x-user-id")] [Required]
        Guid? userId)
    {
        if (userId == null)
        {
            return Unauthorized();
        }

        var query = from r in db.Reservations
            where r.Hotel.Id == hotelId
                  && r.UserId == userId
            select new ReservationWithoutHotel(r.Id, r.From, r.To, r.Comment);

        return await query.ToListAsync();
    }
}
