# National Licensing & Traffic Operations System (NLTOS)

> Enterprise-Level Desktop Application  
> Built with C#, Windows Forms, and Clean 3-Tier Architecture

---

## Overview

**NLTOS (National Licensing & Traffic Operations System)** is a full desktop-based management system that simulates real-world licensing and traffic operations workflows.

The system is developed using **C# and Windows Forms**, following a structured **3-Tier Architecture (Presentation, Business Logic, Data Access)** to ensure separation of concerns, scalability, and maintainability.

This project focuses on implementing controlled business workflows, validation mechanisms, and structured database interaction similar to real governmental licensing systems.

---

## Architecture

The system follows a clean multi-layer architecture:

### Presentation Layer (UI)
- Structured Windows Forms navigation
- Controlled screen transitions
- Context menus and dynamic filtering
- Input validation before submission

### Business Logic Layer (BLL)
- Centralized business rule enforcement
- License eligibility validation
- Test sequencing logic (Vision → Written → Street)
- Pass / Fail state tracking
- Workflow state management and transitions

### Data Access Layer (DAL)
- ADO.NET implementation
- Parameterized SQL queries
- Centralized connection handling
- Secure CRUD operations

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
- Strong validation layer:
  - Age eligibility enforcement
  - Duplicate prevention
  - License state verification
  - Prevention of invalid state transitions

---

## Workflow Logic Highlights

The system enforces real-world constraints such as:

- An applicant cannot proceed to the next test without passing the previous one.
- A license cannot be issued unless all required tests are successfully completed.
- Duplicate active licenses are prevented.
- Detained licenses cannot be processed until properly released.
- Application states transition only through valid business paths.

---

## Screenshots

### Login Screen
![Login](assets/Login%20(2).png)

### Main Dashboard
![Main](assets/Driving_Licenses.png)

### Manage People
![People](assets/Manage_LocalApp.png)

### Manage Users
![Users](assets/Manage_applications.png)

### Street Test Appointments
![Street Test](assets/Driving_Licenses.png)

---

## Technologies Used

- C#
- .NET Framework
- Windows Forms
- SQL Server
- ADO.NET
- 3-Tier Architecture
- Visual Studio
---

## What This Project Demonstrates

- Clean Architecture Implementation
- Strong Separation of Concerns
- Business Rule Enforcement
- Controlled Workflow & State Management
- Structured Database Design
- Modular Desktop System Development

---

## Developed By

This system was fully designed and developed by **Nawaf Aluwairiji**.

The project emphasizes backend architecture, business logic design, and structured database integration within a clean multi-layer desktop environment.

GitHub: https://github.com/TheNawafTech

---

## Feedback & Collaboration

If you have suggestions, improvements, or architectural ideas that could enhance the system,  
feel free to share them through GitHub issues or discussions.

Constructive feedback is always welcome.
