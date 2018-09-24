﻿using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Generic acces to process memory for struct types.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lpBaseAddress">BaseAdress of wanted memory.</param>
        /// <returns></returns>
        public unsafe T Rpm<T>(IntPtr lpBaseAddress) where T : struct 
        {
            T[] buffer = new T[SizeOf<T>()];
            ReadProcessMemory(_gameProcess.Process.Handle, lpBaseAddress, buffer, SizeOf<T>(), out var bytesread);
            return buffer.First(); // [0] would be faster, but First() is safer. Eq of buffer[0] ?? default(T)
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

        public int SizeOf<T>() where T : struct
        {
            return Marshal.SizeOf(default(T));
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
