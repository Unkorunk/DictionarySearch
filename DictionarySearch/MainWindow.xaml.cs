using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace DictionarySearch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly SearchWorker _searchWorker = new SearchWorker();
        private BackgroundWorker _backgroundWorker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();

            StartWorker();
        }

        private void DoWork(object sender, DoWorkEventArgs args) => Thread.Sleep(1000 / 60);

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            lock (_searchWorker.ResultLock)
            {
                if (_searchWorker.IsFresh) OutputListView.Items.Clear();
                _searchWorker.IsFresh = false;
                for (var i = 0; i < 1000 && _searchWorker.Result.Count != 0; i++)
                {
                    OutputListView.Items.Add(_searchWorker.Result.Dequeue());
                }

                Status.Content = OutputListView.Items.Count == 0
                    ? "По вашему запросу ничего не нашлось"
                    : $"Нашлось {OutputListView.Items.Count} результатов";
            }

            StartWorker();
        }

        private void StartWorker()
        {
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += DoWork;
            _backgroundWorker.RunWorkerCompleted += RunWorkerCompleted;
            _backgroundWorker.RunWorkerAsync();
        }

        private void UpdateInterface()
        {
            _searchWorker.UpdateSearchString(InputTextBox.Text, IsSeqSearch.IsChecked == true);
        }

        private void InputTextBox_OnTextChanged(object sender, TextChangedEventArgs e) => UpdateInterface();
        private void IsSeqSearch_OnEvent(object sender, RoutedEventArgs e) => UpdateInterface();
    }
}