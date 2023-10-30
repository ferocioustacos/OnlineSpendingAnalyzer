# SpendingInfo
This app is intended to easily view and eventually classify amazon transactions from their new data export tool which no longer directly exports a csv.
May support other platforms in the future.

## Usage

### Loading Amazon Orders
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

#### Loading 

## TODO
- Populate Settings page to support:
	-- Setting possible categories
	-- Loading and saving of previously defined categories
- Remove additional whitespace in category dropdown menu
- Add styling to `Manually Label Orders` button in Amazon Tab
- Add ways to manually add/remove individual transactions