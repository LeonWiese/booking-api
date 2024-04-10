using booking_api.Database;
using booking_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace booking_api.Controllers;

[Route("/hotels")]
public class HotelsController(AppDbContext db) : Controller
{
    [HttpGet]
    public async Task<List<Hotel>> GetHotels()
    {
        return await db.Hotels.ToListAsync();
    }
}
