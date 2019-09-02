using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MSsqlTool.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Application = System.Windows.Application;
using Cursor = System.Windows.Forms.Cursor;
using Cursors = System.Windows.Forms.Cursors;
using DataGrid = System.Windows.Controls.DataGrid;
using MessageBox = System.Windows.MessageBox;


namespace MSsqlTool.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private const int MaxTabCount = 6;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private WindowState _currentWindowState;
        private CursorType _currentCursor;
        private TablesDataModel _tableData;
        private SqlDataAdapter _dataAdapterForUpdate;
        private TableFullNameModel _currentTable;
        private bool _isDataGridOpened;
        private bool _isAllSelected;
        private bool _isTabFoldOpened;

        public ICommand CloseWindowCommand { get; }
        public ICommand ChangeWindowStateCommand { get; }
        public ICommand MinimizeWindowCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand OpenTableCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CloseTabCommand { get; }
        public ICommand CloseFoldTabCommand { get; }
        public ICommand ApplyUpdateCommand { get; }
        public ICommand ClickTabCommand { get; }
        public ICommand ClickFoldCommand { get; }
        public ICommand CloseOtherTabsCommand { get; }
        public ICommand CloseAllTabsCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand CheckForSelectAllCommand { get; }

        public WindowState CurrentWindowState
        {
            get => _currentWindowState;
            set
            {
                _currentWindowState = value;
                RaisePropertyChanged(()=>CurrentWindowState);
            }
        }
        public CursorType CurrentCursor
        {
            get => _currentCursor;
            set
            {
                _currentCursor = value;
                RaisePropertyChanged(()=>CurrentCursor);
            }
        }
        public ObservableCollection<SqlMenuModel> MainDatabaseList { get; set; }
        public ObservableCollection<OpenedTablesModel> OpenedTables { get; }
        public ObservableCollection<OpenedTablesModel> OpenedFoldedTables { get; }
        public TablesDataModel TableData
        {
            get => _tableData;
            set
            {
                _tableData = value;
                RaisePropertyChanged(() => TableData);
            }
        }
        public bool IsAllSelected
        {
            get => _isAllSelected;
            set
            {
                _isAllSelected = value;
                RaisePropertyChanged(()=>IsAllSelected);
            }
        }
        public bool IsDataGridOpened
        {
            get => _isDataGridOpened;
            set
            {
                _isDataGridOpened = value;
                RaisePropertyChanged(()=>IsDataGridOpened);
            }
        }
        public bool IsTabFoldOpened
        {
            get => _isTabFoldOpened;
            set
            {
                if (_isTabFoldOpened != value)
                {
                    _isTabFoldOpened = value;
                    RaisePropertyChanged(() => IsTabFoldOpened);
                }
            }
        }

        public MainViewModel()
        {
            CloseWindowCommand = new RelayCommand(OnCloseWindowCommandExecuted);
            ChangeWindowStateCommand = new RelayCommand(OnChangeWindowStateCommandExecuted);
            MinimizeWindowCommand = new RelayCommand<string>(OnMinimizeWindowCommandExecuted);
            ExportCommand = new RelayCommand<string>(OnExportCommandExecuted);
            DeleteCommand = new RelayCommand<string>(OnDeleteCommandExecuted);
            ImportCommand = new RelayCommand(OnImportCommandExecuted);
            OpenTableCommand = new RelayCommand<TableFullNameModel>(OnOpenTableCommandExecuted);
            RefreshCommand = new RelayCommand(OnRefreshCommandExecuted);
            CloseTabCommand = new RelayCommand<TableFullNameModel>(OnCloseTabCommandExecuted);
            CloseFoldTabCommand = new RelayCommand<TableFullNameModel>(OnCloseFoldTabCommandExecuted);
            ApplyUpdateCommand = new RelayCommand(OnApplyUpdateCommandExecuted);
            ClickTabCommand = new RelayCommand<TableFullNameModel>(OnClickTabCommandExecuted);
            ClickFoldCommand = new RelayCommand<TableFullNameModel>(OnClickFoldCommandExecuted);
            CloseOtherTabsCommand = new RelayCommand<TableFullNameModel>(OnCloseOtherTabsCommandExecuted);
            CloseAllTabsCommand = new RelayCommand(OnCloseAllTabsCommandExecuted);
            SelectAllCommand = new RelayCommand<DataGrid>(OnSelectAllCommandExecuted);
            CheckForSelectAllCommand = new RelayCommand(OnCheckForSelectAllCommandExecuted);

            CurrentWindowState = WindowState.Maximized;
            CurrentCursor = CursorType.Wait;
            OpenedTables = new ObservableCollection<OpenedTablesModel>();
            OpenedFoldedTables = new ObservableCollection<OpenedTablesModel>();
            _dataAdapterForUpdate = new SqlDataAdapter();

            MainDatabaseList = SqlMenuModel.InitializeData();
        }

        private void OnCloseWindowCommandExecuted()
        {
            Application.Current.Shutdown();
            _dataAdapterForUpdate.Dispose();
        }

        private void OnChangeWindowStateCommandExecuted()
        {
            CurrentWindowState = (CurrentWindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized);
        }
            
        private void OnMinimizeWindowCommandExecuted(string windowName)
        {
            CurrentWindowState = WindowState.Minimized;
        }

        private void OnExportCommandExecuted(string databaseName)
        {
            var chooseExportFolder = new FolderBrowserDialog {Description = @"选择导出路径"};
            if (chooseExportFolder.ShowDialog() == DialogResult.OK)
            {
                var exportFileLocation = chooseExportFolder.SelectedPath;
                try
                {
                    var allBakFiles = Directory.GetFiles(exportFileLocation, "*.bak");
                    var bakFileName = $"{exportFileLocation}\\{databaseName}.bak";
                    bakFileName = bakFileName.Replace("\\\\", "\\");
                    DeleteLocalBakFile(allBakFiles, bakFileName);
                    SqlHelperModel.ExportDataBaseHelper(databaseName, exportFileLocation);
                    ShowMessage("Export Success!", "Success");
                }
                catch (SqlException e)
                {
                    ShowMessage("Export Failed", "Error");
                    Logger.Error(e.Message);
                    throw;
                }

            }
        }

        private void OnDeleteCommandExecuted(string databaseName)
        {
            CurrentCursor = CursorType.Wait;
            if (ConfirmDeleteDataBase(databaseName))
            {
                try
                {
                    SqlHelperModel.DropDataBaseHelper(databaseName);
                    DeleteTabsWithDataBaseDeleted(databaseName);
                    MainDatabaseList = SqlMenuModel.InitializeData();
                    CurrentCursor = CursorType.Arrow;
                    ShowMessage($"Success Delete Local DataBase {databaseName}", "Success");
                }
                catch (SqlException e)
                {
                    ShowMessage("Delete Failed", "Error");
                    Logger.Error(e.Message);
                }
            }
        }

        private void OnImportCommandExecuted()
        {
            var chooseFileDialog = new OpenFileDialog
            {
                Title = @"选择导入文件", Multiselect = false, Filter = @"数据库备份文件(*.bak)|*.bak"
            };
            if (chooseFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var filePath = chooseFileDialog.FileName;
                    var logicName = SqlHelperModel.GetLogicNameFromBak(filePath);
                    if (!IsOverWriteLocalDataBase(logicName)) return;
                    CurrentCursor = CursorType.Wait;
                    SqlHelperModel.ImportDataBaseHelper(filePath);
                    MainDatabaseList = SqlMenuModel.InitializeData();
                    CurrentCursor = CursorType.Arrow;
                    DeleteTabsWithDataBaseDeleted(logicName);
                    ShowMessage("Import Success","Success");
                }
                catch (SqlException e)
                {
                    ShowMessage("Import Failed","Error");
                    Logger.Error(e.Message);
                    throw;
                }
                
            }
        }

        private void OnOpenTableCommandExecuted(TableFullNameModel tableFullName)
        {
            if (CanAddIntoOpenTab(tableFullName))
            {
                OpenedTables.Add(new OpenedTablesModel(tableFullName));
                CheckSelectAllState();
            }
            else if (CanAddIntoOpenTabFolder(tableFullName))
            {
                CheckSelectAllState();
                IsTabFoldOpened = true;
                OpenedFoldedTables.Add(OpenedTables[5]);
                OpenedTables[MaxTabCount-1] = new OpenedTablesModel(tableFullName);
            }
            else if (IsThisTableOpenedInFolder(tableFullName))
            {
                CheckSelectAllState();
                MoveThisTabToOpenTabList(tableFullName);
            }

            foreach (var tab in OpenedFoldedTables)
            {
                tab.IsChoosed = false;
            }
            SetElseTabsFalse(tableFullName);
            GetTableData(tableFullName);
        }

        private void OnRefreshCommandExecuted()
        {
            MainDatabaseList = SqlMenuModel.InitializeData();
        }

        private void OnCloseTabCommandExecuted(TableFullNameModel tableFullName)
        {
            var deleteTab = OpenedTables.FirstOrDefault(table => table.TableFullName == tableFullName);
            OpenedTables.Remove(deleteTab);
            if (OpenedFoldedTables.Count != 0)
            {
                OpenedTables.Add(OpenedFoldedTables[0]);
                OpenedFoldedTables.RemoveAt(0);
            }
            else
            {
                IsTabFoldOpened = false;
            }
            if (OpenedTables.Count != 0)
            {
                if (deleteTab != null && deleteTab.IsChoosed)
                {
                    CheckSelectAllState();
                    OpenedTables[0].IsChoosed = true;
                    GetTableData(OpenedTables[0].TableFullName);
                }
            }
            else
            {
                IsDataGridOpened = false;
                IsAllSelected = false;
                TableData = null;
            }
        }

        private void OnCloseFoldTabCommandExecuted(TableFullNameModel tableFullName)
        {
            OpenedFoldedTables.Remove(
                OpenedFoldedTables.First(table => table.TableFullName == tableFullName));

            IsTabFoldOpened = (OpenedFoldedTables.Count != 0);
        }

        private void OnApplyUpdateCommandExecuted()
        {
            if (TableData.DataInTable.GetChanges() == null)return;
            try
            {
                SqlHelperModel.ApplyUpdateHelper(_dataAdapterForUpdate, TableData.DataInTable);
            }
            catch (SqlException e)
            {
                ShowMessage(e.Message,"Error");
                Logger.Error(e.Message);
            }
            GetTableData(_currentTable);
        }

        private void OnClickTabCommandExecuted(TableFullNameModel tableFullName)
        {
            CheckSelectAllState();
            SetElseTabsFalse(tableFullName);
            GetTableData(tableFullName);
        }

        private void OnClickFoldCommandExecuted(TableFullNameModel tableFullName)
        {
            CheckSelectAllState();
            MoveThisTabToOpenTabList(tableFullName);
            SetElseTabsFalse(tableFullName);
            GetTableData(tableFullName);
            foreach (var tab in OpenedFoldedTables)
            {
                tab.IsChoosed = false;
            }
        }

        private void OnCloseOtherTabsCommandExecuted(TableFullNameModel tableFullName)
        {
            var targetTab = OpenedTables.First(table => table.TableFullName == tableFullName);
            OpenedTables.Clear();
            OpenedFoldedTables.Clear();
            OpenedTables.Add(targetTab);
            IsTabFoldOpened = false;
            if (!targetTab.IsChoosed)
            {
                SetElseTabsFalse(tableFullName);
                GetTableData(tableFullName);
                CheckSelectAllState();
            }
        }

        private void OnCloseAllTabsCommandExecuted()
        {
            CheckSelectAllState();
            OpenedTables.Clear();
            OpenedFoldedTables.Clear();
            TableData = new TablesDataModel();
            IsDataGridOpened = false;
        }

        private void OnSelectAllCommandExecuted(DataGrid dataGrid)
        {
            if (IsAllSelected)
            {
                dataGrid.SelectAll();
            }
            else
            {
                dataGrid.UnselectAll();
            }
        }

        private void OnCheckForSelectAllCommandExecuted()
        {
            if (IsAllSelected)
            {
                IsAllSelected = false;
            }
        }

        private bool IsThisTableOpenedInTab(TableFullNameModel tableFullName)
        {
            return OpenedTables.Any(table => table.TableFullName == tableFullName);
        }

        private bool IsThisTableOpenedInFolder(TableFullNameModel tableFullName)
        {
            return OpenedFoldedTables.Any(table => table.TableFullName == tableFullName);
        }

        private void SetElseTabsFalse(TableFullNameModel tableFullName)
        {
            foreach (var openedTable in OpenedTables)
            {
                openedTable.IsChoosed = openedTable.TableFullName == tableFullName;
            }
        }

        private void GetTableData(TableFullNameModel tableFullName)
        {
            Cursor.Current = Cursors.WaitCursor;
            _currentTable = tableFullName;
            try
            {
                TableData = new TablesDataModel
                {
                    DataBaseName = tableFullName.DataBaseName,
                    TableName = tableFullName.TableName,
                    DataInTable = SqlHelperModel.GetTableDataHelper(tableFullName, out _dataAdapterForUpdate)
                };
            }
            catch (SqlException e)
            {
                ShowMessage($"Get Data of {tableFullName.GetFormattedName()} Failed","Error");
                Logger.Error(e.Message);
                throw;
            }
            Cursor.Current = Cursors.Default;
            IsDataGridOpened = true;
        }
            
        private void DeleteTabsWithDataBaseDeleted(string databaseName)
        {
            if (OpenedTables.Count != 0)
            {
                var deleteTables = OpenedTables.Where(tab => tab.TableFullName.DataBaseName == databaseName).ToList();
                foreach (var t in deleteTables)
                {
                    OpenedTables.Remove(t);
                }
            }

            if (OpenedFoldedTables.Count != 0)
            {
                var deleteTables = OpenedFoldedTables.Where(tab => tab.TableFullName.DataBaseName == databaseName).ToList();
                foreach (var t in deleteTables)
                {
                    OpenedFoldedTables.Remove(t);
                }
            }

            if (OpenedTables.Count != 0)
            {
                OpenedTables[0].IsChoosed = true;
            }
            else
            {
                IsDataGridOpened = false;
            }

            if (OpenedFoldedTables.Count == 0)
            {
                IsTabFoldOpened = false;
            }
        }

        private void CheckSelectAllState()
        {
            if (IsAllSelected)  
            {
                IsAllSelected = false;
            }
        }

        
        private bool CanAddIntoOpenTab(TableFullNameModel tableFullName)
        {
            return OpenedTables.Count < MaxTabCount && !IsThisTableOpenedInTab(tableFullName);
        }

        private bool CanAddIntoOpenTabFolder(TableFullNameModel tableFullName)
        {
            return !IsThisTableOpenedInTab(tableFullName) && !IsThisTableOpenedInFolder(tableFullName);
        }

        private void MoveThisTabToOpenTabList(TableFullNameModel tableFullName)
        {
            OpenedFoldedTables.Add(OpenedTables[MaxTabCount-1]);
            OpenedTables[MaxTabCount-1] =
                OpenedFoldedTables.First(table => table.TableFullName == tableFullName);
            OpenedFoldedTables.Remove(
                OpenedFoldedTables.First(tab => tab.TableFullName == tableFullName));
        }

        private static bool IsOverWriteLocalDataBase(string logicName)
        {
            if (SqlHelperModel.IsLocalExistThisDataBase(logicName))
            {
                return (MessageBox.Show($"是否覆盖本地数据库{logicName}", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                        MessageBoxResult.Yes);

            }
            return false;
        }

        private static bool ConfirmDeleteDataBase(string databaseName)
        {
            return MessageBox.Show($"是否删除本地数据库 {databaseName}？", "提醒", MessageBoxButton.YesNo,
                       MessageBoxImage.Warning) ==
                   MessageBoxResult.Yes;
        }

        private static void DeleteLocalBakFile(string[] allBakFiles, string bakFileName)
        {
            if (allBakFiles.Contains(bakFileName))
            {
                if (MessageBox.Show("该目录中已有该数据库备份文件，是否覆盖原备份？", "Warning", MessageBoxButton.YesNo,
                        MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    File.Delete(bakFileName);
                }
            }
        }

        private static void ShowMessage(string message, string title)
        {
            MessageBox.Show(message, title);
        }
    }
}