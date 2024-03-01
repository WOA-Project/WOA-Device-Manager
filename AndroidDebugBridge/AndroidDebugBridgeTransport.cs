using MadWizard.WinUSBNet;
using System;
using System.Security.Cryptography;
using System.Text;

namespace AndroidDebugBridge
{
    public class AndroidDebugBridgeTransport : IDisposable
    {
        private bool Disposed = false;
        private readonly USBDevice USBDevice = null;
        private USBPipe InputPipe = null;
        private USBPipe OutputPipe = null;
        RSACryptoServiceProvider RSACryptoServiceProvider = new(2048);

        public AndroidDebugBridgeTransport(string DevicePath)
        {
            USBDevice = new USBDevice(DevicePath);

            foreach (USBPipe Pipe in USBDevice.Pipes)
            {
                if (Pipe.IsIn)
                {
                    InputPipe = Pipe;
                }

                if (Pipe.IsOut)
                {
                    OutputPipe = Pipe;
                }
            }

            if (InputPipe == null || OutputPipe == null)
            {
                throw new Exception("Invalid USB device!");
            }
        }

        private byte[] GetAdbPublicKeyPayload()
        {
            RSAParameters publicKey = RSACryptoServiceProvider.ExportParameters(false);
            (uint n0inv, uint[] n, uint[] rr, int exponent) = AndroidDebugBridgeCryptography.ConvertRSAToADBRSA(publicKey);
            byte[] ConvertedKey = AndroidDebugBridgeCryptography.ADBRSAToBuffer(n0inv, n, rr, exponent);

            StringBuilder keyString = new(720);

            keyString.Append(Convert.ToBase64String(ConvertedKey));
            keyString.Append(" unknown@unknown");
            keyString.Append('\0');

            return Encoding.UTF8.GetBytes(keyString.ToString());
        }

        public void Connect()
        {
            byte[] SystemInformation = Encoding.UTF8.GetBytes("host::\0");
            AndroidDebugBridgeMessage ConnectMessage = new(AndroidDebugBridgeCommands.CNXN, 0x01000000, 4096, SystemInformation);

            // -> CNXN + System Information
            ConnectMessage.SendMessage(OutputPipe);

            // <- AUTH + Token
            AndroidDebugBridgeMessage DeviceAuthMessage = AndroidDebugBridgeMessage.ReadIncomingMessage(InputPipe);

            if (DeviceAuthMessage.CommandIdentifier != AndroidDebugBridgeCommands.AUTH)
            {
                throw new Exception("Message has not sent a message of type AUTH during handshake.");
            }

            if (DeviceAuthMessage.FirstArgument != 1)
            {
                throw new Exception("Message has not sent a message of type AUTH type 1 during handshake.");
            }

            // Token: 1
            // Signature: 2
            // RSA Public: 3
            byte[] PublicKey = GetAdbPublicKeyPayload();
            AndroidDebugBridgeMessage AuthType3 = new(AndroidDebugBridgeCommands.AUTH, 3, 0, PublicKey);

            // -> AUTH (type 3) + Public Key
            AuthType3.SendMessage(OutputPipe);

            // <- CNXN + System Information
            AndroidDebugBridgeMessage DeviceConnectPacket = AndroidDebugBridgeMessage.ReadIncomingMessage(InputPipe);

            if (DeviceConnectPacket.CommandIdentifier != AndroidDebugBridgeCommands.CNXN)
            {
                throw new Exception("Message has not sent a message of type CNXN during handshake.");
            }

            if (DeviceConnectPacket.FirstArgument != 0x01000000)
            {
                throw new Exception("Message has not sent a message of type CNXN version 0x01000000 during handshake.");
            }

            Console.WriteLine(Encoding.ASCII.GetString(DeviceConnectPacket.Payload!));

            // We are now fully connected! Hooray!
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AndroidDebugBridgeTransport()
        {
            Dispose(false);
        }

        public void Close()
        {
            USBDevice?.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                // Other disposables
            }

            // Clean unmanaged resources here.
            Close();

            Disposed = true;
        }
    }
}