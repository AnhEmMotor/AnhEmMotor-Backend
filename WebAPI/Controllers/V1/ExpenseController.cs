using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using AnhEmMotor.Domain.Entities;
using AnhEmMotor.Domain.Enums;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for managing expenses.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ExpenseController : ControllerBase
    {
        private readonly IApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpenseController"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public ExpenseController(IApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all expenses ordered by date descending.
        /// </summary>
        /// <returns>A list of expenses.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var expenses = await _context.Expenses.OrderByDescending(e => e.ExpenseDate).ToListAsync();
            return Ok(expenses);
        }

        /// <summary>
        /// Creates a new expense.
        /// </summary>
        /// <param name="expense">The expense to create.</param>
        /// <returns>The created expense.</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Expense expense)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return Ok(expense);
        }

        /// <summary>
        /// Deletes an expense by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the expense to delete.</param>
        /// <returns>An empty result if successful, or NotFound if the expense does not exist.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
