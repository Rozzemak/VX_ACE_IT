using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
//using System.Windows.Forms;

namespace VX_ACE_IT_CORE.MVC.Model.GameProcess
{
    public class ProcessMethods
    {
        public readonly GameProcess _gameProcess;

        public ProcessMethods(GameProcess gameProcess)
        {
            this._gameProcess = gameProcess;
        }

        /// <summary>
        /// Generic acces to process memory for struct types.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lpBaseAddress">BaseAdress of wanted memory.</param>
        /// <returns></returns>
        public T Rpm<T>(IntPtr lpBaseAddress) where T : struct
        {
            T[] buffer = new T[SizeOf<T>()];
            ReadProcessMemory(_gameProcess.Process.Handle, lpBaseAddress, buffer, SizeOf<T>(), out var bytesread);
            return buffer.FirstOrDefault(); // [0] would be faster, but First() is safer. Eq of buffer[0] ?? default(T)
        }


        public T Rpm<T>(IntPtr lpBaseAddress, List<IntPtr> offsets, out IntPtr valAdress) where T : struct
        {
            IntPtr address = lpBaseAddress;

            var lastOffset = offsets.LastOrDefault();
            int index = offsets.Count - 1;
            if (index < 0)
                index = 0;

            for (int i = 0; i < index; i++)
            {
                address = Rpm<IntPtr>(IntPtr.Add(address, (int)offsets[i]));
            }

            valAdress = default(IntPtr);
            if (lastOffset!= valAdress)
            {
                valAdress = IntPtr.Add(address, (int)lastOffset);
            }
            
            return Rpm<T>(valAdress);
        }


        /// <summary>
        /// Example of non-generic acces to proc. mem, significantly faster ? 
        /// </summary>
        /// <param name="lpBaseAddress"></param>
        /// <returns></returns>
        private int Rpm(IntPtr lpBaseAddress)
        {
            byte[] buffer = new byte[(sizeof(int))];
            ReadProcessMemory(_gameProcess.Process.Handle, lpBaseAddress, buffer, 4, out var bytesread);
            return BitConverter.ToInt32(buffer, 0);
        }

        /// <summary>
        /// Generic acces to process memory for struct types.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lpBaseAddress">BaseAdress of wanted memory.</param>
        /// <returns></returns>
        public bool Wpm<T>(IntPtr lpBaseAddress, T value) where T : struct
        {
            var buffer = new T[SizeOf<T>()];
            buffer[0] = value;
            return WriteProcessMemory(_gameProcess.Process.Handle, lpBaseAddress, buffer, SizeOf<T>(), out var bytesread);
        }

        public bool Wpm<T>(IntPtr lpBaseAddress, T value, List<IntPtr> offsets) where T : struct
        {
            IntPtr address = lpBaseAddress;

            var lastOffset = offsets.LastOrDefault();
            int index = offsets.Count - 1;
            if (index < 0)
                index = 0;

            for (int i = 0; i < index; i++)
            {
                address = Rpm<IntPtr>(IntPtr.Add(address, (int)offsets[i]));
            }

            return Wpm<T>(IntPtr.Add(address, (int)lastOffset), value);
        }


        public int SizeOf<T>() where T : struct
        {
            return Marshal.SizeOf(default(T));
        }


        public bool Is64BitProcess(GameProcess gameProcess)
        {
            if (!Environment.Is64BitOperatingSystem)
                return false;

            if (!IsWow64Process(gameProcess.Process.Handle, out var isWow64Process))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return !isWow64Process;
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
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [MarshalAs(UnmanagedType.AsAny)] object lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process([In] IntPtr processHandle, [Out, MarshalAs(UnmanagedType.Bool)] out bool wow64Process);
    }
}
