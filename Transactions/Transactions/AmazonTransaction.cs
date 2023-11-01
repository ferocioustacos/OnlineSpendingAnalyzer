using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using static SpendingInfo.Transactions.Transactions.ITransaction;


namespace SpendingInfo.Transactions.Transactions
{
    public class AmazonTransaction : TransactionBase, ITransaction, INotifyPropertyChanged
    {
        // Categories for classification
        private static readonly int UNKNOWN_CATEGORY_INT = -1;
        public static readonly string UNKNOWN_CATEGORY = "Unknown";

        private static IList<string> searchableCategories = new string[] {"tech", "clothes", "other"};
        private static IList<string> categories = new string[] { "Tech", "Clothes", "Other" };
        public static IList<string> Categories 
        { 
            get 
            {
                return categories;
            }

            set
            {
                categories = value;
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

                NotifyPropertyChanged();
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
    }
}
