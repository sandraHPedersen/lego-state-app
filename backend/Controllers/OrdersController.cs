using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RedYellowGreen.Api.Data.Models;
using RedYellowGreen.Api.Data.Models.Entities;
using RedYellowGreen.Api.Hubs;

namespace RedYellowGreen.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<StateHub> _hub;

        public OrdersController(AppDbContext db, IHubContext<StateHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.ProductionOrders.OrderBy(o => o.ScheduledStart).ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var o = await _db.ProductionOrders.FindAsync(id);
            if (o == null) return NotFound();
            return Ok(o);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductionOrder order)
        {
            _db.ProductionOrders.Add(order);
            await _db.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("OrderCreated", order);
            return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProductionOrder updated)
        {
            var existing = await _db.ProductionOrders.FindAsync(id);
            if (existing == null) return NotFound();

            existing.OrderNumber = updated.OrderNumber;
            existing.EquipmentId = updated.EquipmentId;
            existing.ScheduledStart = updated.ScheduledStart;
            existing.ScheduledEnd = updated.ScheduledEnd;
            existing.Status = updated.Status;
            existing.Description = updated.Description;

            await _db.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("OrderUpdated", existing);
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _db.ProductionOrders.FindAsync(id);
            if (existing == null) return NotFound();
            _db.ProductionOrders.Remove(existing);
            await _db.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("OrderDeleted", new { id = id });
            return NoContent();
        }
    }
}
