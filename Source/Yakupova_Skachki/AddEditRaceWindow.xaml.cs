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
using System.Windows.Shapes;

namespace Yakupova_Skachki
{
    /// <summary>
    /// Логика взаимодействия для AddEditRaceWindow.xaml
    /// </summary>
    public partial class AddEditRaceWindow : Window
    {
        private Races _currentRace;

        public AddEditRaceWindow(Races race = null)
        {
            InitializeComponent();
            var context = Yakupova_Skachki3Entities.GetContext();

            // Заполняем ComboBox-ы
            CbCompetition.ItemsSource = context.Competitions.ToList();
            CbHorse.ItemsSource = context.Horses.ToList();
            CbJockey.ItemsSource = context.Jockeys.ToList();

            if (race != null)
            {
                _currentRace = race;
                Title = "Редактирование заезда";
                TbRaceNumber.Text = race.RaceNumber.ToString();
                CbCompetition.SelectedValue = race.CompetitionID;

                // Загружаем участников заезда (если они есть)
                var entry = race.RaceEntries.FirstOrDefault();
                if (entry != null)
                {
                    CbHorse.SelectedValue = entry.HorseID;
                    CbJockey.SelectedValue = entry.JockeyID;
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!Login.IsAdmin) return;
            if (CbCompetition.SelectedItem == null || string.IsNullOrWhiteSpace(TbRaceNumber.Text)
                || CbHorse.SelectedItem == null || CbJockey.SelectedItem == null)
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TbRaceNumber.Text, out int raceNum) || raceNum <= 0)
            {
                MessageBox.Show("Номер заезда должен быть положительным числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var context = Yakupova_Skachki3Entities.GetContext();

            if (_currentRace == null)
            {
                // Создаем сам заезд
                Races newRace = new Races
                {
                    RaceNumber = raceNum,
                    CompetitionID = (int)CbCompetition.SelectedValue
                };
                context.Races.Add(newRace);

                // Создаем запись об участии (лошадь + жокей)
                RaceEntries newEntry = new RaceEntries
                {
                    Races = newRace,
                    HorseID = (int)CbHorse.SelectedValue,
                    JockeyID = (int)CbJockey.SelectedValue
                };
                context.RaceEntries.Add(newEntry);
            }
            else
            {
                _currentRace.RaceNumber = raceNum;
                _currentRace.CompetitionID = (int)CbCompetition.SelectedValue;

                var entry = context.RaceEntries.FirstOrDefault(re => re.RaceID == _currentRace.RaceID);
                if (entry != null)
                {
                    entry.HorseID = (int)CbHorse.SelectedValue;
                    entry.JockeyID = (int)CbJockey.SelectedValue;
                }
            }

            context.SaveChanges();
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
