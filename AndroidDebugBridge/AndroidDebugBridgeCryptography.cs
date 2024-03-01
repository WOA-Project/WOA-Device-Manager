using System.IO;
using System.Numerics;
using System.Security.Cryptography;

namespace AndroidDebugBridge
{
    internal class AndroidDebugBridgeCryptography
    {
        private static BigInteger EGCD(BigInteger left,
                              BigInteger right,
                          out BigInteger leftFactor,
                          out BigInteger rightFactor)
        {
            leftFactor = 0;
            rightFactor = 1;
            BigInteger u = 1;
            BigInteger v = 0;
            BigInteger gcd = 0;

            while (left != 0)
            {
                BigInteger q = right / left;
                BigInteger r = right % left;

                BigInteger m = leftFactor - (u * q);
                BigInteger n = rightFactor - (v * q);

                right = left;
                left = r;
                leftFactor = u;
                rightFactor = v;
                u = m;
                v = n;

                gcd = right;
            }

            return gcd;
        }

        private static BigInteger ModInverse(BigInteger value, BigInteger modulo)
        {
            BigInteger egcd = EGCD(value, modulo, out BigInteger x, out _);
            if (1 != egcd)
            {
                //throw new ArgumentException("Invalid modulo", nameof(modulo));
            }

            if (x < 0)
            {
                x += modulo;
            }

            return x % modulo;
        }

        internal static byte[] ADBRSAToBuffer(uint n0inv, uint[] n, uint[] rr, int exponent)
        {
            int len = n.Length;

            byte[] buffer = new byte[12 + (8 * len)];
            using MemoryStream memoryStream = new(buffer);
            using BinaryWriter binaryWriter = new(memoryStream);

            binaryWriter.Write(len);
            binaryWriter.Write(n0inv);

            foreach (uint nElement in n)
            {
                binaryWriter.Write(nElement);
            }

            foreach (uint rrElement in rr)
            {
                binaryWriter.Write(rrElement);
            }

            binaryWriter.Write(exponent);

            return buffer;
        }

        internal static (uint n0inv, uint[] n, uint[] rr, int exponent) BufferToADBRSA(byte[] buffer)
        {
            using MemoryStream memoryStream = new(buffer);
            using BinaryReader binaryReader = new(memoryStream);

            int len = binaryReader.ReadInt32();

            if (buffer.Length != 12 + (8 * len))
            {
                throw new InvalidDataException("Invalid ADB RSA Buffer!");
            }

            uint n0inv = binaryReader.ReadUInt32();

            uint[] n = new uint[len];
            for (int i = 0; i < len; i++)
            {
                n[i] = binaryReader.ReadUInt32();
            }

            uint[] rr = new uint[len];
            for (int i = 0; i < len; i++)
            {
                rr[i] = binaryReader.ReadUInt32();
            }

            int exponent = binaryReader.ReadInt32();

            return (n0inv, n, rr, exponent);
        }

        internal static (uint n0inv, uint[] n, uint[] rr, int exponent) ConvertRSAToADBRSA(RSAParameters publicKeyParameters)
        {
            byte[] modulus = publicKeyParameters.Modulus!;
            byte[] exponent = publicKeyParameters.Exponent!;

            const int keyLengthInDWORDs = 64; // 2048 bits / 256 bytes / 64 DWORDs

            int e = (int)new BigInteger(exponent);

            BigInteger r32 = new BigInteger(1) << 32;

            BigInteger n = new(modulus);
            BigInteger r = new BigInteger(1) << (keyLengthInDWORDs * 32);
            BigInteger rr = BigInteger.ModPow(r, new BigInteger(2), n);

            BigInteger remainder = BigInteger.Remainder(n, r32);
            BigInteger tn0inv = ModInverse(remainder, r32);
            uint n0inv = (uint)(BigInteger.Negate(tn0inv) & uint.MaxValue);

            uint[] nTable = new uint[keyLengthInDWORDs];
            uint[] rrTable = new uint[keyLengthInDWORDs];

            for (int i = 0; i < keyLengthInDWORDs; i++)
            {
                rr = BigInteger.DivRem(rr, r32, out remainder);
                rrTable[i] = (uint)remainder;

                n = BigInteger.DivRem(n, r32, out remainder);
                nTable[i] = (uint)(remainder & uint.MaxValue);
            }

            return (n0inv, nTable, rrTable, e);
        }
    }
}