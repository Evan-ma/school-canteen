using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using My_Menu;
using System.IO;
using FaceSDK;
using System.Data;

namespace DBLayer
{
    class SQLModel
    {
        private const string DATABASE_NAME = "BrushFaceSystemDB";

        public static bool InitDB()
        {
            try
            {
                if (SQLEngine.Instance.Connection == null) return false;

                //创建数据库
                string sqlstr = "CREATE DATABASE IF NOT EXISTS " + DATABASE_NAME + ";"
                    + "use " + DATABASE_NAME + ";";
                MySqlCommand cmd = new MySqlCommand(sqlstr, SQLEngine.Instance.Connection);
                cmd.ExecuteNonQuery();
                //创建表
                FaceInfoBase.MakeTable();
                UserInfo.MakeTable();
                MoneyRecord.MakeTable();

                return true;

            }catch(Exception ex)
            {
                return false;
            }
            
        }
    }
    class User
    {
        public int uid;

        public FaceInfoBase faceinfobase;
        public UserInfo userinfo;
        public MoneyRecord[] moneyrecords;

        public User(int uid)
        {
            this.uid = uid;
            this.faceinfobase = null;
            this.userinfo = null;
            this.moneyrecords = null;
        }

        public FaceInfoBase GetFaceInfoBase()
        {
            return faceinfobase ?? (faceinfobase = FaceInfoBase.FindByUid(uid));
        }

        public UserInfo GetUserInfo()
        {
            return userinfo ?? (userinfo = UserInfo.Get(uid));
        }

        //public MoneyRecord[] GetMoneyRecordo()
        //{
        //    //return moneyrecords ?? (moneyrecords = MoneyRecord.FindByUid(uid));
        //}

        #region IntegrateTest
        public static void Test()
        {
            Console.WriteLine("TODO");
        }
        #endregion
    }

    class FaceInfoBase
    {
        public static string table_name = "faceinfotable";

        public int? uid = null;
        public FaceInfo faceInfo;

        public static void MakeTable()
        {
            string sql = "CREATE TABLE IF NOT EXISTS " + table_name + " (" +
                "uid INT AUTO_INCREMENT PRIMARY KEY," +
                "gender TINYINT DEFAULT 0," +
                "facetype TINYINT DEFAULT 0," +
                "featurelength INT DEFAULT 0," +
                "featuredata BINARY," +
                "picdata BINARY" +
                ") Engine = MyISAM CHARSET = utf8;";
            MySqlCommand cmd = new MySqlCommand(sql, SQLEngine.Instance.Connection);
            if (cmd.ExecuteNonQuery() != 0) Console.WriteLine("Error Create Table {0}", table_name);
        }

        public FaceInfoBase() { }
        public FaceInfoBase(FaceInfo faceInfo) {
            this.uid = null;
            this.faceInfo = faceInfo;
        }

        public static FaceInfoBase FindByUid(int uid)
        {
            Console.WriteLine("这个接口可能没意义，暂时不实现");
            return null;
        }

        public static FaceInfoBase Create(FaceInfo faceinfo)
        {
            FaceInfoBase faceInfoBase = new FaceInfoBase(faceinfo);
            int gender = 1;
            int faceType;
            switch (faceinfo.angleType)
            {
                case FaceData.FaceAngleType.Middle:
                    faceType = 0;
                    break;
                case FaceData.FaceAngleType.Up:
                    faceType = 1;
                    break;
                case FaceData.FaceAngleType.Down:
                    faceType = 2;
                    break;
                case FaceData.FaceAngleType.Left:
                    faceType = 3;
                    break;
                case FaceData.FaceAngleType.Right:
                    faceType = 4;
                    break;
                default:
                    faceType = 5;
                    break;

            }
            if(faceinfo.gender == FaceInfo.Gender.Femail)
            {
                gender = 0;
            }
            string sql = String.Format("INSERT INTO {0}" +
                "(gender, facetype, featurelength, featuredata, picdata)" +
                " VALUES({1}, {2}, {3}, @feature, @bmp);", table_name,
                faceinfo.gender, faceType, faceinfo.featureLenth);
            MySqlCommand cmd = new MySqlCommand(sql, SQLEngine.Instance.Connection);
            cmd.Parameters.Add("@feature", MySqlDbType.Binary, faceinfo.feature.Length);
            cmd.Parameters["@feature"].Value = faceinfo.feature;
            cmd.Parameters.Add("@bmp", MySqlDbType.Binary, PhotoImageInsert(faceinfo.FaceShotBmp).Length);
            cmd.Parameters["@bmp"].Value = PhotoImageInsert(faceinfo.FaceShotBmp);
            if (cmd.ExecuteNonQuery() != 1)
            {
                Console.WriteLine("Error UPDATE model {0}", faceinfo);
                return null;
            }
            else
            {
                sql = String.Format("SELECT MAX(uid) FROM {0};", table_name);
                cmd = new MySqlCommand(sql, SQLEngine.Instance.Connection);
                faceInfoBase.uid = (Int32)cmd.ExecuteScalar();
                return faceInfoBase;
            }
        }

