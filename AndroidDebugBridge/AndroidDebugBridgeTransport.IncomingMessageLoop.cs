using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace AndroidDebugBridge
{
    public partial class AndroidDebugBridgeTransport
    {
        private uint LocalId = 0;
        private uint RemoteId = 0;
        private static bool ReceivedOK = false;

        private static readonly Dictionary<uint, uint> OpenedStreamIdMap = new();
        private static readonly Dictionary<uint, uint> PendingStreamIdMap = new();
        private static readonly Dictionary<uint, uint> ClosedStreamIdMap = new();

        private void IncomingMessageLoop()
        {
            ReadMessageAsync((AndroidDebugBridgeMessage incomingMessage) =>
            {
                HandleIncomingMessage(incomingMessage);
                IncomingMessageLoop();
            }, VerifyCrc: false);
        }

        private void HandleIncomingMessage(AndroidDebugBridgeMessage incomingMessage)
        {
            Debug.WriteLine($"< new AndroidDebugBridgeMessage(AndroidDebugBridgeCommands.{incomingMessage.CommandIdentifier}, 0x{incomingMessage.FirstArgument:X8}, 0x{incomingMessage.FirstArgument:X8}, );");
            if (incomingMessage.Payload != null)
            {
                Debug.WriteLine(BitConverter.ToString(incomingMessage.Payload));
                //Debug.WriteLine(Encoding.UTF8.GetString(incomingMessage.Payload));
            }

            switch (incomingMessage.CommandIdentifier)
            {
                case AndroidDebugBridgeCommands.CNXN:
                    {
                        PhoneSupportedProtocolVersion = incomingMessage.FirstArgument;
                        PhoneConnectionString = Encoding.ASCII.GetString(incomingMessage.Payload!);
                        IsConnected = true;

                        Debug.WriteLine($"Device connected. (Phone Supported Protocol Version: {PhoneSupportedProtocolVersion}, PhoneConnectionString: {PhoneConnectionString})");
                        break;
                    }
                case AndroidDebugBridgeCommands.AUTH:
                    {
                        if (incomingMessage.FirstArgument == 1)
                        {
                            Debug.WriteLine("Device sent us an authentication challenge.");

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
                            SendMessage(AuthType3);
                        }

                        break;
                    }
                case AndroidDebugBridgeCommands.OKAY:
                    {
                        ReceivedOK = true;
                        RemoteId = incomingMessage.FirstArgument;
                        break;
                    }
                case AndroidDebugBridgeCommands.WRTE:
                    {
                        byte messageType = incomingMessage.Payload[0];
                        if (messageType == 1)
                        {
                            uint length = BitConverter.ToUInt32(incomingMessage.Payload[1..5]);
                            Console.Write(Encoding.UTF8.GetString(incomingMessage.Payload[5..((int)length + 5)]));

                            AndroidDebugBridgeMessage OkayResponse = AndroidDebugBridgeMessage.GetReadyMessage(incomingMessage.FirstArgument, incomingMessage.SecondArgument);
                            SendMessage(OkayResponse);
                        }
                        else if (messageType == 3)
                        {
                            // Close notification.
                            Debug.WriteLine("Device sent a close notification.");
                        }
                        else
                        {
                            Debug.WriteLine($"Unknown message type: {messageType}");
                        }

                        break;
                    }
                case AndroidDebugBridgeCommands.OPEN:
                    {
                        break;
                    }
                case AndroidDebugBridgeCommands.CLSE:
                    {
                        uint remoteId = incomingMessage.FirstArgument;
                        if (OpenedStreamIdMap.ContainsKey(remoteId))
                        {
                            // This case is when the device sends a close first.
                            uint localId = OpenedStreamIdMap[remoteId];

                            // Remove from the map
                            OpenedStreamIdMap.Remove(remoteId);

                            AndroidDebugBridgeMessage OkayResponse = AndroidDebugBridgeMessage.GetReadyMessage(localId, localId);
                            SendMessage(OkayResponse);

                            // Add to pending map
                            PendingStreamIdMap.Add(remoteId, localId);

                            // Close ourselves too
                            AndroidDebugBridgeMessage CloseMessage = AndroidDebugBridgeMessage.GetCloseMessage(localId, localId);
                            SendMessage(CloseMessage);
                        }
                        else if (PendingStreamIdMap.ContainsKey(remoteId))
                        {
                            // This case is when we sent a close first, then the device replied with OKAY, and then sent a close message
                            // We need to give the previous id that we lost.
                            uint localId = PendingStreamIdMap[remoteId];
                            Debug.WriteLine($"Device Successfully acknowledged the closed stream: {localId}");

                            // Remove from the map
                            PendingStreamIdMap.Remove(remoteId);
                            // Add to pending map
                            ClosedStreamIdMap.Add(remoteId, localId);
                        }
                        else
                        {
                            // ???
                            Debug.WriteLine($"Device acknowledged an unknown closed stream: {remoteId}");
                        }
                        break;
                    }
                case AndroidDebugBridgeCommands.SYNC:
                    {
                        break;
                    }
            }
        }

        private void WaitForOKAYMessage()
        {
            while (!ReceivedOK)
            {
                Thread.Sleep(100);
            }
            ReceivedOK = false;
        }
    }
}