using System;
using System.Threading.Tasks;
using UnityEngine;

namespace AstroCafe.Solana.Unity
{
    public class Connector
    {
        public enum NetworkId : ushort
        {
            Mainnet = 0,
            Devnet = 1,
        }

        public static string url { get; set; }

        public static async Task<string> Sign(string _message)
        {
            // open application
            var message = Uri.EscapeDataString(_message);
            Application.OpenURL(url + "?action=sign" + "&message=" + message);
            // set clipboard to empty
            GUIUtility.systemCopyBuffer = "";
            // wait for clipboard response
            var clipBoard = "";
            while (clipBoard == "")
            {
                clipBoard = GUIUtility.systemCopyBuffer;
                await Task.Delay(100);
            }

            // check if clipboard response is valid
            if (clipBoard.Length == 132 || clipBoard.Length == 133) // ADDRESS-SIGNATURE: 44 + 1 + 87(or 88)
                return clipBoard;
            else
                throw new Exception("sign error");
        }

        public static async Task<string> SendTransaction(NetworkId _networkId, string _txMessage)
        {
            // open application
            Application.OpenURL(url + "?action=send" + "&networkId=" + (int)_networkId + "&txMessage=" + _txMessage);
            // set clipboard to empty
            GUIUtility.systemCopyBuffer = "";
            // wait for clipboard response
            var clipBoard = "";
            while (clipBoard == "")
            {
                clipBoard = GUIUtility.systemCopyBuffer;
                await Task.Delay(100);
            }
            // check if clipboard response is valid
            if (clipBoard.Length == 87 || clipBoard.Length == 88)
                return clipBoard;
            else
                throw new Exception("transaction error");
        }
    }
}
