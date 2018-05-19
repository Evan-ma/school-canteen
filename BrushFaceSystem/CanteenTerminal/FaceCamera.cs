using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceSDK
{
    public class FaceCamera
    {
        /***************************固定参数*****************************/
        private const int NOD_SHAKE_MAX_ANGLE = 18;
        private const int NOD_MIN_ANGLE = 25;
        private const int SHAKE_MIN_ANGLE = 33;

        /***************************可配参数、公有变量*******************/
        public Mat Frame;
        public FaceRecgnize Face;
        public FaceDictionary FaceDic;
        public DataAccess Data;

        public PictureBox PicBoxRealTime;
        public PictureBox PicBoxShotFace;
        public PictureBox PicBoxShotFullView;
        public PictureBox PicBoxFoundPic;
        public string CameraInfo { private set; get; }
        public int PictureWidth { private set; get; }
        public int PictureHight { private set; get; }
        public bool isInitialized { private set; get; }
        public bool isOpened { private set; get; }
        public bool isStarted { private set; get; }
        public int MinFaceWidth
        {
            set
            {
                if(value < FaceRecgnize.DEFAULT_MIN_FACE_WIDTH)
                    Face.MinFaceWidth = FaceRecgnize.DEFAULT_MIN_FACE_WIDTH;
                else if (value > Math.Min(PictureWidth, PictureHight))
                    Face.MinFaceWidth = Math.Min(PictureWidth, PictureHight);
                else
                    Face.MinFaceWidth = value;
            }
            get { return Face.MinFaceWidth; }
        }
        public bool FaceAutoUpdateOn { set; get; }
        public FaceData.FaceAngleType RequestAngleType { set; get; }//设置要获取的人脸类型
        public event FaceEventHandler FaceHandler;//人脸事件处理
        private bool _SpeechEnable = false;
        public bool SpeechEnable
        {
            set
            {
                _SpeechEnable = value; 
                if (value)
                {
                    _Speech = new SpeechSynthesizer();
                    _Speech.Rate = 1;
                    _Speech.Volume = 100;
                }
                else
                {
                    if (_Speech == null) return;
                    _Speech.SpeakAsyncCancelAll();
                    _Speech.Dispose();
                    _Speech = null;
                }
            }
            get { return _SpeechEnable; }
        }
        public static FaceCamera Instance { get; private set; }

        /***************************私有变量*****************************/
        private VideoCapture _Capture;
        private int _LastFaceID;
        private Dictionary<int, Rectangle> _LastFacePos;
        private FaceCommand _FaceCmd;
        private int _MaxPitchAngle;
        private int _MinPitchAngle;
        private int _MaxYawAngle;
        private int _MinYawAngle;
        private SpeechSynthesizer _Speech;

        /***************************控制接口实现*****************************/
        public FaceCamera()
        {
            isInitialized = false;
            isOpened = false;
            isStarted = false;
            Instance = this;
        }
        /// <summary>
        /// 初始化函数。开始之前需要先初始化。
        /// </summary>
        /// <returns></returns>
        public bool Init()
        {
            if (isInitialized)
            {
                Console.WriteLine("Init: Camera is already initialized!");
                return false;
            }
            //初始化变量
            PicBoxRealTime = null;
            PicBoxShotFace = null;
            PicBoxShotFullView = null;
            PicBoxFoundPic = null;
            isStarted = false;
            FaceAutoUpdateOn = false;
            _LastFacePos = new Dictionary<int, Rectangle>();

            //初始化人脸识别库
            int init = FaceDetectSDK.Init(1);//0:VGA; 1:FHD
            if (init < 0)
            {
                Console.WriteLine("Init: FaceDetectSDK Init Failed!");
                return false;
            }
            //创建人脸识别对象
            Face = new FaceRecgnize();
            //创建图像帧对象
            Frame = new Mat();
            //配置人脸识别类参数
            Face.DrawPenThickness = 3;
            Face.MinFaceWidth = 20;
            FaceCmd = FaceCommand.None;
            Face.ShotFaceHandler += onShotFaceHander;
            Face.RemoveFaceHandler += onRemoveFaceHander;

            //创建人脸字典
            FaceDic = new FaceDictionary(this, Face);
            Data = new DataAccess(Face, FaceDic);
            Data.DataSource = DataAccess.DataSrcType.FileSystem;

            isInitialized = true;
            return true;
        }
        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            if (!isInitialized) return;

            Close();
            SpeechEnable = false;
            if (Frame != null)
            {
                Frame.Dispose();
                Frame = null;
            }
            if (Face != null)
            {
                Face.Dispose();
                Face = null;
            }
            if (FaceDic != null)
            {
                FaceDic.Clear();
                FaceDic = null;
            }
            FaceDetectSDK.Exit();//关闭识别库

            isInitialized = false;
        }
        public bool Open(string caminfo, int cameraW, int cameraH)
        {
            if (!isInitialized)
            {
                Console.WriteLine("Open: Camera has not been initialized!");
                return false;
            }
            if (isOpened)
            {
                Console.WriteLine("Open: Camera is already opened!");
                return false;
            }

            //FaceCmd = FaceCommand.None;
            PictureWidth = cameraW;
            PictureHight = cameraH;
            CameraInfo = caminfo;

            //创建相机捕捉器
            int cameraNo;
            if (string.IsNullOrEmpty(caminfo))
            {
                Console.WriteLine("Open: Camera Parameter Error!");
                return false;//参数错误
            }
            else if (int.TryParse(caminfo, out cameraNo))
            {
                _Capture = new VideoCapture(cameraNo);//调用usb摄像头参数给 0 即可，一般分辨率为640*480
            }
            else
            {
                //string url = "rtsp://admin:yx123456@192.168.1.64:554/h264/ch1/main/av_stream";
                _Capture = new VideoCapture(caminfo);//调用usb摄像头参数给 0 即可，一般分辨率为640*480
            }

            _Capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, cameraW);
            _Capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, cameraH);
            _Capture.ImageGrabbed += ProcessFrame;
            if (!_Capture.IsOpened)
            {
                Console.WriteLine("Open: Camera Open Failed!");
                return false;//相机打开失败
            }
            PictureWidth = (int)_Capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth);
            PictureHight = (int)_Capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight);

            Console.WriteLine("Open: Camera Open Success!");
            isOpened = true;
            return true;
        }
        public void Close()
        {
            if (!isOpened) return;
            if (isStarted) Stop();

            PicBoxRealTime = null;

            if (_Capture != null)
            {
                _Capture.Dispose();
                _Capture = null;
            }
            isOpened = false;
        }
        public bool SetResolution(int cameraW, int cameraH)
        {
            if (!_Capture.IsOpened || isStarted) return false;
            _Capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, cameraW);
            _Capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, cameraH);
            PictureWidth = (int)_Capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth);
            PictureHight = (int)_Capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight);
            return true;
        }
        /// <summary>
        /// 开始相机。
        /// </summary>
        public void Start()
        {
            if (!isInitialized || _Capture == null || isStarted) return;
            if (!_Capture.IsOpened) return;

            _LastFacePos.Clear();
            Face.TracingFaceList.Clear();
            FaceCmd = FaceCommand.None;

            _Capture.Start();
            isStarted = true;
        }
        /// <summary>
        /// 停止相机。
        /// </summary>
        public void Stop()
        {
            if (!isStarted) return;
            _Capture.Stop();
            isStarted = false;
        }

        /// <summary>
        /// 设置人脸识别命令
        /// </summary>
        public FaceCommand FaceCmd
        {
            set
            {
                switch (value)
                {
                    default:
                    case FaceCommand.None:
                        Face.ShotFaceType = FaceRecgnize.FaceShotType.ShotNone;
                        break;
                    case FaceCommand.ShotOneAndFind:
                        Face.ShotFaceType = FaceRecgnize.FaceShotType.ShotOne;
                        break;
                    case FaceCommand.ShotAllAndFind:
                        Face.ShotFaceType = FaceRecgnize.FaceShotType.ShotAll;
                        break;
                    case FaceCommand.NodShakeDetect:
                        Face.ShotFaceType = FaceRecgnize.FaceShotType.ShotOneContinue;
                        Face.RequestFaceFeature = false;
                        Face.ShotTimeInterval = 0;
                        initNodShadeParams();
                        break;
                    case FaceCommand.FaceCollect:
                        Face.ShotFaceType = FaceRecgnize.FaceShotType.ShotOneContinue;
                        Face.RequestFaceFeature = true;
                        Face.ShotTimeInterval = 0;
                        break;
                }
                _FaceCmd = value;
                _LastFacePos.Clear();
            }
            get { return _FaceCmd; }
        }

        /// <summary>
        /// 手动抓拍单张人脸。
        /// </summary>
        public void onHandShotOne()
        {
            onHandShotProcess(Frame.Bitmap, FaceEvent.EventType.ShotOne);
        }
        /// <summary>
        /// 静态图片识别，手动抓拍所有人脸。
        /// </summary>
        /// <param name="bmp"></param>
        public void onHandShotAll(Bitmap bmp)
        {
            onHandShotProcess(bmp, FaceEvent.EventType.StillShotAll);
        }
        /// <summary>
        /// 视频图像中，手动抓拍所有人脸。
        /// </summary>
        public void onHandShotAll()
        {
            onHandShotProcess(Frame.Bitmap, FaceEvent.EventType.ShotAll);
        }
        public void onHandShotOneAsync()
        {
            Action f = () =>
            {
                onHandShotProcess(Frame.Bitmap, FaceEvent.EventType.ShotOne);
            };
            f.BeginInvoke(null, null);
        }
        public void onHandShotOneAsync(Bitmap bmp)
        {
            Action f = () =>
            {
                onHandShotProcess(bmp, FaceEvent.EventType.ShotOne);
            };
            f.BeginInvoke(null, null);
        }
        public void onHandShotOneAsync(Bitmap bmp, int topN)
        {
            Action f = () =>
            {
                if (topN > 1)
                    onHandShotProcess(bmp, FaceEvent.EventType.ShotOne);
                else
                {
                    Face.ShotFaceType = FaceRecgnize.FaceShotType.ShotOne;
                    List<FaceInfo> fl = Face.FaceRecg(bmp);
                    if (fl.Count == 1)
                    {
                        fl[0].FaceShotBmp = Face.GetFaceShotBmp(bmp, fl[0]);
                        //f.FullViewBmp = bmp;
                    }
                    else
                        onFaceShotAndFind(null, FaceEvent.EventType.ShotOne);
                }
            };
            f.BeginInvoke(null, null);
        }
        public void onHandShotAllAsync(Bitmap bmp)
        {
            Action f = () =>
            {
                onHandShotProcess(bmp, FaceEvent.EventType.ShotAll);
            };
            f.BeginInvoke(null, null);
        }
        public void onHandShotAllAsync()
        {
            Action f = () =>
            {
                onHandShotProcess(Frame.Bitmap, FaceEvent.EventType.ShotAll);
            };
            f.BeginInvoke(null, null);
        }
        private void onHandShotProcess(Bitmap bmp, FaceEvent.EventType etype)
        {
            //FaceEvent.EventType etype;
            _FaceCmd = FaceCommand.None;
            switch (etype)
            {
                case FaceEvent.EventType.ShotOne:
                    Face.ShotFaceType = FaceRecgnize.FaceShotType.ShotOne;
                    break;
                case FaceEvent.EventType.ShotAll:
                    Face.ShotFaceType = FaceRecgnize.FaceShotType.ShotAll;
                    break;
                case FaceEvent.EventType.StillShotAll:
                    Face.ShotFaceType = FaceRecgnize.FaceShotType.ShotAll;
                    break;
                default:
                    Face.ShotFaceType = FaceRecgnize.FaceShotType.ShotAll;
                    break;
            }
            
            List<FaceInfo> fl = Face.FaceRecg(bmp);
            foreach (FaceInfo f in fl)
            {
                f.FaceShotBmp = Face.GetFaceShotBmp(bmp, f);
                //f.FullViewBmp = bmp;
                onFaceShotAndFind(f, etype);
            }
            if (fl.Count == 0)
                onFaceShotAndFind(null, etype);
        }

        public void SetSpeakAsync(string text)
        {
            if (_Speech != null)
            {
                _Speech.SpeakAsyncCancelAll();
                _Speech.SpeakAsync(text);
            }
        }
        public void SetSpeakOrderedAsync(string text)
        {
            if (_Speech != null)
            {
                _Speech.SpeakAsync(text);
            }
        }
        public void CancelSpeak()
        {
            _Speech.SpeakAsyncCancelAll();
        }

        /***************************内部视频线程*****************************/
        private void ProcessFrame(object sender, EventArgs arg)
        {
            try
            {
                DateTime t1 = DateTime.Now;
                if (_Capture != null && _Capture.Ptr != IntPtr.Zero)
                {
                    //获取摄像头图像
                    _Capture.Retrieve(Frame, 0);

                    //异步识别跟踪
                    Face.FaceRecgAndTraceAsync(Frame.Bitmap);

                    //画线框，平滑效果
                    Rectangle rect = drawFaceWires();
                    //显示人脸采集的提示信息
                    if (_FaceCmd == FaceCommand.FaceCollect)
                        drawFaceCollectHint(rect);

                    //显示实时图像
                    if (PicBoxRealTime != null)
                    {
                        Action d = () =>
                        {
                            PicBoxRealTime.Image = Frame.Bitmap;
                        };
                        PicBoxRealTime.Invoke(d);
                    }
                    //Console.WriteLine("ProcessFrame: t=" + (DateTime.Now - t1).TotalMilliseconds);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private Rectangle drawFaceWires()
        {
            //DateTime t1 = DateTime.Now;
            Rectangle rect = new Rectangle(Frame.Bitmap.Width / 2 - 50, Frame.Bitmap.Height / 2 - 50, 100, 100);
            //foreach (FaceInfo f in Face.TracingFaceList)
            for (int i = 0; i < Face.TracingFaceList.Count; i++)
            {
                FaceInfo f = Face.TracingFaceList[i];
                if ((DateTime.Now - f.procTime).TotalMilliseconds > FaceRecgnize.MAX_FRAME_INTERVAL * 2)
                    continue;
                //rect = f.getPos();
                if (_LastFacePos.ContainsKey(f.faceid))
                {
                    rect = _LastFacePos[f.faceid];//获取上次位置
                    int dx = f.posLeft + f.posRight - rect.Left - rect.Right;//框中心位置x的偏移量
                    int dy = f.posTop + f.posBottom - rect.Top - rect.Bottom;//y
                    double d_ratio = Math.Sqrt(dx * dx + dy * dy) / 4 / rect.Width * 0.9;//本次画框坐标调整比率

                    int dw = f.FaceWidth() - rect.Width;//宽度偏移量
                    double w_ratio = (double)Math.Abs(dw) / rect.Width * 0.95;//宽度调整比率
                    rect.Height = rect.Width = rect.Width + (int)(dw * w_ratio);//新框宽高
                    if (rect.Width > 2 * f.FaceWidth() || rect.Width < f.FaceWidth() / 2)//安全矫正
                        rect.Height = rect.Width = f.FaceWidth();

                    rect.X = (int)((rect.Left + rect.Right + dx * d_ratio - rect.Width) / 2);//新框左上角x
                    rect.Y = (int)((rect.Top + rect.Bottom + dy * d_ratio - rect.Width) / 2);//新框左上角y

                    if (rect.X < 0 || rect.Y < 0 || rect.X > PictureWidth || rect.Y > PictureHight)//安全矫正
                        rect = f.getPos();

                    _LastFacePos[f.faceid] = rect;//记录本次位置
                }
                else
                {
                    rect = _LastFacePos[f.faceid] = f.getPos();
                }
                //设置颜色
                if (f.userid != 0) Face.DrawPenColor = Color.Red;
                else Face.DrawPenColor = Color.Green;

                //画框
                Face.DrawFaceWire(Frame.Bitmap, rect);

                //人脸框上写字
                if(_FaceCmd == FaceCommand.ShotOneAndFind ||
                    _FaceCmd == FaceCommand.ShotAllAndFind)
                {
                    string strinfo = (f.userid == 0 ? "???" : FaceDic.GetUserName(f.userid))
                    + " " + (f.gender == FaceInfo.Gender.Mail ? "男" : "女")
                    + " " + f.age + "岁";
                    Point strPos = new Point(rect.Left + rect.Width / 2, rect.Bottom + 3);
                    StringFormat strformat = new StringFormat();
                    strformat.Alignment = StringAlignment.Center;
                    Font font = new Font("微软雅黑", Math.Min(f.FaceWidth() / 20 + 7, 20));
                    SolidBrush sbrush = new SolidBrush(Face.DrawPenColor);
                    Graphics g = Graphics.FromImage(Frame.Bitmap);
                    g.DrawString(strinfo, font, sbrush, strPos, strformat);
                    g.Dispose();
                }
                
            }
            //Console.WriteLine("drawFaceWires: runtime=" + (DateTime.Now - t1).TotalMilliseconds);
            return rect;
        }
        private void drawFaceCollectHint(Rectangle rect)
        {
            Point arrowBegin = new Point();
            Point arrowEnd = new Point();
            //Rectangle strPos = new Rectangle();
            int strHeight = 45;
            Point strPos = new Point(rect.X + rect.Width / 2, rect.Y + strHeight);
            int arrowLenth = 40;
            int arrowgap = 7;
            string strinfo;
            StringFormat strformat = new StringFormat();
            switch (RequestAngleType)
            {
                case FaceData.FaceAngleType.Middle:
                    strinfo = "请保持脸部正中";
                    strPos.X = rect.X + rect.Width / 2;
                    strPos.Y = rect.Y + rect.Height + arrowgap;
                    strformat.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                    break;
                case FaceData.FaceAngleType.Up:
                    strinfo = "请抬头";
                    arrowBegin.X = rect.X + rect.Width / 2;
                    arrowBegin.Y = rect.Y - arrowgap;
                    arrowEnd.X = arrowBegin.X;
                    arrowEnd.Y = arrowBegin.Y - arrowLenth;
                    strPos.X = arrowEnd.X;
                    strPos.Y = arrowEnd.Y - strHeight - arrowgap;
                    strformat.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                    break;
                case FaceData.FaceAngleType.Down:
                    strinfo = "请低头";
                    arrowBegin.X = rect.X + rect.Width / 2;
                    arrowBegin.Y = rect.Y + rect.Height + arrowgap;
                    arrowEnd.X = arrowBegin.X;
                    arrowEnd.Y = arrowBegin.Y + arrowLenth;
                    strPos.X = arrowEnd.X;
                    strPos.Y = arrowEnd.Y;
                    strformat.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                    break;
                case FaceData.FaceAngleType.Left:
                    strinfo = "请左转头";
                    arrowBegin.X = rect.X - arrowgap;
                    arrowBegin.Y = rect.Y + rect.Height / 2;
                    arrowEnd.X = arrowBegin.X - arrowLenth;
                    arrowEnd.Y = arrowBegin.Y;
                    strPos.X = arrowEnd.X - strHeight - arrowgap;
                    strPos.Y = arrowEnd.Y;
                    strformat.FormatFlags = StringFormatFlags.DirectionVertical;
                    break;
                case FaceData.FaceAngleType.Right:
                    strinfo = "请右转头";
                    arrowBegin.X = rect.X + rect.Width + arrowgap;
                    arrowBegin.Y = rect.Y + rect.Height / 2;
                    arrowEnd.X = arrowBegin.X + arrowLenth;
                    arrowEnd.Y = arrowBegin.Y;
                    strPos.X = arrowEnd.X;
                    strPos.Y = arrowEnd.Y;
                    strformat.FormatFlags = StringFormatFlags.DirectionVertical;
                    break;
                default:
                    strinfo = "人脸采集出错";
                    break;
            }
            Graphics g = Graphics.FromImage(Frame.Bitmap);
            strformat.Alignment = StringAlignment.Center;
            Pen p = new Pen(Face.DrawPenColor, 5);
            p.StartCap = LineCap.Round;
            p.EndCap = LineCap.ArrowAnchor;
            g.DrawLine(p, arrowBegin, arrowEnd);
            Font font = new Font("微软雅黑", 30);
            SolidBrush sbrush = new SolidBrush(Face.DrawPenColor);
            g.DrawString(strinfo, font, sbrush, strPos, strformat);
            g.Dispose();
        }

        /***************************事件处理方法实现*****************************/
        private void onShotFaceHander(FaceInfo f)
        {
            switch (_FaceCmd)
            {
                case FaceCommand.ShotOneAndFind:
                    onFaceShotAndFind(f, FaceEvent.EventType.ShotOne);
                    break;
                case FaceCommand.ShotAllAndFind:
                    onFaceShotAndFind(f, FaceEvent.EventType.ShotAll);
                    break;
                case FaceCommand.NodShakeDetect:
                    onNodShakeDetect(f);
                    break;
                case FaceCommand.FaceCollect:
                    onFaceCollect(f);
                    break;
                default:
                    break;
            }
        }
        private void onFaceShotAndFind(FaceInfo f, FaceEvent.EventType t)
        {
            try
            {
                bool isSame = false;
                if (f != null && f.userid == 0)
                {
                    _LastFaceID = f.faceid;
                    //更新抓拍全景照片
                    updateFullViewBmp(f);
                    //更新抓拍特写照片
                    updateShotFace(f);

                    //数据库检索并显示检索到的照片
                    int score = 0, uid = 0;
                    FaceData fd = null;
                    uid = FaceDic.FindMostSimilarFace(f, out score, out fd);
                    //在TracingList中标记相同人脸
                    if (score >= Face.SameFaceThreshold)
                    {
                        f.userid = uid;
                        isSame = true;
                        //合并uid相同的人脸
                        FaceInfo mergeto;
                        if (Face.MergeTracingList(f, out mergeto))
                        {
                            //被合并后，需要删除
                            onRemoveFaceHander(f);
                            return;
                        }
                        else
                        {
                            isSame = true;
                        }
                    }
                    else
                        f.userid = 0;
                    f.text = score.ToString();
                    updateFoundPic(f, fd);
                }
                FaceEvent e = new FaceEvent
                {
                    type = t,
                    faceinfo = f,
                    isFoundSame = isSame
                    //userid = uid,
                    //score = score
                };
                if (SpeechEnable && isSame)
                {
                    //进行语音播报
                    string text = FaceDic.GetUserName(f.userid);
                    if (text.Trim().Length != 0)
                    {
                        text += "，你好。";
                        _Speech.SpeakAsync(text);
                    }
                }
                //回调上层
                if (FaceHandler != null) FaceHandler(e);
            }
            catch (Exception ex)
            {
                Console.WriteLine("FaceCamera->shotFaceAndFind:" + ex);
            }
        }
        private void onFaceShotAndFindSame(FaceInfo f, FaceEvent.EventType t)
        {
            try
            {
                if (f == null) return;
                if (f.userid != 0) return;

                int score = 0, uid = 0;
                FaceData fd = null;
                _LastFaceID = f.faceid;

                //数据库检索
                uid = FaceDic.FindSameFace(f, out score, out fd);

                if (uid != 0)
                {
                    //更新抓拍全景照片
                    updateFullViewBmp(f);
                    //更新抓拍特写照片
                    updateShotFace(f);
                    //显示检索到的照片
                    updateFoundPic(f, fd);
                    //在TracingList中标记相同人脸
                    f.text = score.ToString();
                    f.userid = uid;

                    FaceEvent e = new FaceEvent
                    {
                        type = t,
                        faceinfo = f,
                        //userid = uid,
                        //score = score
                    };
                    if (FaceHandler != null) FaceHandler(e);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FaceCamera->shotFaceAndFind:" + ex);
            }
        }
        private void onNodShakeDetect(FaceInfo f)
        {
            try
            {
                _MaxPitchAngle = Math.Max(_MaxPitchAngle, f.pitchAngle);
                _MinPitchAngle = Math.Min(_MinPitchAngle, f.pitchAngle);
                _MaxYawAngle = Math.Max(_MaxYawAngle, f.yawAngle);
                _MinYawAngle = Math.Min(_MinYawAngle, f.yawAngle);

                if (_MaxPitchAngle - _MinPitchAngle > NOD_MIN_ANGLE &&
                    _MaxYawAngle - _MinYawAngle < NOD_SHAKE_MAX_ANGLE)
                {
                    FaceEvent e = new FaceEvent
                    {
                        type = FaceEvent.EventType.HeadNodDetected,
                        faceinfo = f,
                    };
                    if (FaceHandler != null) FaceHandler(e);
                    initNodShadeParams();
                }
                else if (_MaxPitchAngle - _MinPitchAngle < NOD_SHAKE_MAX_ANGLE &&
                   _MaxYawAngle - _MinYawAngle > SHAKE_MIN_ANGLE)
                {
                    FaceEvent e = new FaceEvent
                    {
                        type = FaceEvent.EventType.HeadShakeDetected,
                        faceinfo = f,
                    };
                    if (FaceHandler != null) FaceHandler(e);
                    initNodShadeParams();
                }
                else if (_MaxPitchAngle - _MinPitchAngle >= NOD_SHAKE_MAX_ANGLE &&
                   _MaxYawAngle - _MinYawAngle >= NOD_SHAKE_MAX_ANGLE)
                {
                    _MaxPitchAngle = 0;
                    _MinPitchAngle = 100;
                    _MaxYawAngle = 0;
                    _MinYawAngle = 100;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FaceCamera->nodShakeDetect:" + ex);
            }
        }
        private void initNodShadeParams()
        {
            _MaxPitchAngle = 0;
            _MinPitchAngle = 100;
            _MaxYawAngle = 0;
            _MinYawAngle = 100;
        }
        private void onFaceCollect(FaceInfo f)
        {
            try
            {
                if (f.angleType == RequestAngleType)
                {
                    FaceEvent e = new FaceEvent
                    {
                        type = FaceEvent.EventType.FaceCollected,
                        faceinfo = f,
                    };
                    if (FaceHandler != null) FaceHandler(e);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FaceCamera->faceCollect:" + ex);
            }
        }
        private void onRemoveFaceHander(FaceInfo f)
        {
            if (_LastFacePos.ContainsKey(f.faceid)) _LastFacePos.Remove(f.faceid);

            if (_FaceCmd == FaceCommand.None) return;

            if (f.faceid == _LastFaceID)
            {
                if (PicBoxShotFace != null)
                {
                    Action d = () =>
                    {
                        PicBoxShotFace.Image = null;
                    };
                    PicBoxShotFace.Invoke(d);
                }
                if (PicBoxShotFullView != null)
                {
                    Action d = () =>
                    {
                        PicBoxShotFullView.Image = null;
                    };
                    PicBoxShotFullView.Invoke(d);
                }
                if (PicBoxFoundPic != null)
                {
                    Action d = () =>
                    {
                        PicBoxFoundPic.Image = null;
                    };
                    PicBoxFoundPic.Invoke(d);
                }
            }
            _LastFaceID = 0;
            FaceEvent e = new FaceEvent
            {
                type = FaceEvent.EventType.FaceRemoved,
                faceinfo = f,
            };
            if (FaceHandler != null) FaceHandler(e);
        }

        private void updateFullViewBmp(FaceInfo f)
        {
            if (PicBoxShotFullView != null)
            {
                Action d = () =>
                {
                    if (f == null)
                        PicBoxShotFullView.Image = Frame.Bitmap;
                    else
                        PicBoxShotFullView.Image = f.FullViewBmp;// Face.DrawFaceWire(f.FullViewBmp,f.getPos());
                };
                PicBoxShotFullView.BeginInvoke(d);
            }
        }
        private void updateShotFace(FaceInfo f)
        {
            if (PicBoxShotFace != null)
            {
                Action d = () =>
                {
                    if (f == null)
                        PicBoxShotFace.Image = null;
                    else
                    {
                        if (FaceAutoUpdateOn)
                            PicBoxShotFace.Image =
                            FaceRecgnize.DeepCopyBitmap((Bitmap)f.FaceShotBmp);//防止同时占用的异常，拷贝一份
                        else
                            PicBoxShotFace.Image = f.FaceShotBmp;
                    }
                };
                PicBoxShotFace.BeginInvoke(d);
            }

        }
        private void updateFoundPic(FaceInfo f, FaceData fd)
        {
            if (f == null || fd == null) return;

            if (PicBoxFoundPic != null)
            {
                Action d = () =>
                {
                    PicBoxFoundPic.Image = Data.GetFaceOrgPic(fd);
                };
                PicBoxFoundPic.BeginInvoke(d);
            }
            //自动更新人脸字典数据结构
            if (FaceAutoUpdateOn && f.userid!= 0)
            {
                Data.UpdateFacePicture(f, fd);
                FaceDic.AddFace(f.userid, f);
                Console.WriteLine("FaceCamera->AutoFaceDictionaryUpdate: uid=" + f.userid + "FaceAngleType=" + f.angleType);
            }
        }

        /***************************数据库方法实现*****************************/

        public delegate void FaceEventHandler(FaceEvent e);
        public delegate void FaceShotEventHandler(FaceInfo fi, int uid, int score);
        public delegate void FaceLeftEventHandler(FaceInfo fi);
        
        public enum FaceCommand
        {
            None,
            ShotOneAndFind,
            ShotAllAndFind,
            FaceCollect,
            NodShakeDetect
        }
    }
    public class FaceEvent
    {
        public EventType type;
        public FaceInfo faceinfo;
        public bool isFoundSame = false;
        //public int userid;
        //public int score;
        public enum EventType
        {
            ShotOne,
            ShotAll,
            //StillShotOne,
            StillShotAll,
            FaceCollected,
            HeadNodDetected,
            HeadShakeDetected,
            FaceRemoved,
            FaceError
        }
    }
}
