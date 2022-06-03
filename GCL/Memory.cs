using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace GCL
{

    public class MemoryPointer
    {
        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(int hProcess, Int64 lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(int hProcess, Int64 lpBaseAddress, byte[] buffer, int size, int lpNumberOfBytesWritten);

        public long baseAddress;
        public int[] offsets;

        public readonly GameModule gm;

        public MemoryPointer(long baseAddress, int[] offsets, GameModule gm)
        {
            this.baseAddress = baseAddress;
            this.offsets = offsets;
            this.gm = gm;
        }

        public long ToAddress()
        {
            return this.ToAddress(false);
        }

        public long ToAddress(bool debug)
        {
            long address = this.ReadAddress(this.gm.baseAddress + this.baseAddress);
            if (debug)
                Debug.WriteLine(this.gm.baseAddress.ToString("X") + " + " + this.baseAddress.ToString("X") + " = " + address.ToString("X"));
            long addressBefore;
            for (int i = 0; i < this.offsets.Length - 1; i++)
            {
                addressBefore = address;
                address = this.ReadAddress(address + this.offsets[i]);
                if (debug)
                    Debug.WriteLine("0x" + addressBefore.ToString("X") + " + " + "0x" + this.offsets[i].ToString("X") + " = " + "0x" + address.ToString("X"));
            }
            addressBefore = address;
            int offset = this.offsets[offsets.Length - 1];
            address += offset;
            if (debug)
                Debug.WriteLine("0x" + addressBefore.ToString("X") + " + " + "0x" + offset.ToString("X") + " = " + "0x" + address.ToString("X"));
            return address;
        }

        public long ReadAddress(long memoryAddress)
        {
            byte[] buffer = this.Read(memoryAddress, 16);
            long value = BitConverter.ToInt64(buffer);
            return value;
        }

        private byte[] Read(long memoryAddress)
        {
            return this.Read(memoryAddress, 4);
        }

        private byte[] Read(long memoryAddress, int bytes)
        {
            if (this.gm.process.HasExited) throw new ProcessHasExitedException();
            int bytesRead = 0;
            byte[] buffer = new byte[bytes];
            bool success = ReadProcessMemory(
                this.gm.processHandle,
                memoryAddress,
                buffer,
                buffer.Length,
                ref bytesRead
            );
            if (!success) throw new MemoryReadWriteException();

            return buffer;
        }

        public int ReadInt32()
        {
            byte[] buffer = this.Read(this.ToAddress());
            return BitConverter.ToInt32(buffer, 0);
        }
        public long ReadInt64()
        {
            byte[] buffer = this.Read(this.ToAddress(), 16);
            return BitConverter.ToInt64(buffer, 0);
        }

        public byte[] ReadBytes(int bytes)
        {
            return this.Read(this.ToAddress(), bytes);
        }

        public byte[] ReadBytes()
        {
            return this.ReadBytes(4);
        }

        public float ReadFloat()
        {
            byte[] buffer = this.Read(this.ToAddress());
            return BitConverter.ToSingle(buffer, 0);
        }
        public bool ReadBool()
        {
            int intValue = this.ReadInt32();
            return intValue != 0;
        }

        public void Write(long memoryAddress, byte[] value)
        {
            if (this.gm.process.HasExited) throw new ProcessHasExitedException();
            bool success = WriteProcessMemory(this.gm.processHandle, memoryAddress, value, value.Length, 0); 
            if (!success) throw new MemoryReadWriteException();
        }

        public void WriteInt32(int value)
        {
            byte[] byteValue = BitConverter.GetBytes(value);
            Write(this.ToAddress(), byteValue);
        }
        public void WriteFloat(float value)
        {
            byte[] byteValue = BitConverter.GetBytes(value);
            Write(this.ToAddress(), byteValue);
        }
    }
}