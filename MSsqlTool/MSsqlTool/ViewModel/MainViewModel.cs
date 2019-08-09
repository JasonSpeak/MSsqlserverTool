using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using MSsqlTool.Model;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using NLog;
using NLog.Fluent;


namespace MSsqlTool.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private static readonly string connectString =
            ConfigurationManager.ConnectionStrings["ConnectString"].ToString();

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private enum SysDataBases
        {
            master,
            model,
            msdb,
            tempdb
        }

        private SqlConnection _connection;

        private List<SqlMenuModel> _mainList;

        public ICommand ItemCommand { get; private set; }

        public List<SqlMenuModel> MainList
        {
            get { return _mainList; }
        }

        public MainViewModel()
        {
            InitializeData();
            ItemCommand = new RelayCommand(ItemSelectedExecuted);
        }

        private void InitializeData()
        {
            DataTable dataBaseTable = new DataTable();
            try
            {
                using (_connection = new SqlConnection(connectString))
                {
                    _connection.Open();
                    string selectDataBasesString = "select name from sysdatabases";
                    SqlDataAdapter dataBaseAdapter = new SqlDataAdapter(selectDataBasesString,_connection);
                    dataBaseTable = new DataTable();
                    dataBaseAdapter.Fill(dataBaseTable);
                    _connection.Close();
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
            List<string> tempDataBaseList = new List<string>();
            foreach (DataRow row in dataBaseTable.Rows)
            {
                if (!Enum.IsDefined(typeof(SysDataBases), row["name"]))
                {
                    tempDataBaseList.Add(row["name"].ToString());
                }
            }
            _mainList = new List<SqlMenuModel>();
            List<SqlMenuModel> DataBasesList = new List<SqlMenuModel>();
            foreach (string name in tempDataBaseList)
            {
                List<SqlMenuModel> tablesList = GetTableList(name);
                SqlMenuModel tempMenuModel = new SqlMenuModel(){Name = name,MenuTables = tablesList,level = 2};
                DataBasesList.Add(tempMenuModel);
            }
            _mainList.Add(new SqlMenuModel(){Name = "Êý¾Ý¿â",MenuTables = DataBasesList,level = 1});
        }

        private List<SqlMenuModel> GetTableList(string databaseName)
        {
            List<SqlMenuModel> tableList = new List<SqlMenuModel>();
            SqlConnection getTableConnection = new SqlConnection();
            DataTable TableNames = new DataTable();
            string GetTableConnString =
                String.Format(
                    "data source=.\\SQLEXPRESS;initial catalog={0};integrated security=True;MultipleActiveResultSets=True;App=EntityFramework",
                    databaseName);
            try
            {
                using (getTableConnection = new SqlConnection(GetTableConnString))
                {
                    getTableConnection.Open();
                    string selectTableString = "select name from sys.tables";
                    SqlDataAdapter tablesNameAdapter = new SqlDataAdapter(selectTableString,getTableConnection);
                    TableNames = new DataTable();
                    tablesNameAdapter.Fill(TableNames);
                    getTableConnection.Close();
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }

            foreach (DataRow row in TableNames.Rows)
            {
                tableList.Add(new SqlMenuModel(row["name"].ToString()){level = 3});
            }

            foreach (var s in tableList)
            {
                logger.Trace(s.Name);
            }
            return tableList;
        }

        private void ItemSelectedExecuted()
        {
            MessageBox.Show("HI");
        }
        
    }
}