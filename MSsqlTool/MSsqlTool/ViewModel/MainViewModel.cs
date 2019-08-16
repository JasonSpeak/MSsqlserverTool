﻿using System;
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
using NLog.LogReceiverService;
using DataGrid = System.Windows.Controls.DataGrid;
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

        private RelayCommand<string> _closeTabCommand;

        private RelayCommand<string> _closeFoldTabCommand;

        private RelayCommand _applyUpdateCommand;

        private RelayCommand<string> _clickTabCommand;

        private RelayCommand<string> _clickFoldCommand;

        private RelayCommand<string> _closeOtherTabsCommand;

        private RelayCommand _closeAllTabsCommand;

        private RelayCommand<Window> _closeWindowCommand;

        private RelayCommand<Window> _miniMizeWindowCommand;

        private RelayCommand<Window> _restoreWindowCommand;

        private RelayCommand<Window> _moveWindowCommand;

        private RelayCommand<DataGrid> _selectAllCommand;

        private RelayCommand<DataGrid> _checkForSelectAllCommand;

        private List<SqlMenuModel> _mainDatabaseList;

        private List<OpenedTablesModel> _openedTableList;

        private List<OpenedTablesModel> _openedTableFoldedList;

        private TablesDataModel _tableData;

        private SqlDataAdapter _dataAdapterForUpdate;

        private DataTable _dataTableForUpdate;

        private SqlCommandBuilder _commandBuilderForUpdate;

        private string _currentTable;

        private string _currentwindowState;

        private string _restorePathData;

        private string _restoreButtonTip;

        private BoolModel _isAllSelected;

        private BoolModel _isDataGridFill;

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
                    _openTableCommand = new RelayCommand<string>((tableName) => OpenTableExecuted(tableName));
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

        public RelayCommand<string> CloseTabCommand
        {
            get
            {
                if (_closeTabCommand == null)
                {
                    _closeTabCommand = new RelayCommand<string>((tableName) => CloseTabExecuted(tableName));
                }

                return _closeTabCommand;
            }
            set { _closeTabCommand = value; }
        }

        public RelayCommand<string> CloseFoldTabCommand
        {
            get
            {
                if (_closeFoldTabCommand == null)
                {
                    _closeFoldTabCommand = new RelayCommand<string>((tableName) => CloseFoldTabExecuted(tableName));
                }

                return _closeFoldTabCommand;
            }
            set { _closeFoldTabCommand = value; }
        }

        public RelayCommand ApplyUpdateCommand
        {
            get
            {
                if (_applyUpdateCommand == null)
                {
                    _applyUpdateCommand = new RelayCommand(ApplyUpdateExecuted);
                }

                return _applyUpdateCommand;
            }
            set { _applyUpdateCommand = value; }
        }

        public RelayCommand<string> ClickTabCommand
        {
            get
            {
                if (_clickTabCommand == null)
                {
                    _clickTabCommand = new RelayCommand<string>((tableFullName) => ClickTabExecuted(tableFullName));
                }

                return _clickTabCommand;
            }
            set { _clickTabCommand = value; }
        }

        public RelayCommand<string> ClickFoldCommand
        {
            get
            {
                if (_clickFoldCommand == null)
                {
                    _clickFoldCommand = new RelayCommand<string>((tableFullName) => ClickFoldExecuted(tableFullName));
                }

                return _clickFoldCommand;
            }
            set { _clickFoldCommand = value; }
        }

        public RelayCommand<string> CloseOtherTabsCommand
        {
            get
            {
                if (_closeOtherTabsCommand == null)
                {
                    _closeOtherTabsCommand =
                        new RelayCommand<string>((tableFullName) => CloseOtherTabsExecuted(tableFullName));
                }

                return _closeOtherTabsCommand;
            }
            set { _closeOtherTabsCommand = value; }
        }

        public RelayCommand CloseAllTabsCommand
        {
            get
            {
                if (_closeAllTabsCommand == null)
                {
                    _closeAllTabsCommand = new RelayCommand(CloseAllTabsExecuted);
                }

                return _closeAllTabsCommand;
            }
            set { _closeAllTabsCommand = value; }
        }

        public RelayCommand<Window> CloseWindowCommand
        {
            get
            {
                if (_closeWindowCommand == null)
                {
                    _closeWindowCommand =
                        new RelayCommand<Window>((currentWindow) => CloseWindowExecuted(currentWindow));
                }

                return _closeWindowCommand;
            }
            set { _closeWindowCommand = value; }
        }

        public RelayCommand<Window> MiniMizeWindowCommand
        {
            get
            {
                if (_miniMizeWindowCommand == null)
                {
                    _miniMizeWindowCommand =
                        new RelayCommand<Window>((currentWindow) => MiniMizeWindowExecuted(currentWindow));
                }

                return _miniMizeWindowCommand;
            }
            set { _miniMizeWindowCommand = value; }
        }

        public RelayCommand<Window> RestoreWindowCommand
        {
            get
            {
                if (_restoreWindowCommand == null)
                {
                    _restoreWindowCommand = new RelayCommand<Window>((currentWindow)=> RestoreWindowExecuted(currentWindow));
                }

                return _restoreWindowCommand;
            }
            set { _restoreWindowCommand = value; }
        }

        public RelayCommand<Window> MoveWindowCommand
        {
            get
            {
                if (_moveWindowCommand == null)
                {
                    _moveWindowCommand = new RelayCommand<Window>((currentWindow)=> MoveWindowExecuted(currentWindow));
                }

                return _moveWindowCommand;
            }
            set { _moveWindowCommand = value; }
        }

        public RelayCommand<DataGrid> SelectAllCommand
        {
            get
            {
                if (_selectAllCommand == null)
                {
                    _selectAllCommand = new RelayCommand<DataGrid>((dataGrid)=> SelectAllExecuted(dataGrid));
                }

                return _selectAllCommand;
            }
            set { _selectAllCommand = value; }
        }

        public RelayCommand<DataGrid> CheckForSelectAllCommand
        {
            get
            {
                if (_checkForSelectAllCommand == null)
                {
                    _checkForSelectAllCommand = new RelayCommand<DataGrid>((dataGrid)=> CheckForSelectAllExecuted(dataGrid));
                }

                return _checkForSelectAllCommand;
            }
            set { _checkForSelectAllCommand = value; }
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
                RaisePropertyChanged(() => OpenedTableList);
            }
        }

        public List<OpenedTablesModel> OpenedTableFoldedList
        {
            get => _openedTableFoldedList;
            set
            {
                _openedTableFoldedList = value;
                RaisePropertyChanged(() => OpenedTableFoldedList);
            }
        }

        public TablesDataModel TableData
        {
            get => _tableData;
            set
            {
                _tableData = value;
                RaisePropertyChanged(() => TableData);
            }
        }

        public string CurrentwindowState
        {
            get { return _currentwindowState; }
            set
            {
                _currentwindowState = value;
                RaisePropertyChanged(()=>CurrentwindowState);
            }
        }

        public string RestorePathData
        {
            get { return _restorePathData; }
            set
            {
                _restorePathData = value;
                RaisePropertyChanged(()=>RestorePathData);
            }
        }

        public string RestoreButtonTip
        {
            get { return _restoreButtonTip; }
            set
            {
                _restoreButtonTip = value;
                RaisePropertyChanged(()=>RestoreButtonTip);
            }
        }

        public BoolModel IsAllSelected
        {
            get { return _isAllSelected; }
            set
            {
                _isAllSelected = value;
                RaisePropertyChanged(()=>IsAllSelected);
            }
        }

        public BoolModel IsDataGridFill
        {
            get { return _isDataGridFill; }
            set
            {
                _isDataGridFill = value;
                RaisePropertyChanged(()=>IsDataGridFill);
            }
        }

        public MainViewModel()
        {
            InitializeData();
            CurrentwindowState = "Maximized";
            RestorePathData =
                "F1M11,8L9,8 9,4 9,3 8,3 4,3 4,1 11,1z M8,11L1,11 1,4 3,4 4,4 8,4 8,8 8,9z M11,0L4,0 3,0 3,1 3,3 1,3 0,3 0,4 0,11 0,12 1,12 8,12 9,12 9,11 9,9 11,9 12,9 12,8 12,1 12,0z";
            RestoreButtonTip = "向下还原";
            IsAllSelected = new BoolModel();
            IsAllSelected.IsChecked = false;
            IsDataGridFill = new BoolModel();
            IsDataGridFill.IsChecked = false;
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
                    SqlDataAdapter dataBaseAdapter = new SqlDataAdapter(selectDataBasesString, _connection);
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
                SqlMenuModel tempMenuModel = new SqlMenuModel() { Name = name, MenuTables = tablesList, Level = "databases" };
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
                tableList.Add(new SqlMenuModel(row["name"].ToString()) { Level = "tables", TableFullName = $"{databaseName}.{row["name"].ToString()}" });
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
                    MessageBox.Show("选定的文件夹路径不能为空", "提示");
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
                        SqlCommand exportCommand = new SqlCommand(exportDBString, exportDbConnection);
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

        private void PrepareForImport(SqlConnection dropConn, string databaseName)
        {
            try
            {
                dropConn.Open();
                string getDataBaseString = "select name from sysdatabases";
                SqlDataAdapter getDataBaseAdapter = new SqlDataAdapter(getDataBaseString, dropConn);
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

        private void ImportDataBase(SqlConnection importConn, string filePath, string databaseName)
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

        private void OpenTableExecuted(string tableFullName)
        {
            if (OpenedTableList == null)
            {
                OpenedTableList = new List<OpenedTablesModel>()
                {
                    new OpenedTablesModel(tableFullName)
                };
                SetElseTabsFalse(tableFullName);
            }
            else if (OpenedTableList != null && OpenedTableList.Count < 6 && !IsThisTableOpendInTab(tableFullName))
            {
                OpenedTableList = new List<OpenedTablesModel>(OpenedTableList)
                {
                    new OpenedTablesModel(tableFullName)
                };
                SetElseTabsFalse(tableFullName);
            }
            else if (OpenedTableList != null && OpenedTableList.Count <= 6 && IsThisTableOpendInTab(tableFullName))
            {
                SetElseTabsFalse(tableFullName);
            }
            else if (OpenedTableList.Count == 6 && OpenedTableFoldedList == null && !IsThisTableOpendInTab(tableFullName))
            {
                OpenedTableFoldedList = new List<OpenedTablesModel>()
                {
                    OpenedTableList[5]
                };
                OpenedTableList[5] = new OpenedTablesModel(tableFullName);
                SetElseTabsFalse(tableFullName);
                OpenedTableList = new List<OpenedTablesModel>(OpenedTableList);
            }
            else if (OpenedTableList.Count == 6 && OpenedTableFoldedList != null && !IsThisTableOpenedInFolder(tableFullName))
            {
                OpenedTableFoldedList = new List<OpenedTablesModel>(OpenedTableFoldedList)
                {
                    OpenedTableList[5]
                };
                OpenedTableList[5] = new OpenedTablesModel(tableFullName);
                SetElseTabsFalse(tableFullName);
                OpenedTableList = new List<OpenedTablesModel>(OpenedTableList);
            }
            else if (OpenedTableList.Count == 6 && OpenedTableFoldedList != null && IsThisTableOpenedInFolder(tableFullName))
            {
                OpenedTablesModel tempModel = OpenedTableList[5];
                OpenedTableList[5] = new OpenedTablesModel(tableFullName);
                OpenedTableList = new List<OpenedTablesModel>(OpenedTableList);
                SetElseTabsFalse(tableFullName);
                OpenedTablesModel tableForDelete = new OpenedTablesModel();
                foreach (var table in OpenedTableFoldedList)
                {
                    if (table.TableName == tableFullName)
                    {
                        tableForDelete = table;
                    }
                }
                OpenedTableFoldedList.Remove(tableForDelete);
                OpenedTableFoldedList = new List<OpenedTablesModel>(OpenedTableFoldedList)
                {
                    tempModel
                };
            }
            GetTableData(tableFullName);
        }

        private void OpenedTableFold(string tableFullName)
        {
            if (OpenedTableFoldedList == null)
            {
                OpenedTableFoldedList = new List<OpenedTablesModel>()
                {
                    new OpenedTablesModel(tableFullName){IsChoosed = false}
                };
            }
            else
            {
                OpenedTableFoldedList = new List<OpenedTablesModel>(OpenedTableFoldedList)
                {
                    new OpenedTablesModel(tableFullName){IsChoosed = false}
                };
            }
        }

        private bool IsThisTableOpendInTab(string tableFullName)
        {
            foreach (var table in OpenedTableList)
            {
                if (table.TableName == tableFullName)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsThisTableOpenedInFolder(string tableFullName)
        {
            foreach (var table in OpenedTableFoldedList)
            {
                if (table.TableName == tableFullName)
                {
                    return true;
                }
            }
            return false;
        }

        private void SetElseTabsFalse(string tableFullName)
        {
            if (OpenedTableList != null)
            {
                foreach (var table in OpenedTableList)
                {
                    if (table.IsChoosed && table.TableName != tableFullName)
                    {
                        table.IsChoosed = false;
                    }

                    if (table.TableName == tableFullName)
                    {
                        table.IsChoosed = true;
                    }
                }
            }
        }

        private void RefreshExecuted()
        {
            InitializeData();
        }

        private void CloseTabExecuted(string tableFullName)
        {
            OpenedTablesModel deleteModel = new OpenedTablesModel();
            foreach (var table in OpenedTableList)
            {
                if (table.TableName == tableFullName)
                {
                    deleteModel = table;
                }
            }
            OpenedTableList.Remove(deleteModel);
            if (OpenedTableList.Count == 0)
            {
                IsDataGridFill = new BoolModel();
                IsDataGridFill.IsChecked = false;
                IsAllSelected = new BoolModel();
                IsAllSelected.IsChecked = false;
            }
            if (OpenedTableList.Count == 5 && OpenedTableFoldedList != null)
            {
                OpenedTablesModel deleteFoldModel = new OpenedTablesModel();
                OpenedTableList.Add(OpenedTableFoldedList[0]);
                deleteFoldModel = OpenedTableFoldedList[0];
                OpenedTableFoldedList.Remove(deleteFoldModel);
                if (OpenedTableFoldedList.Count != 0)
                {
                    foreach (var table in OpenedTableFoldedList)
                    {
                        table.IsChoosed = false;
                    }
                    OpenedTableFoldedList = new List<OpenedTablesModel>(OpenedTableFoldedList);
                }
                else
                {
                    OpenedTableFoldedList = null;
                }
            }

            if (deleteModel.IsChoosed == true)
            {
                if (OpenedTableList.Count != 0)
                {
                    OpenedTableList[0].IsChoosed = true;
                    SetElseTabsFalse(OpenedTableList[0].TableName);
                    GetTableData(OpenedTableList[0].TableName);
                }
                else
                {
                    TableData = null;
                }
            }
            OpenedTableList = new List<OpenedTablesModel>(OpenedTableList);

        }

        private void CloseFoldTabExecuted(string tableFullName)
        {
            OpenedTablesModel deleteModel = new OpenedTablesModel();
            foreach (var table in OpenedTableFoldedList)
            {
                if (table.TableName == tableFullName)
                {
                    deleteModel = table;
                }
            }

            OpenedTableFoldedList.Remove(deleteModel);
            OpenedTableFoldedList = new List<OpenedTablesModel>(OpenedTableFoldedList);
        }

        private void GetTableData(string tableFullName)
        {
            _currentTable = tableFullName;
            string databaseName = tableFullName.Split('.')[0];
            string tableName = tableFullName.Split('.')[1];
            _connection = new SqlConnection(GetDifferentConnectionWithName(databaseName));
            try
            {
                _connection.Open();
                string selectAll = $"select * from {tableName}";
                _dataAdapterForUpdate = new SqlDataAdapter(selectAll, _connection);
                _commandBuilderForUpdate = new SqlCommandBuilder(_dataAdapterForUpdate);
                _dataTableForUpdate = new DataTable();
                _dataAdapterForUpdate.Fill(_dataTableForUpdate);
                _connection.Close();
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
            _connection.Close();
            TableData = new TablesDataModel();
            TableData.DataBaseName = databaseName;
            TableData.TableName = tableName;
            TableData.DataInTable = _dataTableForUpdate;
            IsDataGridFill = new BoolModel();
            IsDataGridFill.IsChecked = true;
        }

        private void ApplyUpdateExecuted()
        {
            try
            {
                _connection.Open();
                _dataAdapterForUpdate.Update(_dataTableForUpdate);
                _connection.Close();
                MessageBox.Show("数据修改成功");
                GetTableData(_currentTable);

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                logger.Error(e.Message);
            }
        }

        private void ClickTabExecuted(string tableFullName)
        {
            OpenedTablesModel clickedModel = new OpenedTablesModel();
            foreach (var table in OpenedTableList)
            {
                if (table.TableName == tableFullName)
                {
                    table.IsChoosed = true;
                    SetElseTabsFalse(tableFullName);
                    GetTableData(tableFullName);
                }
            }
        }

        private void ClickFoldExecuted(string tableFullName)
        {
            OpenedTablesModel tempModel = new OpenedTablesModel();
            foreach (var table in OpenedTableFoldedList)
            {
                if (table.TableName == tableFullName)
                {
                    tempModel = table;
                }
            }
            OpenedTableFoldedList.Add(OpenedTableList[5]);
            OpenedTableList[5] = tempModel;
            SetElseTabsFalse(tableFullName);
            GetTableData(tableFullName);
            OpenedTableFoldedList.Remove(tempModel);
            foreach (var table in OpenedTableFoldedList)
            {
                table.IsChoosed = false;
            }
            OpenedTableList = new List<OpenedTablesModel>(OpenedTableList);
            OpenedTableFoldedList = new List<OpenedTablesModel>(OpenedTableFoldedList);
        }

        private void CloseOtherTabsExecuted(string tableFullName)
        {
            OpenedTablesModel oneTabModel = new OpenedTablesModel();
            foreach (var table in OpenedTableList)
            {
                if (table.TableName == tableFullName)
                {
                    oneTabModel = table;
                }
            }
            OpenedTableList = new List<OpenedTablesModel>()
            {
                oneTabModel
            };
            SetElseTabsFalse(tableFullName);
            GetTableData(tableFullName);
            OpenedTableFoldedList = new List<OpenedTablesModel>();
        }

        private void CloseAllTabsExecuted()
        {
            OpenedTableList = new List<OpenedTablesModel>();
            OpenedTableFoldedList = new List<OpenedTablesModel>();
            TableData = new TablesDataModel();
        }

        private void CloseWindowExecuted(Window currentWindow)
        {
            currentWindow.Close();
        }

        private void MiniMizeWindowExecuted(Window currentWindow)
        {
            currentWindow.WindowState = WindowState.Minimized;
        }

        private void RestoreWindowExecuted(Window currentWindow)
        {
            if (CurrentwindowState == "Maximized")
            {
                currentWindow.WindowState = WindowState.Normal;
                CurrentwindowState = "Normal";
                RestorePathData =
                    "F1M11,11L1,11 1,1 11,1z M11,0L1,0 0,0 0,1 0,11 0,12 1,12 11,12 12,12 12,11 12,1 12,0z";
                RestoreButtonTip = "最大化";
            }
            else if(CurrentwindowState == "Normal")
            {
                currentWindow.WindowState = WindowState.Maximized;
                CurrentwindowState = "Maximized";
                RestorePathData =
                    "F1M11,8L9,8 9,4 9,3 8,3 4,3 4,1 11,1z M8,11L1,11 1,4 3,4 4,4 8,4 8,8 8,9z M11,0L4,0 3,0 3,1 3,3 1,3 0,3 0,4 0,11 0,12 1,12 8,12 9,12 9,11 9,9 11,9 12,9 12,8 12,1 12,0z";
                RestoreButtonTip = "向下还原";
            }
        }

        private void MoveWindowExecuted(Window currentWindow)
        {
            currentWindow.DragMove();
        }

        private void SelectAllExecuted(DataGrid dataGrid)
        {
            if (IsAllSelected.IsChecked)
            {
                for (int i = 0; i < dataGrid.Items.Count; i++)
                {
                    DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(i);
                    if (row != null)
                    {
                        row.IsSelected = true;

                    }
                }
                IsAllSelected = new BoolModel();
                IsAllSelected.IsChecked = true;
            }
            else
            {
                for (int i = 0; i < dataGrid.Items.Count; i++)
                {
                    DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(i);
                    if (row != null)
                    {
                        row.IsSelected = false;
                    }
                }
                IsAllSelected = new BoolModel();
                IsAllSelected.IsChecked = false;
            }
        }

        private void CheckForSelectAllExecuted(DataGrid dataGrid)
        {
            if (IsAllSelected.IsChecked)
            {
                IsAllSelected = new BoolModel();
                IsAllSelected.IsChecked = false;
            }
        }

    }
}