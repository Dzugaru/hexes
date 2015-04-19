using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NotUnityTests
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        static extern bool FreeLibrary(IntPtr hModule);

        static IntPtr pDll;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void VoidToVoid();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int IntToInt(int x);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int VoidToInt();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackToVoid(IntPtr cb);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void StringToVoid(IntPtr x);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PtrToVoid(IntPtr cb);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr VoidToPtr();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PtrPtrIntToVoid(IntPtr src, IntPtr dst, int num);


        public static IntToInt sqr;
        public static IntToInt dbl;
        public static VoidToInt five;
        public static PtrToVoid setLogger;
        public static VoidToVoid start;
        public static VoidToPtr getWorldBlockHandle;
        public static PtrPtrIntToVoid copyMemory;

        public static IntPtr GetCallbackPointer(Delegate d)
        {
            return Marshal.GetFunctionPointerForDelegate(d);
        }

        static void Main(string[] args)
        {
            pDll = LoadLibrary("../../../UnityProj/DEngine.dll");

            foreach (var funcField in typeof(Program).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
            {
                var unmanagedFunc = Marshal.GetDelegateForFunctionPointer(GetProcAddress(pDll, funcField.Name), funcField.FieldType);
                funcField.SetValue(null, unmanagedFunc);
            }



            setLogger(GetCallbackPointer((StringToVoid)(msgPtr =>
            {
                string msg = Marshal.PtrToStringUni(msgPtr);
                Console.WriteLine(msg);
            })));

            start();
            var wbhandle = getWorldBlockHandle();
           
            var testBlock = (DWorldBlock)Marshal.PtrToStructure(wbhandle, typeof(DWorldBlock));


            //sqr(9);
            //dbl(2);
            //five();

            Console.ReadLine();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HexXY
    {
        public int x, y;
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe public struct DWorldBlock
    {
        public HexXY position;
        public fixed int cellTypes[4];
        public fixed int cellTypeCounts[3];
        public int nonEmptyCellsCount;
    }
}
