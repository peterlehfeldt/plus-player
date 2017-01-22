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
    /// Interaction logic for ImageCheckBox.xaml
    /// </summary>
    public partial class ImageCheckBox : UserControl
    {
        public delegate void BoolParameter(bool _value);
        public BoolParameter CheckUpdated;

        private bool isChecked = false;
        public bool IsChecked
        {
            get { return isChecked; }
            set {
                isChecked = value;

                if (isChecked)
                {
                    Checked.Visibility = Visibility.Visible;
                    Unchecked.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Checked.Visibility = Visibility.Collapsed;
                    Unchecked.Visibility = Visibility.Visible;
                }

                if (CheckUpdated != null)
                    CheckUpdated(isChecked);
            }
        }

        public ImageCheckBox()
        {
            InitializeComponent();
        }

      
        private void Check_Click(object sender, MouseButtonEventArgs e)
        {
            IsChecked = !isChecked;
        }
    }
}
