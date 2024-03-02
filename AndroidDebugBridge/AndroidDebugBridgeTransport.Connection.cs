using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace AndroidDebugBridge
{
    public partial class AndroidDebugBridgeTransport
    {
        private static bool IsConnected = false;

        private readonly RSACryptoServiceProvider RSACryptoServiceProvider = new(2048);

        public string PhoneConnectionString
        {
            get; private set;
        }

        public uint PhoneSupportedProtocolVersion
        {
            get; private set;
        }

        private byte[] GetAdbPublicKeyPayload()
        {
            RSAParameters publicKey = RSACryptoServiceProvider.ExportParameters(false);
            (uint n0inv, uint[] n, uint[] rr, int exponent) = AndroidDebugBridgeCryptography.ConvertRSAToADBRSA(publicKey);
            byte[] ConvertedKey = AndroidDebugBridgeCryptography.ADBRSAToBuffer(n0inv, n, rr, exponent);
            return Encoding.UTF8.GetBytes($"{Convert.ToBase64String(ConvertedKey)} {Environment.UserName}@{Environment.MachineName}\0");
        }

        public void Connect()
        {
            // -> CNXN + System Information
            AndroidDebugBridgeMessage ConnectMessage = AndroidDebugBridgeMessage.GetConnectMessage();
            SendMessage(ConnectMessage);

            while (!IsConnected)
            {
                Thread.Sleep(100);
            }
        }
    }
}