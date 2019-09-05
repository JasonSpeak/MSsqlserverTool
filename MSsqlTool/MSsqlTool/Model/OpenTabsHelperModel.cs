using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MSsqlTool.Model
{
    internal static class OpenTabsHelperModel
    {
        public static void SetElseTabsFalse(this ObservableCollection<OpenedTablesModel> openedTabs, TableFullNameModel tableFullName)
        {
            if (string.IsNullOrEmpty(tableFullName.DataBaseName) || string.IsNullOrEmpty(tableFullName.TableName))
                throw new ArgumentException(@"tableFullName should not be empty", nameof(tableFullName));

            foreach (var tab in openedTabs)
            {
                tab.IsChoosed = (tab.TableFullName == tableFullName);
            }
        }

        public static void DeleteTabWithDataBaseName(this ObservableCollection<OpenedTablesModel> tabs, string dataBaseName)
        {
            if (string.IsNullOrEmpty(dataBaseName))
                throw new ArgumentException(@"dataBaseName should not be empty", nameof(dataBaseName));

            var deleteTabs = tabs.Where(tab => tab.TableFullName.DataBaseName == dataBaseName).ToList();
            foreach (var tab in deleteTabs)
            {
                tabs.Remove(tab);
            }
        }

        public static void SetAllTabsFalse(this ObservableCollection<OpenedTablesModel> tabs)
        {
            foreach (var tab in tabs)
            {
                tab.IsChoosed = false;
            }
        }

        public static bool IsThisTabOpened(this ObservableCollection<OpenedTablesModel> openedTabs,TableFullNameModel tableFullName)
        {
            if (string.IsNullOrEmpty(tableFullName.DataBaseName) || string.IsNullOrEmpty(tableFullName.TableName))
                throw new ArgumentException(@"tableFullName should not be empty", nameof(tableFullName));

            return openedTabs.Any(table => table.TableFullName == tableFullName);
        }

    }
}
