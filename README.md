
# SpendingInfo
This app is intended to easily view and eventually classify amazon transactions from their new data export tool which no longer directly exports a csv.
May support other platforms in the future.

## Usage

### Amazon Orders
#### Loading from ZIP (from Amazon 'Request My Data' page)
1.) Navigate to `https://www.amazon.com/hz/privacy-central/data-requests/preview.html`
2.) Select `Your Orders` and submit request, following the directions until you download the ZIP file containing your ordering information.
3.) Open SpendingInfo, and click the  `Load From ZIP` button in the Amazon tab.
4.) Navigate to where you downloaded the zip file, and select it.

#### Loading from CSV (exported using SpendingInfo)
1.) Open SpendingInfo, and click the `Load from CSV` button in the Amazon tab.
2.) Navigate to where you previously exported the CSV, and select it.
* Note that when loading from CSV, the headers must exactly match what the program exports. That is, it expects the following headers and types: `{'ID': String, 'Date' : DateTime, 'Amount' : float, 'Description' : String, 'ASIN' : String, 'Category' : int}` where the value for category is the category index (the index for the category in the `AmazonTransaction.Categories` list)

### Export to CSV
* Note that only the 'selected' transactions are exported to a CSV. This means that the program will export all <u>visible</u> transactions, selected using date selection and the search bar. 
1.) After selecting the transactions, select the `Save as CSV` button. Then, select where it should be placed and what to name it.

#### Setting Amazon transaction categories
* Note that each category corresponds to a category index, and so if any preexisting categories are replaced, then all transactions will reflect that change.

1.) Select the `Set Categories` button. Here, you will see the currently active categories. 
	
2.) You can remove a category by using right click on a category you wish to remove, and select `Delete` from the context menu.
	
3.) To add a new category, select `Add New Category`. From here, you can enter the new category name and click `Ok` to create it.


#### Inserting/Deleting a single Amazon transaction
To delete a single transaction, while in the main Amazon tab, right click the desired transaction, and select `Delete Transaction` in the context menu.
To add a single transaction, while in the main Amazon tab, right click anywhere in the Amazon transaction table and select `Insert Transaction`.

## TODO
- Remove additional whitespace in category dropdown menu
- Add ways to manually add/remove individual transactions/categories
