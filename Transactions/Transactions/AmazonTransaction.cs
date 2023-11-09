using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using static SpendingInfo.Transactions.Transactions.ITransaction;


namespace SpendingInfo.Transactions.Transactions
{
    public class AmazonTransaction : TransactionBase, ITransaction, INotifyPropertyChanged
    {
        // Categories for classification
        private static readonly int UNKNOWN_CATEGORY_INT = -1;
        public static readonly string UNKNOWN_CATEGORY = "Unknown";

        private static IList<string> categories = new string[] { "Tech", "Clothes", "Other" };
        private static IList<string> searchableCategories = (from c in categories select c.ToLower()).ToList();
        public static IReadOnlyList<string> Categories 
        { 
            get 
            {
                return (IReadOnlyList<string>)categories;
            }

            set
            {
                categories = (IList<string>) value;
                searchableCategories = (from c in value select c.ToLower()).ToList();
            } 
        }

        private int category;
        private string categoryName;
        public int Category
        {
            get
            {
                return category;
            }

            set
            {
                category = value;
                if (value >= 0 && value < Categories.Count)
                {
                    categoryName = Categories[value];
                }
                else
                {
                    category = -1;
                    categoryName = UNKNOWN_CATEGORY;
                }

                NotifyPropertyChanged(nameof(Category));
            }
        } // category string is Categories[category]

        public string CategoryName
        {
            get { return categoryName; }
        }

        // shouldn't ever need to modify ASIN
        private string asin { get; set; }
        public string ASIN { get { return asin; } }

        public AmazonTransaction(string id, DateTime date, float amount, string description, string ASIN)
            : base(id, date, amount, description, TransactionType.AMAZON)
        {
            asin = ASIN;
            categoryName = UNKNOWN_CATEGORY;
            Category = UNKNOWN_CATEGORY_INT;
        }

        public AmazonTransaction(string id, DateTime date, float amount, string description, string ASIN, int category)
            : base(id, date, amount, description, TransactionType.AMAZON)
        {
            asin = ASIN;
            categoryName = UNKNOWN_CATEGORY;
            Category = category;
        }

        public AmazonTransaction(AmazonTransaction t)
            : base(t.ID, t.Date, t.Amount, t.Description, t.Type)
        {
            asin = t.ASIN;
            categoryName = t.CategoryName;
            Category = t.Category;
        }

        public string GetASIN() => ASIN;
        public string GetCategoryStr() => CategoryName;
        public void SetDescription(string description) => this.description = description;
        public override string ToString() => ITransaction.ToString(this) + $"{GetCategoryStr()} |";

        public dynamic ToCsvRecord()
        {
            dynamic record = new ExpandoObject();
            record.ID = ID;
            record.Date = Date;
            record.Amount = Amount;
            record.Description = Description;
            record.Category = CategoryName; // TODO: store string, and when loading, if string does not match any Category, set unknown
            record.ASIN = ASIN;
            return record;
        }

        public static AmazonTransaction FromCsvRecord(dynamic record)
        {
            return new AmazonTransaction
            (
                record.ID,
                record.Date,
                record.Amount,
                record.Description,
                record.ASIN,
                ValidatePotentialCategory(record.Category)
            );
        }

        /// <summary>
        /// Converts a string to a valid category index
        /// </summary>
        /// <param name="categoryStr"></param>
        /// <returns>An index in {AmazonTransaction.Categories}</returns>
        private static int ValidatePotentialCategory(string categoryStr) 
        {
            categoryStr = categoryStr.ToLower();
            for(int category = 0; category < Categories.Count; category++)
            {
                string categoryName = searchableCategories[category];
                if(categoryName.Equals(categoryStr))
                {
                    return category;
                }
            }

            return UNKNOWN_CATEGORY_INT; // Unknown
        }

        public class CategoryUtil
        {
            public static void AddCategory(string category)
            {
                categories.Add(category);
                searchableCategories.Add(category.ToLower());
            }

            public static void AddCategories(IList<string> newCategories)
            {
                var originalCategories = Categories.ToList();
                foreach (string category in newCategories)
                {
                    AddCategory(category);
                }
            }

            public static IReadOnlyList<string> ReadCategories(string path)
            {
                List<string> categories = new List<string>();
                using (FileStream fs = File.OpenRead(path))
                using (TextReader reader = new StreamReader(fs))
                {
                    while (reader.Peek() != -1)
                    {
                        categories.Add(reader.ReadLine() ?? UNKNOWN_CATEGORY);
                    }
                }

                return categories;
            }

            public static void Load(string path)
            {
                AmazonTransaction.Categories = (IReadOnlyList<string>) ReadCategories(path);
            }

            public static bool Save(string path)
            {
                bool wrote = false;
                using (FileStream fs = File.OpenWrite(path))
                using (TextWriter writer = new StreamWriter(fs))
                {
                    foreach (string category in AmazonTransaction.Categories)
                    {
                        writer.WriteLine(category);
                    }
                    wrote = true;
                }

                return wrote;
            }

            public static void ReplaceCategories(IList<string> newCategories)
            {
                AmazonTransaction.categories.Clear();
                AddCategories(newCategories);
            }
        }

    }
}
