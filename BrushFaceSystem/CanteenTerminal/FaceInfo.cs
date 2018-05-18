using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceSDK
{
    /*********************************人脸信息类*******************************************/
    public class FaceInfo : FaceData //一张人脸信息
    {
        public int PictureQuality = 0; //跟踪使用：记录最佳图片质量
        public DateTime inTime; //人脸进入时间
        public DateTime procTime; //识别处理时间
        public DateTime shotTime;//抓拍的时间戳
        public Image FaceShotBmp;     //人脸特写
        public Image FullViewBmp;
        private FaceDetectSDK.PFD_DETECT_INFO info;

        public FaceInfo() { }
        public FaceInfo(FaceData fd)
        {
            this.faceid = fd.faceid;
            this.userid = fd.userid;
            this.feature = fd.feature;
            this.featureLenth = fd.featureLenth;
            this.angleType = fd.angleType;
            this.text = fd.text;
        }
        public void copyContent(FaceInfo f)
        {
            if (f == this) return;
            featureLenth = f.featureLenth;
            feature = f.feature;
            angleType = f.angleType;
            text = f.text;
            PictureQuality = f.PictureQuality;
            //inTime = f.inTime;
            info = f.info;
            procTime = f.procTime;
            shotTime = f.shotTime;
            FaceShotBmp = f.FaceShotBmp;
            FullViewBmp = f.FullViewBmp;
        }
        public void setInfo(FaceDetectSDK.PFD_DETECT_INFO i) { info = i; }
        public void setInfo(FaceInfo f) { info = f.info; }
        public int age { get { return info.age; } }       //年龄 
        public Gender gender { get { return info.gen == 0 ? Gender.Mail : Gender.Femail; } }    //性别
        public int smile { get { return info.smile; } }//笑容程度
        public int rollAngle { get { return info.roll; } }   //人脸倾斜角度(-180~180度，顺时针是正值)-45~45
        public int yawAngle { get { return info.yaw; } }    //人脸摇头角度(-180~180度，向左摇头是正值)-40~40
        public int pitchAngle { get { return info.pitch; } }    //人脸抬头角度(-180~180度，抬头是正值)-45~30
        public int posLeft { get { return info.faceInfo.rect_l; } }     //人脸最左侧位置
        public int posRight { get { return info.faceInfo.rect_r; } }     //人脸最右侧位置
        public int posTop { get { return info.faceInfo.rect_t; } }     //人脸最上侧位置
        public int posBottom { get { return info.faceInfo.rect_b; } }    //人脸最下侧位置
        public int FaceWidth() { return info.faceInfo.rect_r - info.faceInfo.rect_l; }
        public int FaceHight() { return info.faceInfo.rect_b - info.faceInfo.rect_t; }
        public FaceDetectSDK.PFD_DETECT_INFO getSDKInfo() { return info; }
        public void setPos(FaceInfo f)
        {
            info.faceInfo = f.info.faceInfo;
        }
        public void setPos(FaceDetectSDK.PFD_FACE_POSITION p)
        {
            info.faceInfo = p;
        }
        public void setPos(Rectangle rect)
        {
            info.faceInfo.rect_l = (short)rect.X;
            info.faceInfo.rect_r = (short)rect.Right;
            info.faceInfo.rect_t = (short)rect.Y;
            info.faceInfo.rect_b = (short)rect.Bottom;
        }
        public void Offset(Point p)
        {
            info.faceInfo.rect_l += (short)p.X;
            info.faceInfo.rect_r += (short)p.X;
            info.faceInfo.rect_t += (short)p.Y;
            info.faceInfo.rect_b += (short)p.Y;
        }
        public Rectangle getPos()
        {
            return new Rectangle(info.faceInfo.rect_l, info.faceInfo.rect_t, FaceWidth(), FaceHight());
        }
        public void setState(FaceInfo f)
        {
            info.roll = (short)f.rollAngle;
            info.yaw = (short)f.yawAngle;
            info.pitch = (short)f.pitchAngle;
            info.smile = (short)f.smile;
            angleType = f.angleType;
        }
        public void setAngle(FaceDetectSDK.PFD_DIRECT_INFO f)
        {
            info.roll = f.roll;
            info.yaw = f.yaw;
            info.pitch = f.pitch;
            setAngleType();
        }
        public void setFeature(FaceInfo f)
        {
            //info.age = (short)f.age;
            info.age = info.age == 0 ? (short)f.age :
                //(short)(info.age*0.8 + f.age*0.2);
                (short)Math.Round(info.age * 0.8 + f.age * 0.2);
            info.gen = (short)f.gender;
            featureLenth = f.featureLenth;
            feature = f.feature;
            FaceShotBmp = f.FaceShotBmp;
            FullViewBmp = f.FullViewBmp;
            PictureQuality = f.PictureQuality;
        }
        public void setAngleType()
        {
            const int RollTh = 45;//左右摆头，影响较小
            const int YawTh = 18;//左右转头，影响大
            const int PitchTh1 = 10;//仰头，影响大
            const int PitchTh2 = -24;//低头，影响大
            if (Math.Abs(rollAngle) <= RollTh &&
                Math.Abs(yawAngle) <= YawTh &&
                PitchTh2 <= pitchAngle && pitchAngle <= PitchTh1)
                angleType = FaceAngleType.Middle;
            else if (yawAngle < -YawTh && /*yawAngle是负数*/
                Math.Abs(pitchAngle) < -yawAngle)
                angleType = FaceAngleType.Left;
            else if (yawAngle > YawTh && /*yawAngle是正数*/
                Math.Abs(pitchAngle) < yawAngle)
                angleType = FaceAngleType.Right;
            else if (pitchAngle > PitchTh1)
                angleType = FaceAngleType.Up;
            else if (pitchAngle < YawTh)
                angleType = FaceAngleType.Down;
            else angleType = FaceAngleType.Unknown;
        }
        override public string ToString()
        {
            return "faceid=" + faceid.ToString()
                + "\nLocation(" + posLeft + "," + posTop + "," + posRight + "," + posBottom + ")"
                + "\nWidth=" + FaceWidth() + ",Hight=" + FaceHight()
                + "\nPictureQuality=" + PictureQuality
                + "\nprocTime=" + procTime.ToString();
        }
        public enum Gender { Mail, Femail }
    }

    public class FaceData //一张人脸信息
    {
        public int faceid;
        public int userid;
        public short featureLenth;      //特征值长度
        public byte[] feature = null;//特征值
        public FaceAngleType angleType;//按角度分类
        public string text;    //描述信息

        public FaceData() { }
        public FaceData(FaceData f)
        {
            faceid = f.faceid;
            userid = f.userid;
            featureLenth = f.featureLenth;
            feature = f.feature;
            angleType = f.angleType;
            text = f.text;
        }
        public enum FaceAngleType
        {
            Middle,
            Left,
            Right,
            Up,
            Down,
            Unknown
        }
    }
}