        private static byte[] PhotoImageInsert(Image imgPhoto)
        {
           
            MemoryStream mstream = new MemoryStream();
            imgPhoto.Save(mstream, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] byData = new Byte[mstream.Length];
            mstream.Position = 0;
            mstream.Read(byData, 0, byData.Length); mstream.Close();
            return byData;
        }
        
        public static List<FaceInfoBase> GetList(int gender)
        {
            if(gender >= 1)
            {
                gender = 1;//男
            }
            else
            {
                gender = 0;
            }
            List<FaceInfoBase> faceInfoBases = new List<FaceInfoBase>();
            string sql = String.Format("SELECT * FROM {0} WHERE gender = {1};", table_name, gender);
            MySqlCommand cmd = new MySqlCommand(sql, SQLEngine.Instance.Connection);
            MySqlDataReader rs = cmd.ExecuteReader();
            while (rs.Read())
            {
                int uid = rs.GetInt32("uid");
                FaceInfo faceInfo = new FaceInfo();
                switch (rs.GetInt32("facetype"))
                {
                    case 0:
                        faceInfo.angleType = FaceData.FaceAngleType.Middle;
                        break;
                    case 1:
                        faceInfo.angleType = FaceData.FaceAngleType.Up;
                        break;
                    case 2:
                        faceInfo.angleType = FaceData.FaceAngleType.Down;
                        break;
                    case 3:
                        faceInfo.angleType = FaceData.FaceAngleType.Left;
                        break;
                    case 4:
                        faceInfo.angleType = FaceData.FaceAngleType.Right;
                        break;
                    default:
                        faceInfo.angleType = FaceData.FaceAngleType.Unknown;
                        break;

                }
                faceInfo.featureLenth = (short)rs.GetInt32("featurelength");
                faceInfo.feature = (byte[])rs.GetValue(5);
                faceInfo.FaceShotBmp = ReturnPhoto((byte[])rs.GetValue(6));
                FaceInfoBase faceInfoBase = new FaceInfoBase(faceInfo);
                faceInfoBase.uid = uid;
                faceInfoBases.Add(faceInfoBase);
            }
            rs.Close();
            return faceInfoBases;
        }

        private static Image ReturnPhoto(byte[] streamByte)
        {
            MemoryStream ms = new MemoryStream(streamByte);
            Image img = Image.FromStream(ms);
            return img;
        }

        #region UnitTest
        public static void Test()
        {
            FaceInfoBase.MakeTable();

            Console.WriteLine("这个接口可能没意义，暂时不实现");
        }
        #endregion
    }

    public class UserInfo
    {
        public static string table_name = "userinfotable";
        public int uid;
        public string usernumber;
        public string username;
        public int gender;
        public string phonenumber;
        public string address;
        public string node;
        public double money;

        public static void MakeTable()
        {
            string sql = "CREATE TABLE IF NOT EXISTS " + table_name + " (" +
                "uid INT PRIMARY KEY, " +
                "usernumber TEXT NOT NULL," +
                "username TEXT NOT NULL," +
                "gender TINYINT DEFAULT 0," +
                "phonenumber CHAR(11) NOT NULL," +
                "address TEXT NOT NULL," +
                "node text NOT NULL," +
                "money DOUBLE DEFAULT 0.0" +
                ") Engine=MyISAM CHARSET=utf8;";
            MySqlCommand cmd = new MySqlCommand(sql, SQLEngine.Instance.Connection);
            if (cmd.ExecuteNonQuery() != 0) Console.WriteLine("Error Create Table {0}", table_name);
        }

