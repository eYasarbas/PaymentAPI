using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentAPI.Model;

[Table("Payments")]
public class Payment
{
    [Key] public int Id { get; set; }

    [JsonProperty("id")] public int PaymentId { get; set; }

    [JsonProperty("transactionTime")]
    [Required(ErrorMessage = "TransactionTime field is required.")]
    public DateTime? TransactionTime { get; set; }

    [JsonProperty("amount")]
    [Required(ErrorMessage = "Amount field is required.")]
    [Range(0, double.MaxValue, ErrorMessage = "Amount field must be a valid value.")]
    public decimal? Amount { get; set; }

    [JsonProperty("transactionStatus")]
    [Required(ErrorMessage = "TransactionStatus field is required.")]
    public int? TransactionStatus { get; set; }

    [JsonProperty("transactionTypes")]
    [Required(ErrorMessage = "TransactionTypes field is required.")]
    public int? TransactionTypes { get; set; }

    [JsonProperty("rrn")]
    [StringLength(20, ErrorMessage = "RRN field must be at most 20 characters long.")]
    public string? RRN { get; set; }

    [JsonProperty("provisionNo")]
    [StringLength(10, ErrorMessage = "ProvisionNo field must be at most 10 characters long.")]
    public string? ProvisionNo { get; set; }

    [JsonProperty("bankReferenceNo")]
    [StringLength(20, ErrorMessage = "BankReferenceNo field must be at most 20 characters long.")]
    public string? BankReferenceNo { get; set; }

    [JsonProperty("cardNumberMasked")]
    [RegularExpression(@"^\d{4}-\d{4}-\d{4}-\d{4}$",
        ErrorMessage = "CardNumberMasked field must have a valid card number format.")]
    public string? CardNumberMasked { get; set; }

    [JsonProperty("cardHolderFullName")]
    [StringLength(50, ErrorMessage = "CardHolderFullName field must be at most 50 characters long.")]
    public string? CardHolderFullName { get; set; }
}