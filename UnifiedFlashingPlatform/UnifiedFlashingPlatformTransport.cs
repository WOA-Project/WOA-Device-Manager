/*
* MIT License
* 
* Copyright (c) 2024 The DuoWOA authors
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/
using MadWizard.WinUSBNet;
using System;
using System.Linq;
using System.Numerics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace UnifiedFlashingPlatform
{
    public class UnifiedFlashingPlatformTransport : IDisposable
    {
        private bool Disposed = false;
        private readonly USBDevice USBDevice = null;
        private USBPipe InputPipe = null;
        private USBPipe OutputPipe = null;
        private object UsbLock = new();

        public UnifiedFlashingPlatformTransport(string DevicePath)
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

        public byte[] ExecuteRawMethod(byte[] RawMethod)
        {
            return ExecuteRawMethod(RawMethod, RawMethod.Length);
        }

        public byte[] ExecuteRawMethod(byte[] RawMethod, int Length)
        {
            byte[] Buffer = new byte[0xF000]; // Should be at least 0x4408 for receiving the GPT packet.
            byte[] Result = null;
            lock (UsbLock)
            {
                OutputPipe.Write(RawMethod, 0, Length);
                try
                {
                    int OutputLength = InputPipe.Read(Buffer);
                    Result = new byte[OutputLength];
                    System.Buffer.BlockCopy(Buffer, 0, Result, 0, OutputLength);
                }
                catch { } // Reboot command looses connection
            }
            return Result;
        }

        public void ExecuteRawVoidMethod(byte[] RawMethod)
        {
            ExecuteRawVoidMethod(RawMethod, RawMethod.Length);
        }

        public void ExecuteRawVoidMethod(byte[] RawMethod, int Length)
        {
            lock (UsbLock)
            {
                OutputPipe.Write(RawMethod, 0, Length);
            }
        }

        public byte[] ReadParam(string Param)
        {
            byte[] Request = new byte[0x0B];
            const string Header = "NOKXFR";

            Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes(Header), 0, Request, 0, Header.Length);
            Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes(Param), 0, Request, 7, Param.Length);

            byte[] Response = ExecuteRawMethod(Request);
            if ((Response == null) || (Response.Length < 0x10))
            {
                return null;
            }

            byte[] Result = new byte[Response[0x10]];
            Buffer.BlockCopy(Response, 0x11, Result, 0, Response[0x10]);
            return Result;
        }

        public string ReadStringParam(string Param)
        {
            byte[] Bytes = ReadParam(Param);
            if (Bytes == null)
            {
                return null;
            }

            return System.Text.Encoding.ASCII.GetString(Bytes).Trim('\0');
        }

        public string ReadDevicePlatformID()
        {
            return ReadStringParam("DPI");
        }

        public string ReadDeviceTerminalID()
        {
            return ReadStringParam("DTI");
        }

        public string ReadDeviceUniqueID()
        {
            return ReadStringParam("DUI");
        }

        public string ReadDeviceSerialNumber()
        {
            return ReadStringParam("SN");
        }

        public string ReadDeviceProcessorManufacturer()
        {
            return ReadStringParam("pm");
        }

        public string ReadUnlockID()
        {
            return ReadStringParam("UKID");
        }

        public void Relock()
        {
            byte[] Request = new byte[7];
            const string Header = "NOKXFO";
            Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes(Header), 0, Request, 0, Header.Length);
            ExecuteRawMethod(Request);
        }

        public void SwitchToMassStorageContext()
        {
            byte[] Request = new byte[7];
            const string Header = "NOKXCBM";
            Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes(Header), 0, Request, 0, Header.Length);
            ExecuteRawMethod(Request);
        }

        public void ContinueBoot()
        {
            byte[] Request = new byte[7];
            const string Header = "NOKXCBW";
            Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes(Header), 0, Request, 0, Header.Length);
            ExecuteRawMethod(Request);
        }

        public void Shutdown()
        {
            byte[] Request = new byte[7];
            const string Header = "NOKXCBZ";
            Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes(Header), 0, Request, 0, Header.Length);
            ExecuteRawVoidMethod(Request);
        }

        public void ResetPhone()
        {
            byte[] Request = new byte[7];
            const string Header = "NOKXCBR";
            Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes(Header), 0, Request, 0, Header.Length);
            ExecuteRawMethod(Request);
        }

        public ulong GetLogSize()
        {
            byte[] Request = new byte[0x10];
            const string Header = "NOKXFR";
            const string Param = "LZ";

            Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes(Header), 0, Request, 0, Header.Length);
            Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes(Param), 0, Request, 7, Param.Length);

            Request[14] = 1;
            Request[15] = 1;

            byte[] Response = ExecuteRawMethod(Request);
            if ((Response == null) || (Response.Length < 0x10))
            {
                return 0;
            }

            byte[] Result = new byte[Response[0x10]];
            Buffer.BlockCopy(Response, 0x11, Result, 0, Response[0x10]);

            return BitConverter.ToUInt64(Result.Reverse().ToArray(), 0);
        }

        // WIP!
        public string ReadLog()
        {
            byte[] Request = new byte[0x13];
            const string Header = "NOKXFX";
            ulong BufferSize = 0xF000 - 0xC;

            ulong Length = GetLogSize();
            if (Length == 0)
            {
                return null;
            }

            string LogContent = "";

            for (ulong i = 0; i < Length; i += BufferSize)
            {
                if (i + BufferSize > Length)
                {
                    BufferSize = Length - i;
                }
                uint BufferSizeInt = (uint)BufferSize;

                Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes(Header), 0, Request, 0, Header.Length);
                Request[6] = 1;
                Buffer.BlockCopy(BitConverter.GetBytes(BufferSizeInt).Reverse().ToArray(), 0, Request, 7, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(i).Reverse().ToArray(), 0, Request, 11, 8);

                byte[] Response = ExecuteRawMethod(Request);
                if ((Response == null) || (Response.Length < 0xC))
                {
                    return null;
                }

                int ResultLength = Response.Length - 0xC;
                byte[] Result = new byte[ResultLength];
                Buffer.BlockCopy(Response, 0xC, Result, 0, ResultLength);

                string PartialLogContent = System.Text.Encoding.ASCII.GetString(Result);

                LogContent += PartialLogContent;
            }

            return LogContent;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~UnifiedFlashingPlatformTransport()
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