using System;
// Added for Intel MKL interop
using System.Security;
using System.Runtime.InteropServices;

namespace mkl
{
    [Serializable()]
    public class MKLException : System.Exception
    {
        public MKLException()
            : base() { }
        public MKLException(string message)
            : base(message) { }
        public MKLException(string message, System.Exception inner)
            : base(message, inner) { }
        protected MKLException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }

    public sealed class DFTI
    {
        private DFTI() { }

        /* These are the constants for DFTI
         * Based on the mkl_dfti.h file
         * mkl_dfti.h and DFTI 
         * Copyright (C) 2010 Intel Corporation. 
         * All Rights Reserved.
        */
        // Configuration parameters for DFTI
        public static int BACKWARD_SCALE = 5;
        public static int PLACEMENT = 11;
        public static int PACKED_FORMAT = 21;
        // Configuration values for DFTI
        public static int DOUBLE = 36;
        public static int REAL = 33;
        public static int NOT_INPLACE = 44;
        public static int PACK_FORMAT = 55;
        // Predefined errors for DFTI and their values
        public static int NO_ERROR = 0;
        public static int MEMORY_ERROR = 1;
        public static int INVALID_CONFIGURATION = 2;
        public static int INCONSISTENT_CONFIGURATION = 3;
        public static int NUMBER_OF_THREADS_ERROR = 8;
        public static int MULTITHREADED_ERROR = 4;
        public static int BAD_DESCRIPTOR = 5;
        public static int UNIMPLEMENTED = 6;
        public static int MKL_INTERNAL_ERROR = 7;
        public static int LENGTH_EXCEEDS_INT32 = 9;

        // Wrappers to native DFTI native calls
        public static int DftiCreateDescriptor(ref IntPtr desc,
            int precision, int domain, int dimension, int length)
        {
            return DFTINative.DftiCreateDescriptor(ref desc,
                precision, domain, dimension, length);
        }

        public static int DftiFreeDescriptor(ref IntPtr desc)
        {
            return DFTINative.DftiFreeDescriptor(ref desc);
        }

        public static int DftiSetValue(IntPtr desc,
            int config_param, int config_val)
        {
            return DFTINative.DftiSetValue(desc,
                config_param, __arglist(config_val));
        }

        public static int DftiSetValue(IntPtr desc,
            int config_param, double config_val)
        {
            return DFTINative.DftiSetValue(desc,
                config_param, __arglist(config_val));
        }

        public static int DftiGetValue(IntPtr desc,
            int config_param, ref double config_val)
        {
            return DFTINative.DftiGetValue(desc,
                config_param, __arglist(ref config_val));
        }

        public static int DftiCommitDescriptor(IntPtr desc)
        {
            return DFTINative.DftiCommitDescriptor(desc);
        }

        public static int DftiComputeForward(IntPtr desc,
            [In] double[] x_in, [Out] double[] x_out)
        {
            return DFTINative.DftiComputeForward(desc, x_in, x_out);
        }

        public static int DftiComputeBackward(IntPtr desc,
            [In] double[] x_in, [Out] double[] x_out)
        {
            return DFTINative.DftiComputeBackward(desc, x_in, x_out);
        }
    }

    // Native declarations that call the
    // mkl_rt DLL functions
    // They require Intel Math Kernel Library 10.3 or higher
    // installed on the developer computer
    /* Intel Math Kernel Library 10.3 
     * Copyright (C) 2010 Intel Corporation. 
     * All Rights Reserved.
    */
    [SuppressUnmanagedCodeSecurity]
    internal sealed class DFTINative
    {
        [DllImport("mkl_rt", CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiCreateDescriptor(ref IntPtr desc,
            int precision, int domain, int dimension, int length);
        [DllImport("mkl_rt", CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiCommitDescriptor(IntPtr desc);
        [DllImport("mkl_rt", CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiFreeDescriptor(ref IntPtr desc);
        [DllImport("mkl_rt", CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiSetValue(IntPtr desc,
            int config_param, __arglist);
        [DllImport("mkl_rt", CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiGetValue(IntPtr desc,
            int config_param, __arglist);
        [DllImport("mkl_rt", CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiComputeForward(IntPtr desc,
            [In] double[] x_in, [Out] double[] x_out);
        [DllImport("mkl_rt", CallingConvention = CallingConvention.Cdecl,
             ExactSpelling = true, SetLastError = false)]
        internal static extern int DftiComputeBackward(IntPtr desc,
            [In] double[] x_in, [Out] double[] x_out);
    }
}