using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My_Menu
{
    /*********************************人脸字典类*******************************************/
    public class FaceDictionary : Dictionary<int, List<FaceData>>
    {
        private FaceRecgnize _face;
        public int FaceCount { get; private set; }//获取人脸数据数量

        public FaceDictionary(FaceRecgnize p) : base()
        {
            _face = p;
            FaceCount = 0;
        }

        /// <summary>
        /// 向人脸字典中添加新的人脸。同样角度类型的人脸只存一张。
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="f"></param>
        public void AddFace(int uid, FaceData f)
        {
            FaceData fd = new FaceData(f);
            if (this.ContainsKey(uid))
            {
                //删除相同角度的脸
                for (int i = 0; i < this[uid].Count; i++)
                {
                    if (f.angleType == this[uid][i].angleType)
                        this[uid].RemoveAt(i);
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
        }
        /// <summary>
        /// 从图片添加数据建立人脸数据。
        /// </summary>
        /// <param name="path"></param>
        public void AddFaceFromImage(int uid, string path)
        {
            Bitmap bmp = (Bitmap)Image.FromFile(path);
            List<FaceInfo> flist = _face.FaceRecg(bmp);
            if (flist.Count == 1)
            {
                flist[0].userid = uid;
                flist[0].text = path;
                AddFace(uid, flist[0]);
                FaceCount++;
            }
            bmp.Dispose();
            flist.Clear();
        }
        public List<FaceData> getFacesAt(int i)
        {
            return this.Values.ToList()[i];
        }
        public Image getUserImageAt(int i)
        {
            return Image.FromFile(this.Values.ToList()[i][0].text);
        }
        public Image getUserImageById(int uid)
        {
            if (this.ContainsKey(uid))
                return Image.FromFile(this[uid][0].text);
            else return null;
        }
        public int UseridAt(int index)
        {
            return this.Keys.ToList()[index];
        }
        public int UserCount { get { return this.Count; } }//字典中人数

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
                    s = _face.FaceFeatureMatch(fi, fd);
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
                    s = _face.FaceFeatureMatch(fi, fd);
                    if (s > maxs)
                    {
                        maxs = s;
                        uid = fl.Key;
                        ofd = fd;
                    }
                }
            }
            if (maxs >= _face.SameFaceThreshold)
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
                    s = _face.FaceFeatureMatch(fi, fd);
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

    }
}