        public UserInfo() { }
        public UserInfo (string usernumber, string username, int gender,
            string phonenumber, string address, string node, double money)
        {
            uid = 0;
            this.username = username;
            this.usernumber = usernumber;
            this.gender = gender;
            this.phonenumber = phonenumber;
            this.address = address;
            this.node = node;
            this.money = money;
        }
        public UserInfo(int uid, string usernumber, string username, int gender,
            string phonenumber, string address, string node, double money)
        {
            this.uid = uid;
            this.usernumber = usernumber;
            this.username = username;
            this.gender = gender;
            this.phonenumber = phonenumber;
            this.address = address;
            this.node = node;
            this.money = money;
        }

        public static int Getuid(string usernumber)
        {
            int m = -1; 
            string sql = String.Format("SELECT uid FROM {0} WHERE usernumber = {1};", table_name, usernumber);
            MySqlCommand cmd = new MySqlCommand(sql, SQLEngine.Instance.Connection);
            MySqlDataReader rs = cmd.ExecuteReader();
            if (rs.Read())
            {
                m = rs.GetInt32("uid");
            }
            rs.Close();
                return m;
        }


        public static UserInfo Get(int uid)
        {
            UserInfo userinfo = null;

            string sql = String.Format("SELECT * FROM {0} WHERE uid = {1};", table_name, uid);
            MySqlCommand cmd = new MySqlCommand(sql, SQLEngine.Instance.Connection);
            MySqlDataReader rs = cmd.ExecuteReader();
            if (rs.Read())
            {
                userinfo = new UserInfo(uid,
                    rs.GetString("usernumber"),
                    rs.GetString("username"),
                    rs.GetInt32("gender"),
                    rs.GetString("phonenumber"),
                    rs.GetString("address"),
                    rs.GetString("node"),
                    rs.GetDouble("money"));
            }
            rs.Close();
            return userinfo;
        }
        public static MySqlDataReader GetSqlReader()
        {
            return MySqlHelper.ExecuteReader(SQLEngine.Instance.Connection,
                    "SELECT uid,username FROM " + table_name);
        }
        public static DataSet GetAll()
        {
            return MySqlHelper.ExecuteDataset(SQLEngine.Instance.Connection,
                "select * from " + table_name);
        }

        public static UserInfo Create(int uid, string usernumber, string username, int gender,
            string phonenumber, string address, string node, double money)
        {
            UserInfo userinfo = new UserInfo(uid, usernumber, username, gender,
                phonenumber, address, node, money);

            string sql = String.Format("INSERT INTO {0}" +
                "(uid, usernumber, username, gender, phonenumber, address, node, money)" +
                " VALUES({1}, '{2}', '{3}', {4}, '{5}', '{6}', '{7}', {8});", table_name,
                uid, usernumber, username, gender, phonenumber, address, node, money);
            MySqlCommand cmd = new MySqlCommand(sql, SQLEngine.Instance.Connection);
            if (cmd.ExecuteNonQuery() != 1)
            {
                Console.WriteLine("Error UPDATE model {0}", userinfo);
                return null;
            }
            else
            {
                sql = String.Format("SELECT MAX(uid) FROM {0};", table_name);
                cmd = new MySqlCommand(sql, SQLEngine.Instance.Connection);
                //userinfo.uid = (Int32)cmd.ExecuteScalar();
                return userinfo;
            }
        }
        public bool Save()
        {
            string sql = String.Format("INSERT INTO {0}" +
                "(uid, usernumber, username, gender, phonenumber, address, node, money)" +
                " VALUES({1}, '{2}', '{3}', {4}, '{5}', '{6}', '{7}', {8});", table_name,
                uid, usernumber, username, gender, phonenumber, address, node, money);
            MySqlCommand cmd = new MySqlCommand(sql, SQLEngine.Instance.Connection);
            if (cmd.ExecuteNonQuery() != 1)
            {
                Console.WriteLine("Error UPDATE model {0}", this);
                return false;
            }
            else
            {
                return true;
            }
        }
        public void Update()
        {
            string sql = String.Format("UPDATE {0} SET usernumber = '{2}', username = '{3}', gender = {4}, " +
                "phonenumber = '{5}', address = '{6}', node = '{7}', money = {8} WHERE uid = {1};"
                , table_name, uid, usernumber, username, gender, phonenumber, address, node, money);
            MySqlCommand cmd = new MySqlCommand(sql, SQLEngine.Instance.Connection);
            if (cmd.ExecuteNonQuery() != 1) Console.WriteLine("Error UPDATE model" + this);
        }

