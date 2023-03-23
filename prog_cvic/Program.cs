using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ConsoleTables;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Data;
using System.Xml;

namespace prog_cvic
{
    internal class Program
    {
        #region Public variable
        public static Dictionary<string, List<string>> scheduleFromUIS = new Dictionary<string, List<string>>();
        #endregion

        #region Main method
        static void Main(string[] args)
        {

            int lineCounter = 1;
            bool quit = false;
            // Main loop
            while (!quit)
            {
                StyleOfLines(lineCounter, 1, "Green");
                Console.WriteLine("Přihlásit se a ulazit data");
                StyleOfLines(lineCounter, 2, "Green");
                Console.WriteLine("Zobrazit rozvrh");
                StyleOfLines(lineCounter, 3, "Green");
                Console.WriteLine("Ulozit rozvrh");
                StyleOfLines(lineCounter, 4, "Green");
                Console.WriteLine("Importovat rozvrh");

                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                var key = Console.ReadKey(true).Key;
                lineCounter = UpAndDownManager(key, 4, lineCounter);
                if (key == ConsoleKey.Enter)
                {
                    if (lineCounter == 1) { LogInPage(); }
                    else if (lineCounter == 2) { ScheduleFromUIS(); }
                    else if (lineCounter == 3) { SavingSchedulePage(); }
                    else if (lineCounter == 4) { ReadFilePage(); }
                    else
                    {
                        Console.WriteLine("You chose the wrong line");
                        Console.ReadKey(true);
                    }
                }
                if (key == ConsoleKey.Escape) { quit = true; }

                Console.Clear();
            }
        }
        #endregion

        #region Support methods
        #region Color method
        //Changes color of the current line
        static public void StyleOfLines(int lineCounter, int currentLine, string color)
        {
            if (lineCounter == currentLine) 
            {
                if (color == "Green")
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                else if (color == "Blue")
                {
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                else if (color == "Red")
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
            } 
            else 
            { 
                Console.BackgroundColor = ConsoleColor.Black; 
                Console.ForegroundColor = ConsoleColor.White; 
            }
        }
        #endregion

        #region Login method
        //Login to UIS and parse data
        static private void GetScheduleFromUIS(string name, string password)
        {

            const string link = @"https://is.czu.cz/auth/?lang=cz";
            
            var options = new ChromeOptions();
            options.AddArguments("disable-infobars"); // disabling infobars
            options.AddArguments("--disable-extensions"); // disabling extensions
            options.AddArguments("--disable-gpu"); // applicable to windows os only
            options.AddArguments("--disable-dev-shm-usage"); // overcome limited resource problems
            options.AddArguments("--no-sandbox"); // Bypass OS security model
            options.AddArguments("--headless");
            options.AddArguments("--log-level=3");
            try
            {
                using (var driver = new ChromeDriver(options))// opens  chrome driver and login to UIS
                {
                    driver.Url = link;
                    driver.FindElement(By.Name("credential_0")).SendKeys(name);
                    driver.FindElement(By.Name("credential_1")).SendKeys(password);
                    driver.FindElement(By.Id("login-btn")).Click();

                    // checkes if authentication went right
                    Console.Clear();
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//h1[text() = 'Osobní administrativa']")));
                    driver.FindElement(By.XPath("//h1[text() = 'Osobní administrativa']"));
                    Console.WriteLine("Přihlašení proběhlo úspěšně");
                    // Go to page with schedule and choose right format of schedule 
                    wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//*[@id=\"sekce-24\"]/div[3]/div/div[2]/ul/li[1]/a/b")));
                    driver.Navigate().GoToUrl("https://is.czu.cz/auth/student/moje_studium.pl?_m=3110;lang=cz");
                    wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.LinkText("Osobní rozvrh")));
                    driver.FindElement(By.LinkText("Osobní rozvrh")).Click();
                    driver.FindElement(By.XPath("//option[text() = 'Seznam rozvrhových akcí']")).Click();
                    driver.FindElement(By.Name("zobraz2")).Click();
                    // Gets html of the page and then gets from it text 
                    var scheduleText = driver.FindElement(By.Id("tmtab_1")).GetAttribute("innerHTML");
                    var doc = new HtmlDocument();
                    doc.LoadHtml(scheduleText);
                    var htmlNodes = doc.DocumentNode.SelectNodes("//td");
                    // Gotten text writes to Dictionary
                    scheduleFromUIS.Clear();
                    string[] columnsName = { "days", "starts", "ends", "className", "aktivity", "place", "teacherName", "Omezeni", "Kapacita" };
                    foreach (var column in columnsName)
                    {
                        scheduleFromUIS.Add(column, new List<string>()); //set to scheduleFromUIS Dictionary keys
                    }
                    int i = 0;
                    foreach (var node in htmlNodes)
                    {
                        if (i > 0)
                        {
                            scheduleFromUIS[columnsName[i - 1]].Add(node.InnerText); //set to scheduleFromUIS Dictionary values
                        }
                        i++;
                        if (i > 9) { i = 0; }
                    }
                    Console.WriteLine("Informace ulozena");
                }
            }
            catch
            {
                Console.WriteLine("Nespravní jmeno, heslo nebo spatni internet");
            }
            finally
            {
                Console.WriteLine("Zmacnite tlacitko...");
                Console.ReadKey(true);
            }
            
        }
        #endregion

