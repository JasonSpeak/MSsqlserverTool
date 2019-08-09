using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSsqlTool.Model
{
    public class SqlMenuModel
    {
        public string Name { get; set; }
        public int level { get; set; }
        public List<SqlMenuModel> MenuTables { get; set; }

        public SqlMenuModel()
        {

        }
        public SqlMenuModel(string name)
        {
            Name = name;
        }
    }
}
 