        public void Remove()
        {
            string sql = String.Format("DELETE FROM {0} WHERE uid = {1};", table_name, uid);
            MySqlCommand cmd = new MySqlCommand(sql, SQLEngine.Instance.Connection);
            if (cmd.ExecuteNonQuery() == 1) uid = 0;
        }

        public bool Exists()
        {
            return uid != null;
        }

        public bool IdenticalTo(UserInfo u)
        {
            return usernumber == u.usernumber &&
                username == u.username &&
                gender == u.gender &&
                phonenumber == u.phonenumber &&
                address == u.address &&
                node == u.node &&
                money == u.money;
        }

        public override String ToString()
        {
            return String.Format("<{0}: uid={1}, usernumber='{2}', username='{3}', " +
                "gender={4}, phonenumber='{5}', address='{6}', node='{7}', money={8}>",
                this.GetType(), uid, usernumber, username, gender, phonenumber, address, node, money);
        }

        #region UnitTest
        public static void Test()
        {
            UserInfo.MakeTable();

            UserInfo u1 = UserInfo.Create(1, "mynumber", "whatname", 1, "13923336666", "Utopia", "nodejs", 3.21);
            Console.WriteLine("u1: " + u1);
            if (!u1.Exists()) Console.WriteLine("Create Failed.");

            int id = u1.uid;
            UserInfo u2 = UserInfo.Get(id);
            Console.WriteLine("u2: " + u2);
            if (!u2.IdenticalTo(u1)) Console.WriteLine("Get OK, BUT data inconsistent!");

            string usernumber = "NewNumber";
            string username = "NewName";
            int gender = 0;
            string phonenumber = "110";
            string address = "new Addr";
            string node = "still";
            double money = 2333;
            u2.usernumber = usernumber;
            u2.username = username;
            u2.gender = gender;
            u2.phonenumber = phonenumber;
            u2.address = address;
            u2.node = node;
            u2.money = money;
            u2.Update();
            UserInfo u3 = UserInfo.Get(id);
            Console.WriteLine("u3: " + u3);
            if (!u3.IdenticalTo(u2)) Console.WriteLine("Save/Update Failed.");

            u3.Remove();
            u3 = UserInfo.Get(id);
            if (u3 != null) Console.WriteLine("Remove Failed.");

            Console.WriteLine("Test Completed [UserInfo]!");
        }
        #endregion
    }

    class MoneyRecord
    {
        private static string table_name = "moneyrecordtable";
        private int? recordid;
        public int uid;
        public int changedmoney;
        public DateTime changetime;
        public string username;
        public string usernumber;

        public static void MakeTable()
        {
            string sql = "CREATE TABLE IF NOT EXISTS " + table_name + " (" +
                "recordid INT AUTO_INCREMENT PRIMARY KEY," +
                "uid INT," +
                "changedmoney INT DEFAULT 0," +
                "changetime DATETIME" +
                ") Engine = MyISAM CHARSET = utf8;";
            MySqlCommand cmd = new MySqlCommand(sql, SQLEngine.Instance.Connection);
            if (cmd.ExecuteNonQuery() != 0) Console.WriteLine("Error Create Table {0}", table_name);
        }

        public MoneyRecord() { }
        public MoneyRecord(int uid, int changedmoney, DateTime changetime, string username , string usernumber)
        {
            this.recordid = null;
            this.uid = uid;
            this.changedmoney = changedmoney;
            this.changetime = changetime;
            this.username = username;
            this.usernumber = usernumber;
        }

        public static List<MoneyRecord> Getall()
        {
            string sql = String.Format("SELECT moneyrecordtable.recordid,moneyrecordtable.uid,moneyrecordtable.changedmoney,moneyrecordtable.changetime,userinfotable.username,userinfotable.usernumber FROM {0} left join userinfotable on moneyrecordtable.uid =userinfotable.uid where  userinfotable.username is not null;", table_name);//左连接
            MySqlCommand cmd = new MySqlCommand(sql, SQLEngine.Instance.Connection);
            MySqlDataReader rs = cmd.ExecuteReader();
            List<MoneyRecord> moneyrecords = new List<MoneyRecord>();
            while (rs.Read())
            {
                MoneyRecord moneyrecord = new MoneyRecord(
                    rs.GetInt32("uid"),
                    rs.GetInt32("changedmoney"),
                    rs.GetDateTime("changetime"),
                    rs.GetString("username"),
                    rs.GetString("usernumber"));
                moneyrecord.recordid = rs.GetInt32("recordid");
                moneyrecords.Add(moneyrecord);
            }
            rs.Close();
            return moneyrecords;
        }

