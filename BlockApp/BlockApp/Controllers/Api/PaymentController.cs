using System;
using System.Threading.Tasks;
using BlockApp.Data.Entities;
using BlockApp.Data.Repositories;
using BlockApp.Enum;
using BlockApp.Interfaces;
using BlockApp.Models;
using BlockApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BlockApp.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    public class PaymentController : Controller
    {
        private readonly IRequestProvider _requestProvider;
        private readonly ILogger _logger;
        private readonly IMerchantRepository _merchantRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILogRepository _logRepository;
        private readonly ISubscriptionsRepository _subscriptionsRepository;

        public PaymentController(IRequestProvider requestProvider,
            ILogger<PaymentController> logger,
            IMerchantRepository merchantRepository,
            ITransactionRepository transactionRepository,
            ILogRepository logRepository, 
            ISubscriptionsRepository subscriptionsRepository)
        {
            _requestProvider = requestProvider;
            _logger = logger;
            _merchantRepository = merchantRepository;
            _transactionRepository = transactionRepository;
            _logRepository = logRepository;
            _subscriptionsRepository = subscriptionsRepository;
        }
        
        /// <summary>
        /// Create multisignature 2-of-2 transaction.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/payment/start
        ///     {
        ///         merchant_id=2fd1b93814c34cf78c33fb2fa979e66e
        ///         merchant_secret=AHuPnaslEc0Gld5DxxWm9AbI5CP94cxAllWELw5FDuY
        ///         trx_id=1zxc2Sa2231asd2312
        ///         callback_url=http://safecryptotrades.com/api/payment/TestShop
        ///         wallet_type=1
        ///     }
        ///
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> Start(TransactionModel model)
        {
            if (!System.Enum.IsDefined(typeof(WalletType), model.WalletType))
            {
                throw new ApplicationException($"Wrong wallet type with value '{model.WalletType}'.");
            }

            ShopInfoModel result = null;
            string hash = string.Empty;
            
            if (!await _merchantRepository.ValidateMerchant(model.MerchantId, model.MerchantSecret))
            {
                throw new ApplicationException($"Unable to load merchant with ID '{model.MerchantId}'.");
            }
            
            var merchant = await _merchantRepository.GetByMerchant(model.MerchantId);
            var subscriptions = await _subscriptionsRepository.GetByUserId(merchant.Id);
            
            if (subscriptions != null && !subscriptions.Paid && subscriptions.Expired < DateTime.Now)
            {
                return BadRequest("Subscription has expired.");
            }
            
            if (!string.Equals(merchant.RedirectUri, model.CallbackUrl))
            {
                return BadRequest("The redirect URI is not the same as in the merchant settings.");
            }
            
            try
            {
                result = await _requestProvider
                    .PostAsync<CheckPaymentShop, ShopInfoModel>(model.CallbackUrl, new CheckPaymentShop { TrxId = model.TrxId });
                
                hash = await _transactionRepository.CreateTransactions(result, merchant.Id, model.WalletType);

                var trx = await _transactionRepository.GetByHash(hash);
                await _logRepository.Add(trx.Id, "Transaction created.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            return RedirectToAction(nameof(PurchaserController.Index), "Purchaser", new { id = hash });
        }
        
        /// <summary>
        /// Used to test the callback url that returns the data to create the transaction.
        /// </summary>
        [HttpPost]
        public IActionResult TestShop(CheckPaymentShop model)
        {
            return Json(new ShopInfoModel
            {
                Amount = 0.002m,
                Description = "desc",
                Code = "1"
            });
        }
    }
}