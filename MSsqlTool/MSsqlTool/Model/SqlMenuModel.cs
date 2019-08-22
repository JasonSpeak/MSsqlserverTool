using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using MSsqlTool.ViewModel;
using NLog;

namespace MSsqlTool.Model
{
    public class SqlMenuModel:ObservableObject
    {   
        private static readonly string ConnectString =
            ConfigurationManager.ConnectionStrings["ConnectString"].ToString();

        private static SqlConnection _initializeConn;

        private enum SysDataBases
        {
            master,
            model,
            msdb,   
            tempdb  
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private string _name;

        private string _level;

        private string[] _tableFullName;

        private List<SqlMenuModel> _menuTables;


        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        public string Level
        {
            get => _level;
            set
            {
                _level = value;
                RaisePropertyChanged(() => Level);
            }
        }

        public string[] TableFullName
        {
            get => _tableFullName;
            set
            {
                _tableFullName = value;
                RaisePropertyChanged(()=>TableFullName);
            }
        }

        public List<SqlMenuModel> MenuTables
        {
            get => _menuTables;
            set
            {
                _menuTables = value;
                RaisePropertyChanged(() => MenuTables);
            }
        }

        public static List<SqlMenuModel> InitializeData()
        {
            var dataBaseTable = new DataTable();
            try
            {
                using (_initializeConn = new SqlConnection(ConnectString))
                {
                    _initializeConn.Open();
                    const string selectDataBasesString = "select name from sysdatabases";
                    var dataBaseAdapter = new SqlDataAdapter(selectDataBasesString, _initializeConn);
                    dataBaseTable = new DataTable();
                    dataBaseAdapter.Fill(dataBaseTable);
                    _initializeConn.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("链接本地数据库失败，请确认数据库实例名称是否为 SQLEXPRESS 。", "连接错误", MessageBoxButton.OKCancel,
                    MessageBoxImage.Error);
                Logger.Error(e.Message);
                _initializeConn.Close();
            }
            var tempDataBaseList = new List<string>();
            foreach (DataRow row in dataBaseTable.Rows)
            {
                if (!Enum.IsDefined(typeof(SqlMenuModel.SysDataBases), row["name"]))
                {
                    tempDataBaseList.Add(row["name"].ToString());
                }
            }
            var dataBaseList = new List<SqlMenuModel>();
            foreach (var name in tempDataBaseList)
            {
                var tablesList = GetTableList(name);
                var tempMenuModel = new SqlMenuModel() { Name = name, MenuTables = tablesList, Level = "databases" };
                dataBaseList.Add(tempMenuModel);
            }
            var mainDatabaseList = new List<SqlMenuModel>
            {
                new SqlMenuModel() { Name = "数据库", MenuTables = dataBaseList, Level = "main" }
            };
            return mainDatabaseList;
        }

        private static List<SqlMenuModel> GetTableList(string databaseName)
        {
            var tableList = new List<SqlMenuModel>();
            var getTableConnection = new SqlConnection();
            var tableNames = new DataTable();
            var getTableConnString = GetDifferentConnectionWithName(databaseName);
            try
            {
                using (getTableConnection = new SqlConnection(getTableConnString))
                {
                    getTableConnection.Open();
                    const string selectTableString = "select name from sys.tables";
                    var tablesNameAdapter = new SqlDataAdapter(selectTableString, getTableConnection);
                    tableNames = new DataTable();
                    tablesNameAdapter.Fill(tableNames);
                    getTableConnection.Close();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                getTableConnection.Close();
            }
            getTableConnection.Dispose();
            foreach (DataRow row in tableNames.Rows)
            {
                var tableFullName = new string[2];
                tableFullName[0] = databaseName;
                tableFullName[1] = row["name"].ToString();
                tableList.Add(new SqlMenuModel() { Name = row["name"].ToString(), Level = "tables", TableFullName = tableFullName });
            }
            return tableList;
        }

        public static string GetDifferentConnectionWithName(string name)
        {
            return $"data source=.\\SQLEXPRESS;initial catalog={name};integrated security=True;App=EntityFramework";
        }
    }
}
 