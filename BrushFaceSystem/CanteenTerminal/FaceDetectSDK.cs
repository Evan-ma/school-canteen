#define USE_LIB_35

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace FaceSDK
{
    public class FaceDetectSDK : FaceDetectSDK30
    {
#if USE_LIB_35
        public const int LibVersion = 35;
#else
        public const int LibVersion = 30;
#endif

        private static bool _isInitialized = false;

        public static int Init(int imgSize)
        {
            if (_isInitialized) return 0;
            int ret;
            ret = FaceDetectSDK30.PFD_Init(imgSize);
            if (ret != 0)
            {
                Console.WriteLine("EVAL_x64_Accuracy3.0.dll Init error!");
                return ret;
            }
#if USE_LIB_35
            ret = PFD_Init(imgSize);
            if (ret != 0)
            {
                Console.WriteLine("EVAL_x64_Accuracy3.5.dll Init error!");
            }
#endif
            _isInitialized = true;
            return ret;
        }
        public static int Exit()
        {
            if (!_isInitialized) return 0;
            int ret;
            ret = FaceDetectSDK30.PFD_Exit();
            if (ret != 0)
            {
                Console.WriteLine("EVAL_x64_Accuracy3.0.dll Exit error!");
            }
#if USE_LIB_35
            ret = PFD_Exit();
            if (ret != 0)
            {
                Console.WriteLine("EVAL_x64_Accuracy3.5.dll Exit error!");
            }
#endif
            _isInitialized = false;
            return ret;
        }
#if USE_LIB_35
        [DllImport("EVAL_x64_Accuracy35.dll")]
        new public static extern int PFD_Init(int imgSize);
        [DllImport("EVAL_x64_Accuracy35.dll")]
        new public static extern int PFD_Exit();
        //[DllImport("EVAL_x64_Accuracy35.dll")]
        //public static extern int PFD_FaceRecog([MarshalAs(UnmanagedType.LPArray)] byte[] mem, ref PFD_FACE_DETECT faceInfo,int faceInfoFlag,short faceRote);
        //[DllImport("EVAL_x64_Accuracy35.dll")]
        //public static extern int PFD_AgrRecogImg([MarshalAs(UnmanagedType.LPArray)] byte[] mem, ref PFD_AGR_DETECT agrInfo, short faceRote);
        //[DllImport("EVAL_x64_Accuracy35.dll")]
        //public static extern int PFD_SmileRecogImg([MarshalAs(UnmanagedType.LPArray)] byte[] mem, ref PFD_SMILE_DETECT smileInfo, short faceRote);
        //[DllImport("EVAL_x64_Accuracy35.dll")]
        //public static extern int PFD_DirectRecogImg([MarshalAs(UnmanagedType.LPArray)] byte[] mem, ref PFD_DIRECT_DETECT directInfo, short faceRote);
        //[DllImport("EVAL_x64_Accuracy35.dll")]
        //public static extern int PFD_BlinkRecogImg([MarshalAs(UnmanagedType.LPArray)] byte[] mem, ref PFD_BLINK_DETECT blinkInfo, short faceRote);
        [DllImport("EVAL_x64_Accuracy35.dll")]
        new public static extern int PFD_GetFeature([MarshalAs(UnmanagedType.LPArray)] byte[] mem, ref PFD_FACE_DETECT faceInfo, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] feature, short faceRote, short min_width);
        //[DllImport("EVAL_x64_Accuracy35.dll")]
        //public static extern int PFD_GetFeatureLog([MarshalAs(UnmanagedType.LPArray)] byte[] mem, ref PFD_FACE_DETECT_LOG faceInfo, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] feature, short faceRote, short min_width);
        [DllImport("EVAL_x64_Accuracy35.dll")]
        new public static extern int PFD_GetFeatureByManual([MarshalAs(UnmanagedType.LPArray)] byte[] mem, ref PFD_DETECT_INFO faceInfo, IntPtr feature, short faceRote, ref short fsize);
        [DllImport("EVAL_x64_Accuracy35.dll")]
        new public static extern int PFD_FeatureMatching(short flen1, [MarshalAs(UnmanagedType.LPArray)] byte[] feature1, short flen2, [MarshalAs(UnmanagedType.LPArray)] byte[] feature2);
        //[DllImport("EVAL_x64_Accuracy35.dll")]
        //public static extern int PDB_StoreFeature(int dbId, int id, int usrId, short fsize, [MarshalAs(UnmanagedType.LPArray)] byte[] feature);
        //[DllImport("EVAL_x64_Accuracy35.dll")]
        //public static extern int PDB_DeleteFeature(int dbId, int usid);
        //[DllImport("EVAL_x64_Accuracy35.dll")]
        //public static extern int PDB_DeleteFeatureById(int dbId, int id);
        //[DllImport("EVAL_x64_Accuracy35.dll")]
        //public static extern int PDB_MatchFeature(int dbId, uint threshold, short fsize, [MarshalAs(UnmanagedType.LPArray)] byte[] feature, ushort num, [MarshalAs(UnmanagedType.LPArray)] int[] candidate, [MarshalAs(UnmanagedType.LPArray)] short[] score);
        //[DllImport("EVAL_x64_Accuracy35.dll")]
        //public static extern int PDB_ResetDataBase(int dbId);
        //[DllImport("EVAL_x64_Accuracy35.dll")]
        //public static extern int PDB_DeleteDataBase(int dbId);
        //[DllImport("EVAL_x64_Accuracy35.dll")]
        //public static extern int PDB_AddDataBase(int maxFaceNum);
        //[DllImport("EVAL_x64_Accuracy35.dll")]
        //public static extern int PDB_ChangeFeature(int dbId, int usid, short fsize, [MarshalAs(UnmanagedType.LPArray)] byte[] feature);
        //[DllImport("EVAL_x64_Accuracy35.dll")]
        //public static extern int PDB_GetUserId(int dbId, int id);
#endif
    }
}
