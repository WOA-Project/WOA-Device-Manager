using MadWizard.WinUSBNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FastBoot
{
    public class FastBootTransport : IDisposable
    {
        private bool Disposed = false;
        private readonly USBDevice USBDevice = null;
        private USBPipe InputPipe = null;
        private USBPipe OutputPipe = null;

        public FastBootTransport(string DevicePath)
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

        private (FastBootStatus status, string response, byte[] rawResponse) ReadResponse()
        {
            byte[] responseBuffer = new byte[0x1000];
            int readByteCount = InputPipe.Read(responseBuffer);

            string responseBufferAsString = Encoding.ASCII.GetString(responseBuffer);

            if (responseBufferAsString.Length < 4)
            {
                throw new Exception($"Invalid response: {responseBufferAsString}");
            }

            string statusAsString = responseBufferAsString[0..4];

            FastBootStatus status;

            switch (statusAsString)
            {
                case "INFO":
                    {
                        status = FastBootStatus.INFO;
                        break;
                    }
                case "OKAY":
                    {
                        status = FastBootStatus.OKAY;
                        break;
                    }
                case "DATA":
                    {
                        status = FastBootStatus.DATA;
                        if (readByteCount > 12)
                        {
                            readByteCount = 12;
                            responseBufferAsString = responseBufferAsString[..12];
                        }
                        break;
                    }
                case "FAIL":
                    {
                        status = FastBootStatus.FAIL;
                        break;
                    }
                default:
                    {
                        throw new Exception($"Unknown response: {statusAsString} - {responseBufferAsString}");
                    }
            };

            string responseBufferDataAsString = responseBufferAsString[4..readByteCount];
            byte[] responseBufferData = responseBuffer[4..readByteCount];

            // To view the response data for debugging purposes
            // Console.WriteLine($"[{status}] {responseBufferDataAsString}");

            return (status, responseBufferDataAsString, responseBufferData);
        }

        public (FastBootStatus status, string response, byte[] rawResponse)[] SendCommand(byte[] command)
        {
            OutputPipe.Write(command);

            List<(FastBootStatus, string, byte[])> responseList = new();

            FastBootStatus status;
            do
            {
                (status, string response, byte[] rawResponse) = ReadResponse();
                responseList.Add((status, response, rawResponse));
            }
            while (status == FastBootStatus.INFO);

            return responseList.ToArray();
        }

        public (FastBootStatus status, string response, byte[] rawResponse)[] SendCommand(string command)
        {
            return SendCommand(Encoding.ASCII.GetBytes(command));
        }

        public (FastBootStatus status, string response, byte[] rawResponse) SendData(Stream stream)
        {
            long length = stream.Length;
            byte[] buffer = new byte[0x80000];

            (FastBootStatus status, string response, byte[] rawResponse)[] responses = SendCommand($"download:{length:X8}");
            (FastBootStatus status, string response, byte[] _) = responses.Last();

            if (status != FastBootStatus.DATA)
            {
                throw new InvalidDataException($"Invalid response: {status} - {response}");
            }

            while (length >= 0x80000)
            {
                stream.Read(buffer, 0, 0x80000);
                OutputPipe.Write(buffer);
                length -= 0x80000;
            }

            if (length > 0)
            {
                buffer = new byte[length];
                stream.Read(buffer, 0, (int)length);
                OutputPipe.Write(buffer);
            }

            (status, response, byte[] rawResponse) = ReadResponse();

            if (status != FastBootStatus.OKAY)
            {
                throw new InvalidDataException($"Invalid response: {status} - {response}");
            }

            return (status, response, rawResponse);
        }

        public (FastBootStatus status, string response, byte[] rawResponse) SendData(string filePath)
        {
            using FileStream fileStream = File.OpenRead(filePath);
            return SendData(fileStream);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~FastBootTransport()
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