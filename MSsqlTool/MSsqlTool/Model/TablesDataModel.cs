using GalaSoft.MvvmLight;
using System.Data;

namespace MSsqlTool.Model
{
    public class TablesDataModel:ObservableObject
    {
        private string _tableName;
        private string _databaseName;
        private DataTable _dataInTable;

        public string TableName
        {
            get => _tableName;
            set
            {
                _tableName = value;
                RaisePropertyChanged(()=>TableName);
            }
        }

        public string DataBaseName
        {
            get => _databaseName;
            set
            {
                _databaseName = value;
                RaisePropertyChanged(()=>DataBaseName);
            }
        }

        public DataTable DataInTable
        {
            get => _dataInTable;
            set
            {
                _dataInTable = value;
                RaisePropertyChanged(()=>DataInTable);
            }
        }
    }
}
