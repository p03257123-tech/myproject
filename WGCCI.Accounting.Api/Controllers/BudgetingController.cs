using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WGCCI.Accounting.Api.Data;
using WGCCI.Accounting.Api.DTOs;
namespace WGCCI.Accounting.Api.Controllers
{
    [ApiController]
    [Route("budgeting")]
    [Authorize(Roles = "ADMIN,BURSAR,ACCOUNTANT")]
    public class BudgetingController : ControllerBase
    {
        private readonly AppDbContext _db;

        public BudgetingController(AppDbContext db)
        {
            _db = db;
        }

        // Helper: safely get orgId from claims
        private bool TryGetOrgId(out int orgId)
        {
            orgId = 0;

            var orgClaim = User.FindFirst("org");
            if (orgClaim is null)
            {
                return false;
            }

            return int.TryParse(orgClaim.Value, out orgId);
        }

        [HttpPost("budget")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpsertBudget(
            [FromBody] BudgetDto dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (!TryGetOrgId(out var orgId))
            {
                return Unauthorized("Missing or invalid org claim.");
            }

            var acct = await _db.Accounts
                .FirstOrDefaultAsync(
                    a => a.OrgId == orgId && a.Code == dto.AccountCode,
                    cancellationToken);

            if (acct is null)
            {
                return NotFound("Account");
            }

            var row = await _db.Budgets
                .FirstOrDefaultAsync(
                    b => b.OrgId == orgId &&
                         b.AccountId == acct.Id &&
                         b.Period == dto.Period,
                    cancellationToken);

            bool created = false;

            if (row is null)
            {
                row = new Budget
                {
                    OrgId = orgId,
                    AccountId = acct.Id,
                    Period = dto.Period,
                    Amount = dto.Amount
                };

                _db.Budgets.Add(row);
                created = true;
            }
            else
            {
                row.Amount = dto.Amount;
            }

            await _db.SaveChangesAsync(cancellationToken);

            var result = new
            {
                status = "ok",
                action = created ? "created" : "updated",
                row.Id,
                row.OrgId,
                row.AccountId,
                row.Period,
                row.Amount
            };

            if (created)
            {
                // You can add a proper Location URI if you have a GET endpoint
                return Created(string.Empty, result);
            }

            return Ok(result);
        }

        [HttpPost("forecast")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpsertForecast(
            [FromBody] ForecastDto dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (!TryGetOrgId(out var orgId))
            {
                return Unauthorized("Missing or invalid org claim.");
            }

            var acct = await _db.Accounts
                .FirstOrDefaultAsync(
                    a => a.OrgId == orgId && a.Code == dto.AccountCode,
                    cancellationToken);

            if (acct is null)
            {
                return NotFound("Account");
            }

            var row = await _db.Forecasts
                .FirstOrDefaultAsync(
                    f => f.OrgId == orgId &&
                         f.AccountId == acct.Id &&
                         f.Period == dto.Period,
                    cancellationToken);

            bool created = false;

            if (row is null)
            {
                row = new Forecast
                {
                    OrgId = orgId,
                    AccountId = acct.Id,
                    Period = dto.Period,
                    Amount = dto.Amount,
                    Method = dto.Method
                };

                _db.Forecasts.Add(row);
                created = true;
            }
            else
            {
                row.Amount = dto.Amount;
                row.Method = dto.Method;
            }

            await _db.SaveChangesAsync(cancellationToken);

            var result = new
            {
                status = "ok",
                action = created ? "created" : "updated",
                row.Id,
                row.OrgId,
                row.AccountId,
                row.Period,
                row.Amount,
                row.Method
            };

            if (created)
            {
                return Created(string.Empty, result);
            }

            return Ok(result);
        }
    }
}