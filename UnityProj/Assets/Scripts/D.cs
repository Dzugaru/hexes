using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class D : MonoBehaviour
{
    [DllImport("kernel32.dll")]
    static extern IntPtr LoadLibrary(string dllToLoad);

    [DllImport("kernel32.dll")]
    static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

    [DllImport("kernel32.dll")]
    static extern bool FreeLibrary(IntPtr hModule);

    IntPtr pDll;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void VoidToVoid();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int IntToInt(int x);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int VoidToInt();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void PtrToVoid(IntPtr cb);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr VoidToPtr();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void PtrPtrToVoid(IntPtr name, IntPtr cb);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void HexXYShowObjectTypeFloatToVoid(Engine.HexXY p, ShowObjectType objName, float durSecs);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void HexXYToHexXYToVoid(Engine.HexXY from, Engine.HexXY to);


    private static PtrPtrToVoid setCallback;

    public static PtrToVoid setLogging;
    public static VoidToVoid onStart;    
    public static VoidToPtr queryWorld;    
    public static HexXYToHexXYToVoid calcAndShowPath;



    public static IntPtr GetCallbackPointer(Delegate d)
    {
        return Marshal.GetFunctionPointerForDelegate(d);
    }

    public static string GetStringFromPointer(IntPtr pStr)
    {       
        return Marshal.PtrToStringAnsi(pStr);
    }

    public static void SetCallback(string name, Delegate method)
    {
        var strPtr = Marshal.StringToHGlobalAnsi(name);
        setCallback(strPtr, GetCallbackPointer(method));
        Marshal.FreeHGlobal(strPtr);
    }


    void Awake()
    {
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            UnityEditor.EditorApplication.playmodeStateChanged = OnPlaymodeStateChanged;
        }
#endif

        pDll = LoadLibrary("DEngine.dll");

        foreach (var funcField in typeof(D).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public))
        {
            var unmanagedFunc = Marshal.GetDelegateForFunctionPointer(GetProcAddress(pDll, funcField.Name), funcField.FieldType);
            funcField.SetValue(null, unmanagedFunc);
        }
    }

#if UNITY_EDITOR
    private void OnPlaymodeStateChanged()
    {
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            Debug.Log("Engine freed: " + FreeLibrary(pDll));            
        }
    }
#endif

}

