using UnityEngine;
using AstroCafe.Solana.Unity;
using Solana.Unity.Programs;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Types;
using Solana.Unity.Rpc.Messages;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Wallet;
using Solana.Unity.SDK;
using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Linq;
using Newtonsoft.Json;

public class SignResult
{
    public string signer;
    public string signature;
}

public class ResponseData
{
    [JsonProperty("data")]
    public DataContent Data { get; set; }
}

public class DataContent
{
    [JsonProperty("signature")]
    public string Signature { get; set; }
}

public class ChallengeService : MonoBehaviour
{
    private readonly HttpClient _httpClient;

    public ChallengeService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<string> ProcessChallengeClaim(string url, string challengeId)
    {
        // Get the token (replace with your token retrieval logic)
        string token = GetToken();
        print("Token: " + token);

        if (!string.IsNullOrEmpty(token))
        {
            // Prepare the request payload
            var payload = new { challengeId };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            // Set up the request headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Send the POST request
                var response = await _httpClient.PostAsync($"{url}/challenge/requireClaim", content);

                if (response.IsSuccessStatusCode)
                {
                    // Parse the response
                    var responseBody = await response.Content.ReadAsStringAsync();

                    // Deserialize into a strongly typed object
                    var responseData = JsonConvert.DeserializeObject<ResponseData>(responseBody);

                    if (responseData?.Data?.Signature != null)
                    {
                        string signatureHex = responseData.Data.Signature;
                        print("Signature (Hex): " + signatureHex);

                        return signatureHex;
                    }
                    else
                    {
                        print("Signature not found in response.");
                        return string.Empty;
                    }
                }
                else
                {
                    print("Request failed: " + response.ReasonPhrase);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                print("Error occurred: " + ex.Message);
                return string.Empty;
            }
        }
        return string.Empty;
    }

    private string GetToken()
    {
        // Replace this with your token retrieval logic
        return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjY3MmJjMzYxZWIyMzlkM2QzNmE1ZTVjYiIsInJvbGUiOiJ1c2VyIiwiaWF0IjoxNzMxNDIzNTYwLCJleHAiOjE3MzE1MDk5NjB9.bOW7-7gud22fq8w1_PV40tC8sbwFEg_ERdlrIAfW8A8";
    }
}

public class Test : MonoBehaviour
{
    const string RECENT_BLOCKHASH = "GMfuT6Ju9KGeEkZFHdVcChJwpRh6WHEVPixEzd86LwPz";

    // Start is called before the first frame update
    void Start()
    {
        //Connector.url = "https://astro-cafe-gamefi.github.io/AstroCafe.Solana.Web3.Connect/";
        Connector.url = "http://localhost:5173/";
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
        var challengeService = new ChallengeService();
        var result = await challengeService.ProcessChallengeClaim("http://localhost:4040", "V1StGXR8_Z5jdHi7B-myQ");
        var txHash = await Connector.SendMultiSignTransaction(Connector.NetworkId.Devnet, result);
        if (txHash != null)
        {
            print("txHash: " + txHash);
        }
        else
        {
            print("transaction error");
        }
        //var result = await Sign("It needs to get the payer address. Please sign this message.");
        //if (result != null)
        //{
        //    var txMsg = GetTransactionMessage(result.signer);
        //    var txHash = await Connector.SendTransaction(Connector.NetworkId.Devnet, txMsg);
        //    if (txHash != null)
        //    {
        //        print("txHash: " + txHash);
        //    }
        //    else
        //    {
        //        print("transaction error");
        //    }
        //}
    }

