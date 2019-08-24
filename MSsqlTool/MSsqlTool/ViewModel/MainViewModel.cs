using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MSsqlTool.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using DataGrid = System.Windows.Controls.DataGrid;
using MessageBox = System.Windows.MessageBox;


namespace MSsqlTool.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Private Properties
        private List<SqlMenuModel> _mainDatabaseList;
        private List<OpenedTablesModel> _openedTableList;
        private List<OpenedTablesModel> _openedTableFoldedList;
        private TablesDataModel _tableData;
        private string[] _currentTable;
        private bool _isDataGridOpened;
        private bool _isAllSelected;
        private bool _isTabFoldOpened;
        #endregion

        #region Public Properties
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
            IsAllSelected = false;
            IsDataGridOpened = false;
            IsTabFoldOpened = false;

            CloseWindowCommand = new RelayCommand(OnCloseWindowCommandExecuted);
            ChangeWindowStateCommand = new RelayCommand(OnChangeWindowStateExecuted);
            MinimizeWindowCommand = new RelayCommand(OnMinimizeWindowExecuted);
            ExportCommand = new RelayCommand<string>(OnExportCommandExecuted);
            DeleteCommand = new RelayCommand<string>(OnDeleteExecuted);
            ImportCommand = new RelayCommand(OnImportExecuted);
            OpenTableCommand = new RelayCommand<string[]>(OnOpenTableExecuted);
            RefreshCommand = new RelayCommand(OnRefreshExecuted);
            CloseTabCommand = new RelayCommand<string[]>(OnCloseTabExecuted);
            CloseFoldTabCommand = new RelayCommand<string[]>(OnCloseFoldTabExecuted);
            ApplyUpdateCommand = new RelayCommand(OnApplyUpdateExecuted);
            ClickTabCommand = new RelayCommand<string[]>(OnClickTabExecuted);
            ClickFoldCommand = new RelayCommand<string[]>(OnClickFoldExecuted);
            CloseOtherTabsCommand = new RelayCommand<string[]>(OnCloseOtherTabsExecuted);
            CloseAllTabsCommand = new RelayCommand(OnCloseAllTabsExecuted);
            SelectAllCommand = new RelayCommand<ItemsControl>(OnSelectAllExecuted);
            CheckForSelectAllCommand = new RelayCommand<DataGrid>(OnCheckForSelectAllExecuted);
        }

        #region Executed functions

        private void OnExportCommandExecuted(string databaseName)
        {
            var chooseExportFolder = new FolderBrowserDialog {Description = @"选择导出路径"};
            if (chooseExportFolder.ShowDialog() != DialogResult.OK) return;
            if (string.IsNullOrEmpty(chooseExportFolder.SelectedPath))
            {
                MessageBox.Show("选定的文件夹路径不能为空", "提示");
                return;
            }
            var exportFileLocation = chooseExportFolder.SelectedPath;
            SqlHelperModel.ExportDataBaseHelper(databaseName, exportFileLocation);
        }

        private void OnCloseWindowCommandExecuted()
        {
            System.Windows.Application.Current.Shutdown();
        }                                                                              

        private void OnChangeWindowStateExecuted()
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

        private void OnMinimizeWindowExecuted()
        {
            if (System.Windows.Application.Current.MainWindow != null)
                System.Windows.Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void OnDeleteExecuted(string databaseName)
        {
            SqlHelperModel.DropDataBaseHelper(databaseName);
            MainDatabaseList = SqlMenuModel.InitializeData();
        }

        private void OnImportExecuted()
        {
            OpenFileDialog chooseFileDialog = new OpenFileDialog
            {
                Title = @"选择导入文件", Multiselect = false, Filter = @"数据库备份文件(*.bak)|*.bak"
            };
            if (chooseFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(chooseFileDialog.FileName))
                {
                    MessageBox.Show("你还未选定备份文件！", "提示");
                    return;
                }
                var filePath = chooseFileDialog.FileName;
                var databaseName = Path.GetFileNameWithoutExtension(filePath);
                SqlHelperModel.ImportDataBaseHelper(databaseName,filePath);
                MainDatabaseList = SqlMenuModel.InitializeData();
            }
        }

        private void OnOpenTableExecuted(string[] tableFullName)
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

        private void OnCloseTabExecuted(string[] tableFullName)
        {
            OpenedTablesModel deleteTab = OpenedTableList.FirstOrDefault(table => table.TableFullName == tableFullName);
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

        private void OnCloseFoldTabExecuted(string[] tableFullName)
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
            if (TableData.DataInTable.GetChanges() == null)
            {
                MessageBox.Show("还未进行任何修改");
            }
            else
            {
                SqlHelperModel.ApplyUpdateHelper();
                GetTableData(_currentTable);
            }
        }

        private void OnClickTabExecuted(string[] tableFullName)
        {
            SetElseTabsFalse(tableFullName);
            GetTableData(tableFullName);
        }

        private void OnClickFoldExecuted(string[] tableFullName)
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

        private void OnCloseOtherTabsExecuted(string[] tableFullName)
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

        private bool IsThisTableOpenedInTab(string[] tableFullName)
        {
            return OpenedTableList.Any(table => table.TableFullName == tableFullName);
        }

        private bool IsThisTableOpenedInFolder(string[] tableFullName)
        {
            return OpenedTableFoldedList.Any(table => table.TableFullName == tableFullName);
        }

        private void SetElseTabsFalse(string[] tableFullName)
        {
            OpenedTableList.ForEach(table => table.IsChoosed = false);
            OpenedTableList.FirstOrDefault(table => table.TableFullName == tableFullName).IsChoosed = true;
        }

        private void GetTableData(string[] tableFullName)
        {
            _currentTable = tableFullName;
            TableData = new TablesDataModel
            {
                DataBaseName = tableFullName[0],
                TableName = tableFullName[1],
                DataInTable = SqlHelperModel.GetTableDataHelper(tableFullName)
            };
            IsDataGridOpened = true;
        }
        #endregion
    }
}