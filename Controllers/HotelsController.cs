using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using booking_api.Database;
using booking_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace booking_api.Controllers;

public record HotelWithoutId(string Name, string Location);

public record RealmAccess(string[] roles);

[Route("/hotels")]
[Produces("application/json")]
public class HotelsController(AppDbContext db) : Controller
{
    [HttpGet]
    public async Task<List<Hotel>> GetHotels()
    {
        return await db.Hotels.ToListAsync();
    }

    [HttpGet("{hotelId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Hotel>> GetHotel(Guid hotelId)
    {
        var hotel = await db.Hotels.FindAsync(hotelId);
        if (hotel == null)
        {
            return NotFound();
        }

        return hotel;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Hotel))]
    public async Task<IActionResult> PostHotel([FromBody] HotelWithoutId hotel)
    {
        var roles = JsonSerializer.Deserialize<RealmAccess>(User.FindFirstValue("realm_access") ?? "{}")?.roles;
        if (roles == null || !roles.Contains("create-hotels"))
        {
            return Unauthorized(new { Message = "You do not have the required roles" });
        }

        var hotelId = Guid.NewGuid();
        var newHotel = new Hotel
        {
            Id = hotelId,
            Name = hotel.Name,
            Location = hotel.Location
        };

        db.Hotels.Add(newHotel);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetHotel), new { hotelId }, hotel);
    }

    [HttpDelete("{hotelId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteHotels(Guid hotelId)
    {
        var roles = JsonSerializer.Deserialize<RealmAccess>(User.FindFirstValue("realm_access") ?? "{}")?.roles;
        if (roles == null || !roles.Contains("delete-hotels"))
        {
            return Unauthorized(new { Message = "You do not have the required roles" });
        }

        var rowsDeleted = await db.Hotels
            .Where(hotel => hotel.Id == hotelId)
            .ExecuteDeleteAsync();
        return rowsDeleted == 0 ? NotFound() : Ok();
    }

    [HttpGet("search")]
    public async Task<List<Hotel>> SearchHotels([Required] string query)
    {
        var queryParts = query.Split(' ');
        var hotel = db.Hotels
            .Where(hotel =>
                queryParts.Any(q => hotel.Name.Contains(q))
                || queryParts.Any(q => hotel.Location.Contains(q))
            );

        return await hotel.ToListAsync();
    }
}
