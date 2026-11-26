using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yakupova_Skachki
{
    public class HorsesAddLogic
    {
        public static Horses CreateHorse(string name, string gender, DateTime birthDate)
        {
            
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Кличка лошади не может быть пустой.");

           
            if (name.Length > 100)
                throw new ArgumentException("Кличка не должна превышать 100 символов.");

          
            if (string.IsNullOrWhiteSpace(gender))
                throw new ArgumentException("Пол лошади должен быть указан.");

           
            if (birthDate.Date > DateTime.Today)
                throw new ArgumentException("Дата рождения не может быть в будущем.");

            //проверка на макс возраст
            if (birthDate.Date < DateTime.Today.AddYears(-50))
                throw new ArgumentException("Лошадь не может быть старше 50 лет.");

            return new Horses
            {
                Name = name.Trim(),
                Gender = gender.Trim(),
                BirthDate = birthDate
            };
        }
    }
}
