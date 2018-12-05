using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BlockApp.Data.Entities;
using BlockApp.Data.Repositories;
using BlockApp.Enum;
using BlockApp.Extensions;
using BlockApp.Helpers;
using BlockApp.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BlockApp.Models;
using BlockApp.Models.ManageViewModels;
using Microsoft.AspNetCore.Http.Extensions;

namespace BlockApp.Controllers
{
    [Route("[controller]/[action]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class PurchaserController : Controller
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBitcoinService _bitcoinService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMerchantRepository _merchantRepository;
        private readonly ILogger _logger;
        private readonly ILogRepository _logRepository;
        private readonly IMapper _mapper;
        private readonly IEthereumService _ethereumService;
        private readonly IEmailSender _emailSender;

        public PurchaserController(ITransactionRepository transactionRepository,
            IBitcoinService bitcoinService,
            UserManager<ApplicationUser> userManager,
            IMerchantRepository merchantRepository,
            ILogger<PurchaserController> logger, ILogRepository logRepository,
            IMapper mapper,
            IEthereumService ethereumService,
            IEmailSender emailSender)
        {
            _transactionRepository = transactionRepository;
            _bitcoinService = bitcoinService;
            _userManager = userManager;
            _merchantRepository = merchantRepository;
            _logger = logger;
            _logRepository = logRepository;
            _mapper = mapper;
            _ethereumService = ethereumService;
            _emailSender = emailSender;
            
        }
        
        /// <summary>
        /// Purchaser page. Show transaction with detail by hash.
        /// </summary>
        /// <param name="id">Transaction hash.</param>
        /// <returns>Returns transaction details page and status log.</returns>
        /// <exception cref="ApplicationException">Unable to load transaction.</exception>
        [HttpGet("/Purchaser/{id}")]
        public async Task<ActionResult> Index(string id)
        {
            var trx = await _transactionRepository.GetByHash(id);
            var model = _mapper.Map<TransactionViewModel>(trx);
            
            if(trx == null)
            {
                return this.NotFound();
            }

            switch (trx.WalletType)
            {
                case WalletType.Electrum:
                    await InitializationElectrum(trx, model); break;
                    
                case WalletType.Ethereum:
                    await InitializationEthereum(trx, model); break;
            }
            
            model.Logs = _mapper.Map<IEnumerable<LogViewModel>>(_logRepository.GetByTrx(trx.Id));

            return View(trx.WalletType.ToString(), model);
        }

        /// <summary>
        /// Init ethereum transaction.
        /// </summary>
        /// <param name="trx">Transaction data model.</param>
        /// <param name="model">Transaction view model.</param>
        private async Task InitializationEthereum(Transaction trx, TransactionViewModel model)
        {
            var merchant = await _merchantRepository.GetByUser(trx.UserId);

            if (string.IsNullOrWhiteSpace(trx.Address) && !string.IsNullOrWhiteSpace(trx.XPubKey))
            {
                var account = await this._ethereumService.CreateAddress(trx.EtherId);

                trx.Address = account.Item1;
                trx.PassPhrase = account.Item2;

                var foundsWithdraw = MAD.Withdraw(trx.Amount);
                var foundsDeposit = MAD.Deposit(trx.Amount);

                var estimateGasSize = await _ethereumService.EstimateGasSize(trx.EtherId, merchant.EthereumAddress, trx.XPubKey, foundsWithdraw.Item1, foundsWithdraw.Item2, foundsDeposit.Item1,
                                          trx.Address, trx.PassPhrase);

                trx.Fee = CurrencyConverter.WeiToEther(estimateGasSize.Value);

                await this._transactionRepository.Update(trx);
            }

            if(!string.IsNullOrWhiteSpace(trx.Address))
            {
                model.ABI = this._ethereumService.GetABI();
                model.Balance = CurrencyConverter.WeiToEther((await this._ethereumService.GetBalance(trx.Address)).Value).ToString();
            }

            model.BuyerAddress = trx.XPubKey;
            model.FeeAddress = trx.Address;
            model.FeeValue = trx.Fee.ToString();
        }

        /// <summary>
        /// Init electrum transaction.
        /// </summary>
        /// <param name="trx">Transaction data model.</param>
        /// <param name="model">Transaction view model.</param>
        private async Task InitializationElectrum(Transaction trx, TransactionViewModel model)
        {
            if (trx.Status == StatusTransaction.FullDeposit && _bitcoinService.CheckInTransaction(trx.TrxDepositId, trx.RedeemScript))
            {
                var userMerchant = await _merchantRepository.GetByUser(trx.UserId);
                var founds = MAD.Withdraw(trx.Amount);
                
                var script = _bitcoinService.CreateOutTransactionElectrum(
                    trx.RedeemScript, trx.XPubKey, userMerchant.XPubKey, founds.Item1, founds.Item2);
                
                if (script != null)
                {
                    trx.UnconfirmedWithdrawTx = script;
                    trx.Status = StatusTransaction.UnconfirmedWithdraw;

                    model.UnconfirmedWithdrawTx = script;

                    await _transactionRepository.Update(trx);
                }
            }
        }

        private async Task<bool> CompareBalance(Transaction transaction)
        {
            var balance = await this._ethereumService.GetBalance(transaction.Address);
            return transaction.Fee <= CurrencyConverter.WeiToEther(balance);
        }

        /// <summary>
        /// Create electrum transaction and generate unconfirmed script.
        /// </summary>
        /// <param name="model">Multisig data.</param>
        /// <returns>Returns unconfirmed script.</returns>
        /// <exception cref="ApplicationException">Unable to load transaction.</exception>
        [HttpPost]
        public async Task<IActionResult> GenerateUnconfirmedScript([FromBody]MultisigModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault().ErrorMessage);
            }

            var trx = await _transactionRepository.GetByHash(model.Hash);

            if (trx == null)
            {
                return BadRequest($"Unable to load transaction '{model.Hash}'.");
            }
                
            var merchant = await _merchantRepository.GetByUser(trx.UserId);
            
            if (string.IsNullOrWhiteSpace(merchant.XPubKey))
            {
                return BadRequest("You can not continue to create a transaction because the seller's xPub key is incorrect.");
            }
            
            try
            {
                var founds = MAD.Deposit(trx.Amount);
                Tuple<string, string> script = null;

                script = _bitcoinService.CreateTransactionElectrum(
                    model.XPubKey, merchant.XPubKey, founds.Item1, founds.Item2);
                
                if (script != null)
                {
                    trx.XPubKey = model.XPubKey;
                    trx.UnconfirmedDepositTx = script.Item1;
                    trx.RedeemScript = script.Item2;
                    trx.Status = StatusTransaction.UnconfirmedDeposit;
                    trx.Email = model.Email;
                        
                    await _transactionRepository.Update(trx);
                }

                return Json(new { unconfirmedScript = script?.Item1 });
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                
                return this.BadRequest("Something bad happened while transaction was created.");
            }
        }

