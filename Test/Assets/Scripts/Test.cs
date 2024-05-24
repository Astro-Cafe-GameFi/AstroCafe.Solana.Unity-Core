using UnityEngine;
using AstroCafe.Solana.Unity;
using Solana.Unity.Programs;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Wallet;
using System;
using System.Threading.Tasks;

public class SignResult
{
    public string signer;
    public string signature;
}

public class Test : MonoBehaviour
{
    const string RECENT_BLOCKHASH = "GMfuT6Ju9KGeEkZFHdVcChJwpRh6WHEVPixEzd86LwPz";

    // Start is called before the first frame update
    void Start()
    {
        Connector.url = "https://Astro-Cafe-GameFi.github.io/AstroCafe.Solana.Web3.Connect/";
    }

    async Task<SignResult> Sign(string message)
    {
        var ret = await Connector.Sign(message);
        string[] subs = ret.Split('-');
        if (subs.Length == 2)
        {
            SignResult result = new SignResult();
            result.signer = subs[0];
            result.signature = subs[1];
            return result;
        }
        else
        {
            return null;
        }
    }

    async public void OnSignPress()
    {
        var result = await Sign("Please sign this message.");
        if (result != null)
        {
            print("Signer: " + result.signer);
            print("Signature: " + result.signature);
        }
        else
        {
            print("sign error");
        }
    }

    async public void OnSendPress()
    {
        var result = await Sign("It needs to get the payer address. Please sign this message.");
        if (result != null)
        {
            var txMsg = GetTransactionMessage(result.signer);
            var txHash = await Connector.SendTransaction(Connector.NetworkId.Devnet, txMsg);
            if (txHash != null)
            {
                print("txHash: " + txHash);
            }
            else
            {
                print("transaction error");
            }
        }
    }

    string GetTransactionMessage(string payerAddress)
    {
        var payerPubKey = new PublicKey(payerAddress);

        byte[] msgData = new TransactionBuilder()
                .SetRecentBlockHash(RECENT_BLOCKHASH) // This is replaced in the web page
                .SetFeePayer(payerPubKey)
                .AddInstruction(SystemProgram.Transfer(payerPubKey, new PublicKey("2n97vJmCLJtT7W51zEuRYHmDjxyPB1cZchcBhUzPQ3Fp"), 10000000))
                .AddInstruction(MemoProgram.NewMemo(payerPubKey, "Hello from AstroCafe.Solana.Unity.Test :)"))
                .CompileMessage();
        var msg = BitConverter.ToString(msgData).Replace("-", "");
        return msg;
    }
}
