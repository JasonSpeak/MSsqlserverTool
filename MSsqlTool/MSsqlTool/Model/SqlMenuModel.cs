using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace MSsqlTool.Model
{
    public class SqlMenuModel:ObservableObject
    {
        private string _name;
        private string _level;
        private TableFullNameModel _tableFullName;
        private List<SqlMenuModel> _menuTables;

        private static readonly string ConnectString =
            ConfigurationManager.ConnectionStrings["ConnectString"].ToString();

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

        public TableFullNameModel TableFullName
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

        public static ObservableCollection<SqlMenuModel> InitializeData()
        {
            DataTable dataBaseTable;
            using (var initializeConn = new SqlConnection(ConnectString))
            {
                initializeConn.Open();
                const string selectDataBasesString = "SELECT NAME FROM SYSDATABASES WHERE SID != 0x01";
                var dataBaseAdapter = new SqlDataAdapter(selectDataBasesString, initializeConn);
                dataBaseTable = new DataTable();
                dataBaseAdapter.Fill(dataBaseTable);
                dataBaseAdapter.Dispose();
                initializeConn.Close();
            }
            var tempDataBaseList = (from DataRow row in dataBaseTable.Rows select row["name"].ToString()).ToList();
            var dataBaseList = (from name in tempDataBaseList let tablesList = GetTableList(name) select new SqlMenuModel() { Name = name, MenuTables = tablesList, Level = "databases" }).ToList();
            var mainDatabaseList = new ObservableCollection<SqlMenuModel>
            {
                new SqlMenuModel() { Name = "数据库", MenuTables = dataBaseList, Level = "main" }
            };
            return mainDatabaseList;
        }

        public static string GetDifferentConnectionWithName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            return $"data source=.\\SQLEXPRESS;initial catalog={name};integrated security=True;App=EntityFramework";
        }

        private static List<SqlMenuModel> GetTableList(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName)) return null;
            DataTable tableNames;
            var getTableConnString = GetDifferentConnectionWithName(databaseName);
            using (var getTableConnection = new SqlConnection(getTableConnString))
            {
                getTableConnection.Open();
                const string selectTableString = "SELECT NAME FROM SYS.TABLES";
                var tablesNameAdapter = new SqlDataAdapter(selectTableString, getTableConnection);
                tableNames = new DataTable();
                tablesNameAdapter.Fill(tableNames);
                tablesNameAdapter.Dispose();
                getTableConnection.Close();
            }
            var tableList = new List<SqlMenuModel>();
            foreach (DataRow row in tableNames.Rows)
            {
                var tableFullName = new TableFullNameModel(databaseName,row["name"].ToString());
                tableList.Add(new SqlMenuModel() {Name = row["name"].ToString(), Level = "tables", TableFullName = tableFullName});
            }
            return tableList;
        }
    }
}
 