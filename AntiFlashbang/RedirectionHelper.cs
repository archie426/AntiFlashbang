using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Rocket.Core.Logging;


//Much credits to defcon or kraken or zoomy of whoever
namespace AntiFlashbang
{
    public class OverrideWrapper
    {
        #region Properties
        
        public MethodInfo Original { get; }
        public MethodInfo Modified { get; }

        public IntPtr PtrOriginal { get; }
        public IntPtr PtrModified { get; }

        public OffsetBackup OffsetBackup { get; }

        public bool Detoured { get; private set; }
        public object Instance { get; }
        public bool Local { get; }
        
        #endregion

        public OverrideWrapper(MethodInfo original, MethodInfo modified, object instance = null)
        {
            // Set the variables
            Original = original;
            Modified = modified;
            Instance = instance;
            if (Modified.DeclaringType != null)
                Local = Modified.DeclaringType.Assembly == Assembly.GetExecutingAssembly();

            RuntimeHelpers.PrepareMethod(original.MethodHandle);
            RuntimeHelpers.PrepareMethod(modified.MethodHandle);
            PtrOriginal = Original.MethodHandle.GetFunctionPointer();
            PtrModified = Modified.MethodHandle.GetFunctionPointer();

            OffsetBackup = new OffsetBackup(PtrOriginal);
            Detoured = false;
        }
        
        
        public bool OverrideFunction(IntPtr ptrOriginal, IntPtr ptrModified)
        {
            try
            {
                switch (IntPtr.Size)
                {
                    case sizeof(Int32):
                        unsafe
                        {
                            byte* ptrFrom = (byte*)ptrOriginal.ToPointer();

                            *ptrFrom = 0x68; // PUSH
                            *((uint*)(ptrFrom + 1)) = (uint)ptrModified.ToInt32(); // Pointer
                            *(ptrFrom + 5) = 0xC3; // RETN

                            /* push, offset
                             * retn
                             * 
                             * 
                             */
                        }
                        break;
                    case sizeof(Int64):
                        unsafe
                        {
                            byte* ptrFrom = (byte*)ptrOriginal.ToPointer();

                            *ptrFrom = 0x48;
                            *(ptrFrom + 1) = 0xB8;
                            *((ulong*)(ptrFrom + 2)) = (ulong)ptrModified.ToInt64(); // Pointer
                            *(ptrFrom + 10) = 0xFF;
                            *(ptrFrom + 11) = 0xE0;

                            /* mov rax, offset
                             * jmp rax
                             * 
                             */
                        }
                        break;
                    default:
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return false;
            }
        }

        #region Public Functions
        public bool Override()
        {
            if (Detoured)
                return true;
            bool result = OverrideFunction(PtrOriginal, PtrModified);

            if (result)
                Detoured = true;

            return result;
        }
        
        public bool RevertOverride(OffsetBackup backup)
        {
            try
            {
                unsafe
                {
                    byte* ptrOriginal = (byte*)backup.Method.ToPointer();

                    *ptrOriginal = backup.A;
                    *(ptrOriginal + 1) = backup.B;
                    *(ptrOriginal + 10) = backup.C;
                    *(ptrOriginal + 11) = backup.D;
                    *(ptrOriginal + 12) = backup.E;
                    if (IntPtr.Size == sizeof(Int32))
                    {
                        *((uint*)(ptrOriginal + 1)) = backup.F32;
                        *(ptrOriginal + 5) = backup.G;
                    }
                    else
                    {
                        *((ulong*)(ptrOriginal + 2)) = backup.F64;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Revert()
        {
            if (!Detoured) return;
            bool result = RevertOverride(OffsetBackup);

            if (result)
                Detoured = false;
        }

        public object CallOriginal(object[] args, object instance = null)
        {
            Revert();
            object result = null;
            try
            {
                result = Original.Invoke(instance ?? Instance, args);
            }
            catch (Exception e)
            {
                Logger.Log("ERROR IN OVERRIDDEN METHOD: " + Original.Name);
                Logger.LogException(e);
            }

            Override();
            return result;
        }
        #endregion
    }
}