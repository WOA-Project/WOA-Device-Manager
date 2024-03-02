using MadWizard.WinUSBNet;
using System.Text;

namespace AndroidDebugBridge
{
    internal class AndroidDebugBridgeMessage
    {
        public AndroidDebugBridgeCommands CommandIdentifier
        {
            get; set;
        }

        public uint FirstArgument
        {
            get; set;
        }

        public uint SecondArgument
        {
            get; set;
        }

        public byte[]? Payload
        {
            get; set;
        }

        public AndroidDebugBridgeMessage(AndroidDebugBridgeCommands commandIdentifier, uint firstArgument, uint secondArgument, byte[]? payload = null)
        {
            CommandIdentifier = commandIdentifier;
            FirstArgument = firstArgument;
            SecondArgument = secondArgument;
            Payload = payload;
        }

        public static AndroidDebugBridgeMessage ReadIncomingMessage(USBPipe InputPipe)
        {
            byte[] IncomingMessage = new byte[24];
            _ = InputPipe.Read(IncomingMessage);

            (AndroidDebugBridgeCommands CommandIdentifier, uint FirstArgument, uint SecondArgument, uint CommandPayloadLength, uint CommandPayloadCrc) = AndroidDebugBridgeMessaging.ParseCommandPacket(IncomingMessage);

            if (CommandPayloadLength > 0)
            {
                byte[] Payload = new byte[CommandPayloadLength];
                _ = InputPipe.Read(Payload);
                AndroidDebugBridgeMessaging.VerifyAdbCrc(Payload, CommandPayloadCrc);

                return new AndroidDebugBridgeMessage(CommandIdentifier, FirstArgument, SecondArgument, Payload);
            }

            return new AndroidDebugBridgeMessage(CommandIdentifier, FirstArgument, SecondArgument);
        }

        public void SendMessage(USBPipe OutputPipe)
        {
            byte[] OutgoingMessage = AndroidDebugBridgeMessaging.GetCommandPacket(CommandIdentifier, FirstArgument, SecondArgument, Payload);

            OutputPipe.Write(OutgoingMessage);
            if (Payload != null)
            {
                OutputPipe.Write(Payload);
            }
        }

        internal static AndroidDebugBridgeMessage GetConnectMessage()
        {
            byte[] SystemInformation = Encoding.UTF8.GetBytes("host::features=shell_v2,cmd,stat_v2,ls_v2,fixed_push_mkdir,apex,abb,fixed_push_symlink_timestamp,abb_exec,remount_shell,track_app,sendrecv_v2,sendrecv_v2_brotli,sendrecv_v2_lz4,sendrecv_v2_zstd,sendrecv_v2_dry_run_send,openscreen_mdns");
            return new AndroidDebugBridgeMessage(AndroidDebugBridgeCommands.CNXN, 0x01000001, 0x00100000, SystemInformation);
        }

        internal static AndroidDebugBridgeMessage GetAuthMessage(uint type, byte[] data)
        {
            // type:
            //
            // Token: 1
            // Signature: 2
            // RSA Public: 3
            return new AndroidDebugBridgeMessage(AndroidDebugBridgeCommands.AUTH, type, 0, data);
        }

        internal static AndroidDebugBridgeMessage GetOpenMessage(uint localId, string dest)
        {
            byte[] buffer = Encoding.UTF8.GetBytes($"{dest}\0");
            return new AndroidDebugBridgeMessage(AndroidDebugBridgeCommands.OPEN, localId, 0, buffer);
        }

        internal static AndroidDebugBridgeMessage GetWriteMessage(uint localId, uint remoteId, byte[] data)
        {
            return new AndroidDebugBridgeMessage(AndroidDebugBridgeCommands.WRTE, localId, remoteId, data);
        }

        internal static AndroidDebugBridgeMessage GetCloseMessage(uint localId, uint remoteId)
        {
            return new AndroidDebugBridgeMessage(AndroidDebugBridgeCommands.CLSE, localId, remoteId, null);
        }

        internal static AndroidDebugBridgeMessage GetReadyMessage(uint localId, uint remoteId)
        {
            return new AndroidDebugBridgeMessage(AndroidDebugBridgeCommands.OKAY, localId, remoteId, null);
        }
    }
}