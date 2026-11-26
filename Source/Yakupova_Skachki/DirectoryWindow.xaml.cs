using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Yakupova_Skachki
{
    public enum DirectoryKind { Owners, Jockeys, Competitions }

    public partial class Jockeys
    {
        public string FullName => (LastName + " " + FirstName + " " + Patronymic).Trim();
    }

    public partial class DirectoryWindow : Window
    {
        private sealed class Record
        {
            public string Label { get; set; }
            public object Value { get; set; }
        }

        public DirectoryWindow(DirectoryKind kind)
        {
            InitializeComponent();
            var db = Yakupova_Skachki3Entities.GetContext();
            switch (kind)
            {
                case DirectoryKind.Owners:
                    Title = "Владельцы";
                    People.ItemsSource = db.Owners.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToList()
                        .Select(x => new Record { Label = x.OwnerDisplayName, Value = x }).ToList();
                    break;
                case DirectoryKind.Jockeys:
                    Title = "Жокеи";
                    People.ItemsSource = db.Jockeys.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToList()
                        .Select(x => new Record { Label = x.FullName, Value = x }).ToList();
                    break;
                default:
                    Title = "Соревнования";
                    Details.Visibility = Visibility.Collapsed;
                    RaceScroll.Visibility = Visibility.Visible;
                    People.ItemsSource = db.Competitions.OrderByDescending(x => x.EventDate).ThenBy(x => x.EventTime).ToList()
                        .Select(x => new Record { Label = x.DisplayName, Value = x }).ToList();
                    break;
            }
            People.SelectedIndex = 0;
            if (People.Items.Count == 0) EmptyMessage.Text = "Записей пока нет.";
        }

        private void SelectRecord(object sender, SelectionChangedEventArgs args)
        {
            var record = People.SelectedItem as Record;
            if (record == null) return;
            PersonName.Text = record.Label;
            if (record.Value is Owners owner)
            {
                Profile.Text = "Телефон  " + owner.Phone + "\nАдрес  " + owner.Address;
                SectionTitle.Text = "Лошади владельца";
                Details.ItemsSource = owner.Horses.OrderBy(x => x.Name).Select(x => new
                {
                    Кличка = x.Name, Пол = x.Gender, Дата_рождения = x.BirthDate, Возраст = x.Age
                }).ToList();
                EmptyMessage.Text = Details.Items.Count == 0 ? "У владельца пока нет зарегистрированных лошадей." : "";
            }
            else if (record.Value is Jockeys jockey)
            {
                Profile.Text = string.Format("Дата рождения  {0:dd.MM.yyyy}    ·    Возраст  {1}    ·    Рейтинг  {2}\nАдрес  {3}",
                    jockey.BirthDate, jockey.Age, jockey.Rating?.ToString("0.##") ?? "—", jockey.Address);
                SectionTitle.Text = "Лошади и результаты заездов";
                Details.ItemsSource = jockey.RaceEntries.OrderByDescending(x => x.Races.Competitions.EventDate)
                    .ThenBy(x => x.Races.RaceNumber).Select(entry =>
                    {
                        var race = entry.Races;
                        var result = race.Results.SingleOrDefault(x => x.EntryID == entry.EntryID);
                        return new
                        {
                            Дата = race.Competitions.EventDate, Лошадь = entry.Horses.Name,
                            Заезд = race.RaceNumber, Место = result?.Place.ToString() ?? "Не указано",
                            Время_финиша = result?.FinishTime.ToString() ?? "—",
                            Соревнование = race.Competitions.Name ?? "Без названия"
                        };
                    }).ToList();
                EmptyMessage.Text = Details.Items.Count == 0 ? "У жокея пока нет заявок на заезды." : "";
            }
            else
            {
                var competition = (Competitions)record.Value;
                Profile.Text = competition.Venue + "    ·    Начало " + competition.EventTime.ToString(@"hh\:mm");
                SectionTitle.Text = "Заезды соревнования";
                RaceCards.ItemsSource = competition.Races.OrderBy(r => r.RaceNumber).ThenBy(r => r.RaceID).Select(race =>
                {
                    var entries = race.RaceEntries.ToDictionary(e => e.EntryID);
                    var results = race.Results.OrderBy(r => r.Place).ThenBy(r => r.FinishTime).Select(result =>
                    {
                        RaceEntries entry;
                        entries.TryGetValue(result.EntryID ?? -1, out entry);
                        return new
                        {
                            Finish = result.Place + " место  ·  " + result.FinishTime.ToString("c"),
                            Participant = entry == null ? "Участник не указан" : entry.Horses.Name + " — " + entry.Jockeys.LastName
                        };
                    }).ToList();
                    return new
                    {
                        Title = "Заезд № " + race.RaceNumber,
                        Participants = entries.Values.OrderBy(e => e.Horses.Name).ThenBy(e => e.EntryID)
                            .Select(e => e.Horses.Name + " — " + e.Jockeys.LastName).DefaultIfEmpty("Участники пока не заявлены").ToList(),
                        Results = results,
                        NoResults = results.Count == 0 ? Visibility.Visible : Visibility.Collapsed
                    };
                }).ToList();
                RaceScroll.ScrollToTop();
                EmptyMessage.Text = RaceCards.Items.Count == 0 ? "Заезды пока не зарегистрированы." : "";
            }
        }

        private void FormatColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Header = e.PropertyName.Replace('_', ' ');
            var column = e.Column as DataGridTextColumn;
            if (column != null && e.PropertyType == typeof(DateTime)) column.Binding.StringFormat = "dd.MM.yyyy";
        }
    }
}
