using System;

using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Yakupova_Skachki
{
    public partial class Journal : Window
    {
        private readonly Yakupova_Skachki3Entities db = Yakupova_Skachki3Entities.GetContext();
        private readonly int? horseId;
        private sealed class RaceOption
        {
            public Races Race { get; set; }
            public string Label { get; set; }
        }
        private sealed class Finish
        {
            internal int ResultId { get; set; }
            public int Место { get; set; }
            public string Лошадь { get; set; }
            public string Жокей { get; set; }
            public TimeSpan Время_финиша { get; set; }
        }
        private sealed class Entry
        {
            internal int EntryId { get; set; }
            public string Лошадь { get; set; }
            public string Жокей { get; set; }
        }

        public Journal(Horses horse = null)
        {
            InitializeComponent();
            horseId = horse?.HorseID;
            Heading.Text = Title = horse == null ? "Заезды и результаты" : "Заезды · " + horse.Name;
            AdminTools.Visibility = Login.CanRecordResults ? Visibility.Visible : Visibility.Collapsed;
            LoadRaces();
        }

        private static string FullName(Jockeys jockey)
        {
            return string.Join(" ", new[] { jockey.LastName, jockey.FirstName, jockey.Patronymic }.Where(n => !string.IsNullOrWhiteSpace(n)));
        }
        private void LoadRaces(int? keepRace = null)
        {
            var query = db.Races.AsQueryable();
            if (Day.SelectedDate.HasValue) { var day = Day.SelectedDate.Value.Date; query = query.Where(r => r.Competitions.EventDate == day); }
            if (horseId.HasValue) query = query.Where(r => r.RaceEntries.Any(e => e.HorseID == horseId));
            var options = query.OrderByDescending(r => r.Competitions.EventDate).ThenBy(r => r.Competitions.EventTime).ThenBy(r => r.RaceNumber).ToList()
                .Select(r => new RaceOption { Race = r, Label = string.Format("{0:dd.MM.yyyy} · {1} · Заезд № {2}{3}", r.Competitions.EventDate, r.Competitions.Name ?? r.Competitions.Venue, r.RaceNumber, r.Results.Any() ? " · Есть результаты" : "") }).ToList();
            RacePicker.ItemsSource = options;
            RacePicker.SelectedItem = options.FirstOrDefault(x => x.Race.RaceID == keepRace)
                ?? options.FirstOrDefault(x => x.Race.Results.Any()) ?? options.FirstOrDefault();
            ShowRace();
        }
        private void DateChanged(object sender, SelectionChangedEventArgs e) { LoadRaces(); }
        private void ResetDay(object sender, RoutedEventArgs e) { Day.SelectedDate = null; }
        private void RaceChanged(object sender, SelectionChangedEventArgs e)
        {
            Editor.IsExpanded = false;
            PlaceInput.Clear();
            TimeInput.Clear();
            ShowRace();
        }
        private void TogglePodium(object sender, RoutedEventArgs e) { ShowRace(); }
        private void ShowRace()
        {
            var race = (RacePicker.SelectedItem as RaceOption)?.Race;
            Protocol.ItemsSource = Entries.ItemsSource = null;
            ParticipantsTitle.Text = "Участники";
            Body.Visibility = race == null ? Visibility.Collapsed : Visibility.Visible;
            if (race == null) { Venue.Text = "Заездов по выбранным условиям нет."; return; }
            var entries = race.RaceEntries.ToDictionary(e => e.EntryID);
            var finishes = race.Results.Where(r => Podium.IsChecked != true || (r.Place >= 1 && r.Place <= 3)).OrderBy(r => r.Place).Select(r =>
            {
                RaceEntries entry;
                entries.TryGetValue(r.EntryID ?? -1, out entry);
                return new Finish { ResultId = r.ResultID, Место = r.Place, Лошадь = entry?.Horses.Name ?? "Участник не указан", Жокей = entry == null ? "—" : FullName(entry.Jockeys), Время_финиша = r.FinishTime };
            }).ToList();
            Venue.Text = race.Competitions.Venue + "   ·   Начало соревнования " + race.Competitions.EventTime.ToString(@"hh\:mm");
            bool hasResults = race.Results.Any();
            Podium.Visibility = hasResults ? Visibility.Visible : Visibility.Collapsed;
            Protocol.Visibility = finishes.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            Status.Text = !hasResults ? "Результаты пока не внесены."
                : finishes.Count == 0 ? "Призовых мест пока нет. Снимите фильтр, чтобы посмотреть все результаты." : "";
            Status.Visibility = Status.Text.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
            Assignment.Visibility = finishes.Count > 0 && entries.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            NoEntries.Visibility = entries.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            Entries.Visibility = Editor.Visibility = entries.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            Protocol.ItemsSource = finishes;
            Entries.ItemsSource = entries.Values.OrderBy(e => e.EntryID).Select(e => new Entry { EntryId = e.EntryID, Лошадь = e.Horses.Name, Жокей = FullName(e.Jockeys) }).ToList();
            ParticipantsTitle.Text = "Участники · " + entries.Count;
            LinkButton.IsEnabled = false;
        }

        private void SelectionChangedForAction(object sender, SelectionChangedEventArgs e)
        {
            if (LinkButton != null)
                LinkButton.IsEnabled = Protocol.SelectedItem != null && Entries.SelectedItem != null;
        }

        private void SaveFinish(object sender, RoutedEventArgs e)
        {
            if (!Login.CanRecordResults) return;
            var race = (RacePicker.SelectedItem as RaceOption)?.Race;
            var entry = Entries.SelectedItem as Entry;
            if (race == null || entry == null) { MessageBox.Show(this, "Выберите участника в списке."); return; }

            int place;
            TimeSpan time;
            if (!int.TryParse(PlaceInput.Text.Trim(), out place) || place < 1 ||
                !TimeSpan.TryParseExact(TimeInput.Text.Trim(), @"hh\:mm\:ss", CultureInfo.InvariantCulture, out time) || time <= TimeSpan.Zero)
            {
                MessageBox.Show(this, "Укажите положительное место и время финиша, например 00:01:23.");
                return;
            }
            if (race.Results.Any(x => x.EntryID == entry.EntryId || x.Place == place))
            {
                MessageBox.Show(this, "У участника уже есть результат или место уже записано. Для существующего места используйте привязку к участнику.");
                return;
            }
            var result = new Results { RaceID = race.RaceID, EntryID = entry.EntryId, Place = place, FinishTime = time };
            db.Results.Add(result);
            try { db.SaveChanges(); }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                db.Entry(result).State = System.Data.Entity.EntityState.Detached;
                MessageBox.Show(this, "Не удалось сохранить результат. Проверьте подключение и данные заявки.");
                return;
            }
            if (place > 3) Podium.IsChecked = false;
            LoadRaces(race.RaceID);
        }

        private void Assign(object sender, RoutedEventArgs e)
        {
            if (!Login.CanRecordResults) return;
            var finish = Protocol.SelectedItem as Finish;
            var entry = Entries.SelectedItem as Entry;
            if (entry == null || finish == null) { MessageBox.Show(this, "Выберите результат и заявку."); return; }
            var result = db.Results.Find(finish.ResultId);
            if (result.EntryID == entry.EntryId) return;
            if (result.EntryID.HasValue && MessageBox.Show(this, "Изменить участника для этого результата?", "Протокол", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            try
            {
                result.EntryID = entry.EntryId;
                db.SaveChanges();
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                db.Entry(result).Reload();
                MessageBox.Show(this, "Не удалось сохранить. Проверьте, свободна ли заявка и доступна ли база.");
            }
            ShowRace();
        }

        private void Columns(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Header = e.PropertyName.Replace('_', ' ');
            var column = e.Column as DataGridTextColumn;
            if (column != null && e.PropertyType == typeof(DateTime)) column.Binding.StringFormat = "dd.MM.yyyy";
        }
    }
}
