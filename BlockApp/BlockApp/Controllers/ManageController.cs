using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AutoMapper;
using BlockApp.Data.Entities;
using BlockApp.Data.Repositories;
using BlockApp.Enum;
using BlockApp.Extensions;
using BlockApp.Helpers;
using BlockApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BlockApp.Models;
using BlockApp.Models.ManageViewModels;
using Microsoft.Extensions.Configuration;
using Nethereum.Web3;

namespace BlockApp.Controllers
{
    using X.PagedList;

    [Authorize]
    [Route("[controller]/[action]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ManageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        private readonly IMerchantRepository _merchantRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILogRepository _logRepository;
            
        private readonly IBitcoinService _bitcoinService;
        private readonly ISubscriptionsRepository _subscriptionsRepository;
        private readonly IConfiguration _configuration;

        private readonly IEthereumService _ethereumService;

        [TempData]
        public string StatusMessage { get; set; }
        [TempData]
        public string MerchantSecretTemp { get; set; }
        
        public ManageController(
          UserManager<ApplicationUser> userManager,
          SignInManager<ApplicationUser> signInManager,
          IEmailSender emailSender,
          ILogger<ManageController> logger,
          UrlEncoder urlEncoder,
          IMerchantRepository merchantRepository,
          ITransactionRepository transactionRepository,
          ILogRepository logRepository,
          IMapper mapper,
          IBitcoinService bitcoinService,
          ISubscriptionsRepository subscriptionsRepository,
          IConfiguration configuration,
          IEthereumService ethereumService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _merchantRepository = merchantRepository;
            _transactionRepository = transactionRepository;
            _logRepository = logRepository;
            _mapper = mapper;
            _bitcoinService = bitcoinService;
            _subscriptionsRepository = subscriptionsRepository;
            _configuration = configuration;
            _ethereumService = ethereumService;
        }

        /// <summary>
        /// Account page.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="ApplicationException">
        /// </exception>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new IndexViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsEmailConfirmed = user.EmailConfirmed,
                StatusMessage = StatusMessage
            };

            return View(model);
        }

