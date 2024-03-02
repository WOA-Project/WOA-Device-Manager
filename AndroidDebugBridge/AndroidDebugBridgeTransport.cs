using MadWizard.WinUSBNet;
using System;
using System.Security.Cryptography;
using System.Text;

namespace AndroidDebugBridge
{
    public class AndroidDebugBridgeTransport : IDisposable
    {
        private bool Disposed = false;
        private readonly USBDevice? USBDevice = null;
        private readonly USBPipe? InputPipe = null;
        private readonly USBPipe? OutputPipe = null;
        private readonly RSACryptoServiceProvider RSACryptoServiceProvider = new(2048);
        private uint localId = 0;

        public string PhoneConnectionString
        {
            get; private set;
        }

        public uint PhoneSupportedProtocolVersion
        {
            get; private set;
        }

        public AndroidDebugBridgeTransport(string DevicePath)
        {
            PhoneConnectionString = "";
            PhoneSupportedProtocolVersion = 0;

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
            return Encoding.UTF8.GetBytes($"{Convert.ToBase64String(ConvertedKey)} {Environment.UserName}@{Environment.MachineName}\0");
        }

        public void Connect()
        {
            // -> CNXN + System Information
            AndroidDebugBridgeMessage ConnectMessage = AndroidDebugBridgeMessage.GetConnectMessage();
            ConnectMessage.SendMessage(OutputPipe);

            // <- AUTH (type 1) + Token
            AndroidDebugBridgeMessage DeviceAuthMessage = AndroidDebugBridgeMessage.ReadIncomingMessage(InputPipe);

            if (DeviceAuthMessage.CommandIdentifier != AndroidDebugBridgeCommands.AUTH)
            {
                throw new Exception("Message has not sent a message of type AUTH during handshake.");
            }

            if (DeviceAuthMessage.FirstArgument != 1)
            {
                throw new Exception("Message has not sent a message of type AUTH type 1 during handshake.");
            }

            // Real ADB does this if already accepted once
            //
            // -> AUTH (type 2) + Signed Token with RSA Private Key
            //
            // If ok:
            //   <- CNXN + System Information
            //
            // If not:
            //   <- AUTH (type 1) + Token
            //   -> AUTH (type 3) + RSA Public Key
            //   <- CNXN + System Information

            // Token: 1
            // Signature: 2
            // RSA Public: 3

            // -> AUTH (type 3) + Public Key
            byte[] PublicKey = GetAdbPublicKeyPayload();
            AndroidDebugBridgeMessage AuthType3 = AndroidDebugBridgeMessage.GetAuthMessage(3, PublicKey);
            AuthType3.SendMessage(OutputPipe);

            // <- CNXN + System Information
            AndroidDebugBridgeMessage DeviceConnectPacket = AndroidDebugBridgeMessage.ReadIncomingMessage(InputPipe);

            if (DeviceConnectPacket.CommandIdentifier != AndroidDebugBridgeCommands.CNXN)
            {
                throw new Exception("Message has not sent a message of type CNXN during handshake.");
            }

            PhoneSupportedProtocolVersion = DeviceConnectPacket.FirstArgument;
            PhoneConnectionString = Encoding.ASCII.GetString(DeviceConnectPacket.Payload!);

            // We are now fully connected! Hooray!
        }

        public void Reboot(string mode = "")
        {
            uint shellLocalId = ++localId;

            AndroidDebugBridgeMessage RebootMessage = AndroidDebugBridgeMessage.GetOpenMessage(shellLocalId, $"reboot:{mode}");
            RebootMessage.SendMessage(OutputPipe);

            // The phone can reply ok here but not always the case!
        }

        public void RebootBootloader()
        {
            Reboot("bootloader");
        }

        public void RebootRecovery()
        {
            Reboot("recovery");
        }

        public void RebootFastBootD()
        {
            Reboot("fastboot");
        }

        public void Shell()
        {
            uint shellLocalId = ++localId;

            AndroidDebugBridgeMessage ShellMessage = AndroidDebugBridgeMessage.GetOpenMessage(shellLocalId, "shell:");

            ShellMessage.SendMessage(OutputPipe);

            AndroidDebugBridgeMessage OkayResponse = AndroidDebugBridgeMessage.ReadIncomingMessage(InputPipe);

            if (OkayResponse.CommandIdentifier != AndroidDebugBridgeCommands.OKAY)
            {
                throw new Exception("Message has not sent a message of type OKAY during shell open.");
            }

            AndroidDebugBridgeMessage WriteResponse = AndroidDebugBridgeMessage.ReadIncomingMessage(InputPipe);

            if (WriteResponse.CommandIdentifier != AndroidDebugBridgeCommands.WRTE)
            {
                throw new Exception("Message has not sent a second message of type WRITE during shell open.");
            }

            uint remoteId = WriteResponse.FirstArgument;

            Console.WriteLine(Encoding.UTF8.GetString(WriteResponse.Payload!));

            AndroidDebugBridgeMessage OkayMessage = AndroidDebugBridgeMessage.GetReadyMessage(shellLocalId, remoteId);

            OkayMessage.SendMessage(OutputPipe);
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