    string GetTransactionMessage(string payerAddress)
    {
        var payerPubKey = new PublicKey(payerAddress);

        

        var CONFIG_SEED = "config";
        var CHALLENGE_SEED = "challenge";
        var challengeId = "V1StGXR8_Z5jdHi7B-myQ";
        PublicKey programId = new PublicKey("AK6Zkii2WMPzQ9g6bb7mFvrjNZFCcLsrB62AjCmgF8rj");
        PublicKey mint = new PublicKey("9Jf7YZVy7VYrqkAS1qyKqjXKVtNqH28mSnH26RPre8WG");
        PublicKey treasury = new PublicKey("2CqUNbHGr2BopF4JF6rTmB4onSZmkLArekZ2a85LNHAq");

        var maker = new PublicKey("DvM9a155oaDoLD1y2UzjWSi9P9JVf2ZWt1sTiTEvm9kT");

        var opponent = new PublicKey("3LqZ4mPU4PX3fxiuSK29XEuRw72wkYWdjWjW5ULgUBY9");

        PublicKey.TryFindProgramAddress(new []{Encoding.UTF8.GetBytes(CONFIG_SEED)},programId,out var configPda,out var configBump);
        PublicKey.TryFindProgramAddress(new [] {Encoding.UTF8.GetBytes(CHALLENGE_SEED), maker.KeyBytes, Encoding.UTF8.GetBytes(challengeId)},programId,out var challengePda,out var challengeBump);

        var makerAta = AssociatedTokenAccountProgram
            .DeriveAssociatedTokenAccount(maker, mint);
        var opponentAta = AssociatedTokenAccountProgram
            .DeriveAssociatedTokenAccount(opponent, mint);
        var vaultAta = AssociatedTokenAccountProgram
            .DeriveAssociatedTokenAccount(configPda, mint);
        var treasuryAta = AssociatedTokenAccountProgram
            .DeriveAssociatedTokenAccount(treasury, mint);

        print(configPda);
        print(vaultAta);

        // Token program ID (Solana Token Program)
        PublicKey tokenProgram = TokenProgram.ProgramIdKey;

        // Associated Token Program ID
        PublicKey associatedTokenProgram = AssociatedTokenAccountProgram.ProgramIdKey;

        // System Program ID
        PublicKey systemProgram = SystemProgram.ProgramIdKey;

        ulong amount = (ulong)(0.2 * Math.Pow(10, 11));
        //byte[] encodedData = ConstructCreateChallengeTransactionData(challengeId, amount);
        byte[] encodedData = ConstructStartChallengeTransactionData(challengeId);

        //byte[] encodedData = ConstructCancelChallengeTransactionData(challengeId);

        //var keyContext = new[] {
        //        AccountMeta.ReadOnly(mint, false),
        //        AccountMeta.Writable(maker, true),
        //        AccountMeta.ReadOnly(configPda, false),
        //        AccountMeta.Writable(challengePda, false),
        //        AccountMeta.Writable(makerAta, false),
        //        AccountMeta.Writable(vaultAta, false),
        //        AccountMeta.ReadOnly(tokenProgram, false),
        //        AccountMeta.ReadOnly(systemProgram, false)
        //    };

        var keyContext = new[] {
                AccountMeta.Writable(opponent, true),
                AccountMeta.ReadOnly(maker, false),
                AccountMeta.ReadOnly(mint, false),
                AccountMeta.ReadOnly(configPda, false),
                AccountMeta.Writable(challengePda, false),
                AccountMeta.Writable(opponentAta, false),
                AccountMeta.Writable(vaultAta, false),
                AccountMeta.ReadOnly(tokenProgram, false),
                AccountMeta.ReadOnly(systemProgram, false)
            };

        //var keyContext = new[] {
        //        AccountMeta.Writable(maker, true),
        //        AccountMeta.ReadOnly(mint, false),
        //        AccountMeta.ReadOnly(configPda, false),
        //        AccountMeta.Writable(challengePda, false),
        //        AccountMeta.Writable(makerAta, false),
        //        AccountMeta.Writable(vaultAta, false),
        //        AccountMeta.ReadOnly(tokenProgram, false),
        //        AccountMeta.ReadOnly(systemProgram, false)
        //    };

        var instruction = new TransactionInstruction
        {
            ProgramId = programId,
            Keys = keyContext,
            Data = encodedData
        };

        byte[] msgData = new TransactionBuilder()
                .SetRecentBlockHash(RECENT_BLOCKHASH) // This is replaced in the web page
                .SetFeePayer(payerPubKey)
                .AddInstruction(instruction)
                //.AddInstruction(SystemProgram.Transfer(payerPubKey, new PublicKey("3LqZ4mPU4PX3fxiuSK29XEuRw72wkYWdjWjW5ULgUBY9"), 10000000))
                //.AddInstruction(MemoProgram.NewMemo(payerPubKey, "Hello from AstroCafe.Solana.Unity.Test :)"))
                .CompileMessage();
        var msg = BitConverter.ToString(msgData).Replace("-", "");
        return msg;
    }

