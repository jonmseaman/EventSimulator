
namespace EventSimulator.SellerSite
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using MathNet.Numerics.Distributions;
    using Microsoft.VisualBasic.FileIO;

    /// <summary>
    /// The class contains tools specific to the ecomerce site such as random
    /// data generation and loading data from the product catalog. 
    /// </summary>
    public class SiteHelper
    {
        /// <summary>
        /// Sets up the <see cref="SiteHelper"/> by loading the product catalog.
        /// </summary>
        static SiteHelper()
        {
            // Load product data from file
            var parser = new TextFieldParser(new StreamReader("Data/SellerSite/Products.csv"))
            {
                TextFieldType = FieldType.Delimited
            };
            parser.SetDelimiters(",");

            // Load data while can
            var index = -1;
            while (!parser.EndOfData)
            {
                index++;
                var data = parser.ReadFields();

                // Add that data to our list
                ProductData.Add(data);
                try
                {
                    // Get the product id from data
                    var productId = int.Parse(data[ProductIdIndex]);

                    // Add the data to the map
                    if (!ProductDictionary.ContainsKey(productId))
                    {
                        ProductIndexDictionary.Add(productId, index);
                        ProductDictionary.Add(productId, data);
                    }
                }
                catch (Exception e) when (e is ArgumentOutOfRangeException || e is IndexOutOfRangeException)
                {
                    throw new FormatException("Malformed products.csv");
                }
            }
        }

        #region Private Members

        /// <summary>
        /// List of all product datas
        /// </summary>
        private static readonly List<string[]> ProductData = new List<string[]>();

        /// <summary>
        /// ProductId, ProductData map.
        /// </summary>
        private static readonly Dictionary<int, string[]> ProductDictionary = new Dictionary<int, string[]>();

        /// <summary>
        /// ProductId, Index in ProductData. Used to change the distribution of the product data.
        /// </summary>
        private static readonly Dictionary<int, int> ProductIndexDictionary = new Dictionary<int, int>();

        /// <summary>
        /// Indicates that the ProductId is the first column in CSV.
        /// </summary>
        private const int ProductIdIndex = 0;

        /// <summary>
        /// Indicates that the Price is the second column in CSV.
        /// </summary>
        private const int ProductPriceIndex = 1;

        /// <summary>
        /// The url of the home page on the ecommerce site.
        /// </summary>
        private static string HomePageUrl { get; } = "/";

        /// <summary>
        /// The url prefix for product pages.
        /// </summary>
        private static string ProductPageUrl { get; } = "/products/";
        #endregion

        #region Randoms

        /// <summary>
        /// Randomizer used in methods internally.
        /// </summary>
        private static readonly Random Random = new Random();

        /// <summary>
        /// Returns true, on average, <see cref="percent"/> out of every 100 times.
        /// </summary>
        /// <param name="percent">The chance that this function will return true.</param>
        /// <returns>True <see cref="percent"/> out of every 100 calls. If the supplied
        /// percent is less than zero, returns false, if the supplied percent is greater
        /// than 100, returns true.</returns>
        public static bool Chance(int percent)
        {
            return Random.Next(0, 99) < percent;
        }

        /// <summary>
        /// Provides a number to use as a transaction number.
        /// </summary>
        /// <returns>The transaction number.</returns>
        public static int RandomTransactionNumber()
        {
            return Random.Next(1, int.MaxValue);
        }

        /// <summary>
        /// Provides a random email to be used as a user id for the
        /// ecommerce site.
        /// </summary>
        /// <returns>The user's email.</returns>
        public static string RandomEmail()
        {
            var rand = (int)Normal.Sample(Random, 25000, 7500);
            return $"user{rand}@example.com";
        }

        /// <summary>
        /// Gets a random product id from the product database.
        /// </summary>
        /// <returns>The product id.</returns>
        public static int RandomProductId()
        {
            // Get num items
            var cnt = ProductData.Count;

            // Standard dev. and mean.
            var sd = cnt * 0.16;
            var mu = cnt * 0.5;
            var randIndex = (int)Normal.Sample(Random, mu, sd);

            // Make sure index in range of productData
            randIndex = randIndex < 0 ? 0 : randIndex;
            randIndex = randIndex >= cnt ? cnt - 1 : randIndex;

            // Try to get the product id from the selected data.
            int productId;
            var idStr = ProductData[randIndex][ProductIdIndex];
            int.TryParse(idStr, out productId);

            return productId;
        }

        /// <summary>
        /// Returns a product id from a product that is close by in the
        /// product database. This is meant to produce a similar product.
        /// </summary>
        /// <param name="productId">The product id of the item</param>
        /// <returns></returns>
        public static int SimilarProductId(int productId)
        {
            if (!ProductIndexDictionary.ContainsKey(productId))
                throw new ArgumentOutOfRangeException(nameof(productId));

            var oldIndex = ProductIndexDictionary[productId];
            var newIndex = oldIndex == ProductData.Count - 1 ? 0 : oldIndex + Random.Next(-10, 10);
            newIndex = newIndex % ProductData.Count;
            if (newIndex < 0)
            {
                newIndex = ProductData.Count + newIndex;
            }

            // Get the product id associated with that index.
            int newProductId;
            var idStr = ProductData[newIndex][ProductIdIndex];
            int.TryParse(idStr, out newProductId);

            return newProductId;
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
                double.TryParse(ProductDictionary[productId][ProductPriceIndex], out dPrice);
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
