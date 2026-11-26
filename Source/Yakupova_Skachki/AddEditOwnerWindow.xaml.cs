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
    /// Логика взаимодействия для AddEditOwnerWindow.xaml
    /// </summary>
    public partial class AddEditOwnerWindow : Window
    {
        private Owners _currentOwner;

        public AddEditOwnerWindow(Owners owner = null)
        {
            InitializeComponent();
            if (owner != null)
            {
                _currentOwner = owner;
                Title = "Редактирование владельца";
                TbLastName.Text = owner.LastName;
                TbFirstName.Text = owner.FirstName;
         
                TbPhone.Text = owner.Phone;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!Login.IsAdmin) return;
            // 1. Проверка на заполнение
            if (string.IsNullOrWhiteSpace(TbLastName.Text) || string.IsNullOrWhiteSpace(TbFirstName.Text)
                || string.IsNullOrWhiteSpace(TbPhone.Text))
            {
                MessageBox.Show("Заполните Фамилию, Имя и Телефон!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. ПРОВЕРКА ТЕЛЕФОНА (проверяем именно количество цифр, а не длину строки)
            string phone = TbPhone.Text.Trim();

            // Извлекаем из строки только цифры
            string digitsOnly = new string(phone.Where(char.IsDigit).ToArray());

            // В формате +7 (ХХХ) ХХХ-ХХ-ХХ должно быть 11 цифр
            if (digitsOnly.Length != 11)
            {
                MessageBox.Show("Введите корректный номер телефона полностью в формате +7 (ХХХ) ХХХ-ХХ-ХХ!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var context = Yakupova_Skachki3Entities.GetContext();

            if (_currentOwner == null)
            {
                Owners newOwner = new Owners
                {
                    LastName = TbLastName.Text.Trim(),
                    FirstName = TbFirstName.Text.Trim(),
                    Address = "Не указан", // Заглушка, раз убрали поле
                    Phone = phone
                };
                context.Owners.Add(newOwner);
            }
            else
            {
                _currentOwner.LastName = TbLastName.Text.Trim();
                _currentOwner.FirstName = TbFirstName.Text.Trim();
                _currentOwner.Phone = phone;
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

        private void TbPhone_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Если введенный символ не является цифрой, отменяем ввод
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void TbPhone_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            // Убираем все нецифровые символы из строки для анализа
            string digits = new string(textBox.Text.Where(char.IsDigit).ToArray());

            // Если пользователь стер все, просто выходим
            if (digits.Length == 0) return;

            // Строим красивую строку
            string formatted = "+7";
            if (digits.Length > 1)
            {
                formatted += " (" + digits.Substring(1, Math.Min(3, digits.Length - 1));
            }
            if (digits.Length > 4)
            {
                formatted += ") " + digits.Substring(4, Math.Min(3, digits.Length - 4));
            }
            if (digits.Length > 7)
            {
                formatted += "-" + digits.Substring(7, Math.Min(2, digits.Length - 7));
            }
            if (digits.Length > 9)
            {
                formatted += "-" + digits.Substring(9, Math.Min(2, digits.Length - 9));
            }

            // Ставим курсор в конец
            textBox.Text = formatted;
            textBox.CaretIndex = textBox.Text.Length;
        }

    }
}
