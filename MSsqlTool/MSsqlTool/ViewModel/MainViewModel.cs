using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MSsqlTool.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Application = System.Windows.Application;
using DataGrid = System.Windows.Controls.DataGrid;
using MessageBox = System.Windows.MessageBox;


namespace MSsqlTool.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private static readonly string ConnectString =
            ConfigurationManager.ConnectionStrings["ConnectString"].ToString();
        private const int MaxTabCount = 6;

        private readonly SqlConnection _masterConn;
        private readonly Dispatcher _currentDispatcher;
        private WindowState _currentWindowState;
        private string _currentCursor;
        private List<SqlMenuModel> _mainDatabaseList;
        private SqlDataAdapter _dataAdapterForUpdate;
        private TableFullNameModel _currentTable;
        private DataTable _currentData;
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
        public string CurrentCursor
        {
            get => _currentCursor;
            set
            {
                _currentCursor = value;
                RaisePropertyChanged(()=>CurrentCursor);
            }
        }
        public List<SqlMenuModel> MainDatabaseList
        {
            get => _mainDatabaseList;
            set
            {
                _mainDatabaseList = value;
                RaisePropertyChanged(()=>MainDatabaseList);
            }
        }
        public ObservableCollection<OpenedTablesModel> OpenedTabs { get; }
        public ObservableCollection<OpenedTablesModel> OpenedTabsFolder { get; }
        public DataTable CurrentData
        {
            get => _currentData;
            set
            {
                _currentData = value;
                RaisePropertyChanged(()=>CurrentData);
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

            CurrentWindowState = WindowState.Normal;
            CurrentCursor = "Arrow";
            OpenedTabs = new ObservableCollection<OpenedTablesModel>();
            OpenedTabsFolder = new ObservableCollection<OpenedTablesModel>();
            _dataAdapterForUpdate = new SqlDataAdapter();
            _masterConn = new SqlConnection(ConnectString);
            _currentDispatcher = Application.Current.MainWindow?.Dispatcher;

            MainDatabaseList = SqlMenuModel.InitializeData(_masterConn);
        }

        private void OnCloseWindowCommandExecuted()
        {
            Application.Current.Shutdown();
            _dataAdapterForUpdate.Dispose();
            _masterConn.Dispose();
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
            var chooseExportFolder = new FolderBrowserDialog {Description = @"Choose Export Location"};
            if (chooseExportFolder.ShowDialog() == DialogResult.OK)
            {
                var exportFileLocation = chooseExportFolder.SelectedPath;
                var allBakFiles = Directory.GetFiles(exportFileLocation, "*.bak");
                var bakFileName = $"{exportFileLocation}\\{databaseName}.bak";
                bakFileName = bakFileName.Replace("\\\\", "\\");
                DeleteLocalBakFile(allBakFiles, bakFileName);
                SqlHelperModel.ExportDataBaseHelper(databaseName, exportFileLocation,_masterConn);
                ShowMessage("Export Success!", "Success");
            }
        }

        private void OnDeleteCommandExecuted(string databaseName)
        {
            CurrentCursor = "Wait";
            if (ConfirmDeleteDataBase(databaseName))
            {
                SqlHelperModel.DropDataBaseHelper(databaseName,_masterConn);
                DeleteTabsWithDataBaseDeleted(databaseName);
                MainDatabaseList = SqlMenuModel.InitializeData(_masterConn);
                CurrentCursor = "Arrow";
                ShowMessage($"Success Delete Local DataBase {databaseName}", "Success");
            }
        }

        private void OnImportCommandExecuted()
        {
            var chooseFileDialog = new OpenFileDialog
            {
                Title = @"Choose Import bak file", Multiselect = false, Filter = @"Database Back File(*.bak)|*.bak"
            };
            if (chooseFileDialog.ShowDialog() == DialogResult.OK)
            {
                
                _currentDispatcher?.BeginInvoke(DispatcherPriority.Background,
                    (Action) (() =>
                    {
                        CurrentCursor = "Wait";
                        var filePath = chooseFileDialog.FileName;
                        var logicName = SqlHelperModel.GetLogicNameFromBak(filePath, _masterConn);
                        if (!IsOverWriteLocalDataBase(logicName)) return;
                        SqlHelperModel.ImportDataBaseHelper(filePath, _masterConn);
                        MainDatabaseList = SqlMenuModel.InitializeData(_masterConn);
                        CurrentCursor = "Arrow";
                        DeleteTabsWithDataBaseDeleted(logicName);
                        ShowMessage("Import Success", "Success");
                    }));
            }
        }

        private void OnOpenTableCommandExecuted(TableFullNameModel tableFullName)
        {
            if (CanAddIntoOpenTab(tableFullName))
            {
                IsAllSelected = false;
                OpenedTabs.Add(new OpenedTablesModel(tableFullName));
            }
            else if (CanAddIntoOpenTabFolder(tableFullName))
            {
                IsAllSelected = false;
                IsTabFoldOpened = true;
                OpenedTabsFolder.Add(OpenedTabs[MaxTabCount-1]);
                OpenedTabs[MaxTabCount-1] = new OpenedTablesModel(tableFullName);
            }
            else if (IsThisTableOpenedInFolder(tableFullName))
            {
                IsAllSelected = false;
                MoveThisTabToOpenTabs(tableFullName);
            }
            OpenedTablesModel.SetAllTabsFalse(OpenedTabsFolder);
            OpenedTablesModel.SetElseTabsFalse(OpenedTabs,tableFullName);
            GetTableData(tableFullName);
        }

        private void OnRefreshCommandExecuted()
        {
            MainDatabaseList = SqlMenuModel.InitializeData(_masterConn);
        }

        private void OnCloseTabCommandExecuted(TableFullNameModel tableFullName)
        {
            var deleteTab = OpenedTabs.First(table => table.TableFullName == tableFullName);
            OpenedTabs.Remove(deleteTab);
            if (OpenedTabsFolder.Count != 0)
            {
                OpenedTabs.Add(OpenedTabsFolder[0]);
                OpenedTabsFolder.RemoveAt(0);
                IsTabFoldOpened = (OpenedTabsFolder.Count != 0);
            }
            if (OpenedTabs.Count != 0)
            {
                if (deleteTab.IsChoosed)
                {
                    IsAllSelected = false;
                    OpenedTabs[0].IsChoosed = true;
                    GetTableData(OpenedTabs[0].TableFullName);
                }
            }
            IsDataGridOpened = OpenedTabs.Count != 0;
            IsAllSelected = OpenedTabs.Count != 0;
            
        }

        private void OnCloseFoldTabCommandExecuted(TableFullNameModel tableFullName)
        {
            OpenedTabsFolder.Remove(
                OpenedTabsFolder.First(table => table.TableFullName == tableFullName));
            IsTabFoldOpened = (OpenedTabsFolder.Count != 0);
        }

        private void OnApplyUpdateCommandExecuted()
        {
            if (CurrentData.GetChanges() != null)
            {
                try
                {
                    SqlHelperModel.ApplyUpdateHelper(_dataAdapterForUpdate, CurrentData);
                }
                catch (SqlException e)
                {
                    ShowMessage(e.Message,"Error");
                }
                GetTableData(_currentTable);
            }
        }

        private void OnClickTabCommandExecuted(TableFullNameModel tableFullName)
        {
            if (!OpenedTabs.First(tab => tab.TableFullName == tableFullName).IsChoosed)
            {
                IsAllSelected = false;
                OpenedTablesModel.SetElseTabsFalse(OpenedTabs, tableFullName);
                GetTableData(tableFullName);
            }
        }

        private void OnClickFoldCommandExecuted(TableFullNameModel tableFullName)
        {
            IsAllSelected = false;
            MoveThisTabToOpenTabs(tableFullName);
            OpenedTablesModel.SetElseTabsFalse(OpenedTabs, tableFullName);
            GetTableData(tableFullName);
            OpenedTablesModel.SetAllTabsFalse(OpenedTabsFolder);
        }

        private void OnCloseOtherTabsCommandExecuted(TableFullNameModel tableFullName)
        {
            var lastTab = OpenedTabs.First(table => table.TableFullName == tableFullName);
            OpenedTabs.Clear();
            OpenedTabsFolder.Clear();
            OpenedTabs.Add(lastTab);
            IsTabFoldOpened = false;
            if (!lastTab.IsChoosed)
            {
                OpenedTabs[0].IsChoosed = true;
                GetTableData(tableFullName);
                IsAllSelected = false;
            }
        }

        private void OnCloseAllTabsCommandExecuted()
        {
            IsAllSelected = false;
            OpenedTabs.Clear();
            OpenedTabsFolder.Clear();
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
            IsAllSelected = false;
        }

        private bool IsThisTableOpenedInTab(TableFullNameModel tableFullName)
        {
            return OpenedTabs.Any(table => table.TableFullName == tableFullName);
        }

        private bool IsThisTableOpenedInFolder(TableFullNameModel tableFullName)
        {
            return OpenedTabsFolder.Any(table => table.TableFullName == tableFullName);
        }

        private void GetTableData(TableFullNameModel tableFullName)
        {
            CurrentCursor = "Wait";
            _currentTable = tableFullName;
            CurrentData = SqlHelperModel.GetTableDataHelper(tableFullName, out _dataAdapterForUpdate);
            CurrentCursor = "Arrow";
            IsDataGridOpened = true;
        }
            
        private void DeleteTabsWithDataBaseDeleted(string databaseName)
        {
            OpenedTablesModel.DeleteTabWithDataBaseName(OpenedTabs,databaseName);
            OpenedTablesModel.DeleteTabWithDataBaseName(OpenedTabsFolder,databaseName);
            if (OpenedTabs.Count != 0)
            {
                OpenedTabs[0].IsChoosed = true;
            }
            else
            {
                IsDataGridOpened = false;
            }
            if (OpenedTabsFolder.Count == 0)
            {
                IsTabFoldOpened = false;
            }
        }

        private bool CanAddIntoOpenTab(TableFullNameModel tableFullName)
        {
            return OpenedTabs.Count < MaxTabCount && !IsThisTableOpenedInTab(tableFullName);
        }

        private bool CanAddIntoOpenTabFolder(TableFullNameModel tableFullName)
        {
            return !IsThisTableOpenedInTab(tableFullName) && !IsThisTableOpenedInFolder(tableFullName);
        }

        private void MoveThisTabToOpenTabs(TableFullNameModel tableFullName)
        {
            OpenedTabsFolder.Add(OpenedTabs[MaxTabCount-1]);
            OpenedTabs[MaxTabCount-1] =
                OpenedTabsFolder.First(table => table.TableFullName == tableFullName);
            OpenedTabsFolder.Remove(
                OpenedTabsFolder.First(tab => tab.TableFullName == tableFullName));
        }

        private bool IsOverWriteLocalDataBase(string logicName)
        {
            if (SqlHelperModel.IsLocalExistThisDataBase(logicName,_masterConn))
            {
                return (MessageBox.Show($"Do you want to OVERWRITE local database:\n{logicName}", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                        MessageBoxResult.Yes);

            }
            return true;
        }

        private bool ConfirmDeleteDataBase(string databaseName)
        {
            return MessageBox.Show($"Do you want to DELETE local database:\n{databaseName}？", "Warning", MessageBoxButton.YesNo,
                       MessageBoxImage.Warning) ==
                   MessageBoxResult.Yes;
        }

        private void DeleteLocalBakFile(string[] allBakFiles, string bakFileName)
        {
            if (allBakFiles.Contains(bakFileName))
            {
                if (MessageBox.Show("Do you want to OVERWRITE Old backup file of this database？", "Warning", MessageBoxButton.YesNo,
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