        public static List<MoneyRecord> FindByUid(int uid)
        {
            if (uid <= 0)
            {
                return null;
            }
            string sql = String.Format("SELECT moneyrecordtable.recordid,moneyrecordtable.uid,moneyrecordtable.changedmoney,moneyrecordtable.changetime,userinfotable.username,userinfotable.usernumber  FROM {0} left join userinfotable on moneyrecordtable.uid =userinfotable.uid WHERE userinfotable.uid = {1};", table_name, uid);
            MySqlCommand cmd = new MySqlCommand(sql, SQLEngine.Instance.Connection);
            MySqlDataReader rs = cmd.ExecuteReader();
            List<MoneyRecord> moneyrecords = new List<MoneyRecord>();
            while (rs.Read())
            {
                MoneyRecord moneyrecord = new MoneyRecord(
                    rs.GetInt32("uid"),
                    rs.GetInt32("changedmoney"),
                    rs.GetDateTime("changetime"),
                    rs.GetString("username"),
                    rs.GetString("usernumber"));
                moneyrecord.recordid = rs.GetInt32("recordid");
                moneyrecords.Add(moneyrecord);
            }
            rs.Close();
            return moneyrecords;
        }

        public static MoneyRecord Create(int uid, int changedmoney, DateTime changetime)
        {
            MoneyRecord moneyrecord = new MoneyRecord(uid, changedmoney, changetime,null,null);

            string sql = String.Format("select (case when money >= {0} then 1 when money < {0} THEN 0  else '-1'end) from userinfotable where uid = '{1}';",-changedmoney,uid);
            //new MySqlCommand(sql, SQLEngine.Instance.Connection);
            MySqlCommand cmd0 = new MySqlCommand(sql, SQLEngine.Instance.Connection);
            MySqlDataReader rs = cmd0.ExecuteReader();
            while (rs.Read())
            {
                switch (rs.GetInt32(0))
                {
                    case 0: Console.WriteLine("余额不足"); return null;  break;
                    case -1: Console.WriteLine("错误"); return null; break;
                    case 1: Console.WriteLine("余额不足"); break;
                }  ;
            }
            rs.Close();

             sql = String.Format("update userinfotable set money = money+{0} where uid = '{1}'; ",changedmoney,uid);
            MySqlCommand cmd1 = new MySqlCommand(sql, SQLEngine.Instance.Connection);
            cmd1.ExecuteNonQuery();
            sql = String.Format("INSERT INTO {0}" +
                "(uid, changedmoney, changetime)" +
                " VALUES({1}, {2}, NOW());", table_name,
                uid, changedmoney);
            MySqlCommand cmd = new MySqlCommand(sql, SQLEngine.Instance.Connection);
            if (cmd.ExecuteNonQuery() != 1 && cmd1.ExecuteNonQuery() != 1)
            {
                Console.WriteLine("Error UPDATE model {0}", moneyrecord);
                return null;
            }
            else
            {
                sql = String.Format("SELECT MAX(uid) FROM {0};", table_name);
                cmd = new MySqlCommand(sql, SQLEngine.Instance.Connection);
                moneyrecord.recordid = (Int32)cmd.ExecuteScalar();
                return moneyrecord;
            }
        }
        public override String ToString()
        {
            return String.Format("<{0}: recordid={1}, uid={2}, changedmoney={3}, changetime='{4}'>",
                this.GetType(), recordid, uid, changedmoney, changetime);
        }

        #region UnitTest
        public static void Test()
        {
            MoneyRecord.MakeTable();

            MoneyRecord.Create(1, 233, DateTime.Now);
            MoneyRecord.Create(1, 666, DateTime.UtcNow);
            MoneyRecord.Create(1, 5, DateTime.Today);

            foreach (var mr in FindByUid(1))
                Console.WriteLine(mr);


            Console.WriteLine("Test Completed [MoneyRecord]!");
        }
        #endregion
    }
}