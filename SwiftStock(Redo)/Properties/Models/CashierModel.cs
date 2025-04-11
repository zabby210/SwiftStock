using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace SwiftStock.Models
{
    // This class can be used to define properties for the Cashier page
    public class CashierModel : PageModel
    {
        public List<InventoryItem> InventoryItems { get; set; } = new();
    }
}