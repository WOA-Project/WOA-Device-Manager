using MadWizard.WinUSBNet;
using System;
using System.Threading;

namespace AndroidDebugBridge
{
    public partial class AndroidDebugBridgeTransport
    {
        private readonly USBDevice? USBDevice = null;
        private readonly USBPipe? InputPipe = null;
        private readonly USBPipe? OutputPipe = null;

        private AndroidDebugBridgeMessage ReadMessage(bool VerifyCrc = true)
        {
            byte[] IncomingMessage = ReadFromUsb(24);
            (AndroidDebugBridgeCommands CommandIdentifier, uint FirstArgument, uint SecondArgument, uint CommandPayloadLength, uint CommandPayloadCrc) = AndroidDebugBridgeMessaging.ParseCommandPacket(IncomingMessage);

            if (CommandPayloadLength > 0)
            {
                byte[] Payload = ReadFromUsb(CommandPayloadLength);

                if (VerifyCrc)
                {
                    AndroidDebugBridgeMessaging.VerifyAdbCrc(Payload, CommandPayloadCrc);
                }

                return new AndroidDebugBridgeMessage(CommandIdentifier, FirstArgument, SecondArgument, Payload);
            }

            return new AndroidDebugBridgeMessage(CommandIdentifier, FirstArgument, SecondArgument);
        }

        private void ReadMessageAsync(Action<AndroidDebugBridgeMessage> asyncCallback, bool VerifyCrc = true)
        {
            byte[] IncomingMessage = new byte[24];
            ReadFromUsbAsync(IncomingMessage, (asyncReadCallback) =>
            {
                (AndroidDebugBridgeCommands CommandIdentifier, uint FirstArgument, uint SecondArgument, uint CommandPayloadLength, uint CommandPayloadCrc) = AndroidDebugBridgeMessaging.ParseCommandPacket(IncomingMessage);

                if (CommandPayloadLength > 0)
                {
                    byte[] Payload = new byte[CommandPayloadLength];
                    ReadFromUsbAsync(Payload, (_) =>
                    {
                        if (VerifyCrc)
                        {
                            AndroidDebugBridgeMessaging.VerifyAdbCrc(Payload, CommandPayloadCrc);
                        }

                        asyncCallback.Invoke(new AndroidDebugBridgeMessage(CommandIdentifier, FirstArgument, SecondArgument, Payload));
                    }, asyncReadCallback.AsyncState);
                }
                else
                {
                    asyncCallback.Invoke(new AndroidDebugBridgeMessage(CommandIdentifier, FirstArgument, SecondArgument));
                }
            });
        }

        private byte[] ReadFromUsb(uint size)
        {
            byte[] buffer = new byte[size];
            int attempts = 0;
            while (true)
            {
                try
                {
                    _ = InputPipe.Read(buffer);
                    break;
                }
                catch
                {
                    attempts++;
                    if (attempts > 10)
                    {
                        throw new Exception("Failed to read from USB device!");
                    }
                    Thread.Sleep(100);
                }
            }

            return buffer;
        }

        private void ReadFromUsbAsync(byte[] buffer, AsyncCallback asyncCallback, object? stateObject = null)
        {
            InputPipe.BeginRead(buffer, 0, buffer.Length, (asyncWriteState) =>
            {
                InputPipe.EndRead(asyncWriteState);
                asyncCallback(asyncWriteState);
            }, stateObject);
        }

        internal void SendMessage(AndroidDebugBridgeMessage outgoingMessage)
        {
            byte[] OutgoingMessage = AndroidDebugBridgeMessaging.GetCommandPacket(outgoingMessage.CommandIdentifier, outgoingMessage.FirstArgument, outgoingMessage.SecondArgument, outgoingMessage.Payload);
            WriteToUsb(OutgoingMessage);

            if (outgoingMessage.Payload != null)
            {
                WriteToUsb(outgoingMessage.Payload);
            }
        }

        private void SendMessageAsync(AndroidDebugBridgeMessage androidDebugBridgeMessage, Action? asyncCallback = null)
        {
            byte[] OutgoingMessage = AndroidDebugBridgeMessaging.GetCommandPacket(androidDebugBridgeMessage.CommandIdentifier, androidDebugBridgeMessage.FirstArgument, androidDebugBridgeMessage.SecondArgument, androidDebugBridgeMessage.Payload);
            WriteToUsbAsync(OutgoingMessage, (asyncWriteObject) =>
            {
                if (androidDebugBridgeMessage.Payload != null)
                {
                    WriteToUsbAsync(androidDebugBridgeMessage.Payload, (_) => asyncCallback?.Invoke(), asyncWriteObject.AsyncState);
                }
            });
        }

        private void WriteToUsb(byte[] buffer)
        {
            int attempts = 0;
            while (true)
            {
                try
                {
                    OutputPipe.Write(buffer);
                    break;
                }
                catch
                {
                    attempts++;
                    if (attempts > 10)
                    {
                        throw new Exception("Failed to write to USB device!");
                    }
                    Thread.Sleep(100);
                }
            }
        }

        private void WriteToUsbAsync(byte[] buffer, AsyncCallback asyncCallback, object? stateObject = null)
        {
            OutputPipe.BeginWrite(buffer, 0, buffer.Length, (asyncWriteState) =>
            {
                OutputPipe.EndWrite(asyncWriteState);
                asyncCallback(asyncWriteState);
            }, stateObject);
        }
    }
}