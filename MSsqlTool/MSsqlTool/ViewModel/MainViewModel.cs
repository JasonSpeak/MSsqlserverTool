using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MSsqlTool.Model;
using NLog;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Windows.Controls;
using System.Windows.Forms;
using DataGrid = System.Windows.Controls.DataGrid;
using MessageBox = System.Windows.MessageBox;


namespace MSsqlTool.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Private Properties

        private static readonly string connectString =
            ConfigurationManager.ConnectionStrings["ConnectString"].ToString();

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private enum _sysDataBases
        {
            master,
            model,
            msdb,
            tempdb
        }

        private RelayCommand<string> _exportCommand;

        private RelayCommand<string> _deleteCommand;

        private RelayCommand _importCommand;

        private RelayCommand<string[]> _openTableCommand;

        private RelayCommand _refreshCommand;

        private RelayCommand<string[]> _closeTabCommand;

        private RelayCommand<string[]> _closeFoldTabCommand;

        private RelayCommand _applyUpdateCommand;

        private RelayCommand<string[]> _clickTabCommand;

        private RelayCommand<string[]> _clickFoldCommand;

        private RelayCommand<string[]> _closeOtherTabsCommand;

        private RelayCommand _closeAllTabsCommand;

        private RelayCommand<DataGrid> _selectAllCommand;

        private RelayCommand<DataGrid> _checkForSelectAllCommand;

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

        public RelayCommand<string> DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new RelayCommand<string>((databaseName)=> DeleteExecuted(databaseName));
                }
                return _deleteCommand;
            }
            set { _deleteCommand = value; }
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

        public RelayCommand<string[]> OpenTableCommand
        {
            get
            {
                if (_openTableCommand == null)
                {
                    _openTableCommand = new RelayCommand<string[]>((tableName) => OpenTableExecuted(tableName));
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

        public RelayCommand<string[]> CloseTabCommand
        {
            get
            {
                if (_closeTabCommand == null)
                {
                    _closeTabCommand = new RelayCommand<string[]>((tableName) => CloseTabExecuted(tableName));
                }

                return _closeTabCommand;
            }
            set { _closeTabCommand = value; }
        }

        public RelayCommand<string[]> CloseFoldTabCommand
        {
            get
            {
                if (_closeFoldTabCommand == null)
                {
                    _closeFoldTabCommand = new RelayCommand<string[]>((tableName) => CloseFoldTabExecuted(tableName));
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

        public RelayCommand<string[]> ClickTabCommand
        {
            get
            {
                if (_clickTabCommand == null)
                {
                    _clickTabCommand = new RelayCommand<string[]>((tableFullName) => ClickTabExecuted(tableFullName));
                }

                return _clickTabCommand;
            }
            set { _clickTabCommand = value; }
        }

        public RelayCommand<string[]> ClickFoldCommand
        {
            get
            {
                if (_clickFoldCommand == null)
                {
                    _clickFoldCommand = new RelayCommand<string[]>((tableFullName) => ClickFoldExecuted(tableFullName));
                }

                return _clickFoldCommand;
            }
            set { _clickFoldCommand = value; }
        }

        public RelayCommand<string[]> CloseOtherTabsCommand
        {
            get
            {
                if (_closeOtherTabsCommand == null)
                {
                    _closeOtherTabsCommand =
                        new RelayCommand<string[]>((tableFullName) => CloseOtherTabsExecuted(tableFullName));
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

        public List<SqlMenuModel> MainDatabaseList
        {
            get
            {
                if (_mainDatabaseList == null)
                {
                    _mainDatabaseList = SqlMenuModel.InitializeData();
                }
                return _mainDatabaseList;
            }
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

        public bool IsAllSelected
        {
            get { return _isAllSelected; }
            set
            {
                _isAllSelected = value;
                RaisePropertyChanged(()=>IsAllSelected);
            }
        }

        public bool IsDataGridOpened
        {
            get { return _isDataGridOpened; }
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
            IsAllSelected = false;
            IsDataGridOpened = false;
            IsTabFoldOpened = false;
        }

        #region Executed functions

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
                SqlHelperModel.ExportDataBaseHelper(databaseName,exportFileLocation);
            }

        }

        private void DeleteExecuted(string databaseName)
        {
            SqlHelperModel.DropDataBaseHelper(databaseName);
            MainDatabaseList = SqlMenuModel.InitializeData();
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
                if (string.IsNullOrEmpty(chooseFileDialog.FileName))
                {
                    MessageBox.Show("你还未选定备份文件！", "提示");
                    return;
                }
                filePath = chooseFileDialog.FileName;
                databaseName = Path.GetFileNameWithoutExtension(filePath);
                SqlHelperModel.ImportDataBaseHelper(databaseName,filePath);
                MainDatabaseList = SqlMenuModel.InitializeData();
            }
        }

        private void OpenTableExecuted(string[] tableFullName)
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
                IsTabFoldOpened = true;
                OpenedTableFoldedList = new List<OpenedTablesModel>()
                {
                    OpenedTableList[5]
                };
                OpenedTableList[5] = new OpenedTablesModel(tableFullName);
                SetElseTabsFalse(tableFullName);
                OpenedTableList = new List<OpenedTablesModel>(OpenedTableList);
            }
            else if (OpenedTableList.Count == 6 && OpenedTableFoldedList != null && !IsThisTableOpenedInFolder(tableFullName) && !IsThisTableOpendInTab(tableFullName))
            {
                IsTabFoldOpened = true;
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
                IsTabFoldOpened = true;
                OpenedTablesModel tempModel = OpenedTableList[5];
                OpenedTableList[5] = new OpenedTablesModel(tableFullName);
                OpenedTableList = new List<OpenedTablesModel>(OpenedTableList);
                SetElseTabsFalse(tableFullName);
                OpenedTablesModel tableForDelete = new OpenedTablesModel();
                foreach (var table in OpenedTableFoldedList)
                {
                    if (table.TableFullName == tableFullName)
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

            if (OpenedTableFoldedList != null)
            {
                if (OpenedTableFoldedList.Count != 0)
                {
                    foreach (var table in OpenedTableFoldedList)
                    {
                        table.IsChoosed = false;
                    }
                }
            }
            GetTableData(tableFullName);
        }

        private void RefreshExecuted()
        {
            MainDatabaseList = SqlMenuModel.InitializeData();
        }

        private void CloseTabExecuted(string[] tableFullName)
        {
            OpenedTablesModel deleteModel = new OpenedTablesModel();
            foreach (var table in OpenedTableList)
            {
                if (table.TableFullName == tableFullName)
                {
                    deleteModel = table;
                }
            }
            OpenedTableList.Remove(deleteModel);
            if (OpenedTableList.Count == 0)
            {
                IsDataGridOpened = false;
                IsAllSelected = false;
            }
            if (OpenedTableList.Count == 5 && OpenedTableFoldedList != null)
            {
                if (OpenedTableFoldedList.Count != 0)
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
                        OpenedTableFoldedList = new List<OpenedTablesModel>();
                        IsTabFoldOpened = false;
                    }
                }
            }

            if (deleteModel.IsChoosed == true)
            {
                if (OpenedTableList.Count != 0)
                {
                    OpenedTableList[0].IsChoosed = true;
                    SetElseTabsFalse(OpenedTableList[0].TableFullName);
                    GetTableData(OpenedTableList[0].TableFullName);
                }
                else
                {
                    TableData = null;
                }
            }
            OpenedTableList = new List<OpenedTablesModel>(OpenedTableList);

        }

        private void CloseFoldTabExecuted(string[] tableFullName)
        {
            OpenedTablesModel deleteModel = new OpenedTablesModel();
            foreach (var table in OpenedTableFoldedList)
            {
                if (table.TableFullName == tableFullName)
                {
                    deleteModel = table;
                }
            }

            OpenedTableFoldedList.Remove(deleteModel);
            if (OpenedTableFoldedList.Count == 0)
            {
                IsTabFoldOpened = false;
            }
            OpenedTableFoldedList = new List<OpenedTablesModel>(OpenedTableFoldedList);
        }

        private void ApplyUpdateExecuted()
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

        private void ClickTabExecuted(string[] tableFullName)
        {
            OpenedTablesModel clickedModel = new OpenedTablesModel();
            foreach (var table in OpenedTableList)
            {
                if (table.TableFullName == tableFullName)
                {
                    table.IsChoosed = true;
                    SetElseTabsFalse(tableFullName);
                    GetTableData(tableFullName);
                }
            }
        }

        private void ClickFoldExecuted(string[] tableFullName)
        {
            OpenedTablesModel tempModel = new OpenedTablesModel();
            foreach (var table in OpenedTableFoldedList)
            {
                if (table.TableFullName == tableFullName)
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

        private void CloseOtherTabsExecuted(string[] tableFullName)
        {
            OpenedTablesModel oneTabModel = new OpenedTablesModel();
            foreach (var table in OpenedTableList)
            {
                if (table.TableFullName == tableFullName)
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

        private void SelectAllExecuted(DataGrid dataGrid)
        {
            if (IsAllSelected)
            {
                for (int i = 0; i < dataGrid.Items.Count; i++)
                {
                    DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(i);
                    if (row != null)
                    {
                        row.IsSelected = true;

                    }
                }
                IsAllSelected = true;
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
                IsAllSelected = false;
            }
        }

        private void CheckForSelectAllExecuted(DataGrid dataGrid)
        {
            if (IsAllSelected)
            {
                IsAllSelected = false;
            }
        }

        #endregion

        #region Helper Functions in Executed Functions

        private bool IsThisTableOpendInTab(string[] tableFullName)
        {
            foreach (var table in OpenedTableList)
            {
                if (table.TableFullName == tableFullName)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsThisTableOpenedInFolder(string[] tableFullName)
        {
            foreach (var table in OpenedTableFoldedList)
            {
                if (table.TableFullName == tableFullName)
                {
                    return true;
                }
            }
            return false;
        }

        private void SetElseTabsFalse(string[] tableFullName)
        {
            if (OpenedTableList != null)
            {
                foreach (var table in OpenedTableList)
                {
                    if (table.IsChoosed && table.TableFullName != tableFullName)
                    {
                        table.IsChoosed = false;
                    }

                    if (table.TableFullName == tableFullName)
                    {
                        table.IsChoosed = true;
                    }
                }
            }
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