using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace VX_ACE_IT_CORE.MVC.Model.GameProcess
{
    public class ProcessMethods
    {
        private readonly GameProcess _gameProcess;

        public ProcessMethods(GameProcess gameProcess)
        {
            this._gameProcess = gameProcess;
        }

        public T RPM<T>(IntPtr lpBaseAddress, int byteSize = 4)
        {
            T buffer = default(T);
            ReadProcessMemory(_gameProcess.Process.Handle, lpBaseAddress, buffer, byteSize, out var bytesread);
            return buffer;
        }

        public int RPM(IntPtr lpBaseAddress)
        {
            byte[] buffer = new byte[(sizeof(int))];
            ReadProcessMemory(_gameProcess.Process.Handle, lpBaseAddress, buffer, 4, out var bytesread);
            return BitConverter.ToInt32(buffer, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hProcess"></param>
        /// <param name="lpBaseAddress"></param>
        /// <param name="lpBuffer"></param>
        /// <param name="dwSize"></param>
        /// <param name="lpNumberOfBytesRead">Pointer to numbers read. (past-time), can be null </param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out, MarshalAs(UnmanagedType.AsAny)] object lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

    }
}
