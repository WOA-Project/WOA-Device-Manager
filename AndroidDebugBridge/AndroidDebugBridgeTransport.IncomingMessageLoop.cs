using System;
using System.Collections.Generic;
using System.Text;

namespace AndroidDebugBridge
{
    public partial class AndroidDebugBridgeTransport
    {
        private uint LocalId = 0;

        private readonly List<AndroidDebugBridgeStream> Streams = new();

        public AndroidDebugBridgeStream OpenStream(string OpenString)
        {
            AndroidDebugBridgeStream stream = new(this, ++LocalId, OpenString);
            Streams.Add(stream);
            stream.Open();

            stream.DataClosed += (object? sender, EventArgs args) =>
            {
                Streams.Remove((sender as AndroidDebugBridgeStream)!);
            };

            return stream;
        }

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
            if (incomingMessage.CommandIdentifier != AndroidDebugBridgeCommands.CNXN && incomingMessage.CommandIdentifier != AndroidDebugBridgeCommands.AUTH)
            {
                bool HandledExternally = false;
                foreach (AndroidDebugBridgeStream stream in Streams)
                {
                    if (stream.RemoteIdentifier != 0 && stream.RemoteIdentifier == incomingMessage.FirstArgument)
                    {
                        HandledExternally = stream.HandleIncomingMessage(incomingMessage);
                        break;
                    }
                    else if (stream.RemoteIdentifier == 0)
                    {
                        stream.RemoteIdentifier = incomingMessage.FirstArgument;
                        HandledExternally = stream.HandleIncomingMessage(incomingMessage);
                    }
                }

                if (HandledExternally)
                {
                    return;
                }
            }

            switch (incomingMessage.CommandIdentifier)
            {
                case AndroidDebugBridgeCommands.CNXN:
                    {
                        PhoneSupportedProtocolVersion = incomingMessage.FirstArgument;
                        PhoneConnectionString = Encoding.ASCII.GetString(incomingMessage.Payload!);
                        IsConnected = true;
                        break;
                    }
                case AndroidDebugBridgeCommands.AUTH:
                    {
                        if (incomingMessage.FirstArgument == 1)
                        {
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
            }
        }
    }
}