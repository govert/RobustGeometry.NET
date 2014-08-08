
using HelixToolkit.Wpf;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<int> selectedItemIndexes;
        List<int>[] faces;
        List<Point3D> points;
        bool canShowObj = true;
        public MainWindow()
        {
            InitializeComponent();
            MainViewModel mv = new MainViewModel();
            mv.AddGroup(true);
            this.DataContext = mv;
            pointList.ItemsSource = mv.Points;
            points = mv.Points;
            faces = mv.Faces;
            
        }

        private void pointList_Selected(object sender, RoutedEventArgs e)
        {
            
            ListBoxItem lbi = ((sender as ListBox).SelectedItems as ListBoxItem);
            selectedItemIndexes = new List<int>();
            foreach (object o in (sender as ListBox).SelectedItems)
            {
                selectedItemIndexes.Add(pointList.Items.IndexOf(o));
            }

            Display(canShowObj);
        }

        private void Display(bool canShowObj)
        {
            MainViewModel mv = new MainViewModel();
            mv.AddGroup(canShowObj);
            if (selectedItemIndexes != null)
            {
                mv.AddGroup(selectedItemIndexes);
            }
            this.DataContext = mv;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton selected = (RadioButton)sender;
            canShowObj = false;
            if (selected.Name=="showObj")
            {
                canShowObj = true;
            }
            Display(canShowObj);
        }
        private void showWireframe_Checked(object sender, RoutedEventArgs e)
        {
            foreach (List<int> pts in this.faces)
            {
                if (pts != null)
                {
                    int index = 0;
                    for (index = 0; index < pts.Count - 1; index++)
                    {
                        LinesVisual3D line = new LinesVisual3D();
                        line.Color = Colors.Gold;
                        line.Thickness = 1;
                        line.Points.Add(this.points[pts[index]]);
                        line.Points.Add(this.points[pts[index + 1]]);
                        viewport.Children.Add(line);
                    }
                    LinesVisual3D line2 = new LinesVisual3D();
                    line2.Color = Colors.Gold;
                    line2.Thickness = 1;
                    line2.Points.Add(this.points[pts[index++]]);
                    line2.Points.Add(this.points[pts[0]]);
                    viewport.Children.Add(line2);
                }

            }
        }

        private void showWireframe_Unchecked(object sender, RoutedEventArgs e)
        {
            Display(canShowObj);
        }
    }
}
