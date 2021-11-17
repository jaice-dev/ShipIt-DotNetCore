# ShipIt Inventory Management

## Setup Instructions
Open the project in VSCode.
VSCode should automatically set up and install everything you'll need apart from the database connection!

### Setting up the Database.
Create 2 new postgres databases - one for the main program and one for our test database.
Ask a team member for a dump of the production databases to create and populate your tables.

Restore the dump by running "\i {path to the dump}" in the PSQL tool in pgadmin.

Perform the following DB Migration on both databases:
```
  ALTER TABLE em DROP CONSTRAINT em_pkey;
  ALTER TABLE em ADD COLUMN em_id serial PRIMARY KEY;
```

Also Perform the following on both databases:
```
CREATE view inbound_stock_view as 
SELECT gtin.p_id, gtin_cd, gcp.gcp_cd, gtin_nm, m_g, l_th, ds, min_qt,
                        hld, gln_nm, gln_addr_02, gln_addr_03, gln_addr_04, gln_addr_postalcode, gln_addr_city, contact_tel, contact_mail 
                        FROM gtin 
                        INNER JOIN stock ON gtin.p_id = stock.p_id 
                        INNER JOIN gcp ON gtin.gcp_cd = gcp.gcp_cd
```
  
_(Note: This schema change will not effect any existing clients - it is a backwards
compatible change as any code relying on employee name for DB queries will still function correctly given unique employee names)_

Then for each of the projects, add a `.env` file at the root of the project.
That file should contain a property named `POSTGRES_CONNECTION_STRING`.
It should look something like this:
```
POSTGRES_CONNECTION_STRING=Server=127.0.0.1;Port=5432;Database=your_database_name;User Id=your_database_user; Password=your_database_password;
```

## Update the project frameworks from 3.0-dot-net-core to net5.0 (or whatever you have installed)

## Running The API
Once set up, simply run dotnet run in the ShipIt directory.

## Running The Tests
To run the tests you should be able to run dotnet test in the ShipItTests directory.

## Deploying to Production
TODO
