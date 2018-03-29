//#define USE_LIB_35
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace My_Menu//工程名
{
    /************************************************用法例程TODO**********************************************
    
    ******************************************************************************************************/

    public class FaceRecgnize
    {
        /********************************固定参数（一般不改）***********************************************/
        public const int DEFAULT_SHOT_NONE_PQ = 25;
        public const int DEFAULT_SHOT_ONE_PQ = 15;
        public const int DEFAULT_SHOT_ALL_PQ = 17;
        public const int DEFAULT_SHOT_NONE_TIME = 1000;
        public const int DEFAULT_SHOT_ONE_TIME = 2500;
        public const int DEFAULT_SHOT_ALL_TIME = 5000;
        public const int DEFAULT_SHOT_INTERVAL = 3000;
        public const int DEFAULT_REMOVE_TIME = 5000;
        public const int DEFAULT_LONG_TIME = 100000000;
        public const int DEFAULT_MIN_FACE_WIDTH = 20;
        public const int DEFAULT_PEN_THICKNESS = 5;
        public const int DEFAULT_INIT_PQ = 1000;
        public const int POSITION_TRACE_MAX_TS = 2000;
        public const int SAME_FACE_SCORE_LIB30 = 700;
        public const int SIMILAR_FACE_SCORE_LIB30 = 600;
        public const int SAME_FACE_SCORE_LIB35 = 75;
        public const int SIMILAR_FACE_SCORE_LIB35 = 64;

        /********************************可变属性参数（对外提供配置接口）***********************************/
        public int SameFaceThreshold { get; private set; }
        public int SimilarFaceThreshold { get; private set; }
        public int RemoveTimeInterval { set; get; } //同一个人间隔时间（单位:秒），若超过此值，人数将增加1
        public int MinFaceWidth { set; get; }//为图中可识别人脸最小宽度（像素单位）
        public int DrawFacePenThickness { set; get; } //画框时 线条的宽度 为像素单位
        public FaceData.FaceAngleType RequestAngleType { set; get; }//设置要获取的人脸类型
        public int PicQualityThreshold { set; get; }//抓拍特写质量阈值
        public int ShotTimeThreshold { set; get; }//抓拍时间阈值（ms），如果超出这个时间还没有抓拍到最佳照片，就采用当前最佳值
        public int ShotTimeInterval { set; get; }//抓拍回调时间间隔。每隔这个时间间隔就会重新调用回调函数。
        public FaceShotType _shotFaceType;
        public FaceShotType ShotFaceType {
            set
            {
                lock (this)
                {
                    switch (value)
                    {
                        default:
                        case FaceShotType.ShotNone:
                            ShotTimeThreshold = DEFAULT_SHOT_NONE_TIME;
                            ShotTimeInterval = DEFAULT_LONG_TIME;
                            RemoveTimeInterval = DEFAULT_REMOVE_TIME;
                            PicQualityThreshold = DEFAULT_SHOT_NONE_PQ;
                            break;
                        case FaceShotType.ShotOne:
                            ShotTimeThreshold = DEFAULT_SHOT_ONE_TIME;
                            ShotTimeInterval = DEFAULT_SHOT_INTERVAL;
                            RemoveTimeInterval = DEFAULT_REMOVE_TIME;
                            PicQualityThreshold = DEFAULT_SHOT_ONE_PQ;
                            break;
                        case FaceShotType.ShotAll:
                            ShotTimeThreshold = DEFAULT_SHOT_ALL_TIME;
                            ShotTimeInterval = DEFAULT_SHOT_INTERVAL;
                            RemoveTimeInterval = DEFAULT_LONG_TIME;
                            PicQualityThreshold = DEFAULT_SHOT_ALL_PQ;
                            break;
                    }
                    _shotFaceType = value;
                    _tracingFaceList.Clear();
                    TracedFaceCount = 0;
                }
            }
            get { return _shotFaceType; } }
        public event FaceEventHandler ShotFaceHandler;//事件：用于处理质量好的face
        public event FaceEventHandler RemoveFaceHandler;//事件：用于处理离开的face

        /*********************************获取数据接口******************************************************/
        private int _totalFaceCount = 0;//识别的所有的人脸数量
        public int TracedFaceCount { private set; get; }//统计的人脸数量
        private List<FaceInfo> _tracingFaceList = new List<FaceInfo>();//存储最终列表（经过对比去重后的列表）
        public List<FaceInfo> TracingFaceList//_tracingFaceList 对外接口
        {
            get
            {
                return _tracingFaceList;
            }
        }
        public int TracingFaceCount//追踪人数get接口
        {
            get
            {
                return _tracingFaceList.Count;
            }
        }

        /********************************************内部数据定义******************************************/
        private IntPtr[] _currentFaceFeature = new IntPtr[20];//存放特征值，最多识别20张人脸
        private bool _RecgRunningFlag;  //委托线程进行状态标志

        /*********************************************方法实现*********************************************/
        public FaceRecgnize()//申请特征值存储位置
        {
            if (FaceDetectSDK.LibVersion == 35)
            {
                SameFaceThreshold = SAME_FACE_SCORE_LIB35;
                SimilarFaceThreshold = SIMILAR_FACE_SCORE_LIB35;
            }
            else
            {
                SameFaceThreshold = SAME_FACE_SCORE_LIB30;
                SimilarFaceThreshold = SIMILAR_FACE_SCORE_LIB30;
            }
            //设置参数默认值
            MinFaceWidth = DEFAULT_MIN_FACE_WIDTH;//最小人脸宽度，像素
            DrawFacePenThickness = DEFAULT_PEN_THICKNESS;
            ShotFaceType = FaceShotType.ShotNone;
            //初始化数据
            for (int i = 0; i < 20; i++)
            {
                _currentFaceFeature[i] = Marshal.AllocHGlobal(3000); 
            }
        }
        ~FaceRecgnize()//析构函数
        {
            //释放资源
            for (int i = 0; i < _currentFaceFeature.Length; i++)
            {
                Marshal.FreeHGlobal(_currentFaceFeature[i]);
            }
        }
        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            _tracingFaceList.Clear();
        }


        /// <summary>
        /// 人脸识别函数。同步。
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public List<FaceInfo> FaceRecg(Bitmap bmp)
        {
            lock (this)
            {
                if (bmp == null) return null;
                List<FaceInfo> fl = null;
                FaceRecgProcess(bmp, ProcessLevel.Level_3, ref fl);
                return fl;
            }
        }
        /// <summary>
        /// 人脸识别。异步。
        /// </summary>
        /// <param name="bmp"></param>
        public async Task<List<FaceInfo>> FaceRecgAsync(Bitmap bmp)
        {
            try
            {
                Bitmap bmpcp =  DeepCopyBitmap(bmp);
                List<FaceInfo> flist = await FaceRecgDelegate(bmpcp);
                return flist;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }

        }
        private Task<List<FaceInfo>> FaceRecgDelegate(Bitmap bmp) //识别委托函数返回ReCurrentList
        {
            return Task.Run(() =>
            {
                return FaceRecg(bmp);
            });
        }
        /// <summary>
        /// 人脸识别处理函数。比较耗时。
        /// </summary>
        /// <param name="bmpByte"></param>
        /// <param name="level"></param>
        /// <param name="flist"></param>
        /// <returns></returns>
        private List<FaceInfo> FaceRecgProcess(Bitmap bmp, ProcessLevel level, ref List<FaceInfo> flist)
        {
            try
            {
                byte[] bmpByte = ChangePicFormat(bmp);
                if (flist == null) flist = new List<FaceInfo>();
                //DateTime nowtime = DateTime.Now;
                //byte[] _currentBmpByte = ChangePicFormat(bmp);//格式转换
                FaceDetectSDK.PFD_FACE_DETECT faceInfo = new FaceDetectSDK.PFD_FACE_DETECT();
                int ret;
                switch (level)
                {
                    default:
                    case ProcessLevel.Level_1:
                        //Level 1:50ms
                        ret = FaceDetectSDK.PFD_FaceRecog(bmpByte, ref faceInfo, FaceDetectSDK.PFD_DISABLEINFO, FaceDetectSDK.PFD_OP_FACE_ROLL_0);
                        break;
                    case ProcessLevel.Level_2:
                        //Level 2:120ms
                        ret = FaceDetectSDK.PFD_FaceRecog(bmpByte, ref faceInfo, FaceDetectSDK.PFD_ENABLEINFO, FaceDetectSDK.PFD_OP_FACE_ROLL_0);
                        break;
                    case ProcessLevel.Level_3:
                        //Level 3:400ms
                        ret = FaceDetectSDK.PFD_GetFeature(bmpByte, ref faceInfo, _currentFaceFeature, FaceDetectSDK.PFD_OP_FACE_ROLL_0, (short)MinFaceWidth);
                        break;
                }
                if (ret != 0)
                {
                    Console.WriteLine("SDK:PFD_GetFeature Error ret=" + ret.ToString());
                    return null;
                }
                flist.Clear();
                if (faceInfo.num != 0)
                {
                    int size;
                    switch (_shotFaceType)
                    {
                        case FaceShotType.ShotOne:
                            int max = 0;
                            int index = -1;
                            for (int i = 0; i < faceInfo.num; i++)
                            {
                                if (faceInfo.info[i].enable == 0)
                                {
                                    size = faceInfo.info[i].faceInfo.rect_r - faceInfo.info[i].faceInfo.rect_l;
                                    if(size > max)
                                    {
                                        max = size;
                                        index = i;
                                    }
                                }
                            }
                            addFaceList(ref flist, faceInfo, index, level);
                            break;
                        default:
                        case FaceShotType.ShotNone:
                        case FaceShotType.ShotAll:
                            for (int i = 0; i < faceInfo.num; i++)
                            {
                                if (faceInfo.info[i].enable == 0)
                                {
                                    size = faceInfo.info[i].faceInfo.rect_r - faceInfo.info[i].faceInfo.rect_l;
                                    if (size < MinFaceWidth) continue;
                                    addFaceList(ref flist, faceInfo, i, level);
                                }
                            }
                            break;
                    }
                }
                GC.Collect();
                //DateTime t2 = DateTime.Now;
                //TimeSpan ts = t2 - nowtime;
                //Console.WriteLine("FaceRecg RunTime=" + ts.TotalMilliseconds.ToString());
                return flist;
            }
            catch (Exception ex)
            {
                Console.WriteLine("FaceRecg Error" + ex.ToString());
                return null;
            }
        }
        private void addFaceList(ref List<FaceInfo> flist, FaceDetectSDK.PFD_FACE_DETECT faceInfo, int index, ProcessLevel level)
        {
            if (index < 0) return;
            FaceInfo fi = new FaceInfo  //此处将获取到的人脸信息导出，可增加更多信息
            {
                faceid = ++_totalFaceCount,
                bestQualityValue = DEFAULT_INIT_PQ,
                procTime = DateTime.Now,
                inTime = DateTime.Now
            };
            fi.setInfo(faceInfo.info[index]);
            if (level == ProcessLevel.Level_3)
            {
                fi.featureLenth = faceInfo.info[index].flen;
                byte[] f = new byte[fi.featureLenth];
                Marshal.Copy(_currentFaceFeature[index], f, 0, fi.featureLenth);
                fi.feature = f;
            }
            if (level == ProcessLevel.Level_2 || level == ProcessLevel.Level_3)
                fi.setAngleType();
            flist.Add(fi);
        }
        /// <summary>
        /// 根据图片位图数据获取特定人脸的特征值。
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        private FaceInfo GetFaceFeature(Bitmap bmp, FaceInfo f)
        {
            Bitmap shotbmp = GetFaceShotBmp(bmp, f);
            List<FaceInfo> flist = new List<FaceInfo>();
            FaceRecgProcess(shotbmp, ProcessLevel.Level_3, ref flist);
            if (flist.Count == 1)
            {
                f.setState(flist[0]);
                f.setFeature(flist[0]);
                f.FaceShotBmp = shotbmp;
                f.FullViewBmp = bmp;
                return f;
            }
            else return null;
        }


        /// <summary>
        /// 识别+追踪函数，同步。
        /// </summary>
        /// <param name="bmp"></param>
        public void FaceRecgAndTrace(Bitmap bmp)
        {
            if (bmp == null) return;
            lock (this)
            {
                FaceRecgTraceProcess(bmp);
            }
        }
        /// <summary>
        /// 识别+追踪函数，异步。
        /// </summary>
        /// <param name="bmp"></param>
        public async void FaceRecgAndTraceAsync(Bitmap bmp)
        {
            if (bmp == null) return;
            if (_RecgRunningFlag) return;
            _RecgRunningFlag = true;
            Bitmap bmpcp = DeepCopyBitmap(bmp);//不放到异步处理中，保护原始图像不被破坏
            await FaceRecgTraceDelegate(bmpcp);
            _RecgRunningFlag = false;
        }
        private Task FaceRecgTraceDelegate(Bitmap bmp) //识别并跟踪委托函数 ReTrackinglist
        {
            return Task.Run(() =>
            {
                lock (this)
                {
                    FaceRecgTraceProcess(bmp);
                }
            });
        }
        /*****************************************************************************************
            1.位置跟踪
            2.如果位置跟踪失败，进入相似度跟踪
            3.如果1.2.的跟踪都失败，则增加跟踪对象
            4.如果跟踪成功
              - 更新位置信息，时间戳
              - 如果还未抓拍，确认PQ，如果PQ变好则更新特征值，并进行回调
              - 如果已经抓拍过，则确认抓拍间隔，如果间隔过了，则重新回调，如果PQ合格则更新特征值

            新思路：
            1.跟踪列表为空，或者过了识别间隔，则整体图像识别。逐个处理识别到人脸，进入2
            2.根据当前图像查找跟踪列表。类似目前设计。
            3.1.以外的情况，则独立跟踪。由跟踪列表出发，逐个跟踪列表中的人脸。
              截取比跟踪列表中尺寸更大的图像，然后进行识别，识别到之后，则更新信息
        *****************************************************************************************/
        /// <summary>
        /// 人脸识别跟踪处理函数。比较耗时。
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        private void FaceRecgTraceProcess(Bitmap bmp)
        {
            if (bmp == null) return;
            DateTime t1 = DateTime.Now;
            TimeSpan ts;
            List<FaceInfo> curFList = null;
            if(ShotTimeInterval == 0 && _shotFaceType == FaceShotType.ShotOne)
            {
                FaceInfo tf = GetFrontFace(_tracingFaceList);
                FaceInfo cf = null;
                List<FaceInfo> flist = new List<FaceInfo>();
                if (tf == null)
                {
                    FaceRecgProcess(bmp, ProcessLevel.Level_3, ref flist);
                    _tracingFaceList.AddRange(flist);
                    if (flist.Count == 1) tf = cf = flist[0];
                }
                else
                {
                    Rectangle rect;
                    Bitmap shotbmp = GetFaceShotBmp(bmp, tf, 0.25, out rect);//保留1/8 Margin
                    FaceRecgProcess(shotbmp, ProcessLevel.Level_3, ref flist);
                    if (flist.Count == 1)
                    {
                        cf = flist[0];
                        cf.Offset(rect.Location);
                    }
                    else
                    {
                        FaceRecgProcess(bmp, ProcessLevel.Level_3, ref flist);
                        if (flist.Count == 1) cf = flist[0];
                    }
                }
                if (cf != null)//跟踪成功
                {
                    tf.copyContent(cf);
                    tf.FaceShotBmp = GetFaceShotBmp(bmp, tf);
                    tf.shotTime = DateTime.Now;
                    if (ShotFaceHandler != null) ShotFaceHandler(tf);
                }
            }
            else
            {
                FaceRecgProcess(bmp, ProcessLevel.Level_1, ref curFList);
                //ts = DateTime.Now - t1;
                //Console.WriteLine("FaceRecgTraceProcess: t1=" + ts.TotalMilliseconds);
                FaceTraceUpdateProcess(bmp, curFList);
                //ts = DateTime.Now - t1;
                //Console.WriteLine("FaceRecgTraceProcess: t2=" + ts.TotalMilliseconds);
            }

            //去除追踪列表中超时对象
            for (int i = 0; i < _tracingFaceList.Count; i++)//循环追踪列表
            {
                ts = t1 - _tracingFaceList[i].procTime;
                //1.若更新时间超时RemoveTimeInterval，则删除
                //2.或者，若PQ不合格并且更新时间超时，则删除
                if (ts.TotalMilliseconds >= RemoveTimeInterval ||
                    (_tracingFaceList[i].bestQualityValue > PicQualityThreshold &&
                    ts.TotalMilliseconds >= DEFAULT_REMOVE_TIME))
                {
                    if (RemoveFaceHandler != null) RemoveFaceHandler(_tracingFaceList[i]);
                    _tracingFaceList.Remove(_tracingFaceList[i]);
                }
            }

            ts = DateTime.Now - t1;
            Console.WriteLine("FaceRecgTraceProcess: t1=" + ts.TotalMilliseconds);

        }
        private void FaceTraceUpdateProcess(Bitmap bmp, List<FaceInfo> curFList)
        {
            try
            {
                if (curFList == null || curFList.Count == 0) return;
                if (_tracingFaceList.Count == 0)
                {
                    _tracingFaceList.AddRange(curFList);
                    return;
                }
                TimeSpan ts;
                foreach (FaceInfo current in curFList)//循环当前列表
                {
                    ts = TimeSpan.MaxValue;
                    FaceInfo similar = null;
                    //1.找一遍位置
                    foreach (FaceInfo tracking in _tracingFaceList)//循环追踪列表
                    {
                        if (TrackPositionJudge(tracking, current))//位置对比为同一个人
                        {
                            //找出最新的位置接近的人，防止与离开的人位置重叠
                            ts = current.procTime - tracking.procTime;
                            similar = tracking;
                            break;
                        }
                    }
                    //2.对比相似度
                    //如果找不到位置接近的人，或者，这个位置1秒内没有更新
                    FaceInfo cfi = null;
                    if (similar == null || ts.TotalMilliseconds > POSITION_TRACE_MAX_TS)
                    {
                        similar = null;
                        int maxs = 0;
                        //获取current人脸的特征值feature
                        cfi = GetFaceFeature(bmp, current);
                        if (cfi == null) continue;
                        foreach (FaceInfo tracking in _tracingFaceList)//循环追踪列表对比特征值
                        {
                            int s = FaceFeatureMatch(tracking, current);
                            if (s > SameFaceThreshold)
                            {
                                similar = tracking;
                                maxs = s;
                                break;
                            }
                            else if (s > maxs)
                            {
                                maxs = s;
                                similar = tracking;
                            }
                        }
                        if (maxs < SimilarFaceThreshold) similar = null;
                    }
                    //3.追踪列表与当前列表没有相似的人，添加新成员
                    if (similar == null)
                    {
                        _tracingFaceList.Add(current);
                        continue;
                    }

                    //4.处理相似的人
                    /*更新人脸信息：位置*/
                    similar.setPos(current);
                    //更新处理时间
                    similar.procTime = current.procTime;

                    if (ShotTimeInterval == 0)
                    {
                        //获取current人脸的特征值feature
                        if (cfi == null)
                        {
                            cfi = GetFaceFeature(bmp, current);
                            if (cfi == null)
                                continue;
                            current.shotTime = DateTime.Now;
                        }
                        //similar.setState(current);
                        if (ShotFaceHandler != null) ShotFaceHandler(current);
                        continue;
                    }

                    //首次抓拍，回调
                    if (similar.shotTime == DateTime.MinValue)
                    {
                        //获取current人脸的特征值feature
                        if (cfi == null)
                        {
                            cfi = GetFaceFeature(bmp, current);
                            if (cfi == null) continue;
                        }
                        int cq = GetPicQualityValue(current, similar, bmp);
                        similar.setState(current);
                        /*根据图片质量，挑选更好的特征值及人脸照片*/
                        if (cq < similar.bestQualityValue)//值越小质量越高
                        {
                            updateSimilarInfo(cq, current, similar);
                        }
                        ts = DateTime.Now - similar.inTime;
                        //满足抓拍条件
                        if (cq <= PicQualityThreshold || /*图像质量满足，这种情况similar已经ok*/
                                ts.TotalMilliseconds >= ShotTimeThreshold/*超过抓拍时限则强制抓拍*/)
                        {
                            if (similar.feature == null || similar.FaceShotBmp == null)
                            {
                                updateSimilarInfo(cq, current, similar);
                            }
                            similar.shotTime = DateTime.Now;

                            //合并TracingList中与此相同人脸
                            FaceInfo removed;
                            if (mergeTracingList(similar, out removed))
                            {
                                //保留以下信息
                                similar.faceid = removed.faceid;
                                similar.userid = removed.userid;
                                similar.text = removed.text;
                                similar.inTime = removed.inTime;
                            }
                            else
                            {
                                //统计跟踪人数
                                TracedFaceCount++;
                            }
                            //进行抓拍
                            if (ShotFaceHandler != null) ShotFaceHandler(similar);
                        }
                        continue;
                    }//首次回调end

                    //确认抓拍时间，重新回调
                    ts = DateTime.Now - similar.shotTime;
                    if (ts.TotalMilliseconds > ShotTimeInterval)//超过抓拍间隔就会回调
                    {
                        //获取current人脸的特征值feature
                        if (cfi == null)
                        {
                            cfi = GetFaceFeature(bmp, current);
                            if (cfi == null) continue;
                        }
                        similar.setState(current);//更新状态信息
                        int cq = GetPicQualityValue(current, similar, bmp);
                        if (cq < PicQualityThreshold)//稳定状态下对图片质量要求放宽
                        {
                            //更新特征值
                            updateSimilarInfo(cq, current, similar);
                        }
                        //回调
                        similar.shotTime = DateTime.Now;
                        if (ShotFaceHandler != null) ShotFaceHandler(similar);
                        //合并TracingList中与此相同人脸
                        FaceInfo removed;
                        if (mergeTracingList(similar, out removed))
                        {
                            similar.inTime = removed.inTime;
                            if (RemoveFaceHandler != null) RemoveFaceHandler(removed);
                            //统计跟踪人数
                            TracedFaceCount--;
                        }
                    }

                    //end 相似处理
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FaceRecg Error" + ex.ToString());
            }
        }

        private void updateSimilarInfo(int cq, FaceInfo current, FaceInfo similar)
        {
            //更新特征值
            similar.bestQualityValue = cq;
            similar.setFeature(current);
            //抓拍特写
            similar.FaceShotBmp = current.FaceShotBmp;
            similar.FullViewBmp = current.FullViewBmp;// DeepCopyBitmap(bmp);
        }
        private bool mergeTracingList(FaceInfo similar, out FaceInfo removed)
        {
            for (int i = 0; i < _tracingFaceList.Count; i++)
            {
                if (_tracingFaceList[i].bestQualityValue <= PicQualityThreshold
                    && _tracingFaceList[i] != similar)
                {
                    int s = FaceFeatureMatch(_tracingFaceList[i], similar);
                    if (s >= SimilarFaceThreshold)//进行合并
                    {
                        //similar.userid = _tracingFaceList[i].userid;
                        //_tracingFaceList[i].copyContent(similar);
                        removed = _tracingFaceList[i];
                        _tracingFaceList.RemoveAt(i);
                        return true;
                    }
                }
            }
            removed = null;
            return false;
        }
        /// <summary>
        /// 获取图片质量值。值越小质量越高。
        /// </summary>
        /// <param name="n"></param>
        /// <param name="o"></param>
        /// <param name="bmp"></param>
        /// <returns></returns>
        private int GetPicQualityValue(FaceInfo n, FaceInfo o, Bitmap bmp)
        {
            //1.根据脸部旋转角度判断图片质量
            int v1 = /*Math.Abs(n.rollAngle) +*/ (Math.Abs(n.pitchAngle) + Math.Abs(n.yawAngle)) / 2;

            //2.根据脸部尺寸大小
            int v2 = 20;
            if (bmp != null) v2 = bmp.Width / n.FaceWidth();

            //3.根据脸部图片位移大小判断图片质量
            int v3 = 30;
            if (o != null)
            {
                int dx = (n.posLeft + n.posRight - o.posLeft - o.posRight) / 2;
                int dy = (n.posTop + n.posBottom - o.posTop - o.posBottom) / 2;
                int dr = o.FaceWidth();
                //v3 = (int)(Math.Sqrt(dx * dx + dy * dy) * 50) / dr;
                v3 = (int)((dx * dx + dy * dy) * 50) / dr;
            }

            return v1 + v2 + v3;
        }
        /// <summary>
        /// 根据位置信息判断是否为同一个人。
        /// </summary>
        /// <param name="o"></param>
        /// <param name="n"></param>
        /// <returns>是同一个人返回true。</returns>
        private bool TrackPositionJudge(FaceInfo o, FaceInfo n)
        {
            //如果人脸尺寸偏差太大
            if (Math.Abs(n.FaceWidth() - o.FaceWidth()) > 50)
                return false;
            //根据人脸宽度动态计算：40%-100% * FaceWidth
            int dx = (n.posLeft + n.posRight - o.posLeft - o.posRight) / 2;
            int dy = (n.posTop + n.posBottom - o.posTop - o.posBottom) / 2;
            int weight = 105 - o.FaceWidth() * 50 / 200;
            int dr = (weight < 40 ? 40 : weight) * o.FaceWidth() / 100;

            if (dx * dx + dy * dy > dr * dr)
                return false;
            else
                return true;
        }
        /// <summary>
        /// 获取正在跟踪的人脸中最前面一张人脸。
        /// </summary>
        /// <returns></returns>
        public FaceInfo getTracingFaceByFaceid(int fid)
        {
            lock (this)
            {
                foreach (FaceInfo f in _tracingFaceList)
                {
                    if (fid == f.faceid) return f;
                }
                return null;
            }
        }


        /// <summary>
        /// 特征值对比函数。
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns>返回对比分数。</returns>
        public int FaceFeatureMatch(FaceData f1, FaceData f2)
        {
            try
            {
                return FaceDetectSDK.PFD_FeatureMatching(f1.featureLenth, f1.feature, f2.featureLenth, f2.feature);
            }
            catch (Exception ex)
            {
                Console.WriteLine("PFD_FeatureMatching Error" + ex.ToString());
                return -1;
            }
        }

        /// <summary>
        /// 图片格式转换
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        private byte[] ChangePicFormat(Bitmap bmp)
        {
            Bitmap pic;
            if (bmp.PixelFormat != PixelFormat.Format24bppRgb)
            {
                pic = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb);
                Graphics gra1 = Graphics.FromImage(pic);
                gra1.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height), new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
                gra1.Dispose();
            }
            else
            {
                pic = bmp;
            }
            MemoryStream imgStream = new MemoryStream();
            pic.Save(imgStream, ImageFormat.Bmp);
            byte[] bmpbyte = imgStream.GetBuffer();//GetBuffer比toArray效率高
            imgStream.Dispose();
            return bmpbyte;
        }
        /// <summary>
        /// 图片深度拷贝。
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Bitmap DeepCopyBitmap(Bitmap bmp)

        {
            try
            {
                DateTime t1 = DateTime.Now;
                Bitmap dstBitmap = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb);
                Graphics g = Graphics.FromImage(dstBitmap);
                g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height), 
                    new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
                g.Dispose();

                TimeSpan ts = DateTime.Now - t1;
                Console.WriteLine("DeepCopyBitmap runtime=" + ts.TotalMilliseconds);
                return dstBitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DeepCopyBitmap Error" + ex.ToString());
                return null;
            }

        }
        private Bitmap Byte2Bitmap(byte[] bmpbyte)
        {
            try
            {
                if (bmpbyte == null) return null;
                MemoryStream ms = new MemoryStream(bmpbyte);
                return (Bitmap)Image.FromStream(ms);
            }
            catch (Exception e)
            {
                Console.WriteLine("Byte2Bitmap Error" + e);
                return null;
            }
        }
        /// <summary>
        /// 画人脸线框。画多张人脸。
        /// </summary>
        /// <param name="img"></param>
        /// <param name="flist"></param>
        /// <returns></returns>
        public Image DrawFaceWires(Image img, List<FaceInfo> flist)
        {
            try
            {
                if (flist.Count != 0 && img != null)
                {
                    Graphics g = Graphics.FromImage(img);
                    Pen pen1 = new Pen(Color.Green, DrawFacePenThickness);
                    foreach (FaceInfo f in flist)
                    {
                        g.DrawRectangle(pen1, f.posLeft, f.posTop, f.FaceWidth(), f.FaceHight());
                    }
                    g.Dispose();
                }
                return img;
            }
            catch (Exception e) {
                Console.WriteLine("画框出错" + e);
                return null;
            }
        }
        /// <summary>
        /// 画人脸线框。只画一张人脸。
        /// </summary>
        /// <param name="img"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public Image DrawFaceWire(Image img, Rectangle rect)
        {
            try
            {
                if (rect != null && img != null)
                {
                    Graphics g = Graphics.FromImage(img);
                    Pen pen1 = new Pen(Color.Green, DrawFacePenThickness);
                    g.DrawRectangle(pen1, rect);
                    g.Dispose();
                }
                return img;
            }
            catch (Exception e)
            {
                Console.WriteLine("画框出错" + e);
                return null;
            }
        }
        /// <summary>
        /// 获取人脸特写抓拍图片。
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="f"></param>
        /// <returns>人脸特写bitmap图。</returns>
        public Bitmap GetFaceShotBmp(Bitmap bmp, FaceInfo f)
        {
            try
            {
                if (bmp == null || f == null) return null;
                Bitmap retbmp = new Bitmap(f.FaceWidth(), f.FaceHight(), PixelFormat.Format24bppRgb);//新建一个bmp，用来装载抠图
                Graphics g = Graphics.FromImage(retbmp);
                //截取原图相应区域写入作图区
                g.DrawImage(bmp, new Rectangle(0, 0, f.FaceWidth(), f.FaceHight()), 
                    f.getPos(), GraphicsUnit.Pixel);
                g.Dispose();
                return retbmp;
            }
            catch (Exception e)
            {
                Console.WriteLine("GetFaceShotBmp Error" + e);
                return null;
            }
        }
        /// <summary>
        /// 获取人脸特写抓拍图片。保留Margin。
        /// </summary>
        /// <param name="bmpbyte"></param>
        /// <param name="f"></param>
        /// <param name="margin">Margin占脸宽的百分比。</param>
        /// <returns></returns>
        public Bitmap GetFaceShotBmp(Bitmap bmp, FaceInfo f, double margin, out Rectangle srcRect)
        {
            try
            {
                if (bmp == null || f == null)
                {
                    srcRect = new Rectangle();
                    return null;
                }
                int m = (int)(f.FaceWidth() * margin);
                int l = Math.Max(f.posLeft - m, 0);
                int t = Math.Max(f.posTop - m, 0);
                int w = f.posRight + m - l;
                int h = f.posBottom + m - t;
                srcRect = new Rectangle(l, t, w, h);
                Bitmap retbmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);//新建一个bmp，用来装载抠图
                //创建作图区域
                Graphics g = Graphics.FromImage(retbmp);
                //截取原图相应区域写入作图区
                g.DrawImage(bmp, new Rectangle(0, 0, w, h), srcRect, GraphicsUnit.Pixel);
                g.Dispose();
                return retbmp;
            }
            catch (Exception e)
            {
                Console.WriteLine("GetFaceShotBmp Error" + e);
                srcRect = new Rectangle();
                return null;
            }
        }
        public Bitmap GetFaceShotBmp(Image img, FaceInfo f)
        {
            return GetFaceShotBmp((Bitmap)img, f);
        }
        /// <summary>
        /// 从人脸列表中获得最前面的一张人脸。
        /// </summary>
        /// <param name="flist"></param>
        /// <returns></returns>
        private FaceInfo GetFrontFace(List<FaceInfo> flist)
        {
            if (flist.Count == 0) return null;

            int maxFW = 0, fw;
            FaceInfo maxFI = null;
            foreach (FaceInfo f in flist)//循环当前列表
            {
                fw = f.FaceWidth();
                if (fw > maxFW)
                {
                    maxFW = fw;
                    maxFI = f;
                }
            }
            return maxFI;
        }


        /*********************************类型定义***********************************************/
        public enum ProcessLevel //人脸识别处理级别
        {
            Level_1,//轻量级：只识别人脸位置，速度最快。50ms级别
            Level_2,//中量级：识别人脸位置和角度。120ms级别
            Level_3 //重量级：识别人脸位置、角度、性别、年龄、特征值。400ms级别。
        }

        public enum FaceShotType
        {
            ShotNone,
            ShotOne,//只抓拍最前面一张人脸
            ShotAll//抓拍满足指定尺寸所有识别到的人脸，人脸最小尺寸20
        }
        public delegate void FaceEventHandler(FaceInfo f);

    }

}



