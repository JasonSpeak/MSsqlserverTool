using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MSsqlTool.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private const int MaxTabsCount = 6;
        private readonly Dispatcher _currentDispatcher;

        private bool _isDataGridOpened;
        private bool _isAllSelected;
        private bool _isTabFoldOpened;
        private bool _currentCursorState;
        private WindowState _currentWindowState;
        private List<SqlMenuModel> _mainDatabaseList;
        private SqlDataAdapter _dataAdapterForUpdate;
        private TableFullNameModel _currentTable;
        private DataTable _currentData;

        public bool IsAllSelected
        {
            get => _isAllSelected;
            set
            {
                if (_isAllSelected != value)
                {
                    _isAllSelected = value;
                    RaisePropertyChanged(() => IsAllSelected);
                }
            }
        }
        public bool IsDataGridOpened
        {
            get => _isDataGridOpened;
            set
            {
                if (_isDataGridOpened != value)
                {
                    _isDataGridOpened = value;
                    RaisePropertyChanged(() => IsDataGridOpened);
                }
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
        public bool CurrentCursorState
        {
            get => _currentCursorState;
            set
            {
                _currentCursorState = value;
                RaisePropertyChanged(() => CurrentCursorState);
            }
        }
        public WindowState CurrentWindowState
        {
            get => _currentWindowState;
            set
            {
                _currentWindowState = value;
                RaisePropertyChanged(() => CurrentWindowState);
            }
        }
        public List<SqlMenuModel> MainDatabaseList
        {
            get => _mainDatabaseList;
            set
            {
                _mainDatabaseList = value;
                RaisePropertyChanged(() => MainDatabaseList);
            }
        }
        public DataTable CurrentData
        {
            get => _currentData;
            set
            {
                _currentData = value;
                RaisePropertyChanged(() => CurrentData);
            }
        }
        public ObservableCollection<OpenedTablesModel> OpenedTabs { get; }
        public ObservableCollection<OpenedTablesModel> OpenedTabsFolder { get; }

        public ICommand CloseWindowCommand { get; }
        public ICommand MaximizeOrRestoreCommand { get; }
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

        public MainViewModel()
        {
            CheckSqlServerAvailability();

            CloseWindowCommand = new RelayCommand(OnCloseWindowCommandExecuted);
            MaximizeOrRestoreCommand = new RelayCommand(OnMaximizeOrRestoreCommandExecuted);
            MinimizeWindowCommand = new RelayCommand<string>(OnMinimizeWindowCommandExecuted);
            ExportCommand = new RelayCommand<string>(OnExportCommandExecuted);
            DeleteCommand = new RelayCommand<string>(OnDeleteCommandExecuted);
            ImportCommand = new RelayCommand(OnImportCommandExecuted);
            OpenTableCommand = new RelayCommand<TableFullNameModel>(OnOpenTableCommandExecuted);
            RefreshCommand = new RelayCommand(OnRefreshCommandExecuted);
            CloseTabCommand = new RelayCommand<TableFullNameModel>(OnCloseTabCommandExecuted);
            CloseFoldTabCommand = new RelayCommand<TableFullNameModel>(OnCloseFoldTabCommandExecuted);
            ApplyUpdateCommand = new RelayCommand(OnApplyUpdateCommandExecuted);
            ApplyUpdateCommand = new RelayCommand(OnApplyUpdateCommandExecuted);
            ClickTabCommand = new RelayCommand<TableFullNameModel>(OnClickTabCommandExecuted);
            ClickFoldCommand = new RelayCommand<TableFullNameModel>(OnClickFoldCommandExecuted);
            CloseOtherTabsCommand = new RelayCommand<TableFullNameModel>(OnCloseOtherTabsCommandExecuted);
            CloseAllTabsCommand = new RelayCommand(OnCloseAllTabsCommandExecuted);
            SelectAllCommand = new RelayCommand<DataGrid>(OnSelectAllCommandExecuted);
            CheckForSelectAllCommand = new RelayCommand(OnCheckForSelectAllCommandExecuted);

            CurrentWindowState = WindowState.Normal;
            _dataAdapterForUpdate = new SqlDataAdapter();
            _currentDispatcher = Application.Current.MainWindow?.Dispatcher;
            OpenedTabs = new ObservableCollection<OpenedTablesModel>();
            OpenedTabsFolder = new ObservableCollection<OpenedTablesModel>();

            MainDatabaseList = SqlMenuModel.InitializeData();
        }

        private void OnCloseWindowCommandExecuted()
        {
            Application.Current.Shutdown();
            _dataAdapterForUpdate.Dispose();
            SqlMenuModel.Close();
            SqlHelperModel.Close();
        }

        private void OnMaximizeOrRestoreCommandExecuted()
        {
            CurrentWindowState = (CurrentWindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized);
        }
            
        private void OnMinimizeWindowCommandExecuted(string windowName)
        {
            CurrentWindowState = WindowState.Minimized;
        }

        private void OnExportCommandExecuted(string databaseName)
        {
            var exportFileLocation = GetExportFolder();
            var allBakFiles = Directory.GetFiles(exportFileLocation, "*.bak");
            var bakFileName = $"{exportFileLocation}\\{databaseName}.bak";
            bakFileName = bakFileName.Replace("\\\\", "\\");
            DeleteLocalBakFile(allBakFiles, bakFileName);
            SqlHelperModel.ExportDataBaseHelper(databaseName, exportFileLocation);
            ShowMessage("Export Success!", "Success");
        }

        private void OnDeleteCommandExecuted(string databaseName)
        {
            if (ConfirmDeleteDataBase(databaseName))
            {
                _currentDispatcher?.BeginInvoke(DispatcherPriority.Background,
                    (Action) (() =>
                    {
                        CurrentCursorState = true;
                        SqlHelperModel.DropDataBaseHelper(databaseName);
                        DeleteTabsWithDataBaseDeleted(databaseName);
                        MainDatabaseList = SqlMenuModel.InitializeData();
                        CurrentCursorState = false;
                        ShowMessage($"Success Delete Local DataBase {databaseName}", "Success");
                    }));
            }
        }

        private void OnImportCommandExecuted()
        {
            var filePath = GetImportFileLocation();
            var logicName = SqlHelperModel.GetLogicNameFromBak(filePath);
            if (IsOverWriteLocalDataBase(logicName))
            {
                _currentDispatcher?.BeginInvoke(DispatcherPriority.Background,
                    (Action)(() =>
                    {
                        CurrentCursorState = true;
                        SqlHelperModel.ImportDataBaseHelper(filePath);
                        MainDatabaseList = SqlMenuModel.InitializeData();
                        CurrentCursorState = false;
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
                OpenedTabsFolder.Add(OpenedTabs[MaxTabsCount-1]);
                OpenedTabs[MaxTabsCount-1] = new OpenedTablesModel(tableFullName);
            }
            else if (OpenedTabsFolder.IsThisTabOpened(tableFullName))
            {
                IsAllSelected = false;
                MoveThisTabToOpenTabs(tableFullName);
            }
            OpenedTabsFolder.SetAllTabsFalse();
            OpenedTabs.SetElseTabsFalse(tableFullName);
            GetTableData(tableFullName);
        }

        private void OnRefreshCommandExecuted()
        {
            MainDatabaseList = SqlMenuModel.InitializeData();
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
            else
            {
                IsAllSelected = false;
            }
            IsDataGridOpened = OpenedTabs.Count != 0;
        }

        private void OnCloseFoldTabCommandExecuted(TableFullNameModel tableFullName)
        {
            OpenedTabsFolder.Remove(OpenedTabsFolder.First(table => table.TableFullName == tableFullName));
            IsTabFoldOpened = (OpenedTabsFolder.Count != 0);
        }

        private void OnApplyUpdateCommandExecuted()
        {
            if (CurrentData.GetChanges() != null)
            {
                if(!SqlHelperModel.ApplyUpdateHelper(_dataAdapterForUpdate, CurrentData))
                    ShowMessage("Update table's data failed\nRead Log file for more Information","Error");
                GetTableData(_currentTable);
            }
        }

        private void OnClickTabCommandExecuted(TableFullNameModel tableFullName)
        {
            if (!OpenedTabs.First(tab => tab.TableFullName == tableFullName).IsChoosed)
            {
                IsAllSelected = false;
                OpenedTabs.SetElseTabsFalse(tableFullName);
                GetTableData(tableFullName);
            }
        }

        private void OnClickFoldCommandExecuted(TableFullNameModel tableFullName)
        {
            IsAllSelected = false;
            MoveThisTabToOpenTabs(tableFullName);
            OpenedTabs.SetElseTabsFalse(tableFullName);
            GetTableData(tableFullName);
            OpenedTabsFolder.SetAllTabsFalse();
        }

        private void OnCloseOtherTabsCommandExecuted(TableFullNameModel tableFullName)
        {
            var lastTab = OpenedTabs.First(table => table.TableFullName == tableFullName);
            IsTabFoldOpened = false;
            OpenedTabs.Clear();
            OpenedTabsFolder.Clear();
            OpenedTabs.Add(lastTab);
            if (!lastTab.IsChoosed)
            {
                IsAllSelected = false;
                OpenedTabs[0].IsChoosed = true;
                GetTableData(tableFullName);
            }
        }

        private void OnCloseAllTabsCommandExecuted()
        {
            IsDataGridOpened = false;
            IsAllSelected = false;
            IsTabFoldOpened = false;
            OpenedTabs.Clear();
            OpenedTabsFolder.Clear();
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

        private void CheckSqlServerAvailability()
        {
            if (!SqlHelperModel.CanMasterConnAvailable())
            {
                ShowMessage("Unable to connect SqlServer\nRead Log file for more Information","Error");
            }
        }

        private void GetTableData(TableFullNameModel tableFullName)
        {
            IsDataGridOpened = true;
            _currentDispatcher?.BeginInvoke(DispatcherPriority.Normal,
                (Action)(() =>
                {
                    CurrentCursorState = true;
                    CurrentData = SqlHelperModel.GetTableDataHelper(tableFullName, out _dataAdapterForUpdate);
                    CurrentCursorState = false;
                }));
            _currentTable = tableFullName;
        }
            
        private void DeleteTabsWithDataBaseDeleted(string databaseName)
        {
            OpenedTabs.DeleteTabWithDataBaseName(databaseName);
            OpenedTabsFolder.DeleteTabWithDataBaseName(databaseName);
            if (OpenedTabs.Count != 0)
            {
                OpenedTabs[0].IsChoosed = true;
            }
            IsDataGridOpened = OpenedTabs.Count != 0;
            IsTabFoldOpened = OpenedTabsFolder.Count != 0;
        }

        private void MoveThisTabToOpenTabs(TableFullNameModel tableFullName)
        {
            OpenedTabsFolder.Add(OpenedTabs[MaxTabsCount - 1]);
            OpenedTabs[MaxTabsCount - 1] =
                OpenedTabsFolder.First(table => table.TableFullName == tableFullName);
            OpenedTabsFolder.Remove(
                OpenedTabsFolder.First(tab => tab.TableFullName == tableFullName));
        }

        private bool CanAddIntoOpenTab(TableFullNameModel tableFullName)
        {
            return OpenedTabs.Count < MaxTabsCount && !OpenedTabs.IsThisTabOpened(tableFullName);
        }

        private bool CanAddIntoOpenTabFolder(TableFullNameModel tableFullName)
        {
            return !OpenedTabs.IsThisTabOpened(tableFullName) && !OpenedTabsFolder.IsThisTabOpened(tableFullName);
        }

        private static void ShowMessage(string message, string title)
        {
            MessageBox.Show(message, title);
        }

        private bool IsOverWriteLocalDataBase(string logicName)
        {
            if (SqlHelperModel.IsLocalExistThisDataBase(logicName))
            {
                return (MessageBox.Show($"Do you want to OVERWRITE local database:\n{logicName}", "Warning",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                        MessageBoxResult.Yes);
            }
            return true;
        }

        private bool ConfirmDeleteDataBase(string databaseName)
        {
            return MessageBox.Show($"Do you want to DELETE local database:\n{databaseName}？", "Warning",
                       MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                   MessageBoxResult.Yes;
        }

        private void DeleteLocalBakFile(string[] allBakFiles, string bakFileName)
        {
            if (allBakFiles.Contains(bakFileName))
            {
                if (MessageBox.Show("Do you want to OVERWRITE old backup file of this database？", "Warning", MessageBoxButton.YesNo,
                        MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    File.Delete(bakFileName);
                }
            }
        }

        private static string GetExportFolder()
        {
            var chooseExportFolder = new FolderBrowserDialog { Description = @"Choose Export Location" };
            chooseExportFolder.ShowDialog();
            return chooseExportFolder.SelectedPath;
        }

        private static string GetImportFileLocation()
        {
            var chooseFileDialog = new OpenFileDialog
            {
                Title = @"Choose Import bak file",
                Multiselect = false,
                Filter = @"Database Back File(*.bak)|*.bak"
            };
            chooseFileDialog.ShowDialog();
            return chooseFileDialog.FileName;
        }
    }
}