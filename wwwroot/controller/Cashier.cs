using System;
using System.Collections.Generic;

[Route("Cashier")]
[ApiController]
public class CashierController : Controller
{
    private static readonly List<SaleTransaction> transactions = new();

    [HttpPost("ProcessSale")]
    public IActionResult ProcessSale([FromBody] SaleTransaction transaction)
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role != "Personnel")
        {
            return Unauthorized(new { success = false, message = "Only cashiers can process sales!" });
        }

        transaction.TransactionId = transactions.Count + 1;
        transaction.TransactionDate = DateTime.Now;
        transactions.Add(transaction);

        return Ok(new { success = true, message = "Sale processed successfully!" });
    }

    [HttpGet("GetTransactions")]
    public IActionResult GetTransactions()
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role != "Admin")
        {
            return Unauthorized(new { success = false, message = "Only admins can view transactions!" });
        }

        return Ok(transactions);
    }
}

// Helper class
public class SaleTransaction
{
    public int TransactionId { get; set; }
    public string Cashier { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime TransactionDate { get; set; }
}
