using System.Windows;
namespace Yakupova_Skachki
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Role.Text = Login.IsAdmin ? "Администратор" : Login.CanRecordResults ? "Секретарь заездов" : "Зритель";
            MainFrame.Navigate(new HorsesPage());
            Manager.MainFrame = MainFrame;
        }
        private void OpenJournal(object sender, RoutedEventArgs e)
        {
            new Journal() { Owner = this }.ShowDialog();
            MainFrame.Navigate(new HorsesPage());
        }
        private void OpenOwners(object sender, RoutedEventArgs e)
        {
            new DirectoryWindow(DirectoryKind.Owners) { Owner = this }.ShowDialog();
        }
        private void OpenJockeys(object sender, RoutedEventArgs e)
        {
            new DirectoryWindow(DirectoryKind.Jockeys) { Owner = this }.ShowDialog();
        }
        private void OpenCompetitions(object sender, RoutedEventArgs e)
        {
            new DirectoryWindow(DirectoryKind.Competitions) { Owner = this }.ShowDialog();
        }
    }
}
