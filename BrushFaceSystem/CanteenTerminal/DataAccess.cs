using DBLayer;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;

namespace FaceSDK
{
    public class DataAccess
    {
        /***************************固定参数*****************************/
        //数据库配置
        private const string DATABASE_NAME = "FaceDataBase";
        private const string USER_TABLE_NAME = "FV_UserTable";
        private const string FACE_TABLE_NAME = "FV_FaceTable";
        //本地文件配置
        public const string DEFAULT_SAVE_PATH = ".FaceCameraData";

        /***************************共有属性*****************************/
        public DataSrcType DataSource { set; get; }

        /***************************私有变量*****************************/
        private FaceRecgnize _Face;
        private SqlConnectionStringBuilder _ConnStr;
        private MySqlConnection conn;
        private FaceDictionary _FaceDic;
        private static bool _isDBInitialized = false;
        private string _DataBaseDataSource = "localhost";
        private string _DataBaseUserID = "root";
        private string _DataBasePassword = "123456";
        private const int ERROR_NOT_INIT = -1;
        private const int ERROR_DB_FAILED = -2;

        public DataAccess(FaceRecgnize face, FaceDictionary fdic)
        {
            _Face = face;
            _FaceDic = fdic;
            DataSource = DataSrcType.FileSystem;

            //InitDB();
        }

        /***************************数据库访问接口*****************************/
        public bool InitDB(MySqlConnection c)
        {
            conn = c;
            try
            {
                ////打开连接
                //conn.Open();
                ////创建数据库
                //string sqlstr = "CREATE DATABASE IF NOT EXISTS " + DATABASE_NAME + ";" 
                //    + "use " + DATABASE_NAME + ";";
                //MySqlCommand cmd = new MySqlCommand(sqlstr, conn); 
                //cmd.ExecuteNonQuery();
                ////创建User表
                //string cmdstr = "CREATE TABLE IF NOT EXISTS "+ USER_TABLE_NAME
                //    + "("
                //    + "id int NOT NULL AUTO_INCREMENT primary key,"
                //    + "name varchar(30),"
                //    + "gender int,"
                //    + "age int,"
                //    + "telephone varchar(30)"
                //    + ");";
                //cmd = new MySqlCommand(cmdstr, conn);
                //cmd.ExecuteNonQuery();
                ////增加字符串的数据库名
                //_ConnStr.InitialCatalog = "FaceDataBase";

                //创建Face表
                string cmdstr = "CREATE TABLE IF NOT EXISTS "+ FACE_TABLE_NAME
                    + "("
                    + "id int NOT NULL AUTO_INCREMENT primary key,"
                    + "userid int NOT NULL,"
                    + "featurelength int,"
                    + "feature varbinary(4096),"
                    + "angleType int,"
                    + "imagesize int,"
                    + "imagedata MEDIUMBLOB"
                    + ");";
                MySqlCommand cmd = new MySqlCommand(cmdstr, conn);
                cmd.ExecuteNonQuery();
                //conn.Close();

                _isDBInitialized = true;
                return true;
            }
            catch(Exception ex)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                Console.WriteLine("DataAccess Init Error. " + ex.Message);
                return false;
            }
        }
        public void DeinitDB()
        {
            _isDBInitialized = false;
            if (conn.State == System.Data.ConnectionState.Open)
                conn.Close();
        }
        
