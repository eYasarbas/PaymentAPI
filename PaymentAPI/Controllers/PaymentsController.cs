using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PaymentAPI.DataAccess;
using PaymentAPI.Model;
using System.Data.Common;

namespace PaymentAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly PaymentContext _dbContext;
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentsController> _logger;


    public PaymentsController(IHttpClientFactory httpClientFactory, PaymentContext dbContext,
        ILogger<PaymentsController> logger, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("payments")]
    public async Task<ActionResult> GetPayments()
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(errors);
        }

        var apiUrl = _configuration.GetConnectionString("ApiUrl");
        var activationKey = _configuration.GetConnectionString("ActivationKey");

        var response = await _httpClient.GetAsync($"{apiUrl}?activationKey={activationKey}");

        if (response.IsSuccessStatusCode)
        {
            var jsonData = await response.Content.ReadAsStringAsync();

            var payments = JsonConvert.DeserializeObject<List<Payment>>(jsonData);


            foreach (var payment in payments)
            {
                var existingPayment = _dbContext.Payments.FirstOrDefault(p => p.PaymentId == payment.PaymentId);

                if (existingPayment == null)
                {
                    _dbContext.Payments.Add(payment);
                }
                else
                {
                    _logger.LogError("There is another data with the same ID.");

                    var errorResponse = new { message = "There is another data with the same ID. " };
                    return StatusCode(500, errorResponse);
                }
            }

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error.");

                if (ex.InnerException is DbException dbException) _logger.LogError(dbException, "Database error.");

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                throw;
            }

            return Ok(payments);
        }


        return BadRequest("Request failed");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePayment(int id, Payment payment)
    {
        if (id != payment.PaymentId)
        {
            _logger.LogWarning("Invalid request: ID in the URL does not match the ID in the request body.");
            return BadRequest("Invalid request: ID in the URL does not match the ID in the request body.");
        }

        _dbContext.Entry(payment).State = EntityState.Modified;

        try
        {
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"Payment with ID {id} has been updated.");
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PaymentExists(id))
            {
                _logger.LogWarning($"Payment with ID {id} not found.");
                return NotFound();
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the payment.");
            return StatusCode(500, new { message = "An error occurred while updating the payment." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Payment>> DeletePayment(int id)
    {
        try
        {
            var payment = await _dbContext.Payments.FindAsync(id);
            if (payment == null)
            {
                _logger.LogWarning($"Payment with ID {id} not found.");
                return NotFound();
            }

            _dbContext.Payments.Remove(payment);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Payment with ID {id} has been deleted.");
            return payment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting the payment.");
            return StatusCode(500, new { message = "An error occurred while deleting the payment." });
        }
    }

    private bool PaymentExists(int id)
    {
        return _dbContext.Payments.Any(e => e.PaymentId == id);
    }
}