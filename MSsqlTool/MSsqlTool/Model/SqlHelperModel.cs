using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace MSsqlTool.Model
{
    public static class SqlHelperModel
    {
        private static readonly string ConnectString =
            ConfigurationManager.ConnectionStrings["ConnectString"].ToString();

        public static void ExportDataBaseHelper(string dataBaseName,string exportFileLocation)
        {
            if(string.IsNullOrEmpty(dataBaseName) || string.IsNullOrEmpty(exportFileLocation)) return;

            var exportDbConnectionString = SqlMenuModel.GetDifferentConnectionWithName(dataBaseName);
            SqlConnection exportDbConnection;
            using (exportDbConnection = new SqlConnection(exportDbConnectionString))
            {
                exportDbConnection.Open();
                var exportDbString = string.Format("BACKUP DATABASE [{0}] TO DISK='{1}\\{0}.bak'", dataBaseName, exportFileLocation);
                var exportCommand = new SqlCommand(exportDbString, exportDbConnection);
                exportCommand.ExecuteNonQuery();
                exportCommand.Dispose();
                exportDbConnection.Close();
            }
        }

        public static void DropDataBaseHelper(string dataBaseName)
        {
            if(string.IsNullOrEmpty(dataBaseName)) return;
            using (var dropConn = new SqlConnection(ConnectString))
            {
                dropConn.Open();
                var dropCommand =
                    new SqlCommand(
                        $"USE MASTER;ALTER DATABASE [{dataBaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [{dataBaseName}];",
                        dropConn);
                dropCommand.ExecuteNonQuery();
                dropCommand.Dispose();
                dropConn.Close();
            }
        }

        public static void ImportDataBaseHelper(string filePath)
        {
            if(string.IsNullOrEmpty(filePath)) return;
            var logicName = GetLogicNameFromBak(filePath);
            PrepareForImport(logicName);
            ImportDataBase( logicName,filePath);
        }

        public static DataTable GetTableDataHelper(TableFullNameModel tableFullName,out SqlDataAdapter dataAdapterForUpdate)
        {
            if (string.IsNullOrEmpty(tableFullName.DataBaseName) || string.IsNullOrEmpty(tableFullName.TableName))
            {
                dataAdapterForUpdate = null;
                return null;
            }
            var databaseName = tableFullName.DataBaseName;
            var tableName = tableFullName.TableName;
            var dataTableForUpdate = new DataTable();
            var connection = new SqlConnection(SqlMenuModel.GetDifferentConnectionWithName(databaseName));
            connection.Open();
            var selectAll = $"SELECT * FROM [{tableName}]";
            dataAdapterForUpdate = new SqlDataAdapter(selectAll, connection);
            var dummy = new SqlCommandBuilder(dataAdapterForUpdate);
            dataAdapterForUpdate.Fill(dataTableForUpdate);
            connection.Close();
            return dataTableForUpdate;
        }

        public static void ApplyUpdateHelper(SqlDataAdapter dataAdapterForUpdate,DataTable dataTableForUpdate)
        {
            if(dataAdapterForUpdate == null || dataTableForUpdate == null) return;
            dataAdapterForUpdate.Update(dataTableForUpdate);
        }

        public static string GetLogicNameFromBak(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return null;
            using (var conn = new SqlConnection(ConnectString))
            {
                conn.Open();
                var getLogicNameScript =
                    "DECLARE @Table TABLE (LogicalName varchar(128),[PhysicalName] varchar(128), [Type] varchar, " +
                    "[FileGroupName] varchar(128), [Size] varchar(128), [MaxSize] varchar(128), [FileId] varchar(128)," +
                    "[CreateLSN] varchar(128), [DropLSN] varchar(128), [UniqueId] varchar(128), [ReadOnlyLSN] varchar(128), " +
                    "[ReadWriteLSN] varchar(128),[BackupSizeInBytes] varchar(128), [SourceBlockSize] varchar(128), " +
                    "[FileGroupId] varchar(128), [LogGroupGUID] varchar(128), [DifferentialBaseLSN] varchar(128), " +
                    "[DifferentialBaseGUID] varchar(128), [IsReadOnly] varchar(128), [IsPresent] varchar(128), [TDEThumbprint] varchar(128))" +
                    $"DECLARE @Path varchar(1000)='{filePath}'" +
                    "DECLARE @LogicalNameData varchar(128),@LogicalNameLog varchar(128)" +
                    "INSERT INTO @table EXEC('RESTORE FILELISTONLY FROM DISK = ''' +@Path+ '''')" +
                    "SET @LogicalNameData = (SELECT LogicalName FROM @Table WHERE Type= 'D')" +
                    "SET @LogicalNameLog = (SELECT LogicalName FROM @Table WHERE Type='L')" +
                    "SELECT @LogicalNameData AS [LogicalName]";
                var getLogicNameAdapter = new SqlDataAdapter(getLogicNameScript, conn);
                var logicNameTable = new DataTable();
                getLogicNameAdapter.Fill(logicNameTable);
                var logicName = logicNameTable.Rows[0]["LogicalName"].ToString();
                conn.Close();
                getLogicNameAdapter.Dispose();
                return logicName;
            }
        }

        public static bool IsLocalExistThisDataBase(string logicName)
        {
            if (string.IsNullOrEmpty(logicName))
                return false;
            using (var dropConn = new SqlConnection(ConnectString))
            {
                dropConn.Open();
                const string getDataBaseScript = "SELECT NAME FROM SYSDATABASES";
                var getDataBaseAdapter = new SqlDataAdapter(getDataBaseScript, dropConn);
                var databaseTable = new DataTable();
                getDataBaseAdapter.Fill(databaseTable);
                getDataBaseAdapter.Dispose();
                dropConn.Close();
                if (databaseTable.Rows
                    .Cast<DataRow>().Any(row => row["name"].ToString() == logicName))
                {
                    return true;
                }
            }

            return false;
        }

        private static void PrepareForImport(string logicName)
        {
            if(string.IsNullOrEmpty(logicName)) return;
            using (var dropConn = new SqlConnection(ConnectString))
            {
                dropConn.Open();
                const string getDataBaseString = "SELECT NAME FROM SYSDATABASES";
                var getDataBaseAdapter = new SqlDataAdapter(getDataBaseString, dropConn);
                var databaseTable = new DataTable();
                getDataBaseAdapter.Fill(databaseTable);
                dropConn.Close();
                getDataBaseAdapter.Dispose();

                if (IsLogicDatabaseNameExisted(databaseTable,logicName))
                {
                    DropDataBaseHelper(logicName);
                }
            }
        }

        private static void ImportDataBase(string logicName, string filePath)
        {
            if(string.IsNullOrEmpty(logicName) || string.IsNullOrEmpty(filePath)) return;

            using (var importConn = new SqlConnection(ConnectString))
            {
                importConn.Open();
                var importScript = $"RESTORE DATABASE [{logicName}] FROM DISK='{filePath}';";
                var importCommand = new SqlCommand(importScript, importConn);
                importCommand.ExecuteNonQuery();
                importCommand.Dispose();
                importConn.Close();
            }
        }

        private static bool IsLogicDatabaseNameExisted(DataTable databaseTable,string logicName)
        {
            return databaseTable.Rows.Cast<DataRow>().Any(row => row["name"].ToString() == logicName);
        }
    }
}
