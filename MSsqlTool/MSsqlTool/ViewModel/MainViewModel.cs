using System;
using System.Collections;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MSsqlTool.Model;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using NLog;
using Cursor = System.Windows.Forms.Cursor;
using Cursors = System.Windows.Forms.Cursors;
using DataGrid = System.Windows.Controls.DataGrid;
using MessageBox = System.Windows.MessageBox;


namespace MSsqlTool.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Private Properties
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private List<SqlMenuModel> _mainDatabaseList;
        private List<OpenedTablesModel> _openedTableList;
        private List<OpenedTablesModel> _openedTableFoldedList;
        private TablesDataModel _tableData;
        private SqlDataAdapter _dataAdapterForUpdate;
        private TableFullNameModel _currentTable;
        private bool _isDataGridOpened;
        private bool _isAllSelected;
        private bool _isTabFoldOpened;
        #endregion

        #region Public Properties
        public ICommand CloseWindowCommand { get; }
        public ICommand ChangeWindowStateCommand { get; }
        public ICommand MinimizeWindowCommand { get; }
        public ICommand ResizeWindowCommand { get; }
        public ICommand LoadingRowCommand { get; }
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

        public List<SqlMenuModel> MainDatabaseList
        {
            get => _mainDatabaseList;
            set
            {
                _mainDatabaseList = value;
                RaisePropertyChanged(() => MainDatabaseList);
            }
        }
        public List<OpenedTablesModel> OpenedTableList
        {
            get => _openedTableList;
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
                _isTabFoldOpened = value;
                RaisePropertyChanged(()=>IsTabFoldOpened);
            }
        }
        #endregion

        public MainViewModel()
        {
            MainDatabaseList = SqlMenuModel.InitializeData();
            OpenedTableList = new List<OpenedTablesModel>();
            OpenedTableFoldedList = new List<OpenedTablesModel>();
            _dataAdapterForUpdate = new SqlDataAdapter();
            IsAllSelected = false;
            IsDataGridOpened = false;
            IsTabFoldOpened = false;

            CloseWindowCommand = new RelayCommand(OnCloseWindowCommandExecuted);
            ChangeWindowStateCommand = new RelayCommand(OnChangeWindowStateExecuted);
            MinimizeWindowCommand = new RelayCommand(OnMinimizeWindowExecuted);
            ResizeWindowCommand = new RelayCommand(OnResizeWindowExecuted);
            //LoadingRowCommand = new RelayCommand<>();
            ExportCommand = new RelayCommand<string>(OnExportCommandExecuted);
            DeleteCommand = new RelayCommand<string>(OnDeleteExecuted);
            ImportCommand = new RelayCommand(OnImportExecuted);
            OpenTableCommand = new RelayCommand<TableFullNameModel>(OnOpenTableExecuted);
            RefreshCommand = new RelayCommand(OnRefreshExecuted);
            CloseTabCommand = new RelayCommand<TableFullNameModel>(OnCloseTabExecuted);
            CloseFoldTabCommand = new RelayCommand<TableFullNameModel>(OnCloseFoldTabExecuted);
            ApplyUpdateCommand = new RelayCommand(OnApplyUpdateExecuted);
            ClickTabCommand = new RelayCommand<TableFullNameModel>(OnClickTabExecuted);
            ClickFoldCommand = new RelayCommand<TableFullNameModel>(OnClickFoldExecuted);
            CloseOtherTabsCommand = new RelayCommand<TableFullNameModel>(OnCloseOtherTabsExecuted);
            CloseAllTabsCommand = new RelayCommand(OnCloseAllTabsExecuted);
            SelectAllCommand = new RelayCommand<ItemsControl>(OnSelectAllExecuted);
            CheckForSelectAllCommand = new RelayCommand<DataGrid>(OnCheckForSelectAllExecuted);
        }

        #region Executed functions

        private static void OnExportCommandExecuted(string databaseName)
        {
            var chooseExportFolder = new FolderBrowserDialog {Description = @"选择导出路径"};
            if (chooseExportFolder.ShowDialog() != DialogResult.OK) return;
            if (string.IsNullOrEmpty(chooseExportFolder.SelectedPath))
            {
                MessageBox.Show("选定的文件夹路径不能为空", "提示");
                return;
            }
            var exportFileLocation = chooseExportFolder.SelectedPath;
            var allBakFiles = Directory.GetFiles(exportFileLocation, "*.bak");
            var bakFileName = $"{exportFileLocation}{databaseName}.bak";
            foreach (var file in allBakFiles)
            {
                Logger.Trace(bakFileName);
                Logger.Trace(file);
            }
            if (allBakFiles.Contains(bakFileName))
            {
                if (MessageBox.Show("该目录中已有该数据库备份文件，是否覆盖原备份？", "Warning", MessageBoxButton.YesNo,
                        MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    var fileLocation = $"{exportFileLocation}{databaseName}.bak";
                    try
                    {
                        File.Delete(fileLocation);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.Message);
                    }
                }
            }
            SqlHelperModel.ExportDataBaseHelper(databaseName, exportFileLocation);
        }

        private static void OnCloseWindowCommandExecuted()
        {
            System.Windows.Application.Current.Shutdown();
        }                                                                              

        private static void OnChangeWindowStateExecuted()
        {
            if (System.Windows.Application.Current.MainWindow != null && System.Windows.Application.Current.MainWindow.WindowState == WindowState.Maximized)
            {
                System.Windows.Application.Current.MainWindow.WindowState = WindowState.Normal;
            }
            else
            {
                if (System.Windows.Application.Current.MainWindow != null)
                    System.Windows.Application.Current.MainWindow.WindowState = WindowState.Maximized;
            }
        }

        private static void OnMinimizeWindowExecuted()
        {
            if (System.Windows.Application.Current.MainWindow != null)
                System.Windows.Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private static void OnResizeWindowExecuted()
        {
            if (System.Windows.Application.Current.MainWindow != null && System.Windows.Application.Current.MainWindow.WindowState == WindowState.Maximized)
            {
                System.Windows.Application.Current.MainWindow.BorderThickness = new Thickness(8);
            }
            else
            {
                if (System.Windows.Application.Current.MainWindow != null)
                    System.Windows.Application.Current.MainWindow.BorderThickness = new Thickness(0);
            }
        }

        private static void OnLoadingRowExecuted(DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void OnDeleteExecuted(string databaseName)
        {
            Cursor.Current = Cursors.WaitCursor;
            SqlHelperModel.DropDataBaseHelper(databaseName);
            MainDatabaseList = SqlMenuModel.InitializeData();
            Cursor.Current = Cursors.Default;
            if (OpenedTableList.Count != 0)
            {
                OpenedTableList.RemoveAll(tab => tab.TableFullName.DataBaseName == databaseName);
            }

            if (OpenedTableFoldedList.Count != 0)
            {
                OpenedTableFoldedList.RemoveAll(tab => tab.TableFullName.DataBaseName == databaseName);
            }

            if (OpenedTableList.Count != 0)
            {
                OpenedTableList[0].IsChoosed = true;
            }
            else
            {
                IsDataGridOpened = false;
            }
            OpenedTableList = new List<OpenedTablesModel>(OpenedTableList);
            OpenedTableFoldedList = new List<OpenedTablesModel>(OpenedTableFoldedList);
        }

        private void OnImportExecuted()
        {
            var chooseFileDialog = new OpenFileDialog
            {
                Title = @"选择导入文件", Multiselect = false, Filter = @"数据库备份文件(*.bak)|*.bak"
            };
            if (chooseFileDialog.ShowDialog() != DialogResult.OK) return;
            if (string.IsNullOrEmpty(chooseFileDialog.FileName))
            {
                MessageBox.Show("你还未选定备份文件！", "提示");
                return;
            }
            var filePath = chooseFileDialog.FileName;
            Cursor.Current = Cursors.WaitCursor;
            SqlHelperModel.ImportDataBaseHelper(filePath);
            MainDatabaseList = SqlMenuModel.InitializeData();
            Cursor.Current = Cursors.Default;
        }

        private void OnOpenTableExecuted(TableFullNameModel tableFullName)
        {
            if (OpenedTableList.Count < 6 && !IsThisTableOpenedInTab(tableFullName))
            {
                OpenedTableList.Add(new OpenedTablesModel(tableFullName));
            }
            else if (!IsThisTableOpenedInTab(tableFullName) && !IsThisTableOpenedInFolder(tableFullName))
            {
                IsTabFoldOpened = true;
                OpenedTableFoldedList.Add(OpenedTableList[5]);
                OpenedTableList[5] = new OpenedTablesModel(tableFullName);
            }
            else if (!IsThisTableOpenedInTab(tableFullName) && IsThisTableOpenedInFolder(tableFullName))
            {
                OpenedTableFoldedList.Add(OpenedTableList[5]);
                OpenedTableList[5] =
                    OpenedTableFoldedList.FirstOrDefault(table => table.TableFullName == tableFullName);
                OpenedTableFoldedList.Remove(
                    OpenedTableFoldedList.FirstOrDefault(tab => tab.TableFullName == tableFullName));
            }
            OpenedTableFoldedList.ForEach(tab => tab.IsChoosed = false);
            SetElseTabsFalse(tableFullName);
            OpenedTableList = new List<OpenedTablesModel>(OpenedTableList);
            OpenedTableFoldedList = new List<OpenedTablesModel>(OpenedTableFoldedList);
            GetTableData(tableFullName);
        }

        private void OnRefreshExecuted()
        {
            MainDatabaseList = SqlMenuModel.InitializeData();
        }

        private void OnCloseTabExecuted(TableFullNameModel tableFullName)
        {
            var deleteTab = OpenedTableList.FirstOrDefault(table => table.TableFullName == tableFullName);
            OpenedTableList.Remove(deleteTab);
            if (OpenedTableFoldedList.Count != 0)
            {
                OpenedTableList.Add(OpenedTableFoldedList[0]);
                OpenedTableFoldedList.RemoveAt(0);
            }
            else
            {
                IsTabFoldOpened = false;
            }
            if (OpenedTableList.Count != 0)
            {
                if (deleteTab != null && deleteTab.IsChoosed)
                {
                    OpenedTableList[0].IsChoosed = true;
                }
            }
            else
            {
                IsDataGridOpened = false;
                IsAllSelected = false;
                TableData = null;
            }
            OpenedTableList = new List<OpenedTablesModel>(OpenedTableList);
            OpenedTableFoldedList = new List<OpenedTablesModel>(OpenedTableFoldedList);
        }

        private void OnCloseFoldTabExecuted(TableFullNameModel tableFullName)
        {
            OpenedTableFoldedList.Remove(
                OpenedTableFoldedList.FirstOrDefault(table => table.TableFullName == tableFullName));
            if (OpenedTableFoldedList.Count == 0)
            {
                IsTabFoldOpened = false;
            }
            OpenedTableFoldedList = new List<OpenedTablesModel>(OpenedTableFoldedList);
        }

        private void OnApplyUpdateExecuted()
        {
            if (TableData.DataInTable.GetChanges() == null)return;
            try
            {
                SqlHelperModel.ApplyUpdateHelper(_dataAdapterForUpdate, TableData.DataInTable);
                GetTableData(_currentTable);
            }
            catch (Exception e)
            {
                Logger.Trace(e.Message);
            }
            
        }

        private void OnClickTabExecuted(TableFullNameModel tableFullName)
        {
            SetElseTabsFalse(tableFullName);
            GetTableData(tableFullName);
        }

        private void OnClickFoldExecuted(TableFullNameModel tableFullName)
        {
            OpenedTableFoldedList.Add(OpenedTableList[5]);
            OpenedTableList[5] = OpenedTableFoldedList.FirstOrDefault(table => table.TableFullName == tableFullName);
            SetElseTabsFalse(tableFullName);
            GetTableData(tableFullName);
            OpenedTableFoldedList.Remove(OpenedTableFoldedList.FirstOrDefault(table => table.TableFullName == tableFullName));
            OpenedTableFoldedList.ForEach(table => table.IsChoosed = false);
            OpenedTableList = new List<OpenedTablesModel>(OpenedTableList);
            OpenedTableFoldedList = new List<OpenedTablesModel>(OpenedTableFoldedList);
        }

        private void OnCloseOtherTabsExecuted(TableFullNameModel tableFullName)
        {
            OpenedTableList = new List<OpenedTablesModel>()
            {
                OpenedTableList.FirstOrDefault(table=>table.TableFullName==tableFullName)
            };
            SetElseTabsFalse(tableFullName);
            GetTableData(tableFullName);
            OpenedTableFoldedList = new List<OpenedTablesModel>();
            IsTabFoldOpened = false;
        }

        private void OnCloseAllTabsExecuted()
        {
            OpenedTableList = new List<OpenedTablesModel>();
            OpenedTableFoldedList = new List<OpenedTablesModel>();
            TableData = new TablesDataModel();
        }

        private void OnSelectAllExecuted(ItemsControl dataGrid)
        {
            if (IsAllSelected)
            {
                for (var i = 0; i < dataGrid.Items.Count; i++)
                {
                    var row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(i);
                    if (row != null)
                    {
                        row.IsSelected = true;

                    }
                }
                IsAllSelected = true;
            }
            else
            {
                for (var i = 0; i < dataGrid.Items.Count; i++)
                {
                    var row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(i);
                    if (row != null)
                    {
                        row.IsSelected = false;
                    }
                }
                IsAllSelected = false;
            }
        }

        private void OnCheckForSelectAllExecuted(DataGrid dataGrid)
        {
            if (IsAllSelected)
            {
                IsAllSelected = false;
            }
        }

        #endregion

        #region Helper Functions in Executed Functions

        private bool IsThisTableOpenedInTab(TableFullNameModel tableFullName)
        {
            return OpenedTableList.Any(table => table.TableFullName == tableFullName);
        }

        private bool IsThisTableOpenedInFolder(TableFullNameModel tableFullName)
        {
            return OpenedTableFoldedList.Any(table => table.TableFullName == tableFullName);
        }

        private void SetElseTabsFalse(TableFullNameModel tableFullName)
        {
            OpenedTableList.ForEach(table => table.IsChoosed = false);
            OpenedTableList.FirstOrDefault(table => table.TableFullName == tableFullName).IsChoosed = true;
        }

        private void GetTableData(TableFullNameModel tableFullName)
        {
            _currentTable = tableFullName;
            TableData = new TablesDataModel
            {
                DataBaseName = tableFullName.DataBaseName,
                TableName = tableFullName.TableName,
                DataInTable = SqlHelperModel.GetTableDataHelper(tableFullName,ref _dataAdapterForUpdate)
            };
            IsDataGridOpened = true;
        }
        #endregion
    }
}