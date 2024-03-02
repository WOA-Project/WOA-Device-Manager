using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidDebugBridge
{
    public partial class AndroidDebugBridgeTransport
    {
        public void Shell()
        {
            uint shellLocalId = ++LocalId;

            // Open shell in 256 colors, v2
            AndroidDebugBridgeMessage ShellMessage = AndroidDebugBridgeMessage.GetOpenMessage(shellLocalId, "shell,v2,TERM=xterm-256color,pty:");
            SendMessage(ShellMessage);
            WaitForOKAYMessage();
            OpenedStreamIdMap.Add(RemoteId, LocalId);

            // Configure terminal size
            string terminalConfiguration = $"{Console.WindowWidth}x{Console.WindowHeight},0x0";
            Debug.WriteLine($"Configuring terminal: {terminalConfiguration}");

            byte[] ConfigurationRes = Encoding.UTF8.GetBytes($"{terminalConfiguration}\0");
            byte[] StringLength = BitConverter.GetBytes(ConfigurationRes.Length);
            List<byte> payloadList = ConfigurationRes.ToList();
            payloadList.Insert(0, 0x05);
            payloadList.InsertRange(1, StringLength);

            AndroidDebugBridgeMessage ResolutionMessage = AndroidDebugBridgeMessage.GetWriteMessage(shellLocalId, shellLocalId, payloadList.ToArray());
            SendMessage(ResolutionMessage);
            WaitForOKAYMessage();

            Task consoleInputThread = Task.Run(() =>
            {
                ConsoleKeyInfo readKey;

                while ((readKey = Console.ReadKey(true)).Key != ConsoleKey.Escape && !ClosedStreamIdMap.ContainsValue(shellLocalId) && !PendingStreamIdMap.ContainsValue(shellLocalId))
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(readKey.KeyChar.ToString());
                    byte[] StringLength = BitConverter.GetBytes(buffer.Length);
                    List<byte> payloadList = buffer.ToList();
                    payloadList.Insert(0, 0x00);
                    payloadList.InsertRange(1, StringLength);

                    AndroidDebugBridgeMessage InputMessage = AndroidDebugBridgeMessage.GetWriteMessage(shellLocalId, shellLocalId, payloadList.ToArray());
                    SendMessage(InputMessage);
                    WaitForOKAYMessage();
                }

                if (readKey.Key == ConsoleKey.Escape)
                {
                    // TODO: Closing flow
                }
            });

            consoleInputThread.Wait();
        }
    }
}
