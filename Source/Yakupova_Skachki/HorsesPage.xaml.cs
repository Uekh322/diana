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

namespace Yakupova_Skachki
{
    /// <summary>
    /// Логика взаимодействия для HorsesPage.xaml
    /// </summary>
    public partial class HorsesPage : Page
    {
      
        public Visibility EditVisibility => Login.IsAdmin ? Visibility.Visible : Visibility.Collapsed;

        public HorsesPage()
        {
            InitializeComponent();
            Actions.Visibility = EditVisibility;
            var currentHorses = Yakupova_Skachki3Entities.GetContext().Horses.ToList();
            HorsesListView.ItemsSource = currentHorses;
            ComboCateghory.SelectedIndex = 0;
            ComboBoh.SelectedIndex = 0;
            UpdateHorses();
        }

        private void UpdateHorses()
        {
            var currentHorses = Yakupova_Skachki3Entities.GetContext().Horses.ToList();

            if (!string.IsNullOrEmpty(TBSearch.Text))
            {
                string searchText = TBSearch.Text.ToLower().Replace('ё', 'е');

                currentHorses = currentHorses.Where(h =>
                {
                    bool nameMatch = h.Name != null && h.Name.ToLower().Replace('ё', 'е').Contains(searchText);
                    bool ownerMatch = h.OwnerFullName != null && h.OwnerFullName.ToLower().Replace('ё', 'е').Contains(searchText);
                    bool jockeyMatch = h.JockeysList != null && h.JockeysList.ToLower().Replace('ё', 'е').Contains(searchText);
                    bool compMatch = h.LastCompetition != null && h.LastCompetition.ToLower().Replace('ё', 'е').Contains(searchText);

                    return nameMatch || ownerMatch || jockeyMatch || compMatch;
                }).ToList();
            }

            if (ComboBoh.SelectedIndex > 0)
            {
                string sortType = (ComboBoh.SelectedItem as TextBlock).Text;

                switch (sortType)
                {
                    case "Кличка от А до Я":
                        currentHorses = currentHorses.OrderBy(h => h.Name).ToList();
                        break;
                    case "Кличка от Я до А":
                        currentHorses = currentHorses.OrderByDescending(h => h.Name).ToList();
                        break;
                    case "По возрасту младше":
                        currentHorses = currentHorses.OrderBy(h => h.Age).ToList();
                        break;
                    case "По возрасту старше":
                        currentHorses = currentHorses.OrderByDescending(h => h.Age).ToList();
                        break;
                }
            }

            if (ComboCateghory.SelectedIndex == 1)
            {
                currentHorses = currentHorses.Where(h => h.Gender == "кобыла").ToList();
            }
            if (ComboCateghory.SelectedIndex == 2)
            {
                currentHorses = currentHorses.Where(h => h.Gender == "жеребец").ToList();
            }
            if (ComboCateghory.SelectedIndex == 3)
            {
                currentHorses = currentHorses.Where(h => h.Gender == "мерин").ToList();
            }


            HorsesListView.ItemsSource = currentHorses;
            TBRecordCount.Text = $"Показано записей: {currentHorses.Count}";
        }
        private void TBSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateHorses();
        }

        private void ComboCateghory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateHorses();
        }

        private void ComboBoh_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateHorses();
        }


        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!Login.IsAdmin) return;
            var addWindow = new AddEditHorseWindow(null) { Owner = Window.GetWindow(this) };
            if (addWindow.ShowDialog() == true)
            {
                UpdateHorses(); 
            }
        }

        
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!Login.IsAdmin) return;
            
            var horse = (sender as Button).Tag as Horses;
            if (horse == null) return;

            var editWindow = new AddEditHorseWindow(horse) { Owner = Window.GetWindow(this) };
            if (editWindow.ShowDialog() == true)
            {
                UpdateHorses();
            }
        }

        
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!Login.IsAdmin) return;
            var horse = (sender as Button).Tag as Horses;
            if (horse == null) return;

            // Запрещаем удалять лошадь, если она участвовала в заездах
            if (horse.RaceEntries != null && horse.RaceEntries.Any())
            {
                MessageBox.Show($"Нельзя удалить лошадь «{horse.Name}», так как она участвовала в заездах!",
                                "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

         
            var result = MessageBox.Show($"Вы уверены, что хотите удалить лошадь «{horse.Name}»?",
                                         "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                var context = Yakupova_Skachki3Entities.GetContext();
                
                context.Horses.Attach(horse);
                context.Horses.Remove(horse);
                context.SaveChanges();

                UpdateHorses(); 
            }
        }

        private void BtnAddZaezd_Click(object sender, RoutedEventArgs e)
        {
            if (!Login.IsAdmin) return;
            var addWindow = new AddEditRaceWindow(null) { Owner = Window.GetWindow(this) };
            if (addWindow.ShowDialog() == true)
            {
                MessageBox.Show("Заезд добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateHorses();
            }
        }

        private void BtnAddVlad_Click(object sender, RoutedEventArgs e)
        {
            if (!Login.IsAdmin) return;
            var addWindow = new AddEditOwnerWindow(null) { Owner = Window.GetWindow(this) };
            if (addWindow.ShowDialog() == true)
            {
                MessageBox.Show("Владелец добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateHorses();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var horse = (sender as Button)?.Tag as Horses;
            if (horse == null) return;
            new Journal(horse) { Owner = Window.GetWindow(this) }.ShowDialog();
            UpdateHorses();
        }
    }
}
