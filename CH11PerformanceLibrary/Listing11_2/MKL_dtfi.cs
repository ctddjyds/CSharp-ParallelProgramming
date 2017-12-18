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
        public static int PRECISION = 3;
        public static int FORWARD_DOMAIN = 0;
        public static int DIMENSION = 1;
        public static int LENGTHS = 2;
        public static int NUMBER_OF_TRANSFORMS = 7;
        public static int FORWARD_SCALE = 4;
        public static int BACKWARD_SCALE = 5;
        public static int PLACEMENT = 11;
        public static int COMPLEX_STORAGE = 8;
        public static int REAL_STORAGE = 9;
        public static int CONJUGATE_EVEN_STORAGE = 10;
        public static int DESCRIPTOR_NAME = 20;
        public static int PACKED_FORMAT = 21;
        public static int NUMBER_OF_USER_THREADS = 26;
        public static int INPUT_DISTANCE = 14;
        public static int OUTPUT_DISTANCE = 15;
        public static int INPUT_STRIDES = 12;
        public static int OUTPUT_STRIDES = 13;
        public static int ORDERING = 18;
        public static int TRANSPOSE = 19;
        public static int COMMIT_STATUS = 22;
        public static int VERSION = 23;
        // Configuration values for DFTI
        public static int SINGLE = 35;
        public static int DOUBLE = 36;
        public static int COMPLEX = 32;
        public static int REAL = 33;
        public static int INPLACE = 43;
        public static int NOT_INPLACE = 44;
        public static int COMPLEX_COMPLEX = 39;
        public static int REAL_REAL = 42;
        public static int COMPLEX_REAL = 40;
        public static int REAL_COMPLEX = 41;
        public static int COMMITTED = 30;
        public static int UNCOMMITTED = 31;
        public static int ORDERED = 48;
        public static int BACKWARD_SCRAMBLED = 49;
        public static int NONE = 53;
        public static int CCS_FORMAT = 54;
        public static int PACK_FORMAT = 55;
        public static int PERM_FORMAT = 56;
        public static int CCE_FORMAT = 57;
        public static int VERSION_LENGTH = 198;
        public static int MAX_NAME_LENGTH = 10;
        public static int MAX_MESSAGE_LENGTH = 40;
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