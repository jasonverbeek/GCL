using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace LHGameHack
{

    public class MemoryPointer
    {
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] buffer, int size, int lpNumberOfBytesWritten);

        public string pointerName;
        public int baseAddress;
        public int[] offsets;

        private GameProcess gp;
        private bool frozen;
        private byte[] frozenValue;
        private Thread? freezeThread;

        public MemoryPointer(string pointerName, int baseAddress, int[] offsets, GameProcess gp)
        {
            this.pointerName = pointerName;
            this.baseAddress = baseAddress;
            this.offsets = offsets;
            this.frozen = false;
            this.frozenValue = new byte[4];
            this.gp = gp;
        }

        public void Freeze()
        {
            int address = this.ToAddress();
            this.Freeze(this.ReadBytes(address));
        }

        public void Freeze(byte[] value)
        {
            if (this.freezeThread != null)
                return;
            this.frozenValue = value;
            this.frozen = true;
        }

        public void Thaw()
        {
            if (this.freezeThread == null)
                return;
            this.frozen = false;
        }

        public int ToAddress()
        {
            int address = this.ReadAddress(this.gp.baseAddress + this.baseAddress);
            Console.WriteLine(this.gp.baseAddress.ToString("X") + " + " + this.baseAddress.ToString("X") + " = " + address.ToString("X"));
            int addressBefore = address;
            for (int i = 0; i < this.offsets.Length - 1; i++)
            {
                addressBefore = address;
                address = this.ReadAddress(address + this.offsets[i]);
                Console.WriteLine("0x" + addressBefore.ToString("X") + " + " + "0x" + this.offsets[i].ToString("X") + " = " + "0x" + address.ToString("X"));
            }
            addressBefore = address;
            int offset = this.offsets[this.offsets.Length - 1];
            address = address + offset;
            Console.WriteLine("0x" + addressBefore.ToString("X") + " + " + "0x" + offset.ToString("X") + " = " + "0x" + address.ToString("X"));
            return address;
        }

        public int ReadAddress(int memoryAddress)
        {
            byte[] buffer = this.Read(memoryAddress);
            int value = BitConverter.ToInt32(buffer, 0);
            return value;
        }

        private byte[] Read(int memoryAddress)
        {
            return this.Read(memoryAddress, 4);
        }

        private byte[] Read(int memoryAddress, int bytes)
        {

            int bytesRead = 0;
            byte[] buffer = new byte[bytes];
            ReadProcessMemory(
                this.gp.processHandle,
                memoryAddress,
                buffer,
                buffer.Length,
                ref bytesRead
            );

            return buffer;
        }

        public int ReadInt32()
        {
            byte[] buffer = this.Read(this.ToAddress());
            return BitConverter.ToInt32(buffer, 0);
        }

        public byte[] ReadBytes(int bytes)
        {
            return this.Read(this.ToAddress());
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

        public void WriteInt32(int value)
        {
            byte[] byteValue = BitConverter.GetBytes(value);
            WriteProcessMemory(this.gp.processHandle, this.ToAddress(), byteValue, byteValue.Length, 0);
        }
    }
}