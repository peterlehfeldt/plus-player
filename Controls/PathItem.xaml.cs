using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlusPlayer.Controls
{
    /// <summary>
    /// Interaction logic for PathItem.xaml
    /// </summary>
    public partial class PathItem : UserControl
    {
        private string path;
        public string Path
        {
            get { return path; }
        }

        private SettingsPanel settingPanel;

        public PathItem(string _path, SettingsPanel _settingsPanel)
        {
            InitializeComponent();

            path = _path;

            PathName.Content = path;

            settingPanel = _settingsPanel;
        }

        private void Remove_But_Click(object sender, RoutedEventArgs e)
        {
            settingPanel.MusicFolders_SP.Children.Remove(this);
        }
    }
}
