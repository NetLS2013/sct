using QBitNinja.Client.Models;

namespace BlockApp.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;

    using BlockApp.Interfaces;

    using NBitcoin;
    using NBitcoin.DataEncoders;

    using Newtonsoft.Json.Linq;

    using QBitNinja.Client;

    public class BitcoinService : IBitcoinService
    {
        private Network network;

        public BitcoinService(Network network)
        {
            this.network = network;
        }

        public bool CheckInTransaction(string trx, string redeem)
        {
            Script script = new Script(redeem);

            var client = new QBitNinjaClient(this.network);

            try
            {
                var transactionId = uint256.Parse(trx);
                var transactionResponse = client.GetTransaction(transactionId).Result;
                if (transactionResponse.Block.Confirmations >= 6 && transactionResponse?.Transaction?.Outputs.FirstOrDefault(t => t.IsTo(script.PaymentScript.GetDestinationAddress(this.network))) != null)
                {
                    return true;
                }
            }
            catch (Exception e)
            {

            }

            return false;
        }

        public bool CheckOutTransaction(string trx, string redeem)
        {
            Script script = new Script(redeem);

            var client = new QBitNinjaClient(this.network);

            try
            {
                var transactionId = uint256.Parse(trx);
                var transactionResponse = client.GetTransaction(transactionId).Result;

                if (transactionResponse.Block.Confirmations >= 6)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                
            }

            return false;
        }

        public string CreateMultiSigAddress(string pubkey1, string pubkey2)
        {
            PubKey pubKey1;
            PubKey pubKey2;

            try
            {
                pubKey1 = new PubKey(pubkey1);
                pubKey2 = new PubKey(pubkey2);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Public key is incorrect");
            }

            Script scriptPubKey =
                PayToMultiSigTemplate.Instance.GenerateScriptPubKey(
                    2,
                    pubKey1, pubKey2);

            return scriptPubKey.ToString();
        }

        public Tuple<string, string> CreateTransaction(string redeemScript, decimal amount1, decimal amount2)
        {
            if (amount1 <= 0m && amount2 <= 0m)
            {
                throw new ArgumentException("Amount is less or equal zero");
            }

            Script scriptPubKey;
            try
            {
                scriptPubKey = new Script(redeemScript);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Redeem script is incorrect");
            }

            decimal estimateFeeSize = 500m;
            var fee = this.Fee(estimateFeeSize);
            if (fee == null)
            {
                throw new Exception("Cannot get fee");
            }

            var pubKeys = scriptPubKey.GetDestinationPublicKeys();

            var pub1 = GetPubKey(pubKeys[0], Money.Coins(amount1) + fee / 2);
            var pub2 = GetPubKey(pubKeys[1], Money.Coins(amount2) + fee / 2);

            var t = new Transaction();
            t.AddOutput(Money.Coins(amount1) + Money.Coins(amount2), scriptPubKey.Hash);

            if (pub1 != null && pub2 != null)
            {
                int index = 0;

                this.AddInputs(t, pub1.Item1, pub1.Item2, ref index, Money.Coins(amount1) + fee / 2);
                this.AddInputs(t, pub2.Item1, pub2.Item2, ref index, Money.Coins(amount2) + fee / 2);

                return new Tuple<string, string>(t.ToHex(), scriptPubKey.ToString());
            }

            return null;
        }

        public string CreateOutTransactionElectrum(string redeemScript, string xpubkey1, string xpubkey2, decimal amount1, decimal amount2)
        {
            if (amount1 <= 0m && amount2 <= 0m)
            {
                throw new ArgumentException("Amount is less or equal zero");
            }

            Script scriptPubKey;
            try
            {
                scriptPubKey = new Script(redeemScript);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Redeem script is incorrect");
            }

            ExtPubKey xpubKey1;
            ExtPubKey xpubKey2;

            try
            {
                xpubKey1 = ExtPubKey.Parse(xpubkey1, Network.Main);
                xpubKey2 = ExtPubKey.Parse(xpubkey2, Network.Main);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Extended public key is incorrect");
            }

            decimal estimateFeeSize = 400m;
            var fee = this.Fee(estimateFeeSize);
            if (fee == null)
            {
                throw new Exception("Cannot get fee");
            }

            var pubKeys = scriptPubKey.GetDestinationPublicKeys();

            var index1 = this.FindPubKey(xpubKey1, pubKeys[0]);
            var index2 = this.FindPubKey(xpubKey2, pubKeys[1]);

            var unspentCoins = GetUnspentCoins(scriptPubKey);
            
            var t = new Transaction();
            t.AddOutput(Money.Coins(amount1) - fee / 2, pubKeys[0].Hash);
            t.AddOutput(Money.Coins(amount2) - fee / 2, pubKeys[1].Hash);

            Base58CheckEncoder base58CheckEncoder = new Base58CheckEncoder();
            var bytes1 = base58CheckEncoder.DecodeData(xpubKey1.GetWif(Network.Main).ToString());
            string hex1 = BitConverter.ToString(bytes1).Replace("-", "").ToLower();

            var bytes2 = base58CheckEncoder.DecodeData(xpubKey2.GetWif(Network.Main).ToString());
            string hex2 = BitConverter.ToString(bytes2).Replace("-", "").ToLower();

            // special script sig in electrum have to contain these magic vars
            var s = "0001ff01ff4cad524c53ff" + hex1 + "0000"
                    + (index1 > 9 ? index1.ToString() : "0" + index1) + "00" + "4c53ff" + hex2 + "0000"
                    + (index2 > 9 ? index2.ToString() : "0" + index2) + "00" + "52ae";

            int i = 0;
            Money spendMoney = Money.Zero;

            foreach (var coin in unspentCoins)
            {
                t.AddInput(new TxIn(coin.Key.Outpoint));
                t.Inputs[i].ScriptSig = new Script(s);
                i++;

                spendMoney += coin.Key.Amount;
                if (spendMoney >= Money.Coins(amount1) + Money.Coins(amount2))
                {
                    if (spendMoney > Money.Coins(amount1) + Money.Coins(amount2))
                    {
                        t.AddOutput(spendMoney - (Money.Coins(amount1) + Money.Coins(amount2)), scriptPubKey.Hash);
                    }

                    spendMoney = null;
                    break;
                }
            }

            if (spendMoney == null)
            {
                return t.ToHex().Replace("b64cb40001ff01ff4cad524c53ff", "b40001ff01ff4cad524c53ff");
            }

            return null;
        }
       
        public Tuple<string, string> CreateTransactionElectrum(string xpubkey1, string xpubkey2, decimal amount1, decimal amount2)
        {
            if (amount1 <= 0m && amount2 <= 0m)
            {
                throw new ArgumentException("Amount is less or equal zero");
            }

            ExtPubKey pubKey1;
            ExtPubKey pubKey2;

            try
            {
                pubKey1 = ExtPubKey.Parse(xpubkey1, Network.Main);
                pubKey2 = ExtPubKey.Parse(xpubkey2, Network.Main);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Extended public key is incorrect");
            }

            decimal estimateFeeSize = 500m;
            var fee = this.Fee(estimateFeeSize);
            if (fee == null)
            {
                throw new Exception("Cannot get fee");
            }

            var pub1 = GetPubKey(pubKey1, Money.Coins(amount1) + fee / 2);
            var pub2 = GetPubKey(pubKey2, Money.Coins(amount2) + fee / 2);

            if (pub1 != null && pub2 != null)
            {
                Script scriptPubKey =
                    PayToMultiSigTemplate.Instance.GenerateScriptPubKey(
                        2,
                        pub1.Item1, pub2.Item1);

                var t = new Transaction();
                t.AddOutput(Money.Coins(amount1) + Money.Coins(amount2), scriptPubKey.Hash);

                int index = 0;

                this.AddInputs(t, pubKey1, pub1.Item3, pub1.Item2, ref index, Money.Coins(amount1) + fee / 2);
                this.AddInputs(t, pubKey2, pub2.Item3, pub2.Item2, ref index, Money.Coins(amount2) + fee / 2);

                return new Tuple<string, string>(t.ToHex().Replace("594c5701ff4c53ff", "5701ff4c53ff"), scriptPubKey.ToString());
            }

            return null;
        }
        
        public string GenerateAddress(int id, string extPubKey)
        {
            return ExtPubKey.Parse(extPubKey).Derive((uint)id).PubKey.GetAddress(Network.TestNet).ToString();
        }
        
        public bool CheckEqualsBalance(string address, decimal amount)
        {
            var client = new QBitNinjaClient(this.network);
            
            var balance = client.GetBalance(new BitcoinPubKeyAddress(address)).Result;

            return balance.Operations.Any(o => o.Amount >= Money.Coins(amount) && o.Confirmations == 6);
        }

        private int FindPubKey(ExtPubKey xpubKey, PubKey pubKey)
        {
            var receive = xpubKey.Derive(0);

            for (int i = 0; i < 100; i++)
            {
                if (receive.Derive((uint)i).PubKey.ToHex() == pubKey.ToHex())
                {
                    return i;
                }
            }

            throw new ArgumentException("Pubkey not found");
        }

        private void AddInputs(Transaction transaction, ExtPubKey pubKey, List<Coin> coins, int addressNumber, ref int index, Money money)
        {
            Base58CheckEncoder base58CheckEncoder = new Base58CheckEncoder();
            var bytes = base58CheckEncoder.DecodeData(pubKey.GetWif(Network.Main).ToString());
            string hex = BitConverter.ToString(bytes).Replace("-", "").ToLower();

            Money spentMoney = Money.Zero;
            for (int i = 0; i < coins.Count; i++)
            {
                transaction.AddInput(new TxIn(coins[i].Outpoint));
                var s = "01ff4c53ff" + hex + "0000"
                        + (addressNumber > 9 ? addressNumber.ToString() : "0" + addressNumber) + "00";
                transaction.Inputs[i + index].ScriptSig = new Script(s);

                spentMoney += coins[i].Amount;

                if (spentMoney >= money)
                {
                    if (spentMoney - money > 0)
                    {
                        transaction.AddOutput(spentMoney - money, pubKey.Derive(0).Derive((uint)addressNumber).PubKey.Hash);
                    }
                    
                    index = i + 1;
                    return;
                }
            }

            index = coins.Count;
        }

        private void AddInputs(Transaction transaction, PubKey pubKey, List<Coin> coins, ref int index, Money money)
        {
            Money spentMoney = Money.Zero;
            for (int i = 0; i < coins.Count; i++)
            {
                transaction.AddInput(new TxIn(coins[i].Outpoint));
                transaction.Inputs[i + index].ScriptSig = new Script(pubKey.ToString());

                spentMoney += coins[i].Amount;

                if (spentMoney >= money)
                {
                    if (spentMoney - money > 0)
                    {
                        transaction.AddOutput(spentMoney - money, pubKey.ScriptPubKey);
                    }

                    index = i + 1;
                    return;
                }
            }

            index = coins.Count;
        }

        private Tuple<PubKey, int, List<Coin>> GetPubKey(ExtPubKey extPubKey, Money amount)
        {
            var receive = extPubKey.Derive(0);
            for (int i = 0; i < 100; i++)
            {
                var item = receive.Derive((uint)i).PubKey;
                var coins = this.GetUnspentCoins(item);
                this.GetBalances(coins, out Money confBalance, out Money unconfBalance);
                if (confBalance >= amount)
                {
                    return new Tuple<PubKey, int, List<Coin>>(item, i, coins.Select(pair => pair.Key).ToList());
                }
            }
            
            return null;
        }

        private Tuple<PubKey, List<Coin>> GetPubKey(PubKey pubKey, Money amount)
        {
            var coins = this.GetUnspentCoins(pubKey);
            this.GetBalances(coins, out Money confBalance, out Money unconfBalance);
            if (confBalance >= amount)
            {
                return new Tuple<PubKey, List<Coin>>(pubKey, coins.Select(pair => pair.Key).ToList());
            }

            return null;
        }

        private void GetBalances(Dictionary<Coin, bool> addressHistoryRecords, out Money confirmedBalance, out Money unconfirmedBalance)
        {
            confirmedBalance = Money.Zero;
            unconfirmedBalance = Money.Zero;
            foreach (var record in addressHistoryRecords)
            {
                if (record.Value)
                {
                    confirmedBalance += record.Key.Amount;
                }
                else
                {
                    unconfirmedBalance += record.Key.Amount;
                }
            }
        }

        private Dictionary<Coin, bool> GetUnspentCoins(IEnumerable<PubKey> pubKeys)
        {
            var unspentCoins = new Dictionary<Coin, bool>();
            foreach (var key in pubKeys)
            {
                var destination = key.ScriptPubKey.GetDestinationAddress(this.network);

                var client = new QBitNinjaClient(this.network);
                var balanceModel = client.GetBalance(destination, unspentOnly: true).Result;
                foreach (var operation in balanceModel.Operations)
                {
                    foreach (var elem in operation.ReceivedCoins.Select(coin => coin as Coin))
                    {
                        unspentCoins.Add(elem, operation.Confirmations > 0);
                    }
                }
            }

            return unspentCoins;
        }

        private Dictionary<Coin, bool> GetUnspentCoins(PubKey pubKeys)
        {
            var unspentCoins = new Dictionary<Coin, bool>();

            var destination = pubKeys.GetAddress(this.network);

            var client = new QBitNinjaClient(this.network);
            var balanceModel = client.GetBalance(destination, unspentOnly: true).Result;
            foreach (var operation in balanceModel.Operations)
            {
                foreach (var elem in operation.ReceivedCoins.Select(coin => coin as Coin))
                {
                    unspentCoins.Add(elem, operation.Confirmations > 0);
                }
            }

            return unspentCoins;
        }

        private Dictionary<Coin, bool> GetUnspentCoins(Script script)
        {
            var unspentCoins = new Dictionary<Coin, bool>();

            var client = new QBitNinjaClient(this.network);
            var balanceModel = client.GetBalance(script.PaymentScript, unspentOnly: true).Result;
            foreach (var operation in balanceModel.Operations)
            {
                foreach (var elem in operation.ReceivedCoins.Select(coin => coin as Coin))
                {
                    unspentCoins.Add(elem, operation.Confirmations > 0);
                }
            }

            return unspentCoins;
        }

        private Money Fee(decimal txSize)
        {
            Money fee = null;
            try
            {
                using (var client = new HttpClient())
                {
                    string request = @"https://bitcoinfees.21.co/api/v1/fees/recommended";
                    var result = client.GetAsync(request, HttpCompletionOption.ResponseContentRead).Result;
                    var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
                    var fastestSatoshiPerByteFee = json.Value<decimal>("fastestFee");
                    fee = new Money(fastestSatoshiPerByteFee * txSize, MoneyUnit.Satoshi);
                }
            }
            catch (Exception e)
            {
                
            }

            return fee;
        }
    }
}