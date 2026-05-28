using FluentValidation;
using PurchaseTransactions.Application.Models;

namespace PurchaseTransactions.Application.Services
{
	public sealed class CreatePurchaseTransactionRequestValidator : AbstractValidator<CreatePurchaseTransactionRequest>
	{
		public CreatePurchaseTransactionRequestValidator()
		{
			RuleFor(x => x.Description)
				.NotEmpty()
				.MaximumLength(50).WithMessage("Description must not exceed 50 characters");

			RuleFor(x => x.TransactionDate)
				.NotEmpty()
				.Must(date => date <= DateTime.UtcNow).WithMessage("Transaction date cannot be in the future");

			RuleFor(x => x.Amount)
				.GreaterThan(0).WithMessage("Purchase Amount must be greater than zero")
				.Must(amount => amount == Math.Round(amount, 2)).WithMessage("Purchase Amount must have at most 2 decimal places");
		}
	}

	public sealed class SearchPurchaseTransactionsRequestValidator : AbstractValidator<SearchPurchaseTransactionsRequest>
	{
		public SearchPurchaseTransactionsRequestValidator()
		{
			RuleFor(x => x.Currency)
				.NotEmpty();
		}
	}

	public sealed class GetPurchaseTransactionRequestValidator : AbstractValidator<GetPurchaseTransactionRequest>
	{
		public GetPurchaseTransactionRequestValidator()
		{
			RuleFor(x => x.Currency)
				.NotEmpty();
			RuleFor(x => x.TransactionId)
				.NotEmpty();
		}
	}
}
