namespace Playground
{
    internal class Debug
    {

        public static void ParseUsbLyzerTrace(string path)
        {
            string[] lines = File.ReadAllLines(path);
            string header = "Type,Seq,Time,Elapsed,Duration,Request,Request Details,Raw Data,I/O,C:I:E,Device Object,Device Name,Driver Name,IRP,Status";

            int beginning = 0;
            for (beginning = 0; beginning < lines.Length; beginning++)
            {
                if (lines[beginning] == header)
                {
                    beginning++;
                    break;
                }
            }

            //uint CommandPayloadLength = 0;

            for (beginning++; beginning < lines.Length; beginning++)
            {
                string line = lines[beginning];

                if (!line.Contains(","))
                {
                    break;
                }

                string[] parts = line.Split(',');
                string type = parts[0];
                string seq = parts[1];
                string time = parts[2];
                string elapsed = parts[3];
                string duration = parts[4];
                string request = parts[5];
                string requestDetails = parts[6];
                string rawData = parts[7];
                string io = parts[8];
                string cie = parts[9];
                string deviceObject = parts[10];
                string deviceName = parts[11];
                string driverName = parts[12];
                string irp = parts[13];
                string status = parts[14];
                string byteBuffer = "";
                byte[] buffer = Array.Empty<byte>();
                if (parts.Length > 15)
                {
                    byteBuffer = parts[15];
                    buffer = byteBuffer.Split(' ').Select(x => Convert.ToByte(x, 16)).ToArray();
                }

                /*Console.WriteLine($"Type: {type}");
                Console.WriteLine($"Seq: {seq}");
                Console.WriteLine($"Time: {time}");
                Console.WriteLine($"Elapsed: {elapsed}");
                Console.WriteLine($"Duration: {duration}");
                Console.WriteLine($"Request: {request}");
                Console.WriteLine($"Request Details: {requestDetails}");
                Console.WriteLine($"Raw Data: {rawData}");
                Console.WriteLine($"I/O: {io}");
                Console.WriteLine($"C:I:E: {cie}");
                Console.WriteLine($"Device Object: {deviceObject}");
                Console.WriteLine($"Device Name: {deviceName}");
                Console.WriteLine($"Driver Name: {driverName}");
                Console.WriteLine($"IRP: {irp}");
                Console.WriteLine($"Status: {status}");
                if (buffer != null)
                {
                    Console.WriteLine(BitConverter.ToString(buffer));
                }
                Console.WriteLine();*/


                if (buffer != null && buffer.Length != 0 && driverName == "ACPI")
                {
                    /*if (CommandPayloadLength == 0)
                    {
                        bool Incoming = io == "in";

                        (uint CommandIdentifier, uint FirstArgument, uint SecondArgument, CommandPayloadLength, uint CommandPayloadCrc) = AndroidDebugBridgeMessaging.BufferToCommandPacket(buffer);
                        Console.WriteLine($"{(Incoming ? '<' : '>')} new AndroidDebugBridgeMessage(AndroidDebugBridgeCommands.{System.Text.Encoding.ASCII.GetString(buffer[0..4])}, 0x{FirstArgument:X8}, 0x{FirstArgument:X8}, );");
                    }
                    else
                    {
                        Console.WriteLine(BitConverter.ToString(buffer));
                        Console.WriteLine(System.Text.Encoding.UTF8.GetString(buffer));
                        CommandPayloadLength = 0;
                    }*/

                    bool Incoming = io == "in";

                    string res = System.Text.Encoding.ASCII.GetString(buffer);
                    if (res.StartsWith("NOKXFR"))
                    {
                        Console.WriteLine($"{(Incoming ? '<' : '>')} {BitConverter.ToString(buffer[7..])}");
                        Console.WriteLine("Read Param: " + res[7..]);
                    }
                    else
                    {
                        Console.WriteLine($"{(Incoming ? '<' : '>')} {BitConverter.ToString(buffer)}");
                        Console.WriteLine($"{(Incoming ? '<' : '>')} {System.Text.Encoding.ASCII.GetString(buffer)}");
                    }

                    Console.WriteLine();
                }
            }
        }
    }
}
