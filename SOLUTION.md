OrderNexus — Solution Design and Trade-offs

1. Purpose and scope:
=====================
OrderNexus is a small internal purchase-order intake and tracking application 
built for the LexisNexis Senior .NET / Angular Developer assessment.

It enables a sales representative to choose a customer and catalogue products, 
submit an order, review recent orders and their totals, 
and follow a controlled order-status workflow.

The principal business concern is repeat submission: 
a representative may submit an order again 
if they are uncertain whether the first request succeeded. 
OrderNexus detects an existing order by its customer-provided reference and 
responds consistently without deliberately creating a duplicate.

The solution prioritizes a runnable end-to-end application, 
understandable code, and the assessment's critical requirements 
over a comprehensive sales or inventory platform.

2. Architecture:
================
The repository contains an Angular frontend and a separate ASP.NET Core backend, 
maintained together so an evaluator can inspect the full user journey in one place.
Project	Responsibility
OrderNexus.Domain: Entities, relationships, and calculated order values. 
                   No EF Core dependency.
OrderNexus.Application:	Order use cases, validation, status rules, 
                        DTOs, and repository interfaces.
OrderNexus.Infrastructure: EF Core DbContext, SQLite persistence, repositories, 
                           entity mapping, migrations, and seed data.
OrderNexus.API: HTTP controllers, dependency-injection composition, 
                OpenAPI/Swagger, and centralized error handling.
OrderNexus.Tests: Focused NUnit tests of critical order business behaviour.
ordernexus-web: Angular 20 client for order capture, 
                review, filtering, and permitted actions.

The normal request flow is:
Angular UI -> HTTP controller -> OrderService -> repository interface -> EF Core repository -> SQLite

Responses return through the same layers as DTOs. 
The API depends on Application and Infrastructure for composition; 
Infrastructure implements interfaces defined by Application. 
Domain remains separate from HTTP and persistence details.

Trade-off: 
This is more structure than a minimal CRUD sample requires. 
It was chosen to make responsibilities visible, 
permit isolated service tests, and demonstrate a familiar, 
maintainable backend architecture. 

In this timeboxed implementation, 
several rules remain in the Application service rather than 
being fully encapsulated behind immutable domain-aggregate methods. 
Stronger aggregate encapsulation would be 
a worthwhile refinement in a larger system.

3. Data model and persistence:
==============================
The relational model was designed from the required user workflow. 
The database uses singular table names:
'Customer': customer available for order selection.
'SalesRep': representative associated with an order, when supplied.
'Item': fixed catalogue with SKU, name, and current unit price.
'Currency': supported order currency; the initial catalogue is priced in ZAR.
'OrderStatus': permitted status values.
'Order': customer, external reference, representative, status, currency, notes, and audit fields.
'OrderItem': products, quantity, and captured transaction unit price.

Entities share audit properties: 
- 'Id', 
- 'CreatedBy', 
- 'CreatedDateTime', 
- 'UpdatedBy', 
- 'UpdatedDateTime'

Save operations maintain UTC timestamps. 
User identity fields remain optional because 
authentication is not part of this assessment.

Relationships and indexes are configured using EF Core Fluent API. 
The database enforces a unique '(CustomerId, ExternalReference)' index, 
required foreign keys, a positive quantity constraint, and non-negative stored prices. 

Deleting an order cascades to its lines at the database level; 
the service first checks whether deletion is permitted.

Why SQLite?
SQLite gives the evaluator a persistent relational database 
without installing or configuring a database server. 
EF Core migrations create the schema, 
startup initialization seeds the reference data. 
The '.db' file is local and excluded from version control; 
schema migrations and seeding code are committed.

Trade-off: 
SQLite is suitable for this local assessment, 
not a claim that it is the best operational database for a high-volume, 
multi-user sales platform. 

Production deployment would warrant a review of concurrency, 
backup, database hosting, migration execution, and operational monitoring.

Monetary accuracy
C# exposes unit prices as 'decimal'. 
An EF Core value converter stores monetary values as SQLite 'INTEGER' 
minor units (cents) to avoid binary floating-point storage errors. 
The implementation is deliberately limited to currencies with two decimal places; 
the seeded catalogue uses ZAR. 
Validation rejects negative catalogue prices 
Validation rejects prices exceeding two decimal places 
during the order use case.
'OrderItem.UnitPrice' is a snapshot of the price taken 
when that product is first added to the order. 
It is independent of later changes to 'Item.UnitPrice'. 
Editing a Pending order retains the original unit price of products already present; 
newly selected products use their current catalogue prices.

Calculated fields are not stored:
Line total = quantity × recorded order-item unit price.
Order subtotal = sum of line totals.
Order total = subtotal (there are no taxes, discounts, or additional charges in scope).
The backend computes the authoritative totals; 
Angular may show a preview but does not determine the saved prices or totals.

4. Duplicate-submission behaviour:
==================================
The duplicate key is the combination of customer ID and external reference. 
The Application service trims the reference and 
converts it to uppercase before lookup and storage, 
providing consistent matching for case and surrounding whitespace.

On a new submission, the service validates the request and 
searches for an existing order with that key.
If none exists, the service validates the selected lookup records, 
copies catalogue prices into order lines, and creates an order in 'PENDING' status.
If an existing order has the same relevant submitted content, 
the service returns that existing order instead of creating another. 
The controller distinguishes a new creation ('201 Created') from a repeat ('200 OK').
If the reference already belongs to an order with different submitted information, 
the service returns a '409 Conflict' rather than silently overwriting it.
A database unique index provides a final safeguard against duplicate rows if requests race.

