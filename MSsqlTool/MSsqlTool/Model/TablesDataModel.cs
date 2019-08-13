using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace MSsqlTool.Model
{
    public class TablesDataModel:ObservableObject
    {
        private string _tableName;

        public string TableName
        {
            get => _tableName;
            set
            {
                _tableName = value;
                RaisePropertyChanged(()=>TableName);
            }
        }

        private string _databaseName;

        public string DataBaseName
        {
            get => _databaseName;
            set
            {
                _databaseName = value;
                RaisePropertyChanged(()=>DataBaseName);
            }
        }

        private DataTable _dataInTable;

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
