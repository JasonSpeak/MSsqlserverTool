﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NLog;

namespace MSsqlTool.Model
{
    public class SqlHelperModel
    {
        private static readonly string ConnectString =
            ConfigurationManager.ConnectionStrings["ConnectString"].ToString(); 

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static SqlConnection _connection;

        private static SqlDataAdapter _dataAdapterForUpdate;

        private static DataTable _dataTableForUpdate;

        private static SqlCommandBuilder _commandBuilderForUpdate;

        private static string[] _currentTable;


        public static void ExportDataBaseHelper(string dataBaseName,string exportFileLocation)
        {
            var exportDbConnectionString = SqlMenuModel.GetDifferentConnectionWithName(dataBaseName);
            try
            {
                SqlConnection exportDbConnection;
                using (exportDbConnection = new SqlConnection(exportDbConnectionString))
                {
                    exportDbConnection.Open();
                    var exportDbString = string.Format("backup database [{0}] to disk='{1}\\{0}.bak'", dataBaseName, exportFileLocation);
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
                            $"use master;alter database [{dataBaseName}] set single_user with rollback immediate;drop database [{dataBaseName}];",
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

        public static void ImportDataBaseHelper(string dataBaseName, string filePath)
        {
            try
            {
                using (var conn = new SqlConnection(ConnectString))
                {
                    PrepareForImport(conn, dataBaseName);
                    ImportDataBase(conn, filePath);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        public static DataTable GetTableDataHelper(string[] tableFullName)
        {
            _currentTable = tableFullName;
            var databaseName = tableFullName[0];
            var tableName = tableFullName[1];
            _connection = new SqlConnection(SqlMenuModel.GetDifferentConnectionWithName(databaseName));
            try
            {
                _connection.Open();
                var selectAll = $"select * from [{tableName}]";
                _dataAdapterForUpdate = new SqlDataAdapter(selectAll, _connection);
                _commandBuilderForUpdate = new SqlCommandBuilder(_dataAdapterForUpdate);
                _dataTableForUpdate = new DataTable();
                _dataAdapterForUpdate.Fill(_dataTableForUpdate);
                _connection.Close();
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            _connection.Close();
            return _dataTableForUpdate;
        }

        public static void ApplyUpdateHelper()
        {
            try
            {
                _connection.Open();
                _dataAdapterForUpdate.Update(_dataTableForUpdate);
                _connection.Close();
                MessageBox.Show("数据修改成功");
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


        private static void PrepareForImport(SqlConnection dropConn, string databaseName)
        {
            try
            {
                dropConn.Open();
                const string getDataBaseString = "select name from sysdatabases";
                var getDataBaseAdapter = new SqlDataAdapter(getDataBaseString, dropConn);
                var databaseTable = new DataTable();
                getDataBaseAdapter.Fill(databaseTable);
                dropConn.Close();
                foreach (DataRow row in databaseTable.Rows)
                {
                    if (row["name"].ToString() == databaseName)
                    {
                        DropDataBaseHelper(databaseName);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        private static void ImportDataBase(SqlConnection importConn, string filePath)
        {
            try
            {
                importConn.Open();
                var importString = "DECLARE @Table TABLE (LogicalName varchar(128),[PhysicalName] varchar(128), [Type] varchar, " +
                                   "[FileGroupName] varchar(128), [Size] varchar(128), [MaxSize] varchar(128), [FileId] varchar(128)," +
                                   "[CreateLSN] varchar(128), [DropLSN] varchar(128), [UniqueId] varchar(128), [ReadOnlyLSN] varchar(128), " +
                                   "[ReadWriteLSN] varchar(128),[BackupSizeInBytes] varchar(128), [SourceBlockSize] varchar(128), " +
                                   "[FileGroupId] varchar(128), [LogGroupGUID] varchar(128), [DifferentialBaseLSN] varchar(128), " +
                                   "[DifferentialBaseGUID] varchar(128), [IsReadOnly] varchar(128), [IsPresent] varchar(128), [TDEThumbprint] varchar(128))"+
                                   $"DECLARE @Path varchar(1000)='{filePath}'" +
                                   "DECLARE @LogicalNameData varchar(128),@LogicalNameLog varchar(128)" +
                                   "INSERT INTO @table EXEC('RESTORE FILELISTONLY FROM DISK = ''' +@Path+ '''')" +
                                   "SET @LogicalNameData = (SELECT LogicalName FROM @Table WHERE Type= 'D')" +
                                   "SET @LogicalNameLog = (SELECT LogicalName FROM @Table WHERE Type='L')" +
                                   $"restore database @LogicalNameData from disk='{filePath}';";
                var importCommand = new SqlCommand(importString, importConn);
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
