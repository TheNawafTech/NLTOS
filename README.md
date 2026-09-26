# National Licensing & Traffic Operations System (NLTOS)

A C#/.NET desktop licensing and traffic operations system built with Windows Forms,
SQL Server, ADO.NET and a layered 3-tier architecture.

## Overview

NLTOS models the operations of a driving-license authority. It covers the full path an
applicant takes — registration, application intake, a sequence of driving tests, and
license issuance — along with the services that follow it: renewal, replacement of lost
or damaged licenses, international licenses, and the detention and release of licenses.

The system is a simulation built to model these procedures. It is not connected to any
authority, live service or real data.

## Key Workflows

- **People and users** — registration, updates, image handling, user activation control
- **Local driving-license applications** — intake and lifecycle tracking across new,
  completed and cancelled states
- **Testing** — vision, written and street tests, with appointment scheduling and retakes
- **License issuance** — issuing a license once the application and tests are complete
- **Renewal and replacement** — renewal of expired licenses, replacement of lost or
  damaged ones
- **International licenses** — issued against an existing valid local license
- **Detained licenses** — detention, and release through a dedicated application

## Architecture

The solution is split into three projects, each communicating only with the one
directly beneath it.

| Layer | Project | Responsibility |
| --- | --- | --- |
| Presentation | `NLTOS` | Windows Forms interface, navigation, input validation |
| Business Logic | `NLTOS_Business` | Business rules, eligibility checks, workflow state transitions |
| Data Access | `NLTOS_DataAccess` | SQL Server access via ADO.NET using parameterized queries |

The presentation layer holds no SQL and opens no connections; every database call passes
through the data access layer. Workflow rules live in the business layer, so the same
constraints apply regardless of which screen triggers an operation.

## Business Rules & Workflow Constraints

The business layer answers the questions a rule depends on, and the screens act on the
answers. Each rule below names the check that decides it.

**Tests run in a fixed sequence.** A written test cannot be scheduled until the vision
test has been passed, and a street test not until the written one has. The scheduling
control asks `clsLocalDrivingLicenseApplication.DoesPassTestType` for the prerequisite
and disables the appointment rather than letting it be booked.

**A license is not issued until every test is passed.** Issuance is gated on
`clsTest.PassedAllTests`, reached through
`clsLocalDrivingLicenseApplication.PassedAllTests`, which counts the application's passed
tests rather than reading a status flag.

**No second open application for the same license class.** Before a new local application
is created, `clsApplication.GetActiveApplicationIDForLicenseClass` is asked whether the
applicant already has an open one for that class.

**A license cannot be detained twice, or released while it is not detained.**
`clsLicense.IsDetained`, which asks `clsDetainedLicense.IsLicenseDetained`, guards both
the detention and the release screens. Release is recorded through its own application
rather than by clearing a flag.

**Multi-table operations are all-or-nothing.** Issuing, renewing, replacing and releasing
each write to more than one table. Every one of them runs as a single data-access
operation inside one transaction, so a failure part-way through leaves no half-finished
record behind.

## Tech Stack

```text
C#
.NET Framework 4.8
Windows Forms
SQL Server
ADO.NET
3-Tier Architecture
```

No ORM, micro-ORM or scaffolding is used — the data access layer is hand-written ADO.NET.

Passwords are stored as PBKDF2 hashes with a per-user salt. Operations that write to more
than one table run inside a single transaction. Unhandled exceptions are caught centrally
and written to the Windows event log rather than being swallowed where they occur.

## Database

A relational SQL Server database of **14 tables and 5 views**, tied together by
**27 foreign key constraints**.

The main entity groups are people, users, applications, licenses (local, international
and detained), and tests with their appointments. Licenses reference a license class that
carries its own validity length, fees and minimum age.

The complete schema is in [`Database/NLTOS_Database.sql`](Database/NLTOS_Database.sql).

## Demo

A walkthrough of the main workflows and features:

[Watch the system demo](https://youtu.be/0dtG_A0FHwM)

## Screenshots

**Login**

![Login screen](assets/Login%20%282%29.png)

**Driving licenses**

![Driving licenses](assets/Driving_Licenses.png)

**Applications management**

![Applications management](assets/Manage_applications.png)

**New local license application**

![New local license application](assets/New_Local_License.png)

**Scheduling a test**

![Scheduling a test](assets/Take_Test.png)

The written and street test entries are greyed out here: this applicant has not yet
passed the test each one requires.

## Getting Started

**Requirements:** Visual Studio with .NET Framework 4.8, and SQL Server.

1. Clone the repository.
2. Open `Database/NLTOS_Database.sql` in SQL Server Management Studio and execute it.
   The script creates the NLTOS database, builds its tables and views, and seeds it
   with sample data. It expects the database not to exist yet: if one named NLTOS is
   already present the script stops with a message and leaves it untouched, rather
   than overwriting it.
3. Set the connection string in `NLTOS_DataAccess/clsDataAccessSettings.cs` to match
   your SQL Server instance. The default targets a local instance using Windows
   authentication.
4. Open `NLTOS/NLTOS.sln` in Visual Studio and build the solution.
5. Run.

### Demo Account

Username: `demo.user`<br>
Password: `DemoUser@123`

These credentials belong only to the local sample database created by the setup script.

## Project Structure

```text
NLTOS/
├── Database/                 SQL Server schema script
│
├── NLTOS/                    Presentation Layer — Windows Forms
│   ├── Applications/         Local, international, renewal, replacement, release
│   ├── Licenses/             Issuing, detaining, license history
│   ├── People/
│   ├── Drivers/
│   ├── Tests/                Scheduling and test execution
│   ├── User/
│   ├── Login/
│   └── Global Classes/       Formatting, validation, shared helpers
│
├── NLTOS_Business/           Business Logic Layer
│
└── NLTOS_DataAccess/         Data Access Layer — ADO.NET
```
