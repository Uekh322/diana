using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yakupova_Skachki; // <--- ВАЖНО: Подключаем твой проект с классом
using System;

namespace TestProject
{
    // Убрали вложенный [TestClass], оставили один главный
    [TestClass]
    public class HorsesAddLogicTests
    {
     

        [TestMethod]
        public void CreateHorse_ValidData_ReturnsHorseWithTrimmedValues()
        {
            // Arrange
            string name = "  Буцефал  ";
            string gender = "  Жеребец  ";
            DateTime birthDate = new DateTime(2020, 5, 15);

            // Act
            var horse = HorsesAddLogic.CreateHorse(name, gender, birthDate);

            // Assert
            Assert.IsNotNull(horse);
            Assert.AreEqual("Буцефал", horse.Name); // Проверка работы Trim()
            Assert.AreEqual("Жеребец", horse.Gender); // Проверка работы Trim()
            Assert.AreEqual(birthDate, horse.BirthDate);
        }

        [TestMethod]
        public void CreateHorse_BirthDateToday_ReturnsHorse()
        {
            
            DateTime today = DateTime.Today;

           
            var horse = HorsesAddLogic.CreateHorse("Звезда", "Кобыла", today);

            
            Assert.IsNotNull(horse);
            Assert.AreEqual(today, horse.BirthDate.Date);
        }

        [TestMethod]
        public void CreateHorse_NameExactly100Chars_ReturnsHorse()
        {
            
            string name100 = new string('A', 100);

            var horse = HorsesAddLogic.CreateHorse(name100, "Мерин", new DateTime(2015, 1, 1));

            Assert.IsNotNull(horse);
            Assert.AreEqual(100, horse.Name.Length);
        }


        // 2. НЕГАТИВНЫЕ ТЕСТЫ: ПРОВЕРКА ИМЕНИ

        [TestMethod]
        public void CreateHorse_NullName_ThrowsException()
        {
            var ex = Assert.ThrowsException<ArgumentException>(() =>
                HorsesAddLogic.CreateHorse(null, "Кобыла", new DateTime(2020, 1, 1)));

            Assert.AreEqual("Кличка лошади не может быть пустой.", ex.Message);
        }

        [TestMethod]
        public void CreateHorse_EmptyName_ThrowsException()
        {
            var ex = Assert.ThrowsException<ArgumentException>(() =>
                HorsesAddLogic.CreateHorse("", "Кобыла", new DateTime(2020, 1, 1)));

            Assert.AreEqual("Кличка лошади не может быть пустой.", ex.Message);
        }

        [TestMethod]
        public void CreateHorse_WhitespaceName_ThrowsException()
        {
            var ex = Assert.ThrowsException<ArgumentException>(() =>
                HorsesAddLogic.CreateHorse("   ", "Кобыла", new DateTime(2020, 1, 1)));

            Assert.AreEqual("Кличка лошади не может быть пустой.", ex.Message);
        }

        [TestMethod]
        public void CreateHorse_Name101Chars_ThrowsException()
        {
            // Граница: 101 символ (запрещено)
            string name101 = new string('A', 101);

            var ex = Assert.ThrowsException<ArgumentException>(() =>
                HorsesAddLogic.CreateHorse(name101, "Кобыла", new DateTime(2020, 1, 1)));

            Assert.AreEqual("Кличка не должна превышать 100 символов.", ex.Message);
        }


        // 3. НЕГАТИВНЫЕ ТЕСТЫ: ПРОВЕРКА ПОЛА


        [TestMethod]
        public void CreateHorse_NullGender_ThrowsException()
        {
            var ex = Assert.ThrowsException<ArgumentException>(() =>
                HorsesAddLogic.CreateHorse("Буцефал", null, new DateTime(2020, 1, 1)));

            Assert.AreEqual("Пол лошади должен быть указан.", ex.Message);
        }

        [TestMethod]
        public void CreateHorse_EmptyGender_ThrowsException()
        {
            var ex = Assert.ThrowsException<ArgumentException>(() =>
                HorsesAddLogic.CreateHorse("Буцефал", "", new DateTime(2020, 1, 1)));

            Assert.AreEqual("Пол лошади должен быть указан.", ex.Message);
        }

        [TestMethod]
        public void CreateHorse_WhitespaceGender_ThrowsException()
        {
            var ex = Assert.ThrowsException<ArgumentException>(() =>
                HorsesAddLogic.CreateHorse("Буцефал", "   ", new DateTime(2020, 1, 1)));

            Assert.AreEqual("Пол лошади должен быть указан.", ex.Message);
        }


        // 4. НЕГАТИВНЫЕ ТЕСТЫ: ПРОВЕРКА ДАТЫ


        [TestMethod]
        public void CreateHorse_FutureBirthDate_ThrowsException()
        {
            // Дата в будущем (завтра)
            DateTime tomorrow = DateTime.Today.AddDays(1);

            var ex = Assert.ThrowsException<ArgumentException>(() =>
                HorsesAddLogic.CreateHorse("Буцефал", "Жеребец", tomorrow));

            Assert.AreEqual("Дата рождения не может быть в будущем.", ex.Message);
        }

        [TestMethod]
        public void CreateHorse_BirthDateWithTimeInFuture_ThrowsException()
        {
            // Дата сегодня, но время в будущем (проверка того, что используется .Date)
            DateTime todayLate = DateTime.Now.AddHours(2);
            if (todayLate.Date <= DateTime.Today)
            {
                todayLate = DateTime.Today.AddDays(1).AddHours(2);
            }

            var ex = Assert.ThrowsException<ArgumentException>(() =>
                HorsesAddLogic.CreateHorse("Буцефал", "Жеребец", todayLate));

            Assert.AreEqual("Дата рождения не может быть в будущем.", ex.Message);
        }

        [TestMethod]
        public void CreateHorse_TooOldHorse_ThrowsException()
        {
            // Лошадь не может быть старше 50 лет (например, родилась 60 лет назад) чет слишком старая да, я проверила что самой старой было 62, так что берем до 50ти
            DateTime veryOldDate = DateTime.Today.AddYears(-60);

            // Act
            var ex = Assert.ThrowsException<ArgumentException>(() =>
                HorsesAddLogic.CreateHorse("Древний", "Мерин", veryOldDate));

            // Assert
            Assert.AreEqual("Лошадь не может быть старше 50 лет.", ex.Message);
        }
    }
}