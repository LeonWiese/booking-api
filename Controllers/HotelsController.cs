using booking_api.Database;
using booking_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace booking_api.Controllers;

public record HotelWithoutId(string Name, string Location);

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
    public async Task<IActionResult> PostHotel([FromBody] HotelWithoutId hotel)
    {
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
}






