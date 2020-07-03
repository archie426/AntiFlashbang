using System;
using System.Diagnostics;
using System.Reflection;

namespace AntiFlashbang
{
	#region SubClasses
    public class OffsetBackup
    {
	    #region Variables
	    public IntPtr Method;

	    public byte A, B, C, D, E, G;
	    public ulong F64;
	    public uint F32;
	    #endregion

	    public OffsetBackup(IntPtr method)
	    {
		    Method = method;

		    unsafe
		    {
			    byte* ptrMethod = (byte*)method.ToPointer();

			    A = *ptrMethod;
			    B = *(ptrMethod + 1);
			    C = *(ptrMethod + 10);
			    D = *(ptrMethod + 11);
			    E = *(ptrMethod + 12);
			    if (IntPtr.Size == sizeof(Int32))
			    {
				    F32 = *((uint*)(ptrMethod + 1));
				    G = *(ptrMethod + 5);
			    }
			    else
			    {
				    F64 = *((ulong*)(ptrMethod + 2));
			    }
		    }
	    }
    }
    #endregion
    
}