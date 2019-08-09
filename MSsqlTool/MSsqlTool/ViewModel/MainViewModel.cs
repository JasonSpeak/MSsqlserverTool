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
using NLog;


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

        private List<SqlMenuModel> _dataBaseForMenu;


        public List<SqlMenuModel> DataBaseForMenu
        {
            get { return _dataBaseForMenu; }
            set { _dataBaseForMenu = value; }
        }

        public MainViewModel()
        {
            InitializeData();
        }


        //private List<string> DataBasesWithoutSysDataBase
        //{
        //    get
        //    {
        //        List<string> databases=new List<string>();
        //        foreach (DataRow row in _dataBaseTable.Rows)
        //        {
        //            if (!Enum.IsDefined(typeof(SysDataBases), row["name"]))
        //            {
        //                databases.Add(row["name"].ToString());
        //            }
        //        }
        //        return databases;
        //    }
        //}

        private void InitializeData()
        {
            try
            {
                DataTable dataBaseTable = new DataTable();
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

        }
    }
}