        #region Manage method
        // cheak if you use down or up arrow key to increase or dicrease lineCounter to move between lines 
        static public int UpAndDownManager(ConsoleKey key, int numberOfLines, int lineCounter)
        {
            if (key == ConsoleKey.DownArrow)
            {
                lineCounter++;
                if (lineCounter > numberOfLines) { lineCounter = 1; }
                return lineCounter;
            }
            else if (key == ConsoleKey.UpArrow)
            {
                lineCounter--;
                if (lineCounter < 1) { lineCounter = numberOfLines; }
                return lineCounter;
            }
            return lineCounter;
        }
        #endregion

        #region SaveSchedule method
        // Saves data from scheduleFromUIS Dictionary to .csv or .xml files
        static private void SaveSchedule(string folderPath, string folderName, string fileName, string typeOfFile = ".csv")
        {
            Console.Clear();
            if (!Directory.Exists(folderPath))// cheaks if you use proper path
            {
                Console.WriteLine("Nespravni napsana cesta");
                Console.ReadKey();
                return;
            }
            if (typeOfFile != ".csv" || typeOfFile != ".xml") // cheaks if you use proper type of file
            {
                Console.WriteLine("Nespravni typ souboru");
                Console.ReadKey();
                return;
            }
                folderPath = System.IO.Path.Combine(folderPath, folderName);
            
            //Makes path to file by combining fileName, typeOfFile and folderPath 
            fileName = fileName + typeOfFile;
            var filePath = System.IO.Path.Combine(folderPath, fileName);
            //Creates folder and file
            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
                System.IO.File.Create(filePath).Close();
            }
            else
            {
                if (!System.IO.File.Exists(filePath))
                {
                    System.IO.File.Create(filePath).Close();
                }
                else
                {
                    File.Delete(filePath);
                    System.IO.File.Create(filePath).Close();
                }
            }

            if (typeOfFile == ".csv")//Cheaks what type of file you using
            {
                for (int i = 0; i < 9; i++)
                {
                    foreach(var key in scheduleFromUIS.Keys)
                    {
                        //Replaces all ';' to ':' from Dictionary
                        scheduleFromUIS[key][i] = scheduleFromUIS[key][i].Replace(";", ",");
                    }
                }
                //Creates StringBuilder and then appends keys to it 
                StringBuilder output = new StringBuilder();
                output.AppendLine(string.Join(";", scheduleFromUIS.Keys));
                //Appends rows to StringBuilder strings with values in them
                List<string> row = new List<string>();
                for (int i = 0; i < 9; i++)
                {
                    foreach (var key in scheduleFromUIS.Keys)
                    {
                        row.Add(scheduleFromUIS[key][i]);
                    }
                    output.AppendLine(string.Join(";", row));
                    row.Clear();
                }
                System.IO.File.AppendAllText(filePath, output.ToString());//Appends all lines to the file
            }
            else if (typeOfFile == ".xml")
            {
                    
                XDocument doc = 
                    new XDocument( //Creats Xml document
                    new XElement("root", from kvp in scheduleFromUIS select //Creats root Xml element
                    new XElement(kvp.Key, from value in kvp.Value select //Creates an Xml key element and fills it with keys
                    new XElement("item", value)))); //Creats an Xml value element and fills it with values
                doc.Save(filePath);
            }

            Console.WriteLine("informace ulozena");
            Console.ReadKey(true);

        }
        #endregion

        #region File reading method
        static private void ReadFile(string filePath)
        {
            scheduleFromUIS.Clear();
            //Cheaks if file exist and what type is used
            if (File.Exists(filePath) && ".csv" == filePath.Substring(filePath.Length - 4))
            {
                var fileLines = File.ReadAllLines(filePath);//Reads all lines in file
                string[] columnsName = { "days", "starts", "ends", "className", "aktivity", "place", "teacherName", "Omezeni", "Kapacita" };
                foreach (var key in columnsName)
                {
                    scheduleFromUIS.Add(key, new List<string>());//Adds keys to Dictionary
                }
                foreach (var line in fileLines)
                {
                    if (line != fileLines.First())
                    {
                        var value = line.Split(';');
                        for (int i = 0; i < columnsName.Length; i++)
                        {
                            scheduleFromUIS[columnsName[i]].Add(value[i]);//Adds values to Dictionary
                        }
                    }
                }
                Console.Clear();
                Console.WriteLine("informace importovana");
            }
            else if (File.Exists(filePath) && ".xml" == filePath.Substring(filePath.Length - 4))
            {
                XmlDocument xml = new XmlDocument();//Creates Xml document and then load file data
                xml.Load(filePath);
                foreach (XmlNode node in xml.DocumentElement.ChildNodes)
                {
                    string key = node.Name;
                    List<string> values = new List<string>();

                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        values.Add(childNode.InnerText);
                    }

                    scheduleFromUIS.Add(key, values);
                }
                Console.Clear();
                Console.WriteLine("informace importovana");
            }
            else
            {
                Console.WriteLine("nespravni format cesty nebo soubor neexestuje");
            }
        }
        #endregion
        #endregion

        #region Page methods
        #region login page
        // Creates page for authentication
        static void LogInPage()
        {
            Console.Clear();
            int lineCounter = 1;
            string name = "";
            string password = "";
            bool quit = false;
            while (!quit)
            {
                StyleOfLines(lineCounter, 1, "Green");
                Console.WriteLine($"napište uživatelské jméno: {name}");
                StyleOfLines(lineCounter, 2, "Green");
                Console.WriteLine($"napište heslo: {string.Concat(Enumerable.Repeat("*", password.Length))}");
                StyleOfLines(lineCounter, 3, "Green");
                Console.WriteLine($"přihlásit");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;

                var key = Console.ReadKey(true).Key;
                lineCounter = UpAndDownManager(key, 3, lineCounter);
                switch (key)
                {
                    case ConsoleKey.Enter:
                        if (lineCounter== 1) { name = Console.ReadLine(); }
                        else if (lineCounter== 2) 
                        {
                            StringBuilder starLine = new StringBuilder();
                            while (true)
                            {
                                var hidenKey = Console.ReadKey(true);
                                if (hidenKey.Key == ConsoleKey.Enter)
                                {
                                    Console.WriteLine();
                                    break;
                                }
                                else if (hidenKey.Key == ConsoleKey.Backspace && password.Length > 0)
                                {
                                    password = password.Substring(0, password.Length - 1);
                                    starLine.Remove(starLine.Length - 1, 1);
                                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                                    Console.Write(" ");
                                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                                }
                                else
                                {
                                    password += hidenKey.KeyChar;
                                    starLine.Append("*");
                                    Console.Write("*");
                                }
                            }
                        }
                        else if (lineCounter== 3 && name.Length != 0 && password.Length != 0)
                        {
                            GetScheduleFromUIS(name, password);
                            quit = true;
                        }
                        else 
                        {
                            Console.WriteLine("nevyplnil/a jste všechno");
                            Console.ReadKey();
                        }
                        break;
                    case ConsoleKey.Escape: 
                        quit = true;
                        break;
                }
                Console.Clear();
            }
            
        }
        
        #endregion

        #region Show schedule page 
        // Creates page to visualize schedule's data
        static void ScheduleFromUIS()
        {
            // Add columns names and add row 
            Console.Clear();
            if (scheduleFromUIS.Count > 0)
            {
                var table = new ConsoleTable("Den", "Od", "Do", "Předmět", "Akce", "Místnost", "Vyučující", "Omezení", "Kapacita");
                List<string> row = new List<string>();
                for (int i = 0; i < 9; i++)
                {
                    foreach (var column in scheduleFromUIS.Keys)
                    {
                        row.Add(scheduleFromUIS[column][i]);
                    }
                    table.Rows.Add(row.ToArray());
                    row.Clear();
                }
                table.Write();
            }
            else { Console.WriteLine("Teprve musite prihlasit a nahrat data\nZmacnite tlacitko..."); }
            Console.ReadKey(true);
        }
        #endregion

        #region Save schedule page
        static public void SavingSchedulePage()
        {
            Console.Clear();
            int lineCounter = 1;
            string folderPath = "";
            string folderName = "";
            string fileName = "";
            string typeOfFile = "";
            bool quit = false;
            if (scheduleFromUIS.Count > 0)
            {
                while (!quit)
                {
                    StyleOfLines(lineCounter, 1, "Green");
                    Console.WriteLine($"cesta do slozky: {folderPath}");
                    StyleOfLines(lineCounter, 2, "Green");
                    Console.WriteLine($"jmeno slozky: {folderName}");
                    StyleOfLines(lineCounter, 3, "Green");
                    Console.WriteLine($"jmeno souboru: {fileName}");
                    StyleOfLines(lineCounter, 4, "Green");
                    Console.WriteLine($"typ souboru .csv nebo .xml: {typeOfFile}");
                    StyleOfLines(lineCounter, 5, "Green");
                    Console.WriteLine($"Ulozit");

                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;

                    var key = Console.ReadKey(true).Key;
                    lineCounter = UpAndDownManager(key, 5, lineCounter);
                    if (key == ConsoleKey.Enter)
                    {
                        if (lineCounter == 1) { folderPath = Console.ReadLine(); }
                        else if (lineCounter == 2) { folderName = Console.ReadLine(); }
                        else if (lineCounter == 3) { fileName = Console.ReadLine(); }
                        else if (lineCounter == 4) { typeOfFile= Console.ReadLine(); }
                        else if (lineCounter == 5)
                        {
                            if (fileName != "" && folderName != "" && folderPath != "")
                            {
                                SaveSchedule(folderPath, folderName, fileName, typeOfFile);
                                quit = true;
                            }
                            else
                            {
                                Console.WriteLine("nevyplnil/a jste všechno");
                                Console.ReadKey();
                            }
                        }
                    }
                    else if (key == ConsoleKey.Escape)
                    {
                        quit = true;
                    }
                    Console.Clear();
                }
            }
            else
            {
                Console.WriteLine("Nenačetl/a jste data");
                Console.ReadKey(true);
            }
        }
        #endregion

        #region Read file page
        public static void ReadFilePage()
        {
            Console.Clear();
            int lineCounter = 1;
            string filePath = "";
            bool quit = false;
            while (!quit)
            {
                StyleOfLines(lineCounter, 1, "Green");
                Console.WriteLine($"cesta do slozky: {filePath}");
                StyleOfLines(lineCounter, 2, "Green");
                Console.WriteLine($"Cist");

                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;

                var key = Console.ReadKey(true).Key;
                lineCounter = UpAndDownManager(key, 2, lineCounter);
                if (key == ConsoleKey.Enter)
                {
                    if (lineCounter == 1) { filePath = Console.ReadLine(); }
                    else if (lineCounter == 2)
                    {
                        if (filePath != "")
                        {

                            Console.Clear();
                            ReadFile(filePath);
                            Console.ReadKey(true);
                            quit = true;
                        }
                        else
                        {
                            Console.WriteLine("nevyplnil/a jste všechno");
                            Console.ReadKey();
                        }
                    }
                }
                else if (key == ConsoleKey.Escape)
                {
                    quit = true;
                }
                Console.Clear();
            }
        }
        #endregion
        #endregion
    }
}