using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace MSsqlTool.Model
{
    public class BoolModel:ObservableObject
    {
        private bool _isChecked;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                RaisePropertyChanged(()=>IsChecked);
            }
        }
    }
}
