using Microsoft.AspNetCore.Mvc;
using RedYellowGreen.Api.Data.Models.Entities;
using RedYellowGreen.Api.Interfaces;

namespace RedYellowGreen.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentController : ControllerBase
    {
        private readonly IEquipmentService _service;

        public EquipmentController(IEquipmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var eq = await _service.GetByIdAsync(id);
            if (eq == null) return NotFound();
            return Ok(eq);
        }

        [HttpGet("{id}/history")]
        public async Task<IActionResult> History(int id)
        {
            var h = await _service.GetHistoryAsync(id);
            return Ok(h);
        }

        [HttpGet("{id}/orders")]
        public async Task<IActionResult> Orders(int id)
        {
            var orders = await _service.GetOrdersAsync(id);
            return Ok(orders);
        }

        [HttpGet("{id}/current-order")]
        public async Task<IActionResult> CurrentOrder(int id)
        {
            var current = await _service.GetCurrentOrderAsync(id);
            if (current == null) return NoContent();
            return Ok(current);
        }

        [HttpGet("{id}/supervisor-overview")]
        public async Task<IActionResult> SupervisorOverview(int id)
        {
            var states = await _service.GetHistoryAsync(id);
            var orders = await _service.GetOrdersAsync(id);
            return Ok(new { equipmentId = id, states, orders });
        }

        // PATCH endpoint for partial update of equipment state (preferred REST semantics)
        public record PatchStateRequest(ProductionState NewState, string ChangedBy, string? IdempotencyKey = null);

        [HttpPatch("{id}/state")]
        public async Task<IActionResult> PatchState(int id, PatchStateRequest req)
        {
            var eq = await _service.UpdateStateAsync(id, req.NewState, req.ChangedBy);
            if (eq == null) return NotFound();
            return Ok(eq);
        }

    }
}
