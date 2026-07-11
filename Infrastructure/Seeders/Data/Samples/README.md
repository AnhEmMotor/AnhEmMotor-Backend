# Sample SQL data files

These SQL files are intended as supplemental sample data for SQL Server and follow the dependency order used by the existing seeders.

## Ordering
1. 001-sample-catalog.sql
2. 002-sample-products.sql
3. 003-sample-contacts-leads.sql
4. 004-sample-bookings-vehicles.sql
5. 005-sample-suppliers.sql

## Notes
- The scripts use `SET IDENTITY_INSERT` only when explicit IDs are needed for cross-reference consistency.
- Each script is idempotent and checks for existing rows before inserting.
- These files assume that the base schema already exists and that the core seeder tables were applied first.