Trade-off: 
The initial existence check and insert are 
not a single atomic idempotency operation. 
In a simultaneous-submission race, 
the database constraint protects integrity and 
the exception boundary can return a conflict, 
but it does not guarantee that both identical 
concurrent requests receive the same existing-order response. 
A more comprehensive implementation could catch the specific unique-key failure, 
reload and compare the winning order, 
and return it when equivalent.

5. Order workflow and editing rules:
====================================
The initial order statuses are 'PENDING', 'CONFIRMED', 'FULFILLED', and 'CANCELLED'.
Supported transitions:
'PENDING -> CONFIRMED'
'PENDING -> CANCELLED'
'CONFIRMED -> FULFILLED'
'CONFIRMED -> CANCELLED'
'FULFILLED' and 'CANCELLED' are terminal states. 
Attempts to make an invalid transition result in a conflict response with a helpful message.
Only Pending orders can be edited or deleted. 
Order lines are edited through the Order use case, 
not through an unrestricted standalone OrderItem controller. 
This protects confirmed or completed transactions from ordinary changes. 
In the current assessment implementation, 
deleting a Pending order physically deletes its associated lines.

Trade-off: 
Production order management would more likely retain order history and 
use cancellation or soft deletion with an audit trail. 
Pricing adjustments, discounts, partial fulfilment, user permissions, and order-history 
events are deliberately not implemented.

6. API and error handling:
==========================
The API exposes endpoints to create, list, retrieve, 
update, delete, and change the status of orders. 
Separate read-only lookup endpoints provide customers, 
sales representatives, products, statuses, and currencies to Angular. 
Swagger UI is available in the Development environment 
to inspect and exercise the HTTP contract.
Controllers remain thin and 
delegate business operations to 'OrderService'. 
Expected invalid input, missing orders, and business conflicts are 
translated into appropriate HTTP responses. 
Centralized exception handling produces safe 'ProblemDetails' 
responses with a trace identifier. 
Internal structured logging includes the exception type and message, 
assembly/method information where available, and request context.

Trade-off: 
No JWT login, roles, or authorization policy was introduced 
because the brief describes a small internal exercise 
without requiring identity management. 
The absence of authentication is an explicit scope decision, 
not a recommended production security posture.

7. Angular Client:
==================
The Angular 20 application uses 
TypeScript, standalone Angular configuration, 
a dedicated HTTP service, and ordinary HTML/CSS. 
Its layout supports desktop and smaller screens.

Key user journeys:
------------------
Load read-only lookup values into selectors 
rather than requiring users to type 
catalogue names or maintain master-data screens.

Display recent orders and filter by 
customer, representative, reference, and status.

Create an order by selecting products and entering quantities.

View order details and returned server-side amounts.

Edit or delete Pending orders, and request permitted status changes.

Show validation and API feedback to the user.
During local development, an Angular proxy forwards '/api' requests to ASP.NET Core. 
This avoids hardcoding the API port in individual HTTP calls 
and eliminates the need for an additional cross-origin development configuration.

Trade-off: 
The UI is a focused internal workflow 
rather than a complete design-system implementation. 
Reference-data maintenance, 
public search indexing, server-side rendering, 
and sophisticated pagination were intentionally omitted. 
The list filtering is client-side, which is sufficient for this small dataset; 
server-side filtering and paging would be appropriate as order volume grows.

8. Tests and verification
The dedicated NUnit test suite uses 
mocked repository dependencies to exercise 'OrderService' 
without relying on a live database. 

Test scope targets the most important business cases:
- Required order reference and line-item validation.
- Positive quantities and valid catalogue selections.
- Duplicate submissions with identical or conflicting content.
- Server-calculated totals and captured catalogue prices.
- Permitted and rejected status transitions.
- Edit/delete restrictions and not-found behaviour.

This gives quick feedback on business decisions 
without coupling tests to SQLite setup.

The application's API endpoints and Angular journeys 
are also suitable for manual end-to-end checks using Swagger and the browser.
Trade-off: The suite is intentionally not exhaustive: it is not a replacement for API-level integration tests, database concurrency tests, or frontend component tests. No specific automated pass count is asserted in this document; test results should be verified in the submitted repository before demonstration.

9. Scope decisions and future extensions:
=========================================
The following were deliberately deferred to keep the assessment focused:
- CRUD screens and write endpoints for 
  Customers, SalesReps, Items, Currencies, and OrderStatuses; 
  these are seeded, read-only lookups.

Authentication, authorization, and per-user permissions.

Discounts, price amendments, taxes, exchange rates, 
and multi-currency catalogue prices.

Inventory, stock reservations, 
shipping, payments, and partial fulfilment.

Generalized history/audit events, soft deletion, 
and production-grade reconciliation.

Enterprise deployment infrastructure, external integrations, 
and distributed processing.

Future work would be driven by 
actual business requirements 
rather than introduced speculatively.

10. Running and demonstrating the solution:
===========================================
See 'README.md' for prerequisites, 
repository layout, local .NET and Angular commands, 
SQLite initialization, Swagger access, and NUnit execution.

A short demonstration can follow this sequence:
Start the API and Angular application; show that the seeded lookups are available.
Create an order with more than one catalogue item and verify returned totals.
Show the new order in the recent-orders list and open its details.
Resubmit the same reference and content; show that no duplicate order is created.
Submit the same reference with different content; show the conflict message.
Change the order through a valid status transition and demonstrate a prohibited edit or transition.
Explain the layering, SQLite monetary mapping, and focused NUnit suite.
The main design goal is a small, understandable, 
runnable system in which business correctness is 
enforced in the backend while the 
Angular interface provides a practical order-entry experience.