        /// <summary>
        /// Create ethereum address on which you must throw a commission to create a contract.
        /// </summary>
        /// <param name="model">Contract data.</param>
        /// <returns>Returns address and fee.</returns>
        /// <exception cref="ApplicationException">Unable to load transaction.</exception>
        [HttpPost]
        public async Task<IActionResult> SaveDataEthereum([FromBody]ContractModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault().ErrorMessage);
            }
            
            var trx = await _transactionRepository.GetByHash(model.Hash);

            if (trx == null)
            {
                return BadRequest($"Unable to load transaction '{model.Hash}'.");
            }
            
            var merchant = await _merchantRepository.GetByUser(trx.UserId);

            if (string.IsNullOrWhiteSpace(merchant.EthereumAddress))
            {
                return BadRequest("You can not continue to create a transaction because the seller's ethereum address is incorrect.");
            }

            try
            {
                TransactionViewModel indexViewModel = null;
                trx.XPubKey = model.Address;
                trx.Email = model.Email;

                indexViewModel = _mapper.Map<TransactionViewModel>(trx);

                await InitializationEthereum(trx, indexViewModel);

                return Json(new { address = indexViewModel.FeeAddress, fee = indexViewModel.FeeValue });
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return this.BadRequest("Something bad happened while transaction was created.");
            }
        }

        /// <summary>
        /// Get ethereum balance by address. 
        /// </summary>
        /// <param name="model">Contract data.</param>
        /// <returns>Returns balance.</returns>
        /// <exception cref="ApplicationException">Unable to load transaction.</exception>
        [HttpPost]
        public async Task<IActionResult> Balance([FromBody]ContractModel model)
        {
            var trx = await _transactionRepository.GetByHash(model.Hash);

            if (trx == null)
            {
                throw new ApplicationException($"Unable to load transaction '{model.Hash}'.");
            }

            var balance = await _ethereumService.GetBalance(trx.Address);
            var value = CurrencyConverter.WeiToEther(balance);
            
            return Json(new { balance = value.ToString() });
        }

        /// <summary>
        /// Deploy contract on blockchain.
        /// </summary>
        /// <param name="model">Contract data.</param>
        /// <returns>Returns contract abi and address.</returns>
        /// <exception cref="ApplicationException">Unable to load transaction.</exception>
        [HttpPost]
        public async Task<IActionResult> DeployContract([FromBody]ContractModel model)
        {
            var trx = await _transactionRepository.GetByHash(model.Hash);
            var userMerchant = await _merchantRepository.GetByUser(trx.UserId);
            var user = await _userManager.FindByIdAsync(trx.UserId);
            
            if (trx == null)
            {
                throw new ApplicationException($"Unable to load transaction '{model.Hash}'.");
            }

            var contract = new Tuple<string, string>(null, null);
            
            if (await CompareBalance(trx))
            {
                var foundsWithdraw = MAD.Withdraw(trx.Amount);
                var foundsDeposit = MAD.Deposit(trx.Amount);

                var balance = await this._ethereumService.GetBalance(trx.Address);

                contract = await _ethereumService.DeployContract(trx.EtherId, userMerchant.EthereumAddress, trx.XPubKey, foundsWithdraw.Item1, foundsWithdraw.Item2, foundsDeposit.Item1,
                                          balance, trx.Address, trx.PassPhrase);

                trx.ContractAddress = contract.Item1;
                trx.Status = StatusTransaction.Deployed;

                var message = "Buyer has deploy contract on blockchain.";
                
                await _transactionRepository.Update(trx);
                await _logRepository.Add(trx.Id, message);
                await _emailSender.SendEmailTransactionAsync(user.Email, message);
            }

            return Json(new { Abi = contract.Item2,  ContractAddress = contract.Item1 });
        }

        /// <summary>
        /// Save partially signed script(deposit) for electrum.
        /// </summary>
        /// <param name="model">Multisig data.</param>
        [HttpPost]
        public async Task SendPartiallyDepositScript([FromBody]MultisigModel model)
        {
            try
            {
                await _transactionRepository.SavePartiallyDepositScript(model.Hash, model.PartiallySignedScript);
                
                var trx = await _transactionRepository.GetByHash(model.Hash);
                var user = await _userManager.FindByIdAsync(trx.UserId);

                var message = "Buyer has confirmed deposit transaction.";
                
                await _logRepository.Add(trx.Id, message);
                await _emailSender.SendEmailTransactionAsync(user.Email, message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }
        
        /// <summary>
        /// Save partially signed script(withdraw) for electrum.
        /// </summary>
        /// <param name="model">Multisig data.</param>
        [HttpPost]
        public async Task SendPartiallyWithdrawScript([FromBody]MultisigModel model)
        {
            try
            {
                await _transactionRepository.SavePartiallyWithdrawScript(model.Hash, model.PartiallySignedScript);
                
                var trx = await _transactionRepository.GetByHash(model.Hash);
                var user = await _userManager.FindByIdAsync(trx.UserId);

                var message = "Buyer has confirmed withdraw transaction.";
                
                await _logRepository.Add(trx.Id, message);
                await _emailSender.SendEmailTransactionAsync(user.Email, message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }
    }
}