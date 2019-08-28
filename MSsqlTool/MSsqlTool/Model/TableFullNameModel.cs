namespace MSsqlTool.Model
{
    public class TableFullNameModel
    {
        public string DataBaseName { get; }
        public string TableName { get; }

        public TableFullNameModel(string dataBaseName, string tableName)
        {
            DataBaseName = dataBaseName;
            TableName = tableName;
        }
    }
}
