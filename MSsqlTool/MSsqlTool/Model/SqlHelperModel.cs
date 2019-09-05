using NLog;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace MSsqlTool.Model
{
    public static class SqlHelperModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string ConnectString =
            ConfigurationManager.ConnectionStrings["ConnectString"].ToString();
        private static readonly SqlConnection MasterConn= new SqlConnection(ConnectString);

        public static bool CanMasterConnAvailable()
        {
            try
            {
                MasterConn.Open();
                return true;
            }
            catch (SqlException e)
            {
                Logger.Error(e.Message);
                return false;
            }
            finally
            {
                MasterConn.Close();
            }
        }

        public static void ExportDataBaseHelper(string dataBaseName,string exportFileLocation)
        {
            if(string.IsNullOrEmpty(dataBaseName))
                throw new ArgumentException(@"dataBaseName should not be empty",nameof(dataBaseName));
            if(string.IsNullOrEmpty(exportFileLocation))
                throw new ArgumentException(@"exportFileLocation should not be empty",nameof(exportFileLocation));

            MasterConn.Open();
            var exportScript =
                $"BACKUP DATABASE [{dataBaseName}] TO DISK='{exportFileLocation}\\{dataBaseName}.bak'";
            var exportCommand = new SqlCommand(exportScript, MasterConn);
            exportCommand.ExecuteNonQuery();
            exportCommand.Dispose();
            MasterConn.Close();
        }

        public static void DropDataBaseHelper(string dataBaseName)
        {
            if(string.IsNullOrEmpty(dataBaseName))
                throw new ArgumentException(@"databaseName should not be empty",nameof(dataBaseName));

            MasterConn.Open();
            var dropScript =
                $"USE MASTER;ALTER DATABASE [{dataBaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [{dataBaseName}];";
            var dropCommand = new SqlCommand(dropScript, MasterConn);
            dropCommand.ExecuteNonQuery();
            dropCommand.Dispose();
            MasterConn.Close();
        }

        public static void ImportDataBaseHelper(string filePath)
        {
            if(string.IsNullOrEmpty(filePath))
                throw new ArgumentException(@"filePath should not be empty",nameof(filePath));

            var logicName = GetLogicNameFromBak(filePath);
            PrepareForImport(logicName);
            ImportDataBase( logicName,filePath,MasterConn);
        }

        public static DataTable GetTableDataHelper(TableFullNameModel tableFullName,out SqlDataAdapter dataAdapterForUpdate)
        {
            if (string.IsNullOrEmpty(tableFullName.DataBaseName) || string.IsNullOrEmpty(tableFullName.TableName))
            {
                dataAdapterForUpdate = null;
                throw new ArgumentException(@"tableFullName should not be empty",nameof(tableFullName));
            }

            var databaseName = tableFullName.DataBaseName;
            var tableName = tableFullName.TableName;
            var connection = new SqlConnection(GetDifferentConnectionWithName(databaseName));
            connection.Open();
            var selectAllScript = $"SELECT * FROM [{tableName}]";
            dataAdapterForUpdate = new SqlDataAdapter(selectAllScript, connection);
            var dummy = new SqlCommandBuilder(dataAdapterForUpdate);
            var dataTableForUpdate = new DataTable();
            dataAdapterForUpdate.Fill(dataTableForUpdate);
            connection.Close();
            return dataTableForUpdate;
        }

        public static bool ApplyUpdateHelper(SqlDataAdapter dataAdapterForUpdate,DataTable dataTableForUpdate)
        {
            if(dataAdapterForUpdate == null)
                throw new ArgumentException(@"dataAdapterForUpdate should not be empty",nameof(dataAdapterForUpdate));
            if(dataTableForUpdate == null)
                throw new ArgumentException(@"dataTableForUpdate should not be empty",nameof(dataTableForUpdate));

            try
            {
                dataAdapterForUpdate.Update(dataTableForUpdate);
                return true;
            }
            catch (SqlException e)
            {
                Logger.Error(e.Message);
                return false;
            }
        }

        public static string GetLogicNameFromBak(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException(@"filePath should not be empty",nameof(filePath));

            MasterConn.Open();
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
            var getLogicNameAdapter = new SqlDataAdapter(getLogicNameScript, MasterConn);
            var logicNameTable = new DataTable();
            getLogicNameAdapter.Fill(logicNameTable);
            var logicName = logicNameTable.Rows[0]["LogicalName"].ToString();
            MasterConn.Close();
            getLogicNameAdapter.Dispose();
            
            return logicName;
        }

        public static bool IsLocalExistThisDataBase(string logicName)
        {
            if (string.IsNullOrEmpty(logicName))
                throw new ArgumentException(@"logicName should not be empty",nameof(logicName));

            MasterConn.Open();
            var getDataBaseScript = "SELECT NAME FROM SYSDATABASES";
            var getDataBaseAdapter = new SqlDataAdapter(getDataBaseScript, MasterConn);
            var databaseTable = new DataTable();
            getDataBaseAdapter.Fill(databaseTable);
            getDataBaseAdapter.Dispose();
            MasterConn.Close();

            return databaseTable.Rows.Cast<DataRow>().Any(row => row["name"].ToString() == logicName);
        }

        public static string GetDifferentConnectionWithName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(@"name should not be empty",nameof(name));

            return $"data source=.\\SQLEXPRESS;initial catalog={name};integrated security=True;App=EntityFramework";
        }

        public static void Close()
        {
            MasterConn.Dispose();
        }

        private static void PrepareForImport(string logicName)
        {
            MasterConn.Open();
            var getDataBaseScript = "SELECT NAME FROM SYSDATABASES";
            var getDataBaseAdapter = new SqlDataAdapter(getDataBaseScript, MasterConn);
            var databaseTable = new DataTable();
            getDataBaseAdapter.Fill(databaseTable);
            MasterConn.Close();
            getDataBaseAdapter.Dispose();
            if (IsLogicDatabaseNameExisted(databaseTable,logicName))
            {
                DropDataBaseHelper(logicName);
            }
        }

        private static void ImportDataBase(string logicName, string filePath,SqlConnection MasterConn)
        {
            MasterConn.Open();
            var importScript = $"RESTORE DATABASE [{logicName}] FROM DISK='{filePath}'";
            var importCommand = new SqlCommand(importScript, MasterConn);
            importCommand.ExecuteNonQuery();
            importCommand.Dispose();
            MasterConn.Close();
        }

        private static bool IsLogicDatabaseNameExisted(DataTable databaseTable,string logicName)
        {
            return databaseTable.Rows.Cast<DataRow>().Any(row => row["name"].ToString() == logicName);
        }
    }
}