    public static byte[] EncodeString(string value)
    {
        return Encoding.UTF8.GetBytes(value);
    }

    public static byte[] EncodeU64(ulong value)
    {
        return BitConverter.GetBytes(value); // Little-endian by default in .NET
    }

    public static byte[] HexStringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                         .ToArray();
    }

    public static byte[] ConstructCreateChallengeTransactionData(string challengeId, ulong amount)
    {
        byte[] discriminator = GetDiscriminator("global", "create_challenge");
        print(BitConverter.ToString(discriminator).Replace("-", ""));
        //byte[] discriminator = HexStringToByteArray("aaf42f01010fadef15000000");
        byte[] encodedChallengeId = EncodeString(challengeId);
        byte[] challengeIdLengthPrefix = BitConverter.GetBytes((uint)encodedChallengeId.Length);
        if (!BitConverter.IsLittleEndian) Array.Reverse(challengeIdLengthPrefix);

        byte[] encodedAmount = EncodeU64(amount);
        
        // Combine all parts into one transaction data array
        byte[] transactionData = Combine(discriminator, challengeIdLengthPrefix, encodedChallengeId, encodedAmount);
        print(BitConverter.ToString(transactionData).Replace("-", ""));

        return transactionData;
    }

    public static byte[] ConstructStartChallengeTransactionData(string challengeId)
    {
        byte[] discriminator = GetDiscriminator("global", "start_challenge");
        print(BitConverter.ToString(discriminator).Replace("-", ""));
        //byte[] discriminator = HexStringToByteArray("f160fdbbb19e107a15000000");
        byte[] encodedChallengeId = EncodeString(challengeId);
        byte[] challengeIdLengthPrefix = BitConverter.GetBytes((uint)encodedChallengeId.Length);
        if (!BitConverter.IsLittleEndian) Array.Reverse(challengeIdLengthPrefix);

        byte[] transactionData = Combine(discriminator, challengeIdLengthPrefix, encodedChallengeId);
        print(BitConverter.ToString(transactionData).Replace("-", ""));

        return transactionData;
    }

    public static byte[] ConstructCancelChallengeTransactionData(string challengeId)
    {
        byte[] discriminator = GetDiscriminator("global", "cancel_challenge");
        print(BitConverter.ToString(discriminator).Replace("-", ""));
        //byte[] discriminator = HexStringToByteArray("e7fd0097b35e059815000000");
        byte[] encodedChallengeId = EncodeString(challengeId);
        byte[] challengeIdLengthPrefix = BitConverter.GetBytes((uint)encodedChallengeId.Length);
        if (!BitConverter.IsLittleEndian) Array.Reverse(challengeIdLengthPrefix);

        byte[] transactionData = Combine(discriminator, challengeIdLengthPrefix, encodedChallengeId);
        print(BitConverter.ToString(transactionData).Replace("-", ""));

        return transactionData;
    }


    private static byte[] Combine(params byte[][] arrays)
    {
        int totalLength = 0;
        foreach (var arr in arrays) totalLength += arr.Length;

        byte[] result = new byte[totalLength];
        int offset = 0;
        foreach (var arr in arrays)
        {
            Buffer.BlockCopy(arr, 0, result, offset, arr.Length);
            offset += arr.Length;
        }
        return result;
    }

    public static byte[] GetDiscriminator(string prefix, string name)
    {
        string preimage = $"{prefix}:{name}";
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(preimage));
            byte[] discriminator = new byte[8];
            Array.Copy(hash, discriminator, 8);
            return discriminator;
        }
    }

}
