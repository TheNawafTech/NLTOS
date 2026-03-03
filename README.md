# National Licensing & Traffic Operations System (NLTOS)

Enterprise-Level Desktop Application  
Built with C#, Windows Forms, and Clean 3-Tier Architecture

---

## Overview

NLTOS (National Licensing & Traffic Operations System) is a full desktop-based management system that simulates real-world licensing and traffic authority workflows.

The system was designed and implemented to reflect structured governmental-style processes, including controlled state transitions, validation rules, and layered architecture principles.

This project is not a simple CRUD application. It focuses on workflow control, business rule enforcement, and proper separation between presentation, business logic, and data access layers.

---

## Architecture

The system follows a structured 3-Tier Architecture:

### Presentation Layer (UI)
- Windows Forms interface
- Controlled navigation between screens
- Form validation before submission
- Dynamic filtering and contextual actions
- Clear separation from business logic

### Business Logic Layer (BLL)
- Centralized rule enforcement
- Eligibility validation
- Sequential test logic (Vision → Written → Street)
- Pass/Fail tracking
- Workflow state management
- Prevention of invalid transitions
- Edge-case handling

### Data Access Layer (DAL)
- ADO.NET implementation
- Parameterized SQL queries
- Centralized connection handling
- Secure CRUD operations
- Controlled database interaction isolated from UI

This separation ensures maintainability, scalability, and clean system structure.

---

## Key Features

- Secure authentication system (Login, Remember Me, Change Password)
- Full People Management (Create, Update, Delete, Image Handling)
- User Management with activation control and filtering
- Driving License Services:
  - New Local License
  - International License
  - License Renewal
  - Replacement (Lost / Damaged)
- Detained License Management (Detain / Release with rule validation)
- Application lifecycle tracking (New, Completed, Cancelled)
- Integrated Test Management System:
  - Vision Test
  - Written Test
  - Street Test
  - Sequential validation between test stages
  - Retake logic handling
  - Appointment scheduling with locking mechanism

---

## Workflow Logic Highlights

The system enforces real-world operational constraints such as:

- An applicant cannot proceed to the next test without passing the previous one.
- A license cannot be issued unless all required tests are successfully completed.
- Duplicate active licenses are prevented.
- Detained licenses cannot be processed until properly released.
- Application states transition only through valid business paths.
- Age and eligibility rules are validated before processing.
- Invalid state transitions are blocked at the business layer.

---

## Database Architecture

The system is powered by a structured relational database built on SQL Server.

The schema was designed to reflect realistic licensing authority data relationships, including:

- Normalized relational design
- Identity-based primary keys
- Foreign key constraints enforcing referential integrity
- Logical separation of entities (People, Users, Applications, Licenses, Tests)
- Business-driven relationships supporting workflow validation

All database objects can be recreated using the provided SQL script:

Database/NLTOS_Database.sql

---

## How to Run the Project

### 1. Setup the Database

1. Open SQL Server Management Studio (SSMS)
2. Create a new database named:

   NLTOS

3. Open and execute:

   Database/NLTOS_Database.sql

---

### 2. Configure the Connection String

Update the connection string in:

NLTOS_DataAccess/clsDataAccessSettings.cs

Ensure it matches your local SQL Server configuration.

---

### 3. Run the Application

1. Open NLTOS.sln
2. Build the solution
3. Run the application

---
## Some of Screenshots

### Login Screen
![Login](assets/Login%20(2).png)

### Main Dashboard
![Main](assets/Driving_Licenses.png)

### Manage People
![People](assets/Manage_LocalApp.png)

### Manage Applications
![Applications](assets/Manage_applications.png)

### New Local License
![New Local License](assets/New_Local_License.png)

### Take Test
![Take Test](assets/Take_Test.png)

### User Information
![User Info](assets/User_Info.png)

---

## Development Scope

This system was fully designed and implemented from start to finish by me, covering all stages of development.

The work included:

- Requirements analysis
- System architecture design
- Database schema design
- Multi-layer implementation
- Business logic modeling
- Workflow rule design
- Validation enforcement
- Data integrity control
- UI development
- Debugging and testing across layers

The project was built from concept to final execution without relying on auto-generated frameworks or scaffolding tools.

---

## Problem-Solving & Logical Thinking Growth

Developing this system significantly strengthened my logical thinking and structured problem-solving skills.

Throughout the development process, I worked on:

- Translating real-world licensing rules into enforceable system logic
- Designing controlled workflow transitions
- Preventing invalid system states
- Handling edge cases and sequencing constraints
- Debugging interactions between UI, BLL, and DAL
- Structuring database relationships to support business validation

This project helped me improve my ability to break down complex requirements into manageable, logical components and implement them in a scalable and maintainable way.

---

## What This Project Demonstrates

- Clean 3-Tier Architecture implementation
- Strong separation of concerns
- Backend-oriented system design
- Structured business rule enforcement
- Controlled workflow state management
- Database integrity and relational design
- Logical system thinking and architectural structuring

---

## Developed By

This system was fully designed and developed by Nawaf Aluwairiji.

The project emphasizes backend structure, business logic design, and layered architecture implementation.

GitHub: https://github.com/TheNawafTech

---

## Feedback

Suggestions, improvements, or architectural ideas are welcome.  
Feel free to open an issue or start a discussion on GitHub.
