using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LHGameHack
{
    public class GameProcess
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        const int DELETE = 0x00010000;
        const int READ_CONTROL = 0x00020000;
        const int WRITE_DAC = 0x00040000;
        const int WRITE_OWNER = 0x00080000;
        const int SYNCHRONIZE = 0x00100000;
        const int END = 0xFFF; //if you have Windows XP or Windows Server 2003 you must change this to 0xFFFF
        const int PROCESS_ALL_ACCESS = (DELETE | READ_CONTROL | WRITE_DAC | WRITE_OWNER | SYNCHRONIZE | END);

        public Process process;
        public int processHandle;
        public int baseAddress;


        public GameProcess(string processName, string moduleName)
        {
            this.process = Process.GetProcessesByName(processName)[0];
            this.processHandle = (int)OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);
            this.baseAddress = this.GetModuleAddress(moduleName);
        }

        public MemoryPointer CreatePointer(string pointerName, int baseAddress, int[] offsets)
        {
            return new MemoryPointer(pointerName, baseAddress, offsets, this);
        }

        private int GetModuleAddress(string moduleName)
        {
            for (int i = 0; i < process.Modules.Count; i++)
            {
                if (this.process.Modules[i].ModuleName == moduleName)
                {
                    return (int)this.process.Modules[i].BaseAddress;
                }
            }
            return 0x00000000;
        }
    }
}
