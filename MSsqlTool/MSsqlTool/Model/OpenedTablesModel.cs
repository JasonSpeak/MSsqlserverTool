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
    }
}
