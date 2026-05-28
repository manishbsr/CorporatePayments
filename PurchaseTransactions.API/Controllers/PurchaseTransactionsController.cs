using Microsoft.AspNetCore.Mvc;
using PurchaseTransactions.Application.Models;
using PurchaseTransactions.Application.Services;
using PurchaseTransactions.Domain.Models;


namespace PurchaseTransactions.API.Controllers
{

	[Route("api/[controller]")]
	[ApiController]
	public class PurchaseTransactionsController : ControllerBase
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ILogger<PurchaseTransactionsController> _logger;
		private readonly IPurchaseTransactionsService _purchaseTransactionsService;

		public PurchaseTransactionsController(IHttpContextAccessor httpContextAccessor, ILogger<PurchaseTransactionsController> logger, IPurchaseTransactionsService purchaseTransactionsService)
		{
			_httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_purchaseTransactionsService = purchaseTransactionsService ?? throw new ArgumentNullException(nameof(purchaseTransactionsService));
		}
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created, Type = typeof(List<PurchaseTransaction>))]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult> Create(
			[FromBody] CreatePurchaseTransactionRequest request,
			CancellationToken cancellationToken)
		{


			var item = await _purchaseTransactionsService
				.CreateAsync(request, cancellationToken)
				.ConfigureAwait(continueOnCapturedContext: false);

			return CreatedAtAction(nameof(Create), item);

		}
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PurchaseTransactionTargetCurrency>))]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult> Search(
			[FromQuery] SearchPurchaseTransactionsRequest request,
			CancellationToken cancellationToken)
		{


			var items = await _purchaseTransactionsService
				.SearchAsync(request, cancellationToken)
				.ConfigureAwait(continueOnCapturedContext: false);

			return Ok(items);

		}
		[HttpGet("{transactionId}")]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PurchaseTransactionTargetCurrency>))]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult> GetTransactionAsync(
			[FromRoute] string transactionId, [FromQuery] string targetCurrency,
			CancellationToken cancellationToken)
		{

			var request = new GetPurchaseTransactionRequest
			{
				TransactionId = transactionId,
				Currency = targetCurrency
			};

			var items = await _purchaseTransactionsService
				.GetTransactionAsync(request, cancellationToken)
				.ConfigureAwait(continueOnCapturedContext: false);

			return Ok(items);

		}
		

	}
}
