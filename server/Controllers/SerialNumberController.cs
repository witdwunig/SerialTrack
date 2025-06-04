using Microsoft.AspNetCore.Mvc;
using server.Data;
using server.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SerialNumberAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SerialNumberController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SerialNumberController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("generate/{product}")]
        public async Task<IActionResult> GenerateSerialNumber([FromRoute] string product)
        {
            if (string.IsNullOrEmpty(product))
            {
                return BadRequest("Wymagany prefix");
            }
            var newSerial = new SerialNumber
            {
                Name = product,
                Number = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 12)
            };

            _context.SerialNumbers.Add(newSerial);
            await _context.SaveChangesAsync();

            return Ok(newSerial);
        }
        [HttpGet]
        public IActionResult GetAllSerialNumbers()
        {
            var serials = _context.SerialNumbers.ToList();
            return Ok(serials);
        }

        [HttpDelete("{number}")]
        public async Task<IActionResult> DeleteSerialNumber(string number)
        {
            var serial = await _context.SerialNumbers.FirstOrDefaultAsync(s => s.Number == number);
            if (serial == null)
            {
                return NotFound();
            }

            _context.SerialNumbers.Remove(serial);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content is typically returned for successful DELETE
        }

        // Add this for full UPDATE functionality (PUT)
        [HttpPut("{number}")]
        public async Task<IActionResult> UpdateSerialNumber(string number, SerialNumber updatedSerial)
        {
            if (number != updatedSerial.Number)
            {
                return BadRequest("ID mismatch");
            }

            var existingSerial = await _context.SerialNumbers.FirstOrDefaultAsync(s => s.Number == number);
            if (existingSerial == null)
            {
                return NotFound();
            }

            // Update all properties
            existingSerial.Name = updatedSerial.Name;
            existingSerial.Number = updatedSerial.Number;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SerialNumberExists(number))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // 204 No Content is typical for successful PUT
        }
        private bool SerialNumberExists(string number)
        {
            return _context.SerialNumbers.Any(e => e.Number == number);
        }
    }
}