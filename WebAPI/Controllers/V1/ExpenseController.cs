using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using AnhEmMotor.Domain.Entities;
using AnhEmMotor.Domain.Enums;
using Application.Interfaces.Repositories.Expense;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;

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
        private readonly IExpenseReadRepository _expenseReadRepository;
        private readonly IExpenseInsertRepository _expenseInsertRepository;
        private readonly IExpenseDeleteRepository _expenseDeleteRepository;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpenseController"/> class.
        /// </summary>
        /// <param name="expenseReadRepository">Read repository for expenses.</param>
        /// <param name="expenseInsertRepository">Insert repository for expenses.</param>
        /// <param name="expenseDeleteRepository">Delete repository for expenses.</param>
        /// <param name="unitOfWork">Shared unit of work boundary.</param>
        public ExpenseController(
            IExpenseReadRepository expenseReadRepository,
            IExpenseInsertRepository expenseInsertRepository,
            IExpenseDeleteRepository expenseDeleteRepository,
            IUnitOfWork unitOfWork)
        {
            _expenseReadRepository = expenseReadRepository;
            _expenseInsertRepository = expenseInsertRepository;
            _expenseDeleteRepository = expenseDeleteRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Retrieves all expenses ordered by date descending.
        /// </summary>
        /// <returns>A list of expenses.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var expenses = await _expenseReadRepository.GetAllAsync();
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

            _expenseInsertRepository.Add(expense);
            await _unitOfWork.SaveChangesAsync();

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
            var expense = await _expenseReadRepository.GetByIdAsync(id);
            if (expense == null) return NotFound();

            _expenseDeleteRepository.Remove(expense);
            await _unitOfWork.SaveChangesAsync();

            return Ok();
        }
    }
}
