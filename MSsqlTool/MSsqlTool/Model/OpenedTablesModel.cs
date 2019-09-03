using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;

namespace MSsqlTool.Model
{
    public class OpenedTablesModel:ObservableObject
    {
        private string _tableName;
        private TableFullNameModel _tableFullName;
        private bool _isChoosed;

        public string TableName
        {
            get => _tableName;
            set
            {
                _tableName = value;
                RaisePropertyChanged(()=>TableName);
            }
        }


        public bool IsChoosed
        {
            get => _isChoosed;
            set
            {
                _isChoosed = value;
                RaisePropertyChanged(()=>IsChoosed);
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

        public OpenedTablesModel(TableFullNameModel tableFullName)
        {
            TableFullName = tableFullName;
            TableName = tableFullName.GetFormattedName();
            _isChoosed = false;
        }

        public static void SetElseTabsFalse(ObservableCollection<OpenedTablesModel> openedTabs,TableFullNameModel tableFullName)
        {
            foreach (var tab in openedTabs)
            {
                tab.IsChoosed = (tab.TableFullName == tableFullName);
            }
        }

        public static void DeleteTabWithDataBaseName(ObservableCollection<OpenedTablesModel> tabs, string dataBaseName)
        {
            var deleteTabs = tabs.Where(tab => tab.TableFullName.DataBaseName == dataBaseName).ToList();
            foreach (var tab in deleteTabs)
            {
                tabs.Remove(tab);
            }
        }

        public static void SetAllTabsFalse(ObservableCollection<OpenedTablesModel> tabs)
        {
            foreach (var tab in tabs)
            {
                tab.IsChoosed = false;
            }
        }

    }
}
