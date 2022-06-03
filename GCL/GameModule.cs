using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GCL
{
    public class GameModule
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        const int DELETE = 0x00010000;
        const int READ_CONTROL = 0x00020000;
        const int WRITE_DAC = 0x00040000;
        const int WRITE_OWNER = 0x00080000;
        const int SYNCHRONIZE = 0x00100000;
        const int END = 0xFFF; //if you have Windows XP or Windows Server 2003 you must change this to 0xFFFF
        const int PROCESS_ALL_ACCESS = (DELETE | READ_CONTROL | WRITE_DAC | WRITE_OWNER | SYNCHRONIZE | END);

        private readonly string processName;
        public Process process;
        public int processHandle;
        public long baseAddress;
        public IntPtr hWnd;

        public GameModule(string processName, string moduleName)
        {
            object[] data = Initialize(processName, moduleName);

            this.processName = (string)data[0];
            this.process = (Process)data[1];
            this.processHandle = (int)data[2];
            this.baseAddress = (long)data[3];
            this.hWnd = (IntPtr)data[4];
        }

        public GameModule(string processName)
        {
            string moduleName = processName;
            if (!moduleName.EndsWith(".exe"))
                moduleName = String.Format("{0}.exe", moduleName);
            object[] data = Initialize(processName, moduleName);

            this.processName = (string)data[0];
            this.process = (Process)data[1];
            this.processHandle = (int)data[2];
            this.baseAddress = (long)data[3];
            this.hWnd = (IntPtr)data[4];
        }

        private object[] Initialize(string processName, string moduleName)
        {
            processName = processName.Replace(".exe", null);
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0) throw new ProcessNotFoundException("Process not found, start the game first.");
            process = processes[0];
            processHandle = (int)OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);
            baseAddress = this.GetModuleAddress(moduleName);
            hWnd = this.process.MainWindowHandle;
            return new object[] {
                processName,
                process,
                processHandle,
                baseAddress,
                hWnd
            }; // TODO fix constructors
        }

        public GameModule GetModule(string moduleName)
        {
            return new GameModule(this.processName, moduleName);
        }

        public void FocusWindow()
        {
            SetForegroundWindow(hWnd);
        }

        public static void FocusSelf()
        {
            Process p = Process.GetCurrentProcess();
            SetForegroundWindow(p.MainWindowHandle);
        }

        public MemoryPointer CreatePointer(int baseAddress, int[] offsets)
        {
            return new MemoryPointer(baseAddress, offsets, this);
        }

        public long GetModuleAddress(string moduleName)
        {
            for (int i = 0; i < process.Modules.Count; i++)
            {
                if (this.process.Modules[i].ModuleName == moduleName)
                {
                    return (long)this.process.Modules[i].BaseAddress;
                }
            }
            return 0x00000000;
        }
    }
}