        public int InsertUserInfo(string name, int gender, int age, string telephone)
        {
            try
            {
                if (!_isDBInitialized) return ERROR_NOT_INIT;
                int id = 0;
                //插入一条user记录
                MySqlHelper.ExecuteNonQuery(conn,
                    "INSERT INTO "+ USER_TABLE_NAME+ "(name,gender,age,telephone) " +
                    "VALUES(@name,@gender,@age,@telephone)",
                    new MySqlParameter("@name",name),
                    new MySqlParameter("@gender", gender),
                    new MySqlParameter("@age", age),
                    new MySqlParameter("@telephone", telephone));

                //获取插入记录的id
                MySqlDataReader reader = MySqlHelper.ExecuteReader(conn,
                "SELECT LAST_INSERT_ID()");
                if (reader.Read())
                {
                    id = reader.GetInt32(0);
                    Console.WriteLine("InsertUserInfo ok. id={0}", reader[0]);
                }
                reader.Close();
                return id;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataAccess InsertUserInfo Error. " + ex.Message);
                return ERROR_DB_FAILED;
            }
        }
        public int GetUserIDByName(string name)
        {
            try
            {
                if (!_isDBInitialized) return ERROR_NOT_INIT;
                int id = 0;
                MySqlDataReader reader = MySqlHelper.ExecuteReader(conn,
                    "SELECT id FROM " + USER_TABLE_NAME + " WHERE name=@name;",
                    new MySqlParameter("@name", name));
                if (reader.Read())
                {
                    id = reader.GetInt32(0);
                    Console.WriteLine("GetUserIDByName ok. id={0}", reader[0]);
                }
                reader.Close();
                return id;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataAccess GetUserIDByName Error. " + ex.Message);
                return ERROR_DB_FAILED;
            }
        }
        public int InsertUserInfoOverride(string name, int gender, int age, string telephone)
        {
            int uid = GetUserIDByName(name);
            if (uid == 0) return InsertUserInfo(name, gender, age, telephone);
            else return uid;
        }
        public int GetFaceIDByUid(int uid,FaceData.FaceAngleType type)
        {
            try
            {
                if (!_isDBInitialized) return ERROR_NOT_INIT;
                int id = 0;
                MySqlDataReader reader = MySqlHelper.ExecuteReader(conn,
                    "SELECT id FROM " + FACE_TABLE_NAME + " WHERE userid=@uid and angletype=@type;",
                    new MySqlParameter("@uid", uid),
                    new MySqlParameter("@type", (int)type));
                if (reader.Read())
                {
                    id = reader.GetInt32(0);
                    Console.WriteLine("GetFaceIDByUid ok. id={0}", reader[0]);
                }
                reader.Close();
                return id;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataAccess GetFaceIDByUid Error. " + ex.Message);
                return ERROR_DB_FAILED;
            }
        }
        public int InsertFaceInfo(FaceInfo fi, int userid)
        {
            try
            {
                if (!_isDBInitialized) return ERROR_NOT_INIT;
                if (fi.FaceShotBmp == null) return 0;
                byte[] imgdata = Image2PngByte(fi.FaceShotBmp);
                int id = 0;

                //插入一条user记录
                MySqlHelper.ExecuteNonQuery(conn,
                    "INSERT INTO " + FACE_TABLE_NAME +
                    "(userid,featurelength,feature,angletype,imagesize,imagedata) " +
                    "VALUES(@userid,@featurelength,@feature,@angleType,@imagesize,@imagedata)",
                    new MySqlParameter("@userid", userid),
                    new MySqlParameter("@featurelength", fi.feature.Length),
                    new MySqlParameter("@feature", fi.feature),
                    new MySqlParameter("@angletype", (int)fi.angleType),
                    new MySqlParameter("@imagesize", imgdata.Length),
                    new MySqlParameter("@imagedata", imgdata));

                //获取插入记录的id
                MySqlDataReader reader = MySqlHelper.ExecuteReader(conn,
                    "SELECT LAST_INSERT_ID()");
                if (reader.Read())
                {
                    id = reader.GetInt32(0);
                    Console.WriteLine("InsertFaceInfo ok. id={0}", id);
                }
                reader.Close();
                return id;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataAccess InsertUserInfo Error. " + ex.Message);
                return ERROR_DB_FAILED;
            }
        }
        public int InsertFaceInfoOverride(FaceInfo fi, int userid)
        {
            return UpdateFaceInfo(fi, userid);
        }
        public int UpdateFaceInfo(FaceInfo fi, int userid)
        {
            try
            {
                if (!_isDBInitialized) return ERROR_NOT_INIT;
                if (fi.FaceShotBmp == null) return 0;

                int ret = DeleteFaceInfo(userid, fi.angleType);
                if (ret < 0) return ret;

                return InsertFaceInfo(fi, userid);
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataAccess InsertUserInfo Error. " + ex.Message);
                return ERROR_DB_FAILED;
            }
        }
        public int GetFaceCount()
        {
            try
            {
                if (!_isDBInitialized) return ERROR_NOT_INIT;
                int total = 0;
                MySqlDataReader reader = MySqlHelper.ExecuteReader(conn,
                    "SELECT COUNT(*) FROM " + FACE_TABLE_NAME);
                if (reader.Read())
                {
                    total = reader.GetInt32(0);
                    Console.WriteLine("GetFaceCount total={0}", total);
                }
                reader.Close();
                return total;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataAccess GetFaceCount Error. " + ex.Message);
                return ERROR_DB_FAILED;
            }
        }
        private byte[] Image2PngByte(Image imgPhoto)
        {
            //将Image转换成流数据，并保存为byte[]
            MemoryStream mstream = new MemoryStream();
            imgPhoto.Save(mstream, System.Drawing.Imaging.ImageFormat.Png);
            byte[] byData = new Byte[mstream.Length];
            mstream.Position = 0;
            mstream.Read(byData, 0, byData.Length);
            mstream.Close();
            return byData;
        }
        private Bitmap Stream2Bitmap(byte[] streamByte)
        {
            MemoryStream ms = new MemoryStream(streamByte);
            Image img = Image.FromStream(ms);
            return new Bitmap(img);
        }
        public void LoadFaceDataAsync(Form context, ProgressBar proBar, Action<int> callback)
        {
            Func<int> f = () =>
            {
                return LoadFaceData(proBar);
            };
            AsyncCallback cb = new AsyncCallback((iar) =>
            {
                Action a = () => {
                    AsyncResult ar = (AsyncResult)iar;
                    Func<int> ff = (Func<int>)ar.AsyncDelegate;
                    int picNum = ff.EndInvoke(iar);
                    callback(picNum);
                };
                context.Invoke(a);
            });
            if (callback == null) cb = null;
            f.BeginInvoke(cb, null);
        }
        public int LoadFaceInfo()
        {
            try
            {
                if (!_isDBInitialized) return ERROR_NOT_INIT;
                int count = 0;
                //获取所有人脸信息
                MySqlDataReader reader = MySqlHelper.ExecuteReader(conn,
                    "SELECT id,name FROM " + USER_TABLE_NAME);
                while (reader.Read())
                {
                    int uid = reader.GetInt32(reader.GetOrdinal("id"));
                    string name = reader.GetString(reader.GetOrdinal("name"));
                    _FaceDic.AddNameDic(uid, name);
                    count++;
                }
                reader.Close();
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataAccess LoadFaceInfo Error. " + ex.Message);
                return ERROR_DB_FAILED;
            }
        }
        public int LoadUserInfo()
        {
            try
            {
                int count = 0;
                //获取所有人脸信息
                MySqlDataReader reader = UserInfo.GetSqlReader();
                while (reader.Read())
                {
                    int uid = reader.GetInt32(reader.GetOrdinal("uid"));
                    string name = reader.GetString(reader.GetOrdinal("username"));
                    _FaceDic.AddNameDic(uid, name);
                    count++;
                }
                reader.Close();
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataAccess LoadUserInfo Error. " + ex.Message);
                return ERROR_DB_FAILED;
            }
        }
        public int LoadFaceData(ProgressBar proBar)
        {
            try
            {
                if (!_isDBInitialized) return ERROR_NOT_INIT;
                int count = 0;
                int total = GetFaceCount();
                if (total == 0)
                {
                    Console.WriteLine("LoadFace no record, exit.");
                    return 0;
                }

                if (proBar != null)
                {
                    if (proBar.InvokeRequired)
                    {
                        Action a = () =>
                        {
                            proBar.Maximum = total;
                            proBar.Value = 0;
                        };
                        proBar.Invoke(a);
                    }
                    else
                    {
                        proBar.Maximum = total;
                        proBar.Value = 0;
                    }
                }

                //装载用户信息
                LoadUserInfo();

                //获取所有人脸信息
                MySqlDataReader reader = MySqlHelper.ExecuteReader(conn,
                    "SELECT id,userid,featurelength,feature,angletype FROM " + FACE_TABLE_NAME);

                while (reader.Read())
                {
                    int fid = reader.GetInt32(reader.GetOrdinal("id"));
                    int userid = reader.GetInt32(reader.GetOrdinal("userid"));
                    int featurelength = reader.GetInt32(reader.GetOrdinal("featurelength"));
                    byte[] feature = new byte[featurelength];
                    reader.GetBytes(reader.GetOrdinal("feature"), 0, feature, 0, featurelength);
                    FaceData.FaceAngleType angletype = (FaceData.FaceAngleType)reader.GetInt32(reader.GetOrdinal("angletype"));
                    

                    FaceData fd = new FaceData();
                    fd.faceid = fid;
                    fd.userid = userid;
                    fd.featureLenth = (short)featurelength;
                    fd.feature = feature;
                    fd.angleType = angletype;

                    _FaceDic.AddFace(userid, fd);

                    count++;
                    if (proBar != null)
                    {
                        if (proBar.InvokeRequired)
                        {
                            Action d = () =>
                            {
                                proBar.Value++;
                            };
                            proBar.Invoke(d);
                        }
                        else
                        {
                            proBar.Value++;
                        }
                    }
                }
                reader.Close();
                Console.WriteLine("LoadFace ok. read count={0}", count);
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataAccess LoadFace Error. " + ex.Message);
                return ERROR_DB_FAILED;
            }
        }
        public Bitmap GetFaceImage(int uid)
        {
            try
            {
                if (!_isDBInitialized) return null;
                Bitmap bmp = null;
                MySqlDataReader reader = MySqlHelper.ExecuteReader(conn,
                    "SELECT angletype,imagesize,imagedata FROM " + FACE_TABLE_NAME +
                    " WHERE userid=@userid order by angletype asc",
                    new MySqlParameter("@userid", uid));
                if (reader.Read())
                {
                    int imagesize = reader.GetInt32(reader.GetOrdinal("imagesize"));
                    byte[] imgdata = new byte[imagesize];
                    reader.GetBytes(reader.GetOrdinal("imagedata"), 0, imgdata, 0, imagesize);
                    bmp = Stream2Bitmap(imgdata);
                }
                reader.Close();
                return bmp;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataAccess GetFaceCount Error. " + ex.Message);
                return null;
            }
        }
        public int DeleteUserInfo(int uid)
        {
            try
            {
                if (!_isDBInitialized) return ERROR_NOT_INIT;
                //删除face表中uid的人脸信息
                int ret = DeleteFaceInfo(uid);
                if (ret < 0) return ret;
                ret = MySqlHelper.ExecuteNonQuery(conn,
                    "DELETE FROM " + USER_TABLE_NAME + " WHERE id=@id",
                    new MySqlParameter("@id", uid));
                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataAccess DeleteUserInfo Error. " + ex.Message);
                return ERROR_DB_FAILED;
            }
        }
        public int DeleteFaceInfo(int uid)
        {
            try
            {
                if (!_isDBInitialized) return ERROR_NOT_INIT;
                //删除表格所有内容
                int ret = MySqlHelper.ExecuteNonQuery(conn,
                    "DELETE FROM " + FACE_TABLE_NAME + " WHERE userid=@userid",
                    new MySqlParameter("@userid", uid));
                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataAccess DeleteFaceInfo Error. " + ex.Message);
                return ERROR_DB_FAILED;
            }
        }
        public int DeleteFaceInfo(int uid, FaceData.FaceAngleType type)
        {
            try
            {
                if (!_isDBInitialized) return ERROR_NOT_INIT;
                //删除表格所有内容
                int ret = MySqlHelper.ExecuteNonQuery(conn,
                    "DELETE FROM " + FACE_TABLE_NAME + " WHERE userid=@userid and angletype=@type;",
                    new MySqlParameter("@userid", uid),
                    new MySqlParameter("@type", type));
                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataAccess DeleteFaceInfo Error. " + ex.Message);
                return ERROR_DB_FAILED;
            }
        }
        public int DeleteAllUser()
        {
            try
            {
                if (!_isDBInitialized) return ERROR_NOT_INIT;
                //删除表格所有内容
                int ret = MySqlHelper.ExecuteNonQuery(conn,
                    "TRUNCATE TABLE " + USER_TABLE_NAME);
                
                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataAccess DeleteAllUser Error. " + ex.Message);
                return ERROR_DB_FAILED;
            }
        }
        public int DeleteAllFace()
        {
            try
            {
                if (!_isDBInitialized) return ERROR_NOT_INIT;
                //删除表格所有内容
                int ret = MySqlHelper.ExecuteNonQuery(conn,
                    "TRUNCATE TABLE " + FACE_TABLE_NAME);

                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataAccess DeleteAllFace Error. " + ex.Message);
                return ERROR_DB_FAILED;
            }
        }

