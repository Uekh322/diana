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
    /// Логика взаимодействия для AddEditHorseWindow.xaml
    /// </summary>
    public partial class AddEditHorseWindow : Window
    {
        private Horses _currentHorse;

        // Передаем null, если добавление, или объект, если редактирование
        public AddEditHorseWindow(Horses horse)
        {
            InitializeComponent();

            // Загружаем список владельцев для ComboBox
            CbOwner.ItemsSource = Yakupova_Skachki3Entities.GetContext().Owners.ToList();

            if (horse != null)
            {
                _currentHorse = horse;
                Title = "Редактирование лошади";
                TbName.Text = _currentHorse.Name;
                DpBirthDate.SelectedDate = _currentHorse.BirthDate;
               

                // Устанавливаем пол
                foreach (ComboBoxItem item in CbGender.Items)
                {
                    if (item.Content.ToString() == _currentHorse.Gender)
                    {
                        CbGender.SelectedItem = item;
                        break;
                    }
                }
                CbOwner.SelectedValue = _currentHorse.OwnerID;
            }
            else
            {
                Title = "Добавление лошади";
                DpBirthDate.SelectedDate = DateTime.Now;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!Login.IsAdmin) return;
            // 1. Проверка на заполнение
            if (string.IsNullOrWhiteSpace(TbName.Text) || DpBirthDate.SelectedDate == null || CbGender.SelectedItem == null || CbOwner.SelectedItem == null)
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. ВОТ ТА САМАЯ ПРОВЕРКА НА БУДУЩУЮ ДАТУ
            if (DpBirthDate.SelectedDate.Value.Date > DateTime.Today)
            {
                MessageBox.Show("Дата рождения не может быть в будущем! Выберите корректную дату.",
                                "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return; // Прерываем сохранение
            }

            var context = Yakupova_Skachki3Entities.GetContext();

            if (_currentHorse == null)
            {
                // ДОБАВЛЕНИЕ
                Horses newHorse = new Horses
                {
                    Name = TbName.Text.Trim(),
                    Gender = ((ComboBoxItem)CbGender.SelectedItem).Content.ToString(),
                    BirthDate = DpBirthDate.SelectedDate.Value,
                    OwnerID = (int)CbOwner.SelectedValue,
               
                };
                context.Horses.Add(newHorse);
            }
            else
            {
                // РЕДАКТИРОВАНИЕ
                _currentHorse.Name = TbName.Text.Trim();
                _currentHorse.Gender = ((ComboBoxItem)CbGender.SelectedItem).Content.ToString();
                _currentHorse.BirthDate = DpBirthDate.SelectedDate.Value;
                _currentHorse.OwnerID = (int)CbOwner.SelectedValue;
        
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
