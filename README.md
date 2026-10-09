OrderNexus:
===========
OrderNexus is a full-stack order intake and tracking application 
developed for the LexisNexis Senior .NET / Angular Developer technical assessment.
The application allows sales representatives to 
capture customer purchase orders, 
select products from a small predefined catalogue, 
review orders and their totals, and track order statuses. 
It guards against duplicate submissions of the same customer-provided reference.

Technology:
===========
Backend: C# / ASP.NET Core (.NET 10)
Frontend: Angular 20 / TypeScript / CSS
Persistence: Entity Framework Core 10 with SQLite
API documentation: OpenAPI and Swagger UI
Automated tests: NUnit (critical backend business rules)
Repository layout

OrderNexus/
|-- BackEnd/
|   |-- OrderNexus.Solution/
|       |-- OrderNexus.API/
|       |-- OrderNexus.Application/
|       |-- OrderNexus.Domain/
|       |-- OrderNexus.Infrastructure/
|       |-- OrderNexus.Tests/
|-- frontend/
|   |-- ordernexus-web/
|-- README.md
|-- SOLUTION.md
 
Folder names are shown using the local development layout. 
On a case-sensitive file system, use the actual casing of the checked-out directories.

Prerequisites:
==============
Install the following before running the application:
-----------------------------------------------------
.NET 10 SDK
Node.js 20.19+ and npm (the project was developed using Node.js 20.20.2 and npm 10.8.2)
Git
A browser
Visual Studio 2026 and Visual Studio Code are convenient but not required. 
A separate SQLite server or database installation is not needed.

1. Clone the repository:
------------------------
powershell
git clone <REPOSITORY_URL>
cd OrderNexus

Replace [REPOSITORY_URL] with the URL of the OrderNexus GitHub repository.

2. Run the backend:
-------------------
Open a terminal in the backend solution directory:
powershell
cd BackEnd/OrderNexus.Solution
dotnet restore
dotnet run --project OrderNexus.API --launch-profile https

The project should start using its configured HTTPS development profile. 
During development, the API was accessed at:

https://localhost:7111
https://localhost:7111/swagger

If the actual HTTPS address printed in the terminal differs, 
use that address and update the 
Angular proxy target in 'frontend/ordernexus-web/proxy.conf.json' to match.
If your machine does not trust the local ASP.NET Core HTTPS certificate, run:
powershell
dotnet dev-certs https --trust

Restart the backend after trusting the certificate.
Database initialization
The API uses a local SQLite database configured in 'OrderNexus.API/appsettings.json'. 
On startup, it applies outstanding EF Core migrations and 
inserts the initial lookup data if the relevant tables are empty.
The development database is expected at:

BackEnd/OrderNexus.Solution/OrderNexus.API/Data/OrderNexus.db

The SQLite database file is local to your environment and is excluded from Git. 
No manual SQL script, connection credentials, or separate database service is required.
Initial lookup data includes:
- customers, 
- sales representatives, 
- order statuses, 
- one supported currency (ZAR),
- five catalogue products. 
Orders are created through the application not seeded.

3. Run the Angular frontend:
----------------------------
Keep the backend running. Open a second terminal at the repository root and run:
powershell
cd frontend/ordernexus-web
npm ci
npm start -- --proxy-config proxy.conf.json

Open:

http://localhost:4200

Angular sends requests beginning with '/api' through its 
development proxy to the ASP.NET Core backend. 
If your API runs on a different port, 
change the proxy target accordingly and restart Angular.
For a local installation where 'npm ci' 
cannot be used because the lockfile is absent, use 'npm install'. 
The committed 'package-lock.json' should normally make 'npm ci' the preferred option.

4. Verify the application:
--------------------------
Open 'https://localhost:7111/swagger' and confirm that the Order and Lookup endpoints appear.
Open 'http://localhost:4200' and confirm that the lookup selectors are populated.
Create an order using an existing customer, sales representative, product and quantity.
Submit the order, then confirm it appears in the list and that its totals were calculated by the API.
Open the order to review its details and status.
Submit the same customer and external reference again to verify duplicate-submission handling.
The backend controls business validation, order-state transitions, and pricing. 
The frontend is not the source of truth for monetary totals.

5. Run the NUnit tests:
-----------------------
From 'BackEnd/OrderNexus.Solution', 
run:
powershell
dotnet test OrderNexus.Tests/OrderNexus.Tests.csproj

The focused test suite covers critical order business rules such as 
- input validation, 
- duplicate submissions, 
- totals, 
- status changes, 
- edit/delete restrictions.

Troubleshooting:
----------------
Swagger returns 404
Ensure the API is running in its Development environment and 
use '/swagger' on the actual HTTPS port. 
The API's Swagger UI is configured for development.

Angular cannot reach the API
Start the backend first. 
Check the 'target' in 'proxy.conf.json', 
verify the API's HTTPS port, 
restart the Angular development server after changing proxy settings.

HTTPS certificate warning
Run 'dotnet dev-certs https --trust' and restart the API. 
In the browser, use the trusted development certificate.

Database is missing or empty
Confirm that the API started successfully
Confirm that its connection string points to the expected local database file. 
Startup applies migrations and runs lookup-data seeding; 
Check the API console for any exception.
Port already in use
Stop the process using the port or 
change the API's 'launchSettings.json'/Angular dev-server settings. 
If the API port changes, update the Angular proxy configuration too.

Design notes:
-------------
The solution separates responsibilities into 
- Domain, 
- Application, 
- Infrastructure, and 
- API projects. 

SQLite was selected to keep local setup self-contained and reliable. 

Order unit prices are preserved independently from 
current catalogue prices, 
while line totals and overall totals are 
calculated rather than stored. 
Customer and lookup data are 
deliberately read-only in the assessment interface.
See 'SOLUTION.md' for architecture decisions, constraints, and trade-offs.

Development Notes:
------------------
- Please look at the text file called Investigate.txt
  This file is the backbone of SOP for every tas from my scrumboard
- It shows the small tech-analysis and keeping track
  of branching and commits as the task progress continues.
- Please view the paper Database design
  translated into a full fledged ERD.
  This is a quick design I do while reading the specification
  to map out the Entities and future DB-Tables
- Please view the paper screen designs