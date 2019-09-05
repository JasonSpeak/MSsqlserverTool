using GalaSoft.MvvmLight;

namespace MSsqlTool.Model
{
    public class OpenedTablesModel:ObservableObject
    {
        private bool _isChoosed;

        public string TableName { get; }

        public TableFullNameModel TableFullName { get; }

        public bool IsChoosed
        {
            get => _isChoosed;
            set
            {
                _isChoosed = value;
                RaisePropertyChanged(()=>IsChoosed);
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
