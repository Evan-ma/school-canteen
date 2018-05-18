using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceSDK
{
    /*********************************人脸字典类*******************************************/
    public class FaceDictionary : Dictionary<int, List<FaceData>>
    {
        private FaceCamera _Camera;
        private FaceRecgnize _Face;
        //private DataAccess _Data;
        private Dictionary<int, string> _FaceNameDic;

        public int FaceCount { get; private set; }//获取人脸数据数量

        public FaceDictionary(FaceCamera camera, FaceRecgnize face/*, DataAccess data*/) : base()
        {
            _Camera = camera;
            _Face = face;
            //_Data = data;
            FaceCount = 0;
            _FaceNameDic = new Dictionary<int, string>();
        }

        /// <summary>
        /// 向人脸字典中添加新的人脸。同样角度类型的人脸只存一张。
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="f"></param>
        public void AddFace(int uid, FaceData f)
        {
            FaceData fd = new FaceData(f);
            bool overide = false;
            if (this.ContainsKey(uid))
            {
                //删除相同角度的脸
                for (int i = 0; i < this[uid].Count; i++)
                {
                    if (f.angleType == this[uid][i].angleType)
                    {
                        this[uid].RemoveAt(i);
                        overide = true;
                    }
                }
                //添加新的脸
                this[uid].Add(fd);
            }
            else
            {
                List<FaceData> fl = new List<FaceData>();
                fl.Add(fd);
                this.Add(uid, fl);
            }
            if(!overide) FaceCount++;
        }
        public void AddFace(int uid, FaceData f, string name)
        {
            AddFace(uid, f);
            AddNameDic(uid, name);
        }
        public Image GetUserImageById(int uid)
        {
            
            if (_Camera.Data.DataSource == DataAccess.DataSrcType.FileSystem)
            {
                if (this.ContainsKey(uid))
                    return Image.FromFile(this[uid][0].text);
                else return null;
            }
            else if (_Camera.Data.DataSource == DataAccess.DataSrcType.DataBase)
                return _Camera.Data.GetFaceImage(uid);
            else
                return null;
        }
        public string GetUserName(int uid)
        {
            if (_FaceNameDic.ContainsKey(uid)) return _FaceNameDic[uid];
            else return string.Empty;
        }
        public int GetUserId(string name)
        {
            foreach (var n in _FaceNameDic)
            {
                if (n.Value == name) return n.Key;
            }
            return 0;
        }
        public int GetNextUserId(string name)
        {
            int maxid = 0;
            foreach (var n in _FaceNameDic)
            {
                if (n.Value == name) return n.Key;
                if (n.Key > maxid) maxid = n.Key;
            }
            return ++maxid;
        }
        public int AddNameDic(int uid, string name)
        {
            if (_FaceNameDic.ContainsKey(uid)) return 0;
            _FaceNameDic.Add(uid, name);
            return 1;
        }
        public int GetUseridAt(int index)
        {
            return this.Keys.ToList()[index];
        }
        public List<FaceData> GetFacesAt(int i)
        {
            return this.Values.ToList()[i];
        }
        public FaceData GetFaceData(int fid)
        {
            foreach (var fl in this)
            {
                foreach (FaceData fd in fl.Value)
                {
                    if (fd.faceid == fid) return fd;
                }
            }
            return null;
        }
        public Image GetUserImageAt(int i)
        {
            if (_Camera.Data.DataSource == DataAccess.DataSrcType.FileSystem)
                return Image.FromFile(this.Values.ToList()[i][0].text);
            else if (_Camera.Data.DataSource == DataAccess.DataSrcType.DataBase)
                return _Camera.Data.GetFaceImage(this.Keys.ToList()[i]);
            else
                return null;
        }
        public int UserCount { get { return this.Count; } }//字典中人数
        public int DeleteUserInfo(int uid)
        {
            int ret = 0;
            if (this.ContainsKey(uid)) this.Remove(uid);
            if (_FaceNameDic.ContainsKey(uid)) {
                _FaceNameDic.Remove(uid);
                ret = 1;
            }
            if (_Camera.Data.DataSource == DataAccess.DataSrcType.DataBase)
                ret = _Camera.Data.DeleteUserInfo(uid);
            return ret;
        }
        public int DeleteAllFaces()
        {
            int c = _FaceNameDic.Count;
            _FaceNameDic.Clear();
            this.Clear();
            if (_Camera.Data.DataSource == DataAccess.DataSrcType.DataBase)
            {
                int ret = _Camera.Data.DeleteAllFace();
                if (ret < 0) c = ret;
                ret = _Camera.Data.DeleteAllUser();
                if (ret < 0) c = ret;
            }
            return c;
        }

        /// <summary>
        /// 查找最相似的人脸。
        /// </summary>
        /// <param name="fi"></param>
        /// <param name="score"></param>
        /// <param name="outfd"></param>
        /// <returns></returns>
        public int FindMostSimilarFace(FaceInfo fi, out int score, out FaceData outfd)
        {
            int maxs = 0, s, uid = 0;
            FaceData ofd = null;
            foreach (var fl in this)
            {
                foreach (FaceData fd in fl.Value)
                {
                    s = _Face.FaceFeatureMatch(fi, fd);
                    if (s > maxs)
                    {
                        maxs = s;
                        uid = fl.Key;
                        ofd = fd;
                    }
                }
            }
            score = maxs;
            outfd = ofd;
            return uid;
        }
         /// <summary>
        /// 查找最相似的人脸。
        /// </summary>
        /// <param name="fi"></param>
        /// <param name="outfd"></param>
        /// <returns></returns>
        public int FindMostSimilarFace2(FaceInfo fi, out int score, out FaceData outfd)
        {
            int maxs = 0, tops1 = 0, tops2 = 0, s, uid = 0;
            FaceData ofd = null;
            outfd = null;
            foreach (var fl in this)
            {
                //1.计算一个人的相似度分数
                tops1 = 0;
                tops2 = 0;
                foreach (FaceData fd in fl.Value)
                {
                    if (fd.angleType == FaceData.FaceAngleType.Up && fi.angleType == FaceData.FaceAngleType.Down ||
                        fd.angleType == FaceData.FaceAngleType.Down && fi.angleType == FaceData.FaceAngleType.Up ||
                        fd.angleType == FaceData.FaceAngleType.Left && fi.angleType == FaceData.FaceAngleType.Right ||
                        fd.angleType == FaceData.FaceAngleType.Right && fi.angleType == FaceData.FaceAngleType.Left)
                        continue;//角度相对情况下，跳过
                    s = _Face.FaceFeatureMatch(fi, fd);
                    //Console.WriteLine(Path.GetFileNameWithoutExtension(fd.text) + ": " + s.ToString());
                    if (s > tops1)
                    {
                        tops2 = tops1;
                        tops1 = s;
                        ofd = fd;
                    }
                    else if (s > tops2)
                    {
                        tops2 = s;
                    }
                }
                if (tops2 != 0) s = (tops1 + tops2) / 2;//用前两张脸的平均分作为该人的相似度分值
                else s = (int)(tops1 * 0.95);//只有一张脸的情况下，降低可信度

                //记录最大分数值
                if (s > maxs)
                {
                    maxs = s;
                    uid = fl.Key;
                    outfd = ofd;
                }
            }
            score = maxs;
            return uid;
        }
        public int FindTopNSimilarFace(FaceInfo fi, int N, out int[] scores, out FaceInfo[] outfis)
        {
            if (N > this.FaceCount) N = this.FaceCount;
            if (fi==null || N == 0)
            {
                scores = null;
                outfis = null;
                return -1;
            }
            int s,i;
            scores = new int[N];
            FaceData fd0;
            FaceData[] outfds = new FaceData[N];
            foreach (var fl in this)
            {
                foreach (FaceData fd in fl.Value)
                {
                    s = _Face.FaceFeatureMatch(fi, fd);
                    if (s > scores[N - 1])
                    {
                        scores[N - 1] = s;
                        outfds[N - 1] = fd;
                    }
                    else continue;

                    for (i = N - 1; i > 0; i--)
                    {
                        if (scores[i] > scores[i - 1])
                        {
                            //交换分数
                            s = scores[i];
                            scores[i] = scores[i - 1];
                            scores[i - 1] = s;
                            //交换人脸数据
                            fd0 = outfds[i];
                            outfds[i] = outfds[i - 1];
                            outfds[i - 1] = fd0;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            outfis = new FaceInfo[N];
            for(i = 0; i < N; i++)
            {
                outfis[i] = new FaceInfo(outfds[i]);
                outfis[i].FaceShotBmp = this.GetUserImageById(outfds[i].userid);
            }
            return N;
        }
        /// <summary>
        /// 查找相同的人脸
        /// </summary>
        /// <param name="fi"></param>
        /// <param name="score"></param>
        /// <param name="outfd"></param>
        /// <returns></returns>
        public int FindSameFace(FaceInfo fi, out int score, out FaceData outfd)
        {
            if (fi == null)
            {
                score = 0;
                outfd = null;
                return 0;
            }
            int maxs = 0, s, uid = 0;
            FaceData ofd = null;
            foreach (var fl in this)
            {
                foreach (FaceData fd in fl.Value)
                {
                    s = _Face.FaceFeatureMatch(fi, fd);
                    if (s > maxs)
                    {
                        maxs = s;
                        uid = fl.Key;
                        ofd = fd;
                    }
                }
            }
            if (maxs >= _Face.SameFaceThreshold)
            {
                score = maxs;
                outfd = ofd;
            }
            else
            {
                score = 0;
                outfd = null;
                uid = 0;
            }
            return uid;
        }

    }
}
