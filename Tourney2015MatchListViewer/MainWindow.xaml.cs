using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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

        private readonly long UpdateIntervalMilliseconds;
        private readonly string InputFilePath;
        private readonly Timer RefreshTimer;
        private readonly ObservableCollection<MatchItemVM> _matchItems;

        public MainWindow()
        {
            InitializeComponent();
            var startupSettings = Properties.Settings.Default;
            UpdateIntervalMilliseconds = startupSettings.UpdateIntervalMilliseconds;
            InputFilePath = startupSettings.MatchListCsvFilePath;

            _matchItems = new ObservableCollection<MatchItemVM>();
            _matchItems.CollectionChanged += MatchItems_CollectionChanged;
            
            RefreshTimer = new Timer();
            RefreshTimer.Interval = UpdateIntervalMilliseconds;
            RefreshTimer.Elapsed += RefreshTimer_Elapsed;
            RefreshTimer.Start();
        }

        private void MatchItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            matchScroller.ScrollToEnd();
        }

        public ObservableCollection<MatchItemVM> MatchItems { get { return _matchItems; } }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RefreshTimer.Stop();
            Dispatcher.Invoke((Action)(() => RefreshCsvData(InputFilePath)));
            RefreshTimer.Start();
        }

        public void RefreshCsvData(string filename)
        {
            errorOverlay.Visibility = Visibility.Collapsed;
            var csvContents = "";
            try
            {
                using (var fileReader = new StreamReader(InputFilePath))
                {
                    csvContents = fileReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                errorText.Text = string.Format(
                    "Unable to access input file.\r\nReason: \"{0}\"",
                    ex.Message);
                errorOverlay.Visibility = Visibility.Visible;
            }

            var fileLines = csvContents.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Skip(1); //skip the header
            foreach (var line in fileLines)
            {
                var columns = line.Split(',');
                var matchItem = ConvertToMatchItem(columns);
                if (matchItem != null)
                {
                    AddOrUpdateMatchItem(matchItem);
                }
            }
        }

        protected int? ToNullableInt(string input)
        {
            int result = 0;
            if(Int32.TryParse(input, out result))
                return result;
            return null;
        }

        protected MatchItemVM ConvertToMatchItem(string[] csvColumns)
        {
            const int Column_MatchID = 0;
            const int Column_RingID = 1;
            const int Column_BlueName = 2;
            const int Column_BlueNextMatchID = 3;
            const int Column_RedName = 4;
            const int Column_RedNextMatchID = 5;
            try
            {
                var matchItem = new MatchItemVM();
                matchItem.MatchID = csvColumns.Length > Column_MatchID ? ToNullableInt(csvColumns[Column_MatchID]) : null;
                matchItem.RingID = csvColumns.Length > Column_RingID ? ToNullableInt(csvColumns[Column_RingID]) : null;
                matchItem.BlueName = csvColumns.Length > Column_BlueName ? csvColumns[Column_BlueName] : string.Empty;
                matchItem.BlueNextMatchID = csvColumns.Length > Column_BlueNextMatchID ? ToNullableInt(csvColumns[Column_BlueNextMatchID]) : null;
                matchItem.RedName = csvColumns.Length > Column_RedName ? csvColumns[Column_RedName] : string.Empty;
                matchItem.RedNextMatchID = csvColumns.Length > Column_RedNextMatchID ? ToNullableInt(csvColumns[Column_RedNextMatchID]) : null;
                return matchItem;
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected void AddOrUpdateMatchItem(MatchItemVM matchItem)
        {
            var matchInList = MatchItems.FirstOrDefault(x => x.MatchID == matchItem.MatchID);
            if (matchInList == null)
            {
                MatchItems.Add(matchItem);
            }
            else
            {
                matchInList.RingID = matchItem.RingID;
                matchInList.BlueName = matchItem.BlueName;
                matchInList.BlueNextMatchID = matchItem.BlueNextMatchID;
                matchInList.RedName = matchItem.RedName;
                matchInList.RedNextMatchID = matchItem.RedNextMatchID;
            }
        }

        #region Event Handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
            RefreshCsvData(InputFilePath);
        }

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
