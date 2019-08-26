using GalaSoft.MvvmLight;

namespace MSsqlTool.Model
{
    public class OpenedTablesModel:ObservableObject
    {
        private string _tableName;
        private string[] _tableFullName;
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


        public string[] TableFullName
        {
            get => _tableFullName;
            set
            {
                _tableFullName = value;
                RaisePropertyChanged(()=>TableFullName);
            }
        }

        public OpenedTablesModel()
        {

        }

        public OpenedTablesModel(string[] tableFullName)
        {
            TableFullName = tableFullName;
            TableName = tableFullName[0] + "." + tableFullName[1];
            _isChoosed = false;
        }

    }
}
