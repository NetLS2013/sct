namespace BlockApp.Services
{
    using System;
    using System.IO;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;

    using BlockApp.Helpers;
    using BlockApp.Interfaces;

    using Nethereum.HdWallet;
    using Nethereum.Hex.HexConvertors.Extensions;
    using Nethereum.Hex.HexTypes;
    using Nethereum.RPC.Eth.DTOs;
    using Nethereum.Web3;
    using Nethereum.Web3.Accounts;

    public class EthereumService : IEthereumService
    {
        public EthereumService(string network, string words, string password)
        {
            this.Network = network;
            this.Password = password;
            this.Words = words;
        }

        public string Network { get; set; }
        public string Words { get; set; }
        public string Password { get; set; }

        private int RequiredSign = 2;

        private string AbiPath = "Solidity/bin/MultiSig.abi";
        private string Contract = "Solidity/bin/MultiSig.bin";

        public async Task<Tuple<string, string>> DeployContract(int id, string address1, string address2, decimal amount1, decimal amount2, decimal totalAmount, HexBigInteger fee, string address, string pass)
        {
            var wallet = new Wallet(Words, Password);
            var account = wallet.GetAccount(id);
            var web3 = new Web3(account, this.Network);

            var abi = File.ReadAllText(this.AbiPath);
            var contract = File.ReadAllText(this.Contract);
            
            var estimateGas = await web3.Eth.DeployContract.EstimateGasAsync(abi, contract, address, new string[] { address1, address2 }, new[] { CurrencyConverter.EtherToWei(amount1), CurrencyConverter.EtherToWei(amount2) }, new BigInteger(this.RequiredSign), CurrencyConverter.EtherToWei(totalAmount));
            BigInteger price =  fee.Value / estimateGas;
            var contractDeploy = await web3.Eth.DeployContract.SendRequestAsync(abi, contract, address, estimateGas, new HexBigInteger(price), new HexBigInteger(new BigInteger(Decimal.Zero)), new string[] { address1, address2 }, new [] { CurrencyConverter.EtherToWei(amount1), CurrencyConverter.EtherToWei(amount2) }, new BigInteger(this.RequiredSign), CurrencyConverter.EtherToWei(totalAmount));

            var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(contractDeploy);
            try
            {
                while (receipt == null)
                {
                    Thread.Sleep(5000);
                    receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(contractDeploy);
                }
            }
            catch (Exception e)
            {
                return null;
            }
            
            var contractAddress = receipt.ContractAddress;
            
            return new Tuple<string, string>(contractAddress, abi);
        }

        public async Task<HexBigInteger> EstimateGasSize(int id, string address1, string address2, decimal amount1, decimal amount2, decimal totalAmount, string address, string pass)
        {
            var wallet = new Wallet(Words, Password);
            var account = wallet.GetAccount(id);
            var web3 = new Web3(account, this.Network);

            var abi = File.ReadAllText(this.AbiPath);
            var contract = File.ReadAllText(this.Contract);

            var price = await web3.Eth.GasPrice.SendRequestAsync();
            return new HexBigInteger(System.Numerics.BigInteger.Multiply((await web3.Eth.DeployContract.EstimateGasAsync(abi, contract, address, new string[] { address1, address2 }, new[] { CurrencyConverter.EtherToWei(amount1), CurrencyConverter.EtherToWei(amount2) }, new BigInteger(this.RequiredSign), CurrencyConverter.EtherToWei(totalAmount))).Value, price.Value));
        }

        public async Task<Tuple<string, string>> CreateAddress(int id)
        {
            var wallet = new Wallet(Words, Password);
            
            var account = wallet.GetAccount(id);
            
            return new Tuple<string, string>(account.Address, account.PrivateKey);
        }
        
        public async Task<Tuple<string, string>> CreateAddress(int id, string words, string seedPassword)
        {
            var wallet = new Wallet(words, seedPassword);
            var account = wallet.GetAccount(id);
            
            return new Tuple<string, string>(account.Address, account.PrivateKey);
        }

        public async Task<HexBigInteger> GetBalance(string address)
        {
            var web3 = new Web3(this.Network);
            return await web3.Eth.GetBalance.SendRequestAsync(address);
        }

        public string GetABI()
        {
            return File.ReadAllText(this.AbiPath);
        }
    }
}