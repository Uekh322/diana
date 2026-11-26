using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yakupova_Skachki; // <--- ВАЖНО: Подключаем твой проект с классом
using System;

namespace TestProject
{
    // Убрали вложенный [TestClass], оставили один главный
    [TestClass]
    public class HorsesAddLogicTests
    {
        // ==========================================
        // 1. ПОЗИТИВНЫЕ ТЕСТЫ (Успешное создание)
        // ==========================================

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
            // Arrange
            DateTime today = DateTime.Today;

            // Act
            var horse = HorsesAddLogic.CreateHorse("Звезда", "Кобыла", today);

            // Assert
            Assert.IsNotNull(horse);
            Assert.AreEqual(today, horse.BirthDate.Date);
        }

        [TestMethod]
        public void CreateHorse_NameExactly100Chars_ReturnsHorse()
        {
            // Граница: ровно 100 символов (разрешено)
            string name100 = new string('A', 100);

            var horse = HorsesAddLogic.CreateHorse(name100, "Мерин", new DateTime(2015, 1, 1));

            Assert.IsNotNull(horse);
            Assert.AreEqual(100, horse.Name.Length);
        }

        // ==========================================
        // 2. НЕГАТИВНЫЕ ТЕСТЫ: ПРОВЕРКА ИМЕНИ
        // ==========================================

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateHorse_NullName_ThrowsException()
        {
            HorsesAddLogic.CreateHorse(null, "Кобыла", new DateTime(2020, 1, 1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateHorse_EmptyName_ThrowsException()
        {
            HorsesAddLogic.CreateHorse("", "Кобыла", new DateTime(2020, 1, 1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateHorse_WhitespaceName_ThrowsException()
        {
            HorsesAddLogic.CreateHorse("   ", "Кобыла", new DateTime(2020, 1, 1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateHorse_Name101Chars_ThrowsException()
        {
            // Граница: 101 символ (запрещено)
            string name101 = new string('A', 101);
            HorsesAddLogic.CreateHorse(name101, "Кобыла", new DateTime(2020, 1, 1));
        }

        // ==========================================
        // 3. НЕГАТИВНЫЕ ТЕСТЫ: ПРОВЕРКА ПОЛА
        // ==========================================

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateHorse_NullGender_ThrowsException()
        {
            HorsesAddLogic.CreateHorse("Буцефал", null, new DateTime(2020, 1, 1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateHorse_EmptyGender_ThrowsException()
        {
            HorsesAddLogic.CreateHorse("Буцефал", "", new DateTime(2020, 1, 1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateHorse_WhitespaceGender_ThrowsException()
        {
            HorsesAddLogic.CreateHorse("Буцефал", "   ", new DateTime(2020, 1, 1));
        }

        // ==========================================
        // 4. НЕГАТИВНЫЕ ТЕСТЫ: ПРОВЕРКА ДАТЫ
        // ==========================================

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateHorse_FutureBirthDate_ThrowsException()
        {
            // Дата в будущем (завтра)
            DateTime tomorrow = DateTime.Today.AddDays(1);
            HorsesAddLogic.CreateHorse("Буцефал", "Жеребец", tomorrow);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateHorse_BirthDateWithTimeInFuture_ThrowsException()
        {
            // Дата сегодня, но время в будущем (проверка того, что используется .Date)
            DateTime todayLate = DateTime.Now.AddHours(2);
            if (todayLate.Date <= DateTime.Today)
            {
                todayLate = DateTime.Today.AddDays(1).AddHours(2);
            }
            HorsesAddLogic.CreateHorse("Буцефал", "Жеребец", todayLate);
        }
    }
}