        /***************************文件访问接口*****************************/
        public Dictionary<int, string> getUserNameDic()
        {
            return null;
        }
        public void UpdateFaceOrgPic(FaceData fd)
        {
            
        }
        public Bitmap GetFaceOrgPic(FaceData fd)
        {
            if (DataSource == DataSrcType.FileSystem)
            {
                return new Bitmap(Image.FromFile(fd.text));
            }
            else if (DataSource == DataSrcType.DataBase)
                return GetFaceImage(fd.userid);//TODO
            else
                return null;
        }
        /// <summary>
        /// 从本地文件夹添加人脸图片库。异步函数。
        /// </summary>
        /// <param name="context">页面上下文</param>
        /// <param name="path">文件夹路径</param>
        /// <param name="alldirs">是否包含子文件夹</param>
        /// <param name="proBar">进度条引用。不需要进度条设为null</param>
        /// <param name="callback">图片处理完后进行回调。Action参数为处理的图片数。</param>
        public void LoadFaceDataAsync(Form context, string path, bool alldirs, ProgressBar proBar, Action<int> callback)
        {
            Func<int> f = () =>
            {
                return LoadFaceData(path, alldirs, proBar);
            };
            AsyncCallback cb = new AsyncCallback((iar) =>
            {
                Action a = () =>
                {
                    AsyncResult ar = (AsyncResult)iar;
                    Func<int> ff = (Func<int>)ar.AsyncDelegate;
                    int picNum = ff.EndInvoke(iar);
                    if (picNum >= 0) callback(picNum);
                };
                context.Invoke(a);
            });
            if (callback == null) cb = null;
            f.BeginInvoke(cb, null);
        }
        /// <summary>
        /// 用图片添加人脸数据库。同步函数。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="alldirs"></param>
        /// <param name="proBar"></param>
        /// <returns>处理的图片数。</returns>
        public int LoadFaceData(string path, bool alldirs, ProgressBar proBar)
        {
            //if (DataSource != DataSrcType.FileSystem) return 0;
            if (!Directory.Exists(path)) return 0;

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
                if (proBar.InvokeRequired)
                {
                    Action a = () =>
                    {
                        proBar.Maximum = files.Count();
                        proBar.Value = 0;
                    };
                    proBar.Invoke(a);
                }
                else
                {
                    proBar.Maximum = files.Count();
                    proBar.Value = 0;
                }

            }

