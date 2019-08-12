using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using MSsqlTool.Model;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows.Forms;
using NLog;
using NLog.Fluent;
using DataRow = System.Data.DataRow;
using MessageBox = System.Windows.MessageBox;


namespace MSsqlTool.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private static readonly string connectString =
            ConfigurationManager.ConnectionStrings["ConnectString"].ToString();

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private enum SysDataBases
        {
            master,
            model,
            msdb,
            tempdb
        }

        private SqlConnection _connection;

        private List<SqlMenuModel> _dataBaselist;

        private RelayCommand<string> _exportCommand;

        private RelayCommand _importCommand;

        private RelayCommand<string> _openTableCommand;

        private RelayCommand _refreshCommand;

        private List<SqlMenuModel> _mainDatabaseList;

        private List<OpenedTablesModel> _openedTableList;

        public RelayCommand<string> ExportCommand
        {
            get
            {
                if (_exportCommand == null)
                {
                    _exportCommand = new RelayCommand<string>((databaseName) => ExportExecuted(databaseName));
                }
                return _exportCommand;
            }
            set { _exportCommand = value; }
        }

        public RelayCommand ImportCommand
        {
            get
            {
                if (_importCommand == null)
                {
                    _importCommand = new RelayCommand(ImportExecuted);
                }

                return _importCommand;
            }
            set { _importCommand = value; }
        }

        public RelayCommand<string> OpenTableCommand
        {
            get
            {
                if (_openTableCommand == null)
                {
                    _openTableCommand = new RelayCommand<string>((tableName)=>OpenTableExecuted(tableName));
                }

                return _openTableCommand;
            }
            set { _openTableCommand = value; }
        }

        public RelayCommand RefreshCommand
        {
            get
            {
                if (_refreshCommand == null)
                {
                    _refreshCommand = new RelayCommand(RefreshExecuted);
                }

                return _refreshCommand;
            }
            set { _refreshCommand = value; }
        }

        public List<SqlMenuModel> DataBaselist
        {
            get { return _dataBaselist; }
        }

        public List<SqlMenuModel> MainDatabaseList
        {
            get { return _mainDatabaseList; }
            set
            {
                _mainDatabaseList = value;
                RaisePropertyChanged(() => MainDatabaseList);
            }
        }


        public List<OpenedTablesModel> OpenedTableList
        {
            get { return _openedTableList; }
            set
            {
                _openedTableList = value;
                RaisePropertyChanged(()=>OpenedTableList);
            }
        }

        public MainViewModel()
        {
            InitializeData();
        }

        private void InitializeData()
        {
            DataTable dataBaseTable = new DataTable();
            try
            {
                using (_connection = new SqlConnection(connectString))
                {
                    _connection.Open();
                    string selectDataBasesString = "select name from sysdatabases";
                    SqlDataAdapter dataBaseAdapter = new SqlDataAdapter(selectDataBasesString,_connection);
                    dataBaseTable = new DataTable();
                    dataBaseAdapter.Fill(dataBaseTable);
                    _connection.Close();
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                _connection.Close();
            }
            List<string> tempDataBaseList = new List<string>();
            foreach (DataRow row in dataBaseTable.Rows)
            {
                if (!Enum.IsDefined(typeof(SysDataBases), row["name"]))
                {
                    tempDataBaseList.Add(row["name"].ToString());
                }
            }
            _dataBaselist = new List<SqlMenuModel>();
            foreach (string name in tempDataBaseList)
            {
                List<SqlMenuModel> tablesList = GetTableList(name);
                SqlMenuModel tempMenuModel = new SqlMenuModel(){Name = name, MenuTables = tablesList, Level = "databases"};
                _dataBaselist.Add(tempMenuModel);
            }
            MainDatabaseList = new List<SqlMenuModel>
            {
                new SqlMenuModel() { Name = "数据库", MenuTables = _dataBaselist, Level = "main" }
            };
        }

        private string GetDifferentConnectionWithName(string name)
        {
            return String.Format(
                "data source=.\\SQLEXPRESS;initial catalog={0};integrated security=True;App=EntityFramework",
                name);
        }

        private List<SqlMenuModel> GetTableList(string databaseName)
        {
            List<SqlMenuModel> tableList = new List<SqlMenuModel>();
            SqlConnection getTableConnection = new SqlConnection();
            DataTable TableNames = new DataTable();
            string GetTableConnString = GetDifferentConnectionWithName(databaseName);
            try
            {
                using (getTableConnection = new SqlConnection(GetTableConnString))
                {
                    getTableConnection.Open();
                    string selectTableString = "select name from sys.tables";
                    SqlDataAdapter tablesNameAdapter = new SqlDataAdapter(selectTableString, getTableConnection);
                    TableNames = new DataTable();
                    tablesNameAdapter.Fill(TableNames);
                    getTableConnection.Close();
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                getTableConnection.Close();
            }
            getTableConnection.Dispose();
            foreach (DataRow row in TableNames.Rows)
            {
                tableList.Add(new SqlMenuModel(row["name"].ToString()) { Level = "tables" });
            }
            return tableList;
        }

        private void ExportExecuted(string databaseName)
        {
            string exportFileLocation = "";
            FolderBrowserDialog chooseExportFolder = new FolderBrowserDialog();
            chooseExportFolder.Description = "选择导出路径";
            if (chooseExportFolder.ShowDialog() == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(chooseExportFolder.SelectedPath))
                {
                    MessageBox.Show( "选定的文件夹路径不能为空", "提示");
                    return;
                }
                exportFileLocation = chooseExportFolder.SelectedPath;
                string exportDBConnectionString = GetDifferentConnectionWithName(databaseName);
                SqlConnection exportDbConnection = new SqlConnection();
                try
                {
                    using (exportDbConnection = new SqlConnection(exportDBConnectionString))
                    {
                        exportDbConnection.Open();
                        string exportDBString = String.Format("backup database {0} to disk='{1}\\{0}.bak'", databaseName, exportFileLocation);
                        SqlCommand exportCommand = new SqlCommand(exportDBString,exportDbConnection);
                        exportCommand.ExecuteNonQuery();
                        exportDbConnection.Close();
                        MessageBox.Show(String.Format("数据库 {0} 已成功备份到文件夹 {1} 中", databaseName, exportFileLocation), "提示");
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "导出错误", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    logger.Error(e.Message);
                }
            }

        }

        private void ImportExecuted()
        {
            OpenFileDialog chooseFileDialog = new OpenFileDialog();
            chooseFileDialog.Title = "选择导入文件";
            chooseFileDialog.Multiselect = false;
            chooseFileDialog.Filter = "数据库备份文件(*.bak)|*.bak";
            string filePath = "";
            string databaseName = "";
            if (chooseFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = chooseFileDialog.FileName;
                databaseName = Path.GetFileNameWithoutExtension(filePath);
            }
            try
            {
                using (_connection = new SqlConnection(connectString))
                {
                    PrepareForImport(_connection, databaseName);
                    ImportDataBase(_connection, filePath, databaseName);
                }
                InitializeData();
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
        }

        private void PrepareForImport(SqlConnection dropConn,string databaseName)
        {
            try
            {
                dropConn.Open();
                string getDataBaseString = "select name from sysdatabases";
                SqlDataAdapter getDataBaseAdapter = new SqlDataAdapter(getDataBaseString,dropConn);
                DataTable databaseTable = new DataTable();
                getDataBaseAdapter.Fill(databaseTable);
                dropConn.Close();
                foreach (DataRow row in databaseTable.Rows)
                {
                    if (row["name"].ToString() == databaseName)
                    {
                        DropDataBase(databaseName);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
        }

        private void DropDataBase(string databaseName)
        {
            if (MessageBox.Show($"本地数据库中已有数据库{databaseName}，是否立即删除？", "提醒", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection dropConn = new SqlConnection(connectString))
                    {
                        dropConn.Open();
                        SqlCommand dropCommand =
                            new SqlCommand(
                                $"use master;alter database {databaseName} set single_user with rollback immediate;drop database {databaseName};",
                                dropConn);
                        dropCommand.ExecuteNonQuery();
                        MessageBox.Show($"已在本地删除数据库{databaseName}", "提醒");
                        dropConn.Close();
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("删除出现异常，请查看日志");
                    logger.Error(e.Message);
                }

            }
        }

        private void ImportDataBase(SqlConnection importConn, string filePath,string databaseName)
        {
            try
            {
                importConn.Open();
                string importString = $"restore database {databaseName} from disk='{filePath}'";
                SqlCommand importCommand = new SqlCommand(importString, importConn);
                importCommand.ExecuteNonQuery();
                importConn.Close();
                MessageBox.Show($"导入数据库{databaseName}成功");
            }
            catch (Exception e)
            {
                MessageBox.Show($"导入数据库{databaseName}出错");
                logger.Error(e.Message);
            }
        }

        private void OpenTableExecuted(string tableName)
        {
            if (OpenedTableList == null)
            {
                OpenedTableList = new List<OpenedTablesModel>()
                {
                    new OpenedTablesModel(tableName)
                };
                logger.Trace(tableName);
            }
            else
            {
                SetElseTabsFalse();
                OpenedTableList = new List<OpenedTablesModel>(OpenedTableList)
                {
                    new OpenedTablesModel(tableName)
                };
            }
            //OpenedTableList = (new List<string>(){"123", "456", "789"});

        }

        private void SetElseTabsFalse()
        {
            if (OpenedTableList != null)
            {
                foreach (var table in OpenedTableList)
                {
                    if (table.IsChoosed)
                    {
                        table.IsChoosed = false;
                    }
                }
            }
        }

        private void RefreshExecuted()
        {
            InitializeData();
        }

    }
}