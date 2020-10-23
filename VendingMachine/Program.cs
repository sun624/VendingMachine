/*
 * Author: Yichun Sun
 * MSSA JBLM 34
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Collections;
using System.Net.Mail;
using System.Net.Mime;

/// <summary>
/// welcome to my vending machine. 
/// it has a robust user interaction process, foolproof.
/// it has super user account which can show information about the vending machine
/// it can email vendor if something is sold out.
/// it has a money change system which gives back change in the combination of bills and coins.
/// </summary>
#warning Email replenishment notification needs modification to your existing security configuration. It may not work on your specific email account. 
         
namespace VendingMachine
{
    #region enumeration
    //create enumeration coins, make the index its value.
    public enum coins
    {
        penny = 1,
        nickel = 5,
        dime = 10,
        quarter = 25
    }
    //create enumeration coins, make the index its value.
    public enum bills
    {
        one_dollar = 1,
        five_dollar = 5,
        ten_dollar = 10,
        twenty_dollar = 20
    }
    #endregion

    public class Program
    {

        public static void Main(string[] args)
        {
            #region Initialization
            //display welcome window
            Welcome("Welcome.txt", 150, 20);

            //display my vending machine
            DisplayMenu();

            int _initialQuantity = 10;

            // initialize item objects with quantity, name and price. Initial quantity is 10.
            Item Coke = new Item(_initialQuantity, "Coke", 0.99);
            Item Sprite = new Item(_initialQuantity, "Sprite", 0.99);
            Item Fanta = new Item(_initialQuantity, "Fanta", 0.99);
            Item Sandwich = new Item(_initialQuantity, "Sandwich", 1.99);
            Item Hotdog = new Item(_initialQuantity, "Hotdog", 1.99);
            Item Burger = new Item(_initialQuantity, "Burger", 1.99);
            Item Snickers = new Item(_initialQuantity, "Snickers", 1.49);
            Item MilkyWay = new Item(_initialQuantity, "MilkyWay", 1.49);
            Item TicTac = new Item(_initialQuantity, "TicTac", 1.49);
            Item Soap = new Item(_initialQuantity, "Soap", 0.99);
            Item Bleach = new Item(_initialQuantity, "Bleach", 0.99);
            Item Softener = new Item(_initialQuantity, "Softener", 0.99);

            //initialize an menu list for all the above items
            var menu = new List<Item>
            {
                Coke, Sprite, Fanta, Sandwich, Hotdog, Burger, Snickers, MilkyWay, TicTac, Soap, Bleach, Softener
            };

            //create a dictionary with location as key and item object as value.
            var lookup = new Dictionary<string, Item>();
            lookup.Add("A1", Coke);
            lookup.Add("A2", Sprite);
            lookup.Add("A3", Fanta);
            lookup.Add("B1", Sandwich);
            lookup.Add("B2", Hotdog);
            lookup.Add("B3", Burger);
            lookup.Add("C1", Snickers);
            lookup.Add("C2", MilkyWay);
            lookup.Add("C3", TicTac);
            lookup.Add("D1", Soap);
            lookup.Add("D2", Bleach);
            lookup.Add("D3", Softener);

            // initialize moneyCoins dictionary to include penny,nickel,dime and quarter
            var moneyCoins = new Dictionary<coins, int>();
            moneyCoins.Add(coins.penny, 100);
            moneyCoins.Add(coins.nickel, 100);
            moneyCoins.Add(coins.dime, 100);
            moneyCoins.Add(coins.quarter, 100);

            // initialize moneyBills dictionary to include one,five and ten dollar bills
            var moneyBills = new Dictionary<bills, int>();
            moneyBills.Add(bills.one_dollar, 50);
            moneyBills.Add(bills.five_dollar, 50);
            moneyBills.Add(bills.ten_dollar, 50);

            //initialize a customer who has a one dollar bill, one five dollar bill, one ten dollar bill and one twenty dollar bill.
            var customercoins = new Dictionary<coins, int>();
            customercoins.Add(coins.quarter, 0);
            customercoins.Add(coins.dime, 0);
            customercoins.Add(coins.nickel, 0);
            customercoins.Add(coins.penny, 0);

            var customerbills = new Dictionary<bills, int>();
            customerbills.Add(bills.one_dollar, 1);
            customerbills.Add(bills.five_dollar, 1);
            customerbills.Add(bills.ten_dollar, 1);
            customerbills.Add(bills.twenty_dollar, 1);

            var customer = new Customer(customercoins, customerbills);

            //initialize a new order list to store the items you want to purchase
            List<Item> Order = new List<Item>();
            #endregion

            #region Order
            //let user input the location of item he wants.
            string choice = Console.ReadLine();
            string password = File.ReadAllText("AdministratorPassword.txt");
            if (choice == password)
            {
                Superuser(menu, lookup, moneyBills, moneyCoins);
            }
            Console.WriteLine("How many you want?");

            //Convert the user input into a integer as quantity.
            if (!Int32.TryParse(Console.ReadLine(), out int qty))
            {
                Console.WriteLine("Please enter an integer.");
            }
            else
            {
                //create a first order with quantity and name by using dictionary.
                try
                {       //if the item selected is sold out or is not enough, print message
                    if (lookup[choice.ToUpper()].IsSoldOut() || qty > lookup[choice.ToUpper()].Count)
                    {
                        Console.WriteLine("The item you selected is either sold out or doesn't have enough quantity," +
                            " please purchase other items");
                    }
                    else
                    {   //create the first order
                        var firstOrder = new Item(qty, (lookup[choice.ToUpper()].GetName() + "_order"), lookup[choice.ToUpper()].GetPrice());
                        //add the first order to the order list.
                        Order.Add(firstOrder);
                        //reduce the count of the item in the vending machine by one.
                        lookup[choice.ToUpper()].Sold(qty);
                        //display order information: quantity of item, location and item name.
                        Console.WriteLine($"This order has {Order[0].Count} {choice}:{Order[0].GetName()}, Total price is {Order[0].Count * Order[0].GetPrice()}.");

                        //display vending machine item inventory
                        DisplayInventory(menu, lookup);
                    }
                }
                catch (ArgumentOutOfRangeException)
                {   //for example, it customer input A4, show this message.
                    Console.WriteLine("please input from A1-A3,B1-B3,C1-C3,D1-D3");

                }
                catch (KeyNotFoundException)
                {   //for example, it customer input z4, show this message.
                    Console.WriteLine("please input from A1-A3,B1-B3,C1-C3,D1-D3");
                }
            }
            // ask customer if he wants more. if answer if yes, then goes into a loop.
            while (DoYouWantMore())
            {
                DisplayMenu();
                choice = Console.ReadLine();
                Console.WriteLine("How many you want?");
                if (!Int32.TryParse(Console.ReadLine(), out qty))
                {
                    Console.WriteLine("Please enter an integer.");
                }
                else
                {
                    try
                    {
                        if (lookup[choice.ToUpper()].IsSoldOut() || qty > lookup[choice.ToUpper()].Count)
                        {
                            Console.WriteLine("The item you selected is either sold out or doesn't have enough quantity," +
                                       " please purchase other items");
                        }
                        else
                        {   //create follow on orders
                            var restOrders = new Item(qty, (lookup[choice.ToUpper()].GetName() + "_order"), lookup[choice.ToUpper()].GetPrice());

                            foreach (var item in Order)
                            {   // if customer select duplicate item in the previous order, only add quantity to the previous order
                                if (restOrders.GetName() == item.GetName())
                                {
                                    item.Count += qty;
                                    //create an empty order
                                    restOrders = new Item();
                                }

                            }
                            Order.Add(restOrders);
                            //remove empty order in the order list
                            Order.RemoveAll(item => item.Count == 0);
                            //reduce the count of the item in the vending machine by one.
                            lookup[choice.ToUpper()].Sold(qty);

                        }

                        //for every item in the order list, display the order information:
                        //quantity of item, location and item name.
                        DisplayOrderInfo(Order);
                        //display the inventory of the menu list including name location and quantity
                        DisplayInventory(menu, lookup);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        Console.WriteLine("please input from A1-A3,B1-B3,C1-C3,D1-D3");
                    }
                    catch (KeyNotFoundException)
                    {
                        Console.WriteLine("please input from A1-A3,B1-B3,C1-C3,D1-D3");
                    }
                }
            }
            double totalPrice = DisplayOrderInfo(Order);
            #endregion

            #region Payment system
            Console.WriteLine($"Please pay {totalPrice}.");
            Payment(Order, customer, moneyBills, moneyCoins);
            #endregion

            #region Notification
            foreach (var item in menu)
            {
                if (item.IsSoldOut())
                    Email.Send();
            }
            #endregion
        }
        #region methods
        /// <summary>
        /// Super user account, show information about menu inventory and bills and coins inventory 
        /// and a test function for email replenishment notification system
        /// </summary>
        private static void Superuser(List<Item> menu, Dictionary<string, Item> lookup,
           Dictionary<bills, int> bills, Dictionary<coins, int> coins)
        {
            //display welcome super user window
            Welcome("WelcomeSuper.txt", 65, 37);
            do
            {
                //display main menu, has 3 options
                MainMenu();

                Int32.TryParse(Console.ReadLine(), out int option);
                while (option > 3 && option <= 0)
                {   //if customer didn't choose from 1 to 3, choose again
                    Console.WriteLine("Please select from 1 to 3");
                    Int32.TryParse(Console.ReadLine(), out option);
                }
                switch (option)
                {
                    case 1:
                        //display menu items inventory
                        DisplayInventory(menu, lookup);
                        break;
                    case 2:
                        //display bills and coins inventory
                        DisplayMoneyInfo(bills, coins);
                        break;
                    case 3:
                        //email replenishment notification system
                        Console.WriteLine("\nReplenishment email notification is{0} functional.", Email.Send() ? "" : " NOT");
                        break;
                }
                Console.WriteLine("Exit? Y or N");
            } while (Console.ReadLine().ToUpper() == "N"); //if answer is no, return to main menu.
            //if answer is yes, exit the application
            Environment.Exit(0);
        }

        /// <summary>
        /// Display main menu in super user account
        /// </summary>
        private static void MainMenu()
        {
            Console.WriteLine("=======================");
            Console.WriteLine("|Option|     |Function|");
            Console.WriteLine("=======================");
            Console.WriteLine("  1           Inventory");
            Console.WriteLine("  2               Money");
            Console.WriteLine("  3               Test ");
            Console.WriteLine("=======================");
            Console.Write("\nPlease select an option: ");
        }

        /// <summary>
        /// Display vending machine bills and coins inventory
        /// </summary>
        private static void DisplayMoneyInfo(Dictionary<bills, int> bills, Dictionary<coins, int> coins)
        {
            Console.WriteLine("==============================");
            Console.WriteLine("|Money|                  |QTY|");
            Console.WriteLine("==============================");
            // display every bill and quantity
            foreach (var key in bills.Keys)
            {
                Console.WriteLine($"{key.ToString()}\t\t   {bills[key]}");
                Console.WriteLine("------------------------------");
            }
            // display every coin and quantity
            foreach (var key in coins.Keys)
            {
                Console.WriteLine($"{key.ToString()}\t\t\t   {coins[key]}");
                Console.WriteLine("------------------------------");
            }

        }

        /// <summary>
        /// Customer has options to payment method.
        /// </summary>
        private static void Payment(List<Item> Order, Customer customer, Dictionary<bills, int> bills, Dictionary<coins, int> coins)
        {
            Console.WriteLine("\nSelect payment method");
            Console.WriteLine("\n1. Cash\n2. Credit Card\n3. Debit Card");
            //read customer option, only integers
            Int32.TryParse(Console.ReadLine(), out int option);
            while (option > 3 && option <= 0)
            {   //if customer didn't choose from 1 to 3, choose again
                Console.WriteLine("Please select from 1 to 3");
                Int32.TryParse(Console.ReadLine(), out option);
            }
            switch (option)
            {
                case 1:
                    Console.WriteLine("\nPlease insert cash");
                    Cashpayment(Order, customer, bills, coins);
                    break;
                case 2:
                    Console.WriteLine("Please swipe your credit card");
                    CardPayment(Order);
                    break;
                case 3:
                    Console.WriteLine("Please swipe your debit card");
                    CardPayment(Order);
                    break;
            }
        }

        /// <summary>
        /// Cash payment method
        /// </summary>
        private static void Cashpayment(List<Item> Order, Customer cust, Dictionary<bills, int> bills, Dictionary<coins, int> coins)
        {
            decimal totalPrice = (decimal)DisplayOrderInfo(Order);
            //customer pay cash based on total order price
            cust.pay(totalPrice, coins, bills);
            Console.WriteLine("\nThank you and have a nice day!");
            Order.Clear();
        }

        /// <summary>
        /// Credit and debit card payment method and clear order
        /// </summary>
        private static void CardPayment(List<Item> Order)
        {
            Console.WriteLine("waiting to be approved");
            Thread.Sleep(1000);
            Console.WriteLine("Congratulations! Transaction approved.");
            Console.WriteLine("Thank you and have a nice day!");
            Order.Clear();
        }

        /// <summary>
        /// Display order information, include quantity and name.
        /// </summary>
        private static double DisplayOrderInfo(List<Item> Order)
        {
            Console.WriteLine("======== Order Info =========");
            Console.WriteLine("QTY\t   Name\t\tPrice");
            foreach (var item in Order)
            {
                Console.WriteLine($"{item.Count}\t {item.GetName()}\t {item.GetPrice()}");
            }
            Console.WriteLine("===========Total=============");
            //calculate total order item count
            int totalCount = Order.Sum(item => item.Count);
            //calculate total order price
            double totalPrice = Order.Sum(item => item.Count * item.GetPrice());
            Console.WriteLine($"{totalCount}\t\t\t {totalPrice}");
            Console.WriteLine("=============================");
            return totalPrice;
        }

        ///<summary>
        /// Ask user to answer yes or no to do you want more question
        /// </summary>
        private static bool DoYouWantMore()
        {
            Console.WriteLine("Do you want more?");
            string ans = Console.ReadLine();
            //case insensitive, if customer didn't input yYnN,show message.
            while (ans.ToLower() != "y" && ans.ToLower() != "n")
            {
                Console.WriteLine("Please enter Y or N");
                Console.WriteLine("Do you want more?");
                ans = Console.ReadLine();
            }
            return ans.ToLower() == "y";
        }

        ///<summary>
        ///Display inventory information after you make orders
        /// </summary>
        private static void DisplayInventory(List<Item> menu, Dictionary<string, Item> lookup)
        {
            Console.WriteLine("\n**********Inventory************");
            Console.WriteLine("Location    Name     QTY");
            Console.WriteLine("-------------------------------");
            for (int i = 0; i < menu.Count; i++)
            {   // display menu items location, name and quantity
                Console.WriteLine("|{0,5}|{1,10}|{2,5}|", lookup.FirstOrDefault(x =>
                (x.Value.GetName() == menu[i].GetName())).Key, menu[i].GetName(), menu[i].Count);
            }
            Console.WriteLine("*********************************");
        }

        /// <summary>
        /// To display my tiny vending machine
        /// </summary>
        private static void DisplayMenu()
        {
            //set console window to 52 columns width and 27 rows height for a vending machine.
            Console.SetWindowSize(52, 27);
            //reset the font color to white on the console window.
            Console.ForegroundColor = ConsoleColor.White;
            // read all lines from menu.txt into a string array.
            string[] file = File.ReadAllLines("menu.txt");

            //for every line in the file string array, print to the console window.
            foreach (string str in file)
            {
                Console.WriteLine(str);
            }
            Console.WriteLine("Please select the item you'd like to purchase");
        }

        /// <summary> 
        /// A welcome window using ASCII art.
        /// </summary>
        private static void Welcome(string path, int x, int y)
        {
            Console.Clear();
            //set console window to 150 columns width and 20 rows height.
            Console.SetWindowSize(x, y);
            //set the font color to yellow on the console window.
            Console.ForegroundColor = ConsoleColor.Yellow;
            // read all lines from Welcome.txt into a string array.
            string[] file = File.ReadAllLines(path);
            //for every line in the file string array, print to the console window.
            foreach (string str in file)
            {
                Console.WriteLine(str);
            }
            //pause application for 3 seconds.
            Thread.Sleep(3000);
            // clear the content on console window.
            Console.Clear();
        }
    }
    #endregion

    #region classes
    public class Item
    {
        public int Count;
        private double Price { get; }
        private string Name { get; }

        public Item()
        {
        }

        public Item(int count)
        {
            Count = count;
        }

        public Item(int count, string name, double price)
        {
            Count = count;
            Name = name;
            Price = price;
        }

        public string GetName()
        {
            return Name;
        }

        public int GetCount()
        {
            return Count;
        }

        public double GetPrice()
        {
            return Price;
        }

        public void Sold(int qty)
        {
            //if sold, deduct the quantity from item count
            Count -= qty;
            if (Count == 0)
            {
                Console.WriteLine("You are lucky, grabbed the last one(s).");
            }
        }

        public bool IsSoldOut()
        {
            return Count == 0;
        }

    }

    public class Customer
    {   //customer has Bills and Coins property means how much money he has.
        public Dictionary<coins, int> Coins { get; set; }
        public Dictionary<bills, int> Bills { get; set; }
        //constructor, initialize how much money customer has.
        public Customer(Dictionary<coins, int> coins, Dictionary<bills, int> bills)
        {
            Coins = coins;
            Bills = bills;
        }
        /// <summary>
        /// pay method to determine how much he pays for his order and get change from vending machine.
        /// </summary>
        public void pay(decimal totalPrice, Dictionary<coins, int> moneyCoins, Dictionary<bills, int> moneyBills)
        {
            //which bill customer takes to pay based on total order price.
            int cash = AmountPay(totalPrice);
            Console.WriteLine($"\nYou paid {cash} dollars.");
            Console.WriteLine("\nHere is you change.");
            Console.WriteLine("\n++++++++++++++++++++++++++");
            Console.WriteLine("{0,-20}{1,5}", "Money", "QTY");
            Console.WriteLine("---------------------------");
            //once paid, put change into dictionary to store quantity and bills and coins
            Dictionary<string, int> change = GetChange(totalPrice, cash);
            //remove empty bill and coin slot from dictionary
            foreach (var item in change.Where(i => i.Value == 0).ToList())
            {
                change.Remove(item.Key);
            }
            //display different bills and coins and quantities customer get for change.
            foreach (var item in change)
            {
                Console.WriteLine($"{item.Key,-20}{item.Value,5}");
            }
            Console.WriteLine("++++++++++++++++++++++++++");


        }
        /// <summary>
        /// GetChange method used to determine which bill and coin and quantities customer can get for changes.
        /// and the same time, update Bills and Coins dictionary for customers and vending machine.
        /// </summary>
        private Dictionary<string, int> GetChange(decimal totalPrice, int cash)
        {   //exact change
            Decimal difference = cash - totalPrice;
            //number of ten_dollar_bill
            int num_tenbill = (int)(difference / 10);
            //leftover after ten dollar bill is given
            Decimal leftover = difference - 10 * num_tenbill;
            //number of five_dollar_bill
            int num_fivebill = (int)(leftover / 5);
            //leftover after five dollar bill is given
            leftover -= num_fivebill * 5;
            //number of one_dollar_bill
            int num_onebill = (int)leftover;
            //leftover after five dollar bill is given
            leftover -= num_onebill;
            //number of quarter coin, there is float point issue, 
            //so cast double to decimal to solve the problem
            int num_quartercoin = (int)(leftover / (decimal)0.25);
            //leftover after quarter coin is given
            leftover -= num_quartercoin * (decimal)0.25;
            //number of dime coin
            int num_dimecoin = (int)(leftover / (decimal)0.1);
            //leftover after quarter coin is given
            leftover -= num_dimecoin * (decimal)0.1;
            //number of nickel coin
            int num_nickelcoin = (int)(leftover / (decimal)0.05);
            //leftover after nickel coin is given
            leftover -= num_nickelcoin * (decimal)0.05;
            //number of penny coin
            int num_pennycoin = (int)(leftover / (decimal)0.01);

            //update customer.Bills dictionary
            Bills[bills.ten_dollar] += num_tenbill;
            Bills[bills.five_dollar] += num_fivebill;
            Bills[bills.one_dollar] += num_onebill;
            Coins[coins.quarter] += num_quartercoin;
            Coins[coins.dime] += num_dimecoin;
            Coins[coins.nickel] += num_nickelcoin;
            Coins[coins.penny] += num_pennycoin;
            //initialize change dictionary to store the quantity
            var change = new Dictionary<string, int>();
            change.Add("penny", num_pennycoin);
            change.Add("nickel", num_nickelcoin);
            change.Add("dime", num_dimecoin);
            change.Add("quarter", num_quartercoin);
            change.Add("one_dollar_bill", num_onebill);
            change.Add("five_dollar_bill", num_fivebill);
            change.Add("ten_dollar_bill", num_tenbill);
            return change;
        }
        /// <summary>
        /// This method determines how much customer will pay bill based on the total price
        /// It is simplified. Future development can include customer have different combinations of bills to pay.
        /// </summary>
        public int AmountPay(decimal totalPrice)
        {   // if total price less than 1 dollar, customer pay with one dollar bill
            if (totalPrice <= 1)
            {
                Bills[bills.one_dollar]--;
                return 1;
            } // if total price less than 5 dollar, customer pay with five dollar bill
            else if (totalPrice <= 5)
            {
                Bills[bills.five_dollar]--;
                return 5;
            } // if total price less than 10 dollar, customer pay with ten dollar bill
            else if (totalPrice <= 10)
            {
                Bills[bills.ten_dollar]--;
                return 10;
            } // if total price less than 20 dollar, customer pay with twenty dollar bill
            else if (totalPrice <= 20)
            {
                Bills[bills.twenty_dollar]--;
                return 20;
            }
            else
            { // if total price more than 20 dollar, exit application
                Console.WriteLine("You try to purchase too much, please split your order.");
                Environment.Exit(0);
                //Order.clear();
                return -1;
            }
        }
    }

    public class Email
    {

        /// <summary>
        /// send email source:http://csharp.net-informations.com/communications/csharp-smtp-mail.htm
        /// </summary>
        public static bool Send()
        {
            try
            {

                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                string user = File.ReadAllText("emailuser.txt");
                mail.From = new MailAddress(user);
                mail.To.Add("yichun.sun@outlook.com");
                mail.Subject = "Vending machine replenishment";
                mail.Body = "CS205 Vending machine needs replenishment. hurry UP!";

                SmtpServer.Port = 587;
                string password = File.ReadAllText("emailpassword.txt");
                SmtpServer.Credentials = new System.Net.NetworkCredential(user, password);
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
                return true;
            }

            catch (SmtpException ex)
            {
                throw new ApplicationException
                  ("SmtpException has occurred: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
    #endregion
}
