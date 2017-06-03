using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace cardSorting
{
    class Program
    {
        static void Main(string[] args)
        {
            List<String> cards = new List<string>();
            //Read in string from command line to open a text file
            //If no file given use hard coded string
            //Visual studio does not include the program file as a commmand line parameter
            //Only check if the file is present then
            if (args.Length > 0)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(args[0]))
                    {
                        String line = sr.ReadToEnd();

                        cards.AddRange(line.Split(new char[] { ' ', '\u000A', ',', '.', ';', ':', '-', '_', '/', '\n' },
                                       StringSplitOptions.RemoveEmptyEntries));
                        for (int i = 0; i < cards.Count; ++i)
                        {
                            cards[i] = Regex.Replace(cards[i], @"\r\n?|\n", String.Empty);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("File could not be read.... using generic input.");
                    Console.WriteLine(e.Message);
                    cards = new List<String> { "3c", "Js", "2d", "10h", "Kh", "8s", "Ac", "4h" };
                }
            }

            else
            {
                Console.WriteLine("No file given... Using generic input");
                cards = new List<String> { "3c", "Js", "2d", "10h", "Kh", "8s", "Ac", "4h" };
                /*
                 * //This code below generates 10 million random cards to be put in the hand.
                 * //The code was running too fast with the small input given and the duplicated input
                 * //in the text file. There are print statements below that I do not suggest using 
                 * //with the random number generation as it will be an overwhelming print amount.
                 * //The print statements are the foreach loops printing the cards before and after the tests.
                
                Console.WriteLine("No file given... Creating cards randomly");
                cards = RandomCardGenerator(10000000);
                Console.WriteLine("Cards finished generating");
                */
            }

            /*
             * Implementing one version for to be faster and less memory effecient
             * The other implementation will sacrafice some speed for the sake of memory effeciency
             */

            List<string> cards2 = new List<string>(cards);

            Stopwatch timer = new Stopwatch();

            /*
             * This is the implementation where I was trying to create a faster sorting implementation
             * at the cost of using more memory. The implementation creates a dictionary using the suit
             * as the key. Then spot in the dictionary holds a list of Card objects. This implementation
             * allows to thread the sorting on each suit list, but comes at the cost of creating n Card
             * objects. This implementation is also able to expand more easily and could be used to further 
             * implement more interactivity with the cards.
             * 
             * The overall timing of this implementation looks slower than the memory implementation, but
             * this is due to the overhead of creating the card objects. If this was done as a preprocessing
             * step then the sorting is actually faster. This is shown with the raw sorting speeds reported
             * from within the function.
             */
            Console.WriteLine("Starting speed implementation...");
            Console.WriteLine("Deck before sorting:");
            foreach (var item in cards)
                Console.Write(item + " ");
            Console.Write("\n");
            timer.Start();
            Speed(cards);
            timer.Stop();
            TimeSpan ts = timer.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("Speed implementation ran in: " + elapsedTime);
            Console.WriteLine("Deck after speed sort:");
            foreach (var item in cards)
                Console.Write(item + " ");
            Console.WriteLine("\n");

            /*
             * This implementation was simply trying to be more memory effecient. The overall time is faster
             * since the creation of the n card objects are not needed and LINQ is used to group the cards in
             * the list by suit and order the suites as specified using a custom icomparer. Then within the 
             * suit groups the cards are then ordered by number in ascending order from 2-A. In order to work
             * around the problem of comparing letters and numbers for the value, the letters are converted to
             * their respective number value J=11, Q=12, K=13, A=14. This allowed for an easy comparison. This
             * is done in both implementations and does add to the processing time, but if the cards were to be
             * further used in a game they could be kept in the number format and changed on the fly as needed
             * display correctly to the user. 
             * 
             * The overall timing of this implementation looks slightly faster due to the fact that the
             * implementation does not require that n Card objects be made. An ienumerable object is created
             * using LINQ and then this ienumerable can be iterated over to insert the cards in the correct order
             * in the original list. The raw sorting speed of this implementation is slightly slower than the other
             * implementation.
             */
            Console.WriteLine("Starting memory implementation...");
            Console.WriteLine("Deck before sorting:");
            foreach (var item in cards2)
                Console.Write(item + " ");
            Console.Write("\n");
            timer.Reset();
            timer.Start();
            Memory(cards2);
            timer.Stop();
            ts = timer.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("Memory implementation ran in: " + elapsedTime);
            Console.WriteLine("Deck after memory sort:");
            foreach (var item in cards2)
                Console.Write(item + " ");
            Console.WriteLine("\n");

        }

        /*
         * Description: This function is used to generate a number of random cards
         * that can then be used for the other algorithms.
         * 
         * Parameters: int num = The number of cards to be generated.
         * 
         * Return: List<string> = Returns a list of string values representing cards
         * with the the letter value and not the number value for face cards.
         */
        static List<string> RandomCardGenerator(int num)
        {
            List<string> cards = new List<string>();
            List<string> Order = new List<string> { "d", "s", "c", "h" };
            Random rnd = new Random();
            string value = "", suit = "";

            for (int i = 0; i < num; ++i)
            {
                //Create a random number between 2 and 14 for value
                value = Convert.ToString(rnd.Next(2, 15));
                //Create a random number between 0 and 3 for suit
                suit = Order[rnd.Next(0, 4)];

                cards.Add(value + suit);
            }
            return NumsToFaceValue(cards);
        }

        /*
         * Description: This function creates a dictionary containing all the cards
         * that are passed in. The key is the suit and the value is a List<Card>. 
         * These lists are then sorted within threads to increase the speed of the sorting.
         * The cards are put back into the original List<string> format that was passed in
         * to be printed outside this function.
         * 
         * Parameters: List<string> cards = A list containing all of the cards to be sorted
         * 
         * Return: List<string> = Returns a list of string values representing cards
         * with the the letter value and not the number value for face cards in sorted a 
         * custom suit order and in ascending number order within each suit.
         */
        static List<string> Speed(List<string> cards)
        {
            /*
            * Need to split list into 4 sub lists based on suit
            * Sort each sub list based on value and then merge them back together
            * 
            * Note: Threading or parallel processing could make this more effecient for very large data sets.
            *       For this small data set it will actually hinder it since the time to create the threads will
            *       slow it down more than the speed gained from the conccurent processing.
            */
            cards = FaceValueToNums(cards);
            List<string> Order = new List<string> { "d", "s", "c", "h" };

            Dictionary<string, List<Card>> Deck = new Dictionary<string, List<Card>>
                                                  { {"h", new List<Card>() }, {"d", new List<Card>() },
                                                  { "s", new List<Card>() }, {"c", new List<Card>() } };

            //Adding items to correct sub list
            foreach (string item in cards)
            {
                string suit = Convert.ToString(item[item.Length - 1]);
                Deck[suit].Add(new Card(suit, item.Substring(0, (item.Length - 1))));
            }

            Stopwatch timer = new Stopwatch();
            timer.Start();

            List<Thread> tHolder = new List<Thread>();
            foreach (var item in Order)
            {
                tHolder.Add(new Thread(() => { Deck[item] = SpeedHelper(Deck[item]); }));
                tHolder.Last().Start();
            }

            foreach (var item in tHolder)
                item.Join();

            timer.Stop();
            TimeSpan ts = timer.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("Raw speed sorting time: " + elapsedTime);

            int idx = 0;
            foreach (var item in Order)
            {
                foreach (var c in Deck[item])
                {
                    cards[idx++] = c.Name();
                }
            }

            return NumsToFaceValue(cards);
        }

        /*
         * Description: This function is used within the threads to sort the cards of
         * each suit in ascending order.
         * 
         * Parameters: List<Card> cards = A list of card objects to be sorted
         * 
         * Return: List<Card> = The sorted list of card objects.
         */
        static List<Card> SpeedHelper(List<Card> cards)
        {
            int idx = 0;
            foreach (var item in cards.OrderBy(g => g.Value))
            {
                cards[idx++] = item;
            }
            return cards;
        }


        /*
         * Description: This function uses LINQ to sort the cards within the list and does
         * not need to create other objects to hold the cards except the ienumerable returned
         * from the LINQ processing. The cards are put back into the original List<string> 
         * format that was passed in to be printed outside this function.
         * 
         * Parameters: List<string> cards = A list containing all of the cards to be sorted
         * 
         * Return: List<string> = Returns a list of string values representing cards
         * with the the letter value and not the number value for face cards in sorted a 
         * custom suit order and in ascending number order within each suit.
         */
        static List<string> Memory(List<string> cards)
        {
            cards = FaceValueToNums(cards);

            Stopwatch timer = new Stopwatch();
            timer.Start();

            var sorted = cards.GroupBy(s => Convert.ToString(s[s.Length - 1]))
                .OrderBy(g => g.Key, new SuitComparer(StringComparer.CurrentCulture))
                .SelectMany(g => g.OrderBy(c => Convert.ToInt32(c.Substring(0, c.Length - 1))));
            int idx = 0;
            foreach (var item in sorted)
            {
                cards[idx++] = item;
            }

            timer.Stop();
            TimeSpan ts = timer.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("Raw mem sorting time: " + elapsedTime);

            return NumsToFaceValue(cards);
        }

        /*
         * Description: This function converts all face cards from letter form
         * to their respective number within a list.
         * 
         * Parameters: List<string> cards = A list of strings that represent cards
         * that need to have the face cards converted from letters to numbers.
         * 
         * Return: List<string> = The list of strings representing cards that now
         * have the face cards represented by numbers instead of letters.
         */
        static List<string> FaceValueToNums(List<string> cards)
        {
            for (int i = 0; i < cards.Count; ++i)
            {
                if (cards[i].StartsWith("J"))
                    cards[i] = cards[i].Replace("J", "11");
                if (cards[i].StartsWith("Q"))
                    cards[i] = cards[i].Replace("Q", "12");
                if (cards[i].StartsWith("K"))
                    cards[i] = cards[i].Replace("K", "13");
                if (cards[i].StartsWith("A"))
                    cards[i] = cards[i].Replace("A", "14");
            }
            return cards;
        }

        /*
         * Description: This function converts all face cards from number form
         * to their respective letter within a list.
         * 
         * Parameters: List<string> cards = A list of strings that represent cards
         * that need to have the face cards converted from numbers to letters.
         * 
         * Return: List<string> = The list of strings representing cards that now
         * have the face cards represented by letters instead of numbers.
         */
        static List<string> NumsToFaceValue(List<string> cards)
        {
            for (int i = 0; i < cards.Count; ++i)
            {
                if (cards[i].StartsWith("11"))
                    cards[i] = cards[i].Replace("11", "J");
                if (cards[i].StartsWith("12"))
                    cards[i] = cards[i].Replace("12", "Q");
                if (cards[i].StartsWith("13"))
                    cards[i] = cards[i].Replace("13", "K");
                if (cards[i].StartsWith("14"))
                    cards[i] = cards[i].Replace("14", "A");
            }
            return cards;
        }
    }

    /*
     * Description: This class is used to store the cards in an actual object
     * to make them easier to expand on for future use. This class is used within
     * the speed implementation above. 
     * 
     * Variables: string Suit = suit of a card
     *            int Value = value of a card
     * 
     * Functions: Constructor = Takes in values for suit and value both as strings
     *            and sets the variables within the object to those values.
     *            
     *            Name = Coverts and combines the value and suit of a card object
     *            to be returned as a string in the original form that the cards
     *            start in.
     */
    class Card
    {
        public string Suit { get; set; }
        public int Value { get; set; }

        public Card(string suit, string value)
        {
            this.Suit = suit;
            this.Value = Convert.ToInt32(value);
        }

        public string Name()
        {
            return Convert.ToString(this.Value) + this.Suit;
        }
    }

    /*
     * Description: This class creates a custom icomparer that is used in the memory
     * function above to sort the suites in a custom order of d, s, c, h currently.
     */
    class SuitComparer : IComparer<string>
    {
        private readonly IComparer<string> _baseComparer;
        public SuitComparer(IComparer<string> basecomparer)
        {
            _baseComparer = basecomparer;
        }
        public int Compare(string x, string y)
        { 
            if (_baseComparer.Compare(x, y) == 0)
                return 0;

            // Ordering by suit based on order specified in write-up
            //Suit should order d, s, c, h
            if (_baseComparer.Compare(x, "d") == 0)
                return -1;
            if (_baseComparer.Compare(y, "d") == 0)
                return 1;

            if (_baseComparer.Compare(x, "s") == 0)
                return -1;
            if (_baseComparer.Compare(y, "s") == 0)
                return 1;

            if (_baseComparer.Compare(x, "c") == 0)
                return -1;
            if (_baseComparer.Compare(y, "c") == 0)
                return 1;

            if (_baseComparer.Compare(x, "h") == 0)
                return -1;
            if (_baseComparer.Compare(y, "h") == 0)
                return 1;
            return 0;
        }
    }
}
