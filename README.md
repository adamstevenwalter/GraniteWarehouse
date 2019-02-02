# GraniteWarehouse
Migrations makes so much more sense now! Version control for your database structure.
Change startup to use areas and point at the area where you want the app to start.
Important to mark areas in your controllers.
To add a new model and add it to the database: 
1. Create the model in the Models folder and define the attributes. 
2. Open the DbContext.cs file and add a DbSet of the new model type.
3. Open NuGet Package Manager Console and run add-migration migrationName
4. After the add-migation finishes (it sometimes takes a little while) run update-database