        /// <summary>
        /// Update personal info in database.
        /// </summary>
        /// <param name="model">
        /// The model of personal info.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="ApplicationException">
        /// Can be throw when user, email or phone number are invalid
        /// </exception>
        [HttpPost]
        public async Task<IActionResult> SavePersonalInfo([FromBody]PersonalViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault().ErrorMessage);
            }
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    StatusMessage = $"Unable to load user with ID '{_userManager.GetUserId(User)}'.";
                    throw new ApplicationException(StatusMessage);
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                await this._userManager.UpdateAsync(user);

                var phoneNumber = user.PhoneNumber;
                if (model.PhoneNumber != phoneNumber)
                {
                    var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                    if (!setPhoneResult.Succeeded)
                    {
                        StatusMessage = $"Unexpected error occurred setting phone number for user with ID '{user.Id}'.";
                        throw new ApplicationException(StatusMessage);
                    }
                }


            }
            catch (Exception e)
            {
                return this.BadRequest(StatusMessage);
            }

            StatusMessage = "Your profile has been updated.";
            return this.Ok(StatusMessage);
        }

        /// <summary>
        /// Send verification email to user mailbox.
        /// </summary>
        /// <param name="model">
        /// The model with email.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="ApplicationException">
        /// Can be throw when user is invalid
        /// </exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendVerificationEmail()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    StatusMessage = $"Unable to load user with ID '{_userManager.GetUserId(User)}'.";
                    throw new ApplicationException(StatusMessage);
                }
                if (user.EmailConfirmed)
                {
                    StatusMessage = $"Email already confirmed.";
                    throw new ApplicationException(StatusMessage);
                }

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                var email = user.Email;
                await _emailSender.SendEmailConfirmationAsync(email, callbackUrl);
            }
            catch (Exception e)
            {
                return this.BadRequest(StatusMessage);
            }

            StatusMessage = "Verification email sent. Please check your email.";
            return this.Ok(StatusMessage);
        }

        /// <summary>
        /// Change user password using old one.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="ApplicationException">
        /// Can be throw when user is invalid
        /// </exception>
        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault().ErrorMessage);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    StatusMessage = $"Unable to load user with ID '{_userManager.GetUserId(User)}'.";
                    throw new ApplicationException(StatusMessage);
                }

                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    AddErrors(changePasswordResult);
                    StatusMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault().ErrorMessage;
                    return this.BadRequest(StatusMessage);
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("User changed their password successfully.");
            }
            catch (Exception e)
            {
                return this.BadRequest(StatusMessage);
            }
          

            StatusMessage = "Your password has been updated.";
            return this.Ok(StatusMessage);
        }

        /// <summary>
        /// Merchant page.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="ApplicationException">
        /// Can be throw when user is invalid
        /// </exception>
        [HttpGet]
        public async Task<IActionResult> Merchant()
        {
            MerchantViewModel model = null;
            
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var userMerchant = await _merchantRepository.GetByUser(user.Id);

            if (userMerchant == null)
            {
                return View("Merchant/Create");
            }
            
            var subscriptions = await _subscriptionsRepository.GetByUserId(user.Id);
            
            if (subscriptions == null
                || subscriptions.Paid && subscriptions.Expired < DateTime.Now)
            {
                await _subscriptionsRepository.CreatePaymentAddress(user.Id);
            }

            decimal price = Convert.ToDecimal(_configuration["Subscription:" + subscriptions?.WalletType + ":Price"]);

            await Subscriptions(price);
            
            subscriptions = await _subscriptionsRepository.GetByUserId(user.Id);
            
            model = new MerchantViewModel
            {
                MerchantId = userMerchant.MerchantId,
                MerchantSecret = Crypto.GetSha256Hash(userMerchant.MerchantSecret),
                RedirectUri = userMerchant.RedirectUri,
                
                XPubKey = userMerchant.XPubKey,
                EthereumAddress = userMerchant.EthereumAddress,
                
                Status = subscriptions.Expired > DateTime.Now && subscriptions.Paid,
                Address = subscriptions.Address,
                Expired = subscriptions.Expired,
                WalletType = subscriptions.WalletType,
                Price = price,
                
                StatusMessage = StatusMessage
            };

            return View("Merchant/Index", model);
        }
        
        /// <summary>
        /// Subscribe method for update status of payment for using system.
        /// </summary>
        /// <param name="price">
        /// The price for subscription.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="ApplicationException">
        /// Can be throw when user is invalid
        /// </exception>
        [HttpPost]
        public async Task<IActionResult> Subscriptions([FromBody] decimal price)
        {
            var statusMessage = "Your subscription has not been activated.";
            
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            
            var subscriptions = await _subscriptionsRepository.GetByUserId(user.Id);
            var isPaidSubscription = subscriptions.Paid;
            
            if (!string.IsNullOrWhiteSpace(subscriptions.Address) && !subscriptions.Paid)
            {
                switch (subscriptions.WalletType)
                {
                    case WalletType.Electrum:
                    {
                        isPaidSubscription = _bitcoinService.CheckEqualsBalance(subscriptions.Address, price);
                        break;
                    }
                    case WalletType.Ethereum:
                    {
                        var balance =  await _ethereumService.GetBalance(subscriptions.Address);
                        
                        isPaidSubscription = price <= Nethereum.Util.UnitConversion.Convert.FromWei(balance.Value);
                        break;
                    }
                }

                if (isPaidSubscription)
                {
                    var expireInDays = DateTime.Now.AddDays(Convert.ToInt32(_configuration["Subscription:ExpireInDays"]));
                    
                    await _subscriptionsRepository.PaidToAddress(expireInDays, subscriptions.Id);
                    await _emailSender.SendEmailAsync(_configuration["EmailNotifcation"], "Notification:",
                        $"New user {user.Email} subscripe on the safecryptotrades.");

                    statusMessage = "Your subscription successfully activated.";
                }
            }
            
            return new JsonResult(new { Status = isPaidSubscription, StatusMessage = statusMessage });
        }

        /// <summary>
        /// Create new merchant with random id and secret when account just created.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="ApplicationException">
        /// Can be throw when user is invalid
        /// </exception>
        [HttpPost]
        public async Task<IActionResult> CreateMerchant()
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _merchantRepository.CreateMerchant(user.Id);
            
            StatusMessage = "Merchant account successfully created.";

            return RedirectToAction(nameof(Merchant));
        }

        /// <summary>
        /// Save merchant secret, when it changed.
        /// </summary>
        /// <param name="model">
        /// The model of merchant.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="ApplicationException">
        /// </exception>
        [HttpPost]
        public async Task<IActionResult> SaveMerchant([FromBody]MerchantViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault().ErrorMessage);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }

                await _merchantRepository.SaveMerchant(MerchantSecretTemp ?? model.MerchantSecret, model, user.Id);
            }
            catch (Exception e)
            {
                return BadRequest(StatusMessage);
            }
           
            StatusMessage = "Merchant account successfully saved.";
            return this.Ok(StatusMessage);
        }

        /// <summary>
        /// Generate merchant secret.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> GenerateMerchantSecret()
        {
            MerchantSecretTemp = Guid.NewGuid().ToString().Replace("-", "");
                
            return new JsonResult(new { MerchantSecret = Crypto.GetSha256Hash(MerchantSecretTemp) });
        }

        /// <summary>
        /// Generate subscribe address for payment.
        /// </summary>
        /// <param name="model">
        /// The model of subscription.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="ApplicationException">
        /// </exception>
        [HttpPost]
        public async Task<IActionResult> GenerateSubscribeAddress([FromBody]SubscribeModel model)
        {
            if (!System.Enum.IsDefined(typeof(WalletType), model.WalletType))
            {
                return BadRequest($"Wrong wallet type with value '{model.WalletType}'.");
            }
            
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            
            var subscriptions = await _subscriptionsRepository.GetByUserId(user.Id);
            
            var price = Convert.ToDecimal(_configuration["Subscription:" + model.WalletType + ":Price"]);
            var address = string.Empty;

            switch (model.WalletType)
            {
                case WalletType.Electrum:
                {
                    address = _bitcoinService.GenerateAddress(subscriptions.Id, _configuration["Subscription:Electrum:ExtPubKey"]);
                    break;
                }
                case WalletType.Ethereum:
                {
                    var wallet = await _ethereumService.CreateAddress(subscriptions.Id,
                        _configuration["Subscription:Ethereum:Words"], _configuration["Subscription:Ethereum:SeedPassword"]);
                    
                    address = wallet.Item1;
                    break;
                }
            }

            await _subscriptionsRepository.SavePaymentAddress(address, model.WalletType, subscriptions.Id);
            
            return new JsonResult(new { Address = address, Price = price });
        }

        /// <summary>
        /// Transactions page list.
        /// Allow sort, change count results and paging.
        /// </summary>
        /// <param name="sortOrder">
        /// The sort order.
        /// </param>
        /// <param name="page">
        /// Page number.
        /// </param>
        /// <param name="itemPerPage">
        /// Count item per page.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="ApplicationException">
        /// Can be throw when user is invalid
        /// </exception>
        [HttpGet]
        public async Task<IActionResult> Transactions(
            string sortOrder,
            int? page,
            int? itemPerPage)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            int currPage = (!page.HasValue || page.Value < 1) ? 1 : page.Value;
            int currItemPerPage = !itemPerPage.HasValue || itemPerPage.Value < 1 ? 5 : itemPerPage.Value;
            var model = GetTransactions(user.Id, sortOrder);

            ViewData["sortOrder"] = sortOrder;

            return View("Transactions", model.ToPagedList(currPage, currItemPerPage));
        }

        /// <summary>
        /// Concrete single transaction page.
        /// Show general info and log history
        /// </summary>
        /// <param name="id">
        /// The id of transaction.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="ApplicationException">
        /// Can be throw when user is invalid
        /// </exception>
        [HttpGet]
        public async Task<IActionResult> Transaction(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var transaction = await this._transactionRepository.Get(transaction1 => transaction1.Id == id && transaction1.UserId == user.Id);

            if (transaction == null)
            {
                return this.NotFound();
            }

            if (transaction.TrxDepositId != null && transaction.Status == StatusTransaction.PartiallyDeposit)
            {
                if (this._bitcoinService.CheckInTransaction(transaction.TrxDepositId, transaction.RedeemScript))
                {
                    transaction.Status = StatusTransaction.FullDeposit;

                    await _transactionRepository.Update(transaction);
                    await _logRepository.Add(transaction.Id, "Seller has confirmed deposit transaction and send to blockchain.");
                }
            }
            if (transaction.TrxWithdrawId != null && transaction.Status == StatusTransaction.PartiallyWithdraw)
            {
                if (this._bitcoinService.CheckOutTransaction(transaction.TrxWithdrawId, transaction.RedeemScript))
                {
                    transaction.Status = StatusTransaction.FullWithdraw;

                    await _transactionRepository.Update(transaction);
                    await _logRepository.Add(transaction.Id, "Seller has confirmed withdraw transaction and send to blockchain.");
                }
            }

            var model = _mapper.Map<TransactionViewModel>(transaction);
            model.Logs = _mapper.Map<IEnumerable<LogViewModel>>(this._logRepository.GetByTrx(transaction.Id));

            if (model.WalletType == WalletType.Ethereum && !string.IsNullOrWhiteSpace(model.ContractAddress))
            {
                model.ABI = this._ethereumService.GetABI();
            }

            return View("Transaction", model);
        }

        /// <summary>
        /// Send deposit transaction id from blockchain.
        /// Check if transaction is confirmed and valid.
        /// </summary>
        /// <param name="id">
        /// The transaction id from blockchain.
        /// </param>
        /// <param name="trxId">
        /// The transaction id from database.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="ApplicationException">
        /// Can be throw when user is invalid.
        /// </exception>
        [HttpPost]
        public async Task<IActionResult> SendDepositTrxId([FromBody]SetTransactionId setTrx)
        {
            if (!ModelState.IsValid)
            {
                return this.BadRequest();
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }

                var trx = await _transactionRepository.Get(transaction => transaction.Id == setTrx.TransactionId);

                if (trx.UserId != user.Id)
                {
                    throw new ApplicationException($"Transaction doesn't exist");
                }

                if (trx.Status != StatusTransaction.PartiallyDeposit)
                {
                    throw new ApplicationException($"Transaction id cannot be changed");
                }

                trx.TrxDepositId = setTrx.TransactionHash;
                await _transactionRepository.Update(trx);

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return this.BadRequest();
            }

            return this.Ok();
        }

        /// <summary>
        /// Send withdraw transaction id from blockchain.
        /// Check if transaction is confirmed and valid.
        /// </summary>
        /// <param name="id">
        /// The transaction id from blockchain.
        /// </param>
        /// <param name="trxId">
        /// The transaction id from database.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="ApplicationException">
        /// Can be throw when user is invalid
        /// </exception>
        [HttpPost]
        public async Task<IActionResult> SendWithdrawTrxId([FromBody]SetTransactionId setTrx)
        {
            if (!ModelState.IsValid)
            {
                return this.BadRequest();
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }

                var trx = await _transactionRepository.Get(transaction => transaction.Id == setTrx.TransactionId);

                if (trx.UserId != user.Id)
                {
                    throw new ApplicationException($"Wrong user or trx");
                }


                if (trx.Status != StatusTransaction.PartiallyWithdraw)
                {
                    throw new ApplicationException($"Transaction id cannot be changed");
                }

                trx.TrxWithdrawId = setTrx.TransactionHash;
                await _transactionRepository.Update(trx);

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return this.BadRequest();
            }

            return this.Ok();
        }

        private IEnumerable<TransactionViewModel> GetTransactions(
            string userId,
            string sortOrder)
        {
            var transactions = this._transactionRepository.GetAll().Where(transaction1 => transaction1.UserId == userId)
                .AsEnumerable();
            var model = _mapper.Map<IEnumerable<TransactionViewModel>>(transactions);

            foreach (var transaction in model)
            {
                transaction.Logs = _mapper.Map<IEnumerable<LogViewModel>>(this._logRepository.GetByTrx(transaction.Id));
                transaction.Created = transaction.Logs?.Min(viewModel => viewModel.Date) ?? DateTime.Now;
            }

            switch (sortOrder)
            {
                case "amount_desc":
                    model = model.OrderByDescending(s => s.Amount);
                    break;
                case "amount":
                    model = model.OrderBy(s => s.Amount);
                    break;
                case "date":
                    model = model.OrderBy(s => s.Created);
                    break;
                default:
                    model = model.OrderByDescending(s => s.Created);
                    break;
            }

            return model;
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        #endregion
    }
}
