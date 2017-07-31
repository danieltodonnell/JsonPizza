using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net;
using System.IO;

namespace JsonPizza
{
    class Program
    {
        static void Main(string[] args)
        {

            Pizza[] pizzas = readPizzas();
            Combos combos = countToppings(pizzas);
            WriteReport(combos);

        }

        /// <summary>
        /// Open a json stream and parse the response into a JSON array
        /// </summary>
        /// <returns>Array of pizza orders</returns>
        static Pizza[] readPizzas()
        {
            // Requirements asked for "throwaway code", so I'm not putting any error handling in here
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://files.olo.com/pizzas.json");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string rawJson = new StreamReader(response.GetResponseStream()).ReadToEnd();

            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(Pizza[]));
            MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(rawJson));

            Pizza[] pizzas = (Pizza[])js.ReadObject(ms);
            ms.Close();

            return pizzas;
        }

        /// <summary>
        /// Loops through ordered pizzas and counts them
        /// </summary>
        /// <param name="pizzas"></param>
        /// <returns>List of counted and concatinated toppings</returns>
        static Combos countToppings(Pizza[] pizzas)
        {
            var combos = new Combos();
            foreach (Pizza pizza in pizzas)
            {
                // comparing string arrays is expensive and hard, so joining them once is cheaper
                string combo = String.Join(",", pizza.toppings);

                int value;
                //if (combos.ContainsKey(combo))
                if (combos.TryGetValue(combo, out value))
                {
                    combos[combo] = value + 1;
                }
                else
                {
                    combos.Add(combo, 1);
                }
            }
            return combos;
        }

        /// <summary>
        /// Report for Top 20 toppings orders with quantities
        /// </summary>
        /// <param name="combos"></param>
        static void WriteReport(Combos combos)
        {
            List<string> sortedList = combos.OrderByDescending(t => t.Value)
                                                  .Select(t => t.Key)
                                                  .ToList();

            Console.WriteLine("\n TOP 20 TOPPINGS REPORT ");
            Console.WriteLine("==========================");
            Console.WriteLine("Rank Quantity  Toppings");
            Console.WriteLine("--------------------------");
            int count;
            string toppings;
            for (int index = 0; index < 20; index++)
            {
                toppings = sortedList[index];
                count = combos[toppings];
                Console.WriteLine((index + 1).ToString().PadRight(5) + 
                    count.ToString().PadRight(10) 
                    + toppings);
            }
            Console.WriteLine("==========================");
        }
    }

    /// <summary>
    /// POCO to recreate JSON feed into
    /// </summary>
    [DataContract]
    class Pizza
    {
        [DataMember]
        public String[] toppings;
    }

    /// <summary>
    /// Shorthand for toppings combination dictionary
    /// </summary>
    class Combos : SortedDictionary<string, int> {};

}
