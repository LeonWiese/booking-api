using System.ComponentModel.DataAnnotations;
using booking_api.Database;
using booking_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace booking_api.Controllers;

public record ReservationWithoutHotel(Guid Id, DateTime From, DateTime To, string? Comment);

public record CreateReservation(DateTime From, DateTime To, string? Comment);

[Route("/hotels/{hotelId}/reservations")]
[Produces("application/json")]
public class ReservationsController(AppDbContext db) : Controller
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReservationWithoutHotel))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    [HttpGet("{reservationId}")]
    public async Task<ActionResult<ReservationWithoutHotel>> GetReservation(
        Guid hotelId,
        Guid reservationId,
        [FromHeader(Name = "x-user-id")] [Required]
        Guid? userId)
    {
        if (userId == null)
        {
            return Unauthorized();
        }

        var query = from r in db.Reservations
            where r.Id == reservationId
                  && r.Hotel.Id == hotelId
                  && r.UserId == userId
            select new ReservationWithoutHotel(r.Id, r.From, r.To, r.Comment);

        var reservation = await query.FirstAsync();
        return reservation == null ? NotFound() : reservation;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Reservation))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PostReservation(
        Guid hotelId,
        [FromHeader(Name = "x-user-id")] [Required]
        Guid? userId,
        [FromBody] CreateReservation reservation)
    {
        if (userId == null)
        {
            return Unauthorized();
        }

        var id = Guid.NewGuid();
        var newReservation = new Reservation
        {
            Id = id,
            Hotel = db.Hotels.Single(h => h.Id == hotelId),
            UserId = (Guid)userId,
            From = reservation.From,
            To = reservation.To,
            Comment = reservation.Comment
        };
        db.Reservations.Add(newReservation);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReservation), new { hotelId, reservationId = id }, newReservation);
    }

    [HttpDelete("{reservationId}")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteReservation(
        Guid hotelId,
        Guid reservationId,
        [FromHeader(Name = "x-user-id")] [Required]
        Guid? userId)
    {
        if (userId == null)
        {
            return Unauthorized();
        }

        var rowsDeleted = await db.Reservations
            .Where(r =>
                r.Id == reservationId
                && r.UserId == userId
                && r.Hotel.Id == hotelId
            )
            .ExecuteDeleteAsync();

        return rowsDeleted == 0 ? NotFound() : Ok();
    }

    [HttpGet("/reservations")]
    public async Task<ActionResult<List<ReservationWithoutHotel>>> GetUserReservations(
        [FromHeader(Name = "x-user-id")] [Required]
        Guid? userId)
    {
        if (userId == null)
        {
            return Unauthorized();
        }

        var query = from r in db.Reservations
            where r.UserId == userId
            select new ReservationWithoutHotel(r.Id, r.From, r.To, r.Comment);

        return await query.ToListAsync();
    }
}
