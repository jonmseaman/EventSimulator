﻿using MathNet.Numerics.Distributions;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EventSimulator.SellerSite
{
    public class SiteHelper
    {
        static SiteHelper()
        {
            // Load product data from file
            var parser = new TextFieldParser(new StreamReader("Data/SellerSite/Products.csv"))
            {
                TextFieldType = FieldType.Delimited
            };
            parser.SetDelimiters(",");
            // Load data while can
            while (!parser.EndOfData)
            {
                var data = parser.ReadFields();
                // Add that data to our list
                productData.Add(data);
                try
                {
                    // Get the product id from data
                    int productId = int.Parse(data[ProductIdIndex]);
                    // Add the data to the map
                    productDictionary.Add(productId, data);
                }
                catch (Exception e) when (e is ArgumentOutOfRangeException || e is IndexOutOfRangeException)
                {
                    throw new FormatException("Malformed products.csv");
                }

            }

        }

        #region Private Members
        private static List<string[]> productData = new List<string[]>();
        private static Dictionary<int, string[]> productDictionary = new Dictionary<int, string[]>();

        private const int ProductPriceIndex = 4;
        private const int ProductIdIndex = 0;

        private static string HomePageUrl { get; } = "/";
        private static string ProductPageUrl { get; } = "/products/";
        #endregion

        #region Randoms

        /// <summary>
        /// Randomizer used in methods internally.
        /// </summary>
        private static readonly Random Random = new Random();

        /// <summary>
        /// Returns true, on average, <code>percent</code> out of every 100 times.
        /// </summary>
        /// <param name="percent">The chance that this function will return true.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when percent not in range [0,100].</exception>
        /// <returns>True percent out of every 100 calls.</returns>
        public static bool Chance(int percent)
        {
            if (percent < 0 || 100 < percent)
            {
                throw new ArgumentOutOfRangeException("Percent not in range [0,100].");
            }
            return Random.Next(0, 99) < percent;
        }

        public static int RandomTransactionNumber()
        {
            return Random.Next(1, int.MaxValue);
        }

        public static string RandomEmail()
        {
            var rand = (int)Normal.Sample(Random, 25000, 7500);
            return $"user{rand}@example.com";
        }

        public static int RandomProductId()
        {
            // Get num items
            var cnt = productData.Count;
            // Standard dev. and mean.
            var sd = cnt / 0.25;
            var mu = cnt / 0.5;
            var randIndex = (int)Normal.Sample(Random, mu, sd);
            // Make sure index in range of productData
            randIndex = randIndex < 0 ? 0 : randIndex;
            randIndex = randIndex >= cnt ? cnt - 1 : randIndex;
            // Try to get the product id from the selected data.
            int productId;
            var idStr = productData[randIndex][ProductIdIndex];
            int.TryParse(idStr, out productId);

            return productId;
        }

        public static int RandomPrice()
        {
            var rand = (int)Normal.Sample(Random, 1250, 250);
            rand = rand < 250 ? 250 : rand;
            return rand;
        }

        public static int GetPrice(int productId)
        {
            int price;
            try
            {
                // Must convert to price in cents.
                double dPrice;
                double.TryParse(productDictionary[productId][ProductPriceIndex], out dPrice);
                price = (int)Math.Round(100.0 * dPrice);
            }
            catch (ArgumentOutOfRangeException)
            {
                price = RandomPrice();
            }
            return price;
        }

        public static int RandomProductQuantity()
        {
            var rand = (int)Normal.Sample(Random, 5, 2);
            rand = rand < 1 ? 1 : rand;
            return rand;
        }

        public static string RandomUrl()
        {
            var rand = Random.Next(0, 9);
            // 30% to go back to the home page.
            if (rand < 3)
            {
                return HomePageUrl;
            }
            else
            {
                return RandomProductUrl();
            }
        }

        public static string RandomProductUrl()
        {
            return $"{ProductPageUrl}{RandomProductId()}";
        }

        #endregion

        #region Utilities

        public static bool IsUrlAProductPage(string url)
        {
            return Regex.Match(url, $"({ProductPageUrl})[0-9]+").Length == url.Length;
        }

        public static bool IsUrlTheHomePage(string url)
        {
            return url == HomePageUrl;
        }

        public static string ProductUrlFromId(int productId)
        {
            return $"{ProductPageUrl}{productId}";
        }

        /// <summary>
        /// Extracts the product id from a product page url.
        /// </summary>
        /// <param name="nextUrl">A url corresponding to a product page.</param>
        /// <exception cref="ArgumentException">Thrown if the url is not a product url.</exception>
        /// <returns></returns>
        public static int ProductIdFromUrl(string nextUrl)
        {
            if (!nextUrl.StartsWith(ProductPageUrl))
            {
                throw new ArgumentException("The url does not correspond to a product page.");
            }
            var prodIdStr = nextUrl.Substring(ProductPageUrl.Length);
            return int.Parse(prodIdStr);
        }

        #endregion

    }
}