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

namespace MatchListViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly RoutedUICommand NextScreenCommand = new RoutedUICommand("Move window to the next screen.", "NextScreenCommand", typeof(MainWindow));

        public MainWindow()
        {
            InitializeComponent();
        }

        public void RefreshCsvData(string filename)
        {

        }

        #region Event Handlers

        private void CloseWindow_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void MaximizeWindow_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        private void NextScreen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var screens = System.Windows.Forms.Screen.AllScreens.ToList();
            if (screens.Count == 1)
                return;

            var currentPosition = new System.Drawing.Point((int)this.Left, (int)this.Top);
            var currentScreen = System.Windows.Forms.Screen.FromPoint(currentPosition);
            var relativeScreenPosition = new System.Drawing.Point(
                currentPosition.X - currentScreen.Bounds.Left,
                currentPosition.Y - currentScreen.Bounds.Top);


            var nextScreenIndex = screens.IndexOf(currentScreen) + 1;
            if (nextScreenIndex == screens.Count)
                nextScreenIndex = 0;
            currentScreen = screens[nextScreenIndex];

            this.Left = relativeScreenPosition.X + currentScreen.Bounds.Left;
            this.Top = relativeScreenPosition.Y + currentScreen.Bounds.Top;
        }
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowStyle = WindowStyle.None;
            else
                WindowStyle = WindowStyle.SingleBorderWindow;
        }

        #endregion
    }
}