            foreach (string fstr in files)
            {
                Console.WriteLine(fstr);
                string fname = Path.GetFileNameWithoutExtension(fstr); //System.IO.Path.GetFileName(fstr);
                if (fname[0] != '.' && !string.IsNullOrEmpty(fname))//去除.开头的错误文件
                {
                    string n = fname;
                    char lc = fname[fname.Length - 1];
                    while (lc == ' ' || lc == '-' || lc == '_' || char.IsDigit(lc))
                    {
                        fname = fname.Remove(fname.Length - 1);
                        if (fname.Length == 0)
                        {
                            fname = n;
                            break;
                        }
                        lc = fname[fname.Length - 1];
                    }
                    FaceInfo fi = _Face.FaceRecgOne(fstr);
                    int ret = AddUserFace(fi, fname);
                    if (ret < 0) return ret;
                }
                if (proBar != null)
                {
                    if (proBar.InvokeRequired)
                    {
                        Action d = () =>
                        {
                            proBar.Value++;
                        };
                        proBar.Invoke(d);
                    }
                    else
                    {
                        proBar.Value++;
                    }
                }
            }
            //DataSource = DataSrcType.PicFiles;
            return files.Count();
        }
        public void SaveUserFaceAsync(string name, int gender, int age, string telephone,
            FaceInfo fi, string bmpsavepath)
        {
            Action a = () =>
            {
                SaveUserFace(name, gender, age, telephone, fi, bmpsavepath);
            };
            a.BeginInvoke(null, null);
        }
        
        /// <summary>
        /// 保存人脸图片到本地，并添加到人脸字典中。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fi"></param>
        /// <param name="bmpsavepath"></param>
        /// <returns></returns>
        public int SaveUserFace(string name, int gender, int age, string telephone,
            FaceInfo fi, string bmpsavepath)
        {
            try
            {
                if (string.IsNullOrEmpty(name)) return 0;
                if (fi == null) return 0;
                if (string.IsNullOrEmpty(bmpsavepath)) bmpsavepath = DEFAULT_SAVE_PATH;
                if (!Directory.Exists(bmpsavepath)) Directory.CreateDirectory(bmpsavepath);

                if (DataSource == DataSrcType.FileSystem)
                {
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
                }
                
                int uid = AddUserFace(fi, name, gender, age, telephone);
                return uid;
            }
            catch (Exception e)
            {
                Console.WriteLine("SaveFaceAndAddToDic Error: " + e.ToString());
                return 0;
            }
        }
        private int AddUserFace(FaceInfo fi, string name)
        {
            if (fi == null) return 0;
            return AddUserFace(fi, name, (int)fi.gender, fi.age, "");
        }
        private int AddUserFace(FaceInfo fi, string name, int gender, int age, string telephone)
        {
            if (fi == null) return 0;
            int uid = 0;
            int fid = 0;
            switch (DataSource)
            {
                default:
                case DataSrcType.FileSystem:
                    uid = _FaceDic.GetNextUserId(name);
                    break;
                case DataSrcType.DataBase:
                    uid = InsertUserInfoOverride(name, gender, age, telephone);
                    if (uid <= 0) return uid;
                    fid = InsertFaceInfoOverride(fi, uid);
                    if (fid <= 0) return fid;
                    fi.faceid = fid;
                    break;
            }
            
            fi.userid = uid;
            _FaceDic.AddFace(uid, fi, name);//添加到字典中
            return uid;
        }
        public int AddUserFace(FaceInfo fi, UserInfo ui)
        {
            if (fi == null) return 0;
            int uid = 0;
            int fid = 0;
            switch (DataSource)
            {
                default:
                case DataSrcType.FileSystem:
                    uid = _FaceDic.GetNextUserId(ui.username);
                    break;
                case DataSrcType.DataBase:
                    uid = ui.uid = _FaceDic.GetNextUserId(ui.username);
                    if (!ui.Save()) return 0;
                    
                    fid = InsertFaceInfoOverride(fi, uid);
                    if (fid <= 0) return fid;
                    fi.faceid = fid;
                    break;
            }

            fi.userid = uid;
            _FaceDic.AddFace(uid, fi, ui.username);//添加到字典中
            return uid;
        }
        public int AddUserFaces(FaceInfo[] fis, UserInfo ui)
        {
            if (fis == null) return 0;
            if (ui == null) return 0;
            int uid = 0;
            int fid = 0;
            int count = 0;
            uid = ui.uid = _FaceDic.GetNextUserId(ui.username);
            if (!ui.Save()) return 0;
            foreach (FaceInfo fi in fis)
            {
                fid = InsertFaceInfoOverride(fi, uid);
                if (fid <= 0) return fid;
                fi.faceid = fid;
                fi.userid = uid;
                _FaceDic.AddFace(uid, fi, ui.username);//添加到字典中
                count++;
            }

            return count;
        }
        public void AddUserFaceAsync(FaceInfo fi, UserInfo ui)
        {
            Action a = () =>
            {
                AddUserFace(fi,ui);
            };
            a.BeginInvoke(null, null);
        }
        public void AddUserFacesAsync(FaceInfo[] fis, UserInfo ui)
        {
            Action a = () =>
            {
                AddUserFaces(fis, ui);
            };
            a.BeginInvoke(null, null);
        }

        /// <summary>
        /// 更新已登记的人脸图片。
        /// </summary>
        /// <param name="f"></param>
        /// <param name="fd"></param>
        public void UpdateFacePicture(FaceInfo f, FaceData fd)
        {
            if (DataSource == DataSrcType.FileSystem)
            {
                //存储抓拍到的新图片，替换原始图片
                string savepath;
                int uid = f.userid;
                switch (f.angleType)
                {
                    case FaceData.FaceAngleType.Middle:
                        savepath = Path.GetDirectoryName(fd.text) + "\\"
                            + _FaceDic.GetUserName(uid) + "0.jpg";
                        break;
                    case FaceData.FaceAngleType.Up:
                        savepath = Path.GetDirectoryName(fd.text) + "\\"
                            + _FaceDic.GetUserName(uid) + "1.jpg";
                        break;
                    case FaceData.FaceAngleType.Down:
                        savepath = Path.GetDirectoryName(fd.text) + "\\"
                            + _FaceDic.GetUserName(uid) + "2.jpg";
                        break;
                    case FaceData.FaceAngleType.Left:
                        savepath = Path.GetDirectoryName(fd.text) + "\\"
                            + _FaceDic.GetUserName(uid) + "3.jpg";
                        break;
                    case FaceData.FaceAngleType.Right:
                        savepath = Path.GetDirectoryName(fd.text) + "\\"
                            + _FaceDic.GetUserName(uid) + "4.jpg";
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
                //TODO
            }
        }
        

        public enum DataSrcType
        {
            NONE,
            FileSystem,
            DataBase,
            Others
        }
    }
}
