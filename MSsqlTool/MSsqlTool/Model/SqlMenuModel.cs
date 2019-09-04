using System;
using GalaSoft.MvvmLight;
using NLog;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace MSsqlTool.Model
{
    public class SqlMenuModel:ObservableObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private string _name;
        private string _level;
        private TableFullNameModel _tableFullName;
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

        public static List<SqlMenuModel> InitializeData(SqlConnection masterConn)
        {
            if (masterConn == null)
            {
                throw new ArgumentException(@"masterConn should not be empty",nameof(masterConn));
            }
            var dataBaseTable = new DataTable();
            try
            {
                masterConn.Open();
                var selectDataBasesScript = "SELECT NAME FROM SYSDATABASES WHERE SID != 0x01";
                var dataBaseAdapter = new SqlDataAdapter(selectDataBasesScript, masterConn);
                dataBaseAdapter.Fill(dataBaseTable);
                dataBaseAdapter.Dispose();
                masterConn.Close();
            }
            catch (SqlException e)
            {
                Logger.Error(e.Message);
                throw;
            }
            var tempDataBaseList = (from DataRow row in dataBaseTable.Rows select row["name"].ToString()).ToList();
            var dataBaseList = new List<SqlMenuModel>();
            foreach (var name in tempDataBaseList)
            {
                List<SqlMenuModel> tablesList = GetTableList(name);
                dataBaseList.Add(new SqlMenuModel() {Name = name, MenuTables = tablesList, Level = "databases"});
            }
            var mainDatabaseList = new List<SqlMenuModel>
            {
                new SqlMenuModel() { Name = "数据库", MenuTables = dataBaseList, Level = "main" }
            };
            return mainDatabaseList;
        }

        private static List<SqlMenuModel> GetTableList(string databaseName)
        {
            DataTable tableNames;
            var getTableConnString = SqlHelperModel.GetDifferentConnectionWithName(databaseName);
            using (var getTableConnection = new SqlConnection(getTableConnString))
            {
                try
                {
                    getTableConnection.Open();
                    var selectTableScript = "SELECT NAME FROM SYS.TABLES";
                    var tablesNameAdapter = new SqlDataAdapter(selectTableScript, getTableConnection);
                    tableNames = new DataTable();
                    tablesNameAdapter.Fill(tableNames);
                    tablesNameAdapter.Dispose();
                    getTableConnection.Close();
                }
                catch (SqlException e)
                {
                    Logger.Error(e.Message);
                    throw;
                }
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
 