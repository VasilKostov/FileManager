# FileManager

 Project:
 - Lets the user upload any type of files.
 - Supports uploading large files without memory footprits (done by streams).

Technologies:
 - **MSSSQL** used for Database
 - **Simple HTML adn JS** used for the web page.
 - Files with sizes over 50 MB are uploaded in the machine's memory, whereas smaller files are stored in the database.

How to run the application:
 1. Make sure you have MSSMS on your computer. Then in `Package Manager Console` type: `Update-Database`, in order to insert the database and its tables in your SQL Server.
 2. Replace the `ConnectionString` in the `appsettings.json` file with one that works for your SQL Server.
