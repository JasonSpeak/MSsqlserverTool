using System.Security.AccessControl;
using System.Windows.Controls;
using System.Windows;
using MSsqlTool.Model;

namespace MSsqlTool.ViewModel
{
    public class ContextMenuDataTemplateSelector:DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            DataTemplate template = null;
            var selectByLevel = item as SqlMenuModel;
            if (selectByLevel != null && selectByLevel.level == 1)
            {
                if (element != null) template = element.FindResource("MainTemplate") as HierarchicalDataTemplate;
            }
            else if (selectByLevel != null && selectByLevel.level == 2)
            {
                if (element != null) template = element.FindResource("DataBaseTemplate") as HierarchicalDataTemplate;
            }
            else if(selectByLevel != null && selectByLevel.level == 3)
            {
                if (element != null) template = element.FindResource("DataTableTemplate") as HierarchicalDataTemplate;
            }

            return template;
        }
    }
}