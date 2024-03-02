using System;
using System.Threading;

namespace AndroidDebugBridge
{
    public class AndroidDebugBridgeStream : IDisposable
    {
        private bool Disposed = false;

        private bool ReceivedOK = false;
        private bool IsClosed = false;

        private readonly uint LocalIdentifier;
        private readonly string OpenString;
        private readonly AndroidDebugBridgeTransport Transport;

        internal uint RemoteIdentifier = 0;

        public event EventHandler<byte[]>? DataReceived;
        public event EventHandler? DataClosed;

        internal AndroidDebugBridgeStream(AndroidDebugBridgeTransport Transport, uint LocalIdentifier, string OpenString)
        {
            this.LocalIdentifier = LocalIdentifier;
            this.OpenString = OpenString;
            this.Transport = Transport;
        }

        internal void Open()
        {
            Transport.SendMessage(AndroidDebugBridgeMessage.GetOpenMessage(LocalIdentifier, OpenString));
        }

        private void Close()
        {
            if (!IsClosed)
            {
                IsClosed = true;

                Transport.SendMessage(AndroidDebugBridgeMessage.GetCloseMessage(LocalIdentifier, LocalIdentifier));
                WaitForAcknowledgement();
            }
        }

        public void Write(byte[] buffer)
        {
            Transport.SendMessage(AndroidDebugBridgeMessage.GetWriteMessage(LocalIdentifier, LocalIdentifier, buffer));
            WaitForAcknowledgement();
        }

        internal bool HandleIncomingMessage(AndroidDebugBridgeMessage incomingMessage)
        {
            switch (incomingMessage.CommandIdentifier)
            {
                case AndroidDebugBridgeCommands.OKAY:
                    {
                        ReceivedOK = true;
                        return true;
                    }
                case AndroidDebugBridgeCommands.WRTE:
                    {
                        DataReceived?.Invoke(this, incomingMessage.Payload!);
                        return true;
                    }
                case AndroidDebugBridgeCommands.CLSE:
                    {
                        if (!IsClosed)
                        {
                            // < CLSE - Done (here)
                            // > OKAY
                            // > CLSE
                            // < CLSE

                            IsClosed = true;

                            // Send an OKAY back
                            SendAcknowledgement();

                            DataClosed?.Invoke(this, EventArgs.Empty);

                            // Close ourselves too
                            Transport.SendMessage(AndroidDebugBridgeMessage.GetCloseMessage(LocalIdentifier, LocalIdentifier));

                            // Another CLSE will be sent from the device but we removed ourselves (DataClosed) so we won't treat it.
                        }
                        else
                        {
                            // > CLSE - Done
                            // < OKAY - Done
                            // < CLSE - Done (here)
                            // > CLSE

                            // Close ourselves too
                            Transport.SendMessage(AndroidDebugBridgeMessage.GetCloseMessage(LocalIdentifier, LocalIdentifier));

                            // Remove ourselves
                            DataClosed?.Invoke(this, EventArgs.Empty);
                        }

                        return true;
                    }
            }

            return false;
        }

        public void SendAcknowledgement()
        {
            Transport.SendMessage(AndroidDebugBridgeMessage.GetReadyMessage(LocalIdentifier, LocalIdentifier));
        }

        public void WaitForAcknowledgement()
        {
            while (!ReceivedOK)
            {
                Thread.Sleep(100);
            }

            ReceivedOK = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AndroidDebugBridgeStream()
        {
            Dispose(false);
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