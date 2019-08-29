using NLog;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;

namespace MSsqlTool.Model
{
    public static class SqlHelperModel
    {
        private static readonly string ConnectString =
            ConfigurationManager.ConnectionStrings["ConnectString"].ToString(); 

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void ExportDataBaseHelper(string dataBaseName,string exportFileLocation)
        {
            var exportDbConnectionString = SqlMenuModel.GetDifferentConnectionWithName(dataBaseName);
            try
            {
                SqlConnection exportDbConnection;
                using (exportDbConnection = new SqlConnection(exportDbConnectionString))
                {
                    exportDbConnection.Open();
                    var exportDbString = string.Format("BACKUP DATABASE [{0}] TO DISK='{1}\\{0}.bak'", dataBaseName, exportFileLocation);
                    SqlCommand exportCommand = new SqlCommand(exportDbString, exportDbConnection);
                    exportCommand.ExecuteNonQuery();
                    exportDbConnection.Close();
                    MessageBox.Show($"数据库 {dataBaseName} 已成功备份到文件夹 {exportFileLocation} 中", "提示");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "导出错误", MessageBoxButton.YesNo, MessageBoxImage.Error);
                Logger.Error(e.Message);
            }
        }

        public static void DropDataBaseHelper(string dataBaseName)
        {
            if (MessageBox.Show($"是否删除本地数据库 {dataBaseName}？", "提醒", MessageBoxButton.YesNo, MessageBoxImage.Warning) !=
                MessageBoxResult.Yes)
                return;
            try
            {
                using (var dropConn = new SqlConnection(ConnectString))
                {
                    dropConn.Open();
                    var dropCommand =
                        new SqlCommand(
                            $"USE MASTER;ALTER DATABASE [{dataBaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [{dataBaseName}];",
                            dropConn);
                    dropCommand.ExecuteNonQuery();
                    MessageBox.Show($"已在本地删除数据库{dataBaseName}", "提醒");
                    dropConn.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("删除出现异常，请查看日志");
                Logger.Error(e.Message);
            }
        }

        public static void ImportDataBaseHelper(string filePath)
        {
            try
            {
                using (var conn = new SqlConnection(ConnectString))
                {
                    var logicName = GetLogicNameFromBak(filePath);
                    PrepareForImport(conn, logicName);
                    ImportDataBase(conn, logicName,filePath);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        public static DataTable GetTableDataHelper(TableFullNameModel tableFullName,ref SqlDataAdapter dataAdapterForUpdate)
        {
            var databaseName = tableFullName.DataBaseName;
            var tableName = tableFullName.TableName;
            var dataTableForUpdate = new DataTable();
            var connection = new SqlConnection(SqlMenuModel.GetDifferentConnectionWithName(databaseName));
            try
            {
                connection.Open();
                var selectAll = $"SELECT * FROM [{tableName}]";
                dataAdapterForUpdate = new SqlDataAdapter(selectAll, connection);
                var dummy = new SqlCommandBuilder(dataAdapterForUpdate);
                dataAdapterForUpdate.Fill(dataTableForUpdate);
                connection.Close();
            }
            catch (Exception e) 
            {
                connection.Close();
                Logger.Error(e.Message);
            }
            return dataTableForUpdate;
        }

        public static void ApplyUpdateHelper(SqlDataAdapter dataAdapterForUpdate,DataTable dataTableForUpdate)
        {
            try
            {
                dataAdapterForUpdate.Update(dataTableForUpdate);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("不返回任何键列信息"))
                {
                    MessageBox.Show("该数据库缺少主键，无法应用修改");
                }
                Logger.Error(e.Message);
            }
        }

        public static string GetLogicNameFromBak(string filePath)
        {
            using (var conn = new SqlConnection(ConnectString))
            {
                try
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
                    return logicName;
                }
                catch (Exception e)
                {
                    conn.Close();
                    Logger.Error(e.Message);
                    return null;
                }
            }
        }

        private static void PrepareForImport(SqlConnection dropConn, string logicName)
        {
            try
            {
                dropConn.Open();
                const string getDataBaseString = "SELECT NAME FROM SYSDATABASES";
                var getDataBaseAdapter = new SqlDataAdapter(getDataBaseString, dropConn);
                var databaseTable = new DataTable();
                getDataBaseAdapter.Fill(databaseTable);
                dropConn.Close();
                foreach (var row in databaseTable.Rows.Cast<DataRow>()
                    .Where(row => row["name"].ToString() == logicName))
                {
                    DropDataBaseHelper(logicName);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        private static void ImportDataBase(SqlConnection importConn,string logicName, string filePath)
        {
            try
            {
                importConn.Open();
                var importScript = $"RESTORE DATABASE [{logicName}] FROM DISK='{filePath}';";
                var importCommand = new SqlCommand(importScript, importConn);
                importCommand.ExecuteNonQuery();
                importConn.Close();
                MessageBox.Show($"导入数据库成功");
            }
            catch (Exception e)
            {
                MessageBox.Show($"导入数据库出错\n{e.Message}");
                Logger.Error(e.Message);
            }
        }
    }
}
