using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using booking_api.Database;
using booking_api.Models;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReservationWithoutHotel))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ReservationWithoutHotel>>> GetReservations(
        Guid hotelId)
    {
        var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var query = from r in db.Reservations
            where r.Hotel.Id == hotelId
                  && r.UserId == userId
            select new ReservationWithoutHotel(r.Id, r.From, r.To, r.Comment);

        return await query.ToListAsync();
    }

    [HttpGet("{reservationId}")]
    [Authorize]
    public async Task<ActionResult<ReservationWithoutHotel>> GetReservation(
        Guid hotelId,
        Guid reservationId)
    {
        var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var query = from r in db.Reservations
            where r.Id == reservationId
                  && r.Hotel.Id == hotelId
                  && r.UserId == userId
            select new ReservationWithoutHotel(r.Id, r.From, r.To, r.Comment);

        var reservation = await query.FirstAsync();
        return reservation == null ? NotFound() : reservation;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Reservation))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PostReservation(
        Guid hotelId,
        [FromBody] CreateReservation reservation)
    {
        var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

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
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteReservation(
        Guid hotelId,
        Guid reservationId)
    {
        var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

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
    [Authorize]
    public async Task<ActionResult<List<ReservationWithoutHotel>>> GetUserReservations()
    {
        var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var query = from r in db.Reservations
            where r.UserId == userId
            select new ReservationWithoutHotel(r.Id, r.From, r.To, r.Comment);

        return await query.ToListAsync();
    }
}
