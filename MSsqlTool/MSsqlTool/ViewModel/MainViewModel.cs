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

        private List<SqlMenuModel> _mainDatabaseList;

        public List<SqlMenuModel> MainDatabaseList
        {
            get { return _mainDatabaseList; }
            set
            {
                _mainDatabaseList = value;
                RaisePropertyChanged(() => MainDatabaseList);
            }
        }

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

        public ICommand ItemCommand { get; private set; }

        public List<SqlMenuModel> DataBaselist
        {
            get { return _dataBaselist; }
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
            }
            _connection.Dispose();
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
                SqlMenuModel tempMenuModel = new SqlMenuModel(){Name = name,MenuTables = tablesList,Level = "database"};
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
                "data source=.\\SQLEXPRESS;initial catalog={0};integrated security=True;MultipleActiveResultSets=True;App=EntityFramework",
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
                    SqlDataAdapter tablesNameAdapter = new SqlDataAdapter(selectTableString,getTableConnection);
                    TableNames = new DataTable();
                    tablesNameAdapter.Fill(TableNames);
                    getTableConnection.Close();
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
            foreach (DataRow row in TableNames.Rows)
            {
                tableList.Add(new SqlMenuModel(row["name"].ToString()){Level = "tables"});
            }
            getTableConnection.Dispose();
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
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                }

                MessageBox.Show(String.Format("数据库 {0} 已成功备份到文件夹 {1} 中", databaseName, exportFileLocation), "提示");
            }

        }

        private void ImportExecuted()
        {
            PrepareForImport("test");
        }

        private void PrepareForImport(string databaseName)
        {
            databaseName = "mvb";
            try
            {
                if (MessageBox.Show("本地数据库中已有该数据库，是否立即删除？", "提醒", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                    MessageBoxResult.Yes)
                {
                    using (SqlConnection dropConn = new SqlConnection(connectString))
                    {
                        try
                        {
                            dropConn.Open();
                            SqlCommand dropCommand = new SqlCommand($"Use Master;drop database {databaseName};", dropConn);
                            dropCommand.ExecuteNonQuery();
                            MessageBox.Show("已在本地删除该数据库", "提醒");
                            dropConn.Close();
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("删除出现异常，请查看日志");
                            logger.Error(e.Message);
                            dropConn.Close();
                        }
                    }
                   
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
        }
    }
}