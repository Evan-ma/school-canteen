using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace My_Menu
{
    class FaceCamera
    {
        /***************************固定参数*****************************/
        private const int NOD_SHAKE_MAX_ANGLE = 18;
        private const int NOD_MIN_ANGLE = 25;
        private const int SHAKE_MIN_ANGLE = 33;

        /***************************可配参数、公有变量*******************/
        public Mat Frame;
        public FaceRecgnize Face;
        public FaceDictionary FaceDic;

        public PictureBox PicBoxRealTime;
        public PictureBox PicBoxShotFace;
        public PictureBox PicBoxShotFullView;
        public PictureBox PicBoxFoundPic;
        public int PictureWidth { private set; get; }
        public int PictureHight { private set; get; }
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
        public bool FaceDictionaryAutoUpdateOn { set; get; }
        public FaceData.FaceAngleType RequestAngleType { set; get; }//设置要获取的人脸类型
        public event FaceEventHandler FaceHandler;//人脸事件处理

        /***************************私有变量*****************************/
        private VideoCapture _capture;
        private bool _isStarted;
        private static bool _isInit = false;
        private FaceDataSourceType _faceDataSourceType;
        private int _lastFaceID;
        private Dictionary<int, Rectangle> _lastFacePos;
        private Dictionary<int, string> _faceNameDic = new Dictionary<int, string>();
        private FaceCommand _faceCmd;
        private int _maxPitchAngle;
        private int _minPitchAngle;
        private int _maxYawAngle;
        private int _minYawAngle;

        /***************************控制接口实现*****************************/
        public FaceCamera()
        {
        }
        /// <summary>
        /// 初始化函数。开始之前需要先初始化。
        /// </summary>
        /// <returns></returns>
        public int Init()
        {
            return Init(0, 640, 480, 1);
        }
        public int Init(int cameraNo, int cameraW, int cameraH, int FaceRecgSize)
        {
            if (_isInit) return -3;
            //初始化变量
            PicBoxRealTime = null;
            PicBoxShotFace = null;
            PicBoxShotFullView = null;
            PicBoxFoundPic = null;
            _isStarted = false;
            FaceDictionaryAutoUpdateOn = false;
            _lastFacePos = new Dictionary<int, Rectangle>();
            PictureWidth = cameraW;
            PictureHight = cameraH;

            //初始化人脸识别库
            int init = FaceDetectSDK.Init(FaceRecgSize);//0:VGA; 1:FHD
            if (init < 0)
            {
                return -1;
            }
            //创建人脸识别对象
            Face = new FaceRecgnize();
            //创建图像帧对象
            Frame = new Mat();
            //配置人脸识别类参数
            Face.DrawFacePenThickness = 3;
            Face.MinFaceWidth = 40;
            FaceCmd = FaceCommand.None;
            Face.ShotFaceHandler += onShotFaceHander;
            Face.RemoveFaceHandler += onRemoveFaceHander;
            //创建相机捕捉器
            //string url = "rtsp://admin:yx123456@192.168.1.64:554/h264/ch1/main/av_stream";
            //_capture = new VideoCapture(url);//调用usb摄像头参数给 0 即可，一般分辨率为640*480
            _capture = new VideoCapture(cameraNo);//调用usb摄像头参数给 0 即可，一般分辨率为640*480
            _capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, cameraW);
            _capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, cameraH);
            _capture.ImageGrabbed += ProcessFrame;
            if (!_capture.IsOpened) return -2;
            PictureWidth = (int)_capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth);
            PictureHight = (int)_capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight);

            //创建人脸字典
            FaceDic = new FaceDictionary(Face);

            _isInit = true;
            return 0;
        }
        /// <summary>
        /// 开始相机。
        /// </summary>
        public void Start()
        {
            if (_capture == null || _isStarted) return;
            if (!_capture.IsOpened) return;
            _capture.Start();
            _isStarted = true;
        }
        /// <summary>
        /// 停止相机。
        /// </summary>
        public void Stop()
        {
            if (!_isStarted) return;
            _capture.Stop();
            _isStarted = false;
        }
        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            _capture.Dispose();
            _capture = null;
            Frame.Dispose();
            Frame = null;
            Face.Dispose();
            FaceDic.Clear();
            Face = null;
            FaceDetectSDK.Exit();//关闭识别库
            _isInit = false;
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
                    case FaceCommand.ShotOneFindSimiler:
                        Face.ShotFaceType = FaceRecgnize.FaceShotType.ShotOne;
                        break;
                    case FaceCommand.ShotOneFindSame:
                        Face.ShotFaceType = FaceRecgnize.FaceShotType.ShotOne;
                        break;
                    case FaceCommand.ShotAllFindSimiler:
                        Face.ShotFaceType = FaceRecgnize.FaceShotType.ShotAll;
                        break;
                    case FaceCommand.ShotAllFindSame:
                        Face.ShotFaceType = FaceRecgnize.FaceShotType.ShotAll;
                        break;
                    case FaceCommand.NodShakeDetect:
                        Face.ShotFaceType = FaceRecgnize.FaceShotType.ShotOne;
                        Face.ShotTimeInterval = 0;
                        initNodShadeParams();
                        break;
                    case FaceCommand.FaceCollect:
                        Face.ShotFaceType = FaceRecgnize.FaceShotType.ShotOne;
                        Face.ShotTimeInterval = 0;
                        break;
                }
                _faceCmd = value;
                _lastFacePos.Clear();
            }
            get { return _faceCmd; }
        }

        /// <summary>
        /// 手动抓拍单张人脸。
        /// </summary>
        public void onHandShotOne()
        {
            onHandShotProcess(Frame.Bitmap, FaceEvent.EventType.ShotOne, true);
        }
        /// <summary>
        /// 静态图片识别，手动抓拍所有人脸。
        /// </summary>
        /// <param name="bmp"></param>
        public void onHandShotAll(Bitmap bmp)
        {
            onHandShotProcess(bmp, FaceEvent.EventType.StillShotAll, false);
        }
        /// <summary>
        /// 视频图像中，手动抓拍所有人脸。
        /// </summary>
        public void onHandShotAll()
        {
            onHandShotProcess(Frame.Bitmap, FaceEvent.EventType.ShotAll, false);
        }
        private void onHandShotProcess(Bitmap bmp, FaceEvent.EventType etype, bool isOne)
        {
            _faceCmd = FaceCommand.None;
            if (isOne)
                Face.ShotFaceType = FaceRecgnize.FaceShotType.ShotOne;
            else
                Face.ShotFaceType = FaceRecgnize.FaceShotType.ShotAll;
            List<FaceInfo> fl = Face.FaceRecg(bmp);
            foreach (FaceInfo f in fl)
            {
                f.FaceShotBmp = Face.GetFaceShotBmp(bmp, f);
                f.FullViewBmp = bmp;
                onFaceShotAndFind(f, etype);
            }
            if (fl.Count == 0)
                onFaceShotAndFind(null, etype);
        }

        public bool get = false;

        /***************************内部视频线程*****************************/
        private void ProcessFrame(object sender, EventArgs arg)
        {
            try
            {
                if (_capture != null && _capture.Ptr != IntPtr.Zero)
                {
                    _capture.Retrieve(Frame, 0);//获取摄像头图像
                    Face.FaceRecgAndTraceAsync(Frame.Bitmap);//异步识别

                    //画线框，平滑效果
                    Rectangle rect = new Rectangle(Frame.Bitmap.Width / 2 - 50, Frame.Bitmap.Height / 2 - 50, 100, 100);
                    foreach (FaceInfo f in Face.TracingFaceList)
                    {
                        if ((DateTime.Now - f.procTime).TotalMilliseconds > 500) continue;
                        if (_lastFacePos.ContainsKey(f.faceid))
                        {
                            rect = _lastFacePos[f.faceid];//获取上次位置
                            int dx = f.posLeft + f.posRight - rect.Left - rect.Right;//框中心位置x的偏移量
                            int dy = f.posTop + f.posBottom - rect.Top - rect.Bottom;//y
                            double d_ratio = Math.Sqrt(dx * dx + dy * dy) / 4 / rect.Width * 0.9;//本次画框坐标调整比率

                            int dw = f.FaceWidth() - rect.Width;//宽度偏移量
                            double w_ratio = (double)Math.Abs(dw) / rect.Width * 0.95;//宽度调整比率
                            rect.Height = rect.Width = rect.Width + (int)(dw * w_ratio);//新框宽高

                            rect.X = (int)((rect.Left + rect.Right + dx * d_ratio - rect.Width) / 2);//新框左上角x
                            rect.Y = (int)((rect.Top + rect.Bottom + dy * d_ratio - rect.Width) / 2);//新框左上角y

                            _lastFacePos[f.faceid] = rect;//记录本次位置
                        }
                        else
                        {
                            rect = _lastFacePos[f.faceid] = f.getPos();
                        }
                        Face.DrawFaceWire(Frame.Bitmap, rect);
                    }
                    //显示人脸采集的提示信息
                    drawFaceCollectHint(rect);

                    //显示实时图像
                    if (PicBoxRealTime != null)
                    {
                        Action d = () =>
                        {
                            PicBoxRealTime.Image = Frame.Bitmap;
                        };
                        PicBoxRealTime.Invoke(d);
                        get = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void drawFaceCollectHint(Rectangle rect)
        {
            if (_faceCmd == FaceCommand.FaceCollect)
            {
                Point arrowBegin = new Point();
                Point arrowEnd = new Point();
                //Rectangle strPos = new Rectangle();
                int strHeight = 30;
                Point strPos = new Point(rect.X + rect.Width / 2, rect.Y + strHeight);
                int arrowLenth = 40;
                int arrowgap = 10;
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
                Pen p = new Pen(Color.Red, 5);
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.ArrowAnchor;
                g.DrawLine(p, arrowBegin, arrowEnd);
                Font font = new Font("宋体", 30);
                SolidBrush sbrush = new SolidBrush(Color.Red);
                g.DrawString(strinfo, font, sbrush, strPos, strformat);
                g.Dispose();
            }
        }

        /***************************事件处理方法实现*****************************/
        private void onShotFaceHander(FaceInfo f)
        {
            switch (_faceCmd)
            {
                case FaceCommand.ShotOneFindSimiler:
                    onFaceShotAndFind(f, FaceEvent.EventType.ShotOne);
                    break;
                case FaceCommand.ShotOneFindSame:
                    onFaceShotAndFindSame(f, FaceEvent.EventType.ShotOneFoundSame);
                    break;
                case FaceCommand.ShotAllFindSimiler:
                    onFaceShotAndFind(f, FaceEvent.EventType.ShotAll);
                    break;
                case FaceCommand.ShotAllFindSame:
                    onFaceShotAndFindSame(f, FaceEvent.EventType.ShotAllFoundSame);
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
                    _lastFaceID = f.faceid;
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
                    }
                    else
                        f.userid = 0;
                    f.text = score.ToString();
                    updateFoundPic(f, fd, uid);
                }
                FaceEvent e = new FaceEvent
                {
                    type = t,
                    faceinfo = f,
                    isFoundSame = isSame
                    //userid = uid,
                    //score = score
                };
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
                _lastFaceID = f.faceid;

                //数据库检索
                uid = FaceDic.FindSameFace(f, out score, out fd);

                if (uid != 0)
                {
                    //更新抓拍全景照片
                    updateFullViewBmp(f);
                    //更新抓拍特写照片
                    updateShotFace(f);
                    //显示检索到的照片
                    updateFoundPic(f, fd, uid);
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
                _maxPitchAngle = Math.Max(_maxPitchAngle, f.pitchAngle);
                _minPitchAngle = Math.Min(_minPitchAngle, f.pitchAngle);
                _maxYawAngle = Math.Max(_maxYawAngle, f.yawAngle);
                _minYawAngle = Math.Min(_minYawAngle, f.yawAngle);

                if (_maxPitchAngle - _minPitchAngle > NOD_MIN_ANGLE &&
                    _maxYawAngle - _minYawAngle < NOD_SHAKE_MAX_ANGLE)
                {
                    FaceEvent e = new FaceEvent
                    {
                        type = FaceEvent.EventType.HeadNodDetected,
                        faceinfo = f,
                    };
                    if (FaceHandler != null) FaceHandler(e);
                    initNodShadeParams();
                }
                else if (_maxPitchAngle - _minPitchAngle < NOD_SHAKE_MAX_ANGLE &&
                   _maxYawAngle - _minYawAngle > SHAKE_MIN_ANGLE)
                {
                    FaceEvent e = new FaceEvent
                    {
                        type = FaceEvent.EventType.HeadShakeDetected,
                        faceinfo = f,
                    };
                    if (FaceHandler != null) FaceHandler(e);
                    initNodShadeParams();
                }
                else if (_maxPitchAngle - _minPitchAngle >= NOD_SHAKE_MAX_ANGLE &&
                   _maxYawAngle - _minYawAngle >= NOD_SHAKE_MAX_ANGLE)
                {
                    _maxPitchAngle = 0;
                    _minPitchAngle = 100;
                    _maxYawAngle = 0;
                    _minYawAngle = 100;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FaceCamera->nodShakeDetect:" + ex);
            }
        }
        private void initNodShadeParams()
        {
            _maxPitchAngle = 0;
            _minPitchAngle = 100;
            _maxYawAngle = 0;
            _minYawAngle = 100;
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
            if (_lastFacePos.ContainsKey(f.faceid)) _lastFacePos.Remove(f.faceid);

            if (_faceCmd == FaceCommand.None) return;

            if (f.faceid == _lastFaceID)
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
            _lastFaceID = 0;
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
                        if (FaceDictionaryAutoUpdateOn)
                            PicBoxShotFace.Image =
                            FaceRecgnize.DeepCopyBitmap((Bitmap)f.FaceShotBmp);//防止同时占用的异常，拷贝一份
                        else
                            PicBoxShotFace.Image = f.FaceShotBmp;
                    }
                };
                PicBoxShotFace.BeginInvoke(d);
            }

        }
        private void updateFoundPic(FaceInfo f, FaceData fd, int uid)
        {
            if (f == null || fd == null || uid == 0) return;

            if (PicBoxFoundPic != null)
            {
                Action d = () =>
                {
                    if (_faceDataSourceType == FaceDataSourceType.PicFiles)
                    {
                        if (FaceDictionaryAutoUpdateOn)
                        {
                            Bitmap bmp = (Bitmap)Image.FromFile(fd.text);
                            PicBoxFoundPic.Image = FaceRecgnize.DeepCopyBitmap(bmp);//与文件解锁，便于之后覆盖文件
                                bmp.Dispose();
                        }
                        else
                            PicBoxFoundPic.Image = new Bitmap(Image.FromFile(fd.text));
                    }
                    else if (_faceDataSourceType == FaceDataSourceType.DataBase)
                        PicBoxFoundPic.Image = null;//TODO
                    };
                PicBoxFoundPic.BeginInvoke(d);
            }
            //自动更新人脸字典数据结构
            if (FaceDictionaryAutoUpdateOn)
            {
                if (_faceDataSourceType == FaceDataSourceType.PicFiles)
                {
                    //存储抓拍到的新图片，替换原始图片
                    string savepath;
                    switch (f.angleType)
                    {
                        case FaceData.FaceAngleType.Middle:
                            savepath = Path.GetDirectoryName(fd.text) + "\\" + GetUserNameById(uid) + "0.jpg";
                            break;
                        case FaceData.FaceAngleType.Up:
                            savepath = Path.GetDirectoryName(fd.text) + "\\" + GetUserNameById(uid) + "1.jpg";
                            break;
                        case FaceData.FaceAngleType.Down:
                            savepath = Path.GetDirectoryName(fd.text) + "\\" + GetUserNameById(uid) + "2.jpg";
                            break;
                        case FaceData.FaceAngleType.Left:
                            savepath = Path.GetDirectoryName(fd.text) + "\\" + GetUserNameById(uid) + "3.jpg";
                            break;
                        case FaceData.FaceAngleType.Right:
                            savepath = Path.GetDirectoryName(fd.text) + "\\" + GetUserNameById(uid) + "4.jpg";
                            break;
                        default:
                            savepath = fd.text;
                            break;
                    }
                    Console.WriteLine("Save shot face: " + savepath);
                    f.text = savepath;
                    if (f.FaceShotBmp != null) f.FaceShotBmp.Save(savepath);
                }
                else
                {
                    //数据库更新
                }

                FaceDic.AddFace(uid, f);
                Console.WriteLine("FaceCamera->AutoFaceDictionaryUpdate: uid=" + uid + "FaceAngleType=" + f.angleType);
            }
        }

        /***************************数据库方法实现*****************************/
        /// <summary>
        /// 用图片添加人脸数据库。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="alldirs"></param>
        /// <param name="proBar"></param>
        /// <returns></returns>
        public int AddFaceDataFromPicPath(string path, bool alldirs, ProgressBar proBar)
        {
            if (!string.IsNullOrEmpty(path))
            {
                string[] fs1, fs2, fs3;
                if (alldirs)
                {
                    fs1 = Directory.GetFiles(path, "*.BMP", SearchOption.AllDirectories);
                    fs2 = Directory.GetFiles(path, "*.JPG", SearchOption.AllDirectories);
                    fs3 = Directory.GetFiles(path, "*.PNG", SearchOption.AllDirectories);
                }
                else
                {
                    fs1 = Directory.GetFiles(path, "*.BMP", SearchOption.TopDirectoryOnly);
                    fs2 = Directory.GetFiles(path, "*.JPG", SearchOption.TopDirectoryOnly);
                    fs3 = Directory.GetFiles(path, "*.PNG", SearchOption.TopDirectoryOnly);
                }
                var files = fs1.Concat(fs2).Concat(fs3);
                if (proBar != null)
                {
                    proBar.Maximum = files.Count();
                    proBar.Value = 0;
                }
                
                foreach (string fstr in files)
                {
                    Console.WriteLine(fstr);
                    string fname = Path.GetFileNameWithoutExtension(fstr); //System.IO.Path.GetFileName(fstr);
                    if (fname[0] != '.' && !string.IsNullOrEmpty(fname))//去除.开头的错误文件
                    {
                        char lc = fname[fname.Length - 1];
                        while (lc == ' ' || lc == '-' || lc == '_' || char.IsDigit(lc))
                        {
                            fname = fname.Remove(fname.Length - 1);
                            lc = fname[fname.Length - 1];
                        }
                        FaceDic.AddFaceFromImage(GetUserIdByName(fname), fstr);//添加到识别类中
                    }
                    if (proBar != null) proBar.Value++;
                }
                _faceDataSourceType = FaceDataSourceType.PicFiles;
                return files.Count();
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 根据userid获取用户名。为图片文件方式提供。
        /// </summary>
        /// <param name="uid"></param>
        /// <returns>返回用户名。实为图片文件名。</returns>
        public string GetUserNameById(int uid)
        {
            if (_faceNameDic.ContainsKey(uid)) return _faceNameDic[uid];
            else return string.Empty;
        }
        public int SaveFaceAndAddToDic(string name, FaceInfo fi, string bmpsavepath)
        {
            if (string.IsNullOrEmpty(name)) return 0;
            if (string.IsNullOrEmpty(bmpsavepath)) bmpsavepath = ".FaceCameraData";
            if (!Directory.Exists(bmpsavepath)) Directory.CreateDirectory(bmpsavepath);

            switch (fi.angleType)
            {
                case FaceData.FaceAngleType.Middle:
                    fi.text = bmpsavepath + "\\" + name + "0.jpg";
                    break;
                case FaceData.FaceAngleType.Up:
                    fi.text = bmpsavepath + "\\" + name + "1.jpg";
                    break;
                case FaceData.FaceAngleType.Down:
                    fi.text = bmpsavepath + "\\" + name + "2.jpg";
                    break;
                case FaceData.FaceAngleType.Left:
                    fi.text = bmpsavepath + "\\" + name + "3.jpg";
                    break;
                case FaceData.FaceAngleType.Right:
                    fi.text = bmpsavepath + "\\" + name + "4.jpg";
                    break;
                default:
                    break;
            }
            //保存图片
            fi.FaceShotBmp.Save(fi.text);
            int uid = GetUserIdByName(name);
            FaceDic.AddFace(uid, fi);
            _faceDataSourceType = FaceDataSourceType.PicFiles;
            return uid;
        }
        private int GetUserIdByName(string name)
        {
            int maxid = 0;
            foreach (var n in _faceNameDic)
            {
                if (n.Value == name) return n.Key;
                if (n.Key > maxid) maxid = n.Key;
            }
            _faceNameDic.Add(++maxid, name);
            return maxid;
        }


        public delegate void FaceEventHandler(FaceEvent e);
        public delegate void FaceShotEventHandler(FaceInfo fi, int uid, int score);
        public delegate void FaceLeftEventHandler(FaceInfo fi);
        public enum FaceDataSourceType
        {
            NONE,
            PicFiles,
            DataBase,
            Others
        }
        public enum FaceCommand
        {
            None,
            ShotOneFindSimiler,
            ShotOneFindSame,
            ShotAllFindSimiler,
            ShotAllFindSame,
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
            ShotOneFoundSame,
            ShotAll,
            ShotAllFoundSame,
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
