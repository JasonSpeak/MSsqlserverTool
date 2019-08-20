using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace MSsqlTool.Model
{
    public class OpenedTablesModel:ObservableObject
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

        private bool _isChoosed;

        public bool IsChoosed
        {
            get => _isChoosed;
            set
            {
                _isChoosed = value;
                RaisePropertyChanged(()=>IsChoosed);
            }
        }

        private string[] _tableFullName;

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

        public OpenedTablesModel(string name)
        {
            TableName = name;
            IsChoosed = true;
        }

        public OpenedTablesModel(string[] tableFullName)
        {
            TableFullName = tableFullName;
            TableName = tableFullName[0] + "." + tableFullName[1];
            IsChoosed = false;
        }

    }
}
