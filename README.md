# 🩸 VitaFlow – Blood Donation Management for Healthcare Facility

**VitaFlow** is a web-based system built with ASP.NET Core Razor Pages, designed to support blood donation management for a healthcare facility. The system helps connect donors and recipients efficiently, ensuring timely blood supply and organized blood inventory.

---

## 📌 Project Objectives

Provide a streamlined solution to:

* Connect blood donors and emergency cases in need.
* Search compatible blood types based on full and component transfusions.
* Manage blood inventory and donor history.
* Handle urgent blood donation scenarios effectively.

---

## 🚀 Key Features

* Homepage introducing the healthcare facility, medical resources, and experience-sharing blog.
* Donor registration with blood type and readiness schedule.
* Search for compatible blood types (whole blood and components: red cells, plasma, platelets).
* Locate donors/recipients based on geographical distance.
* Register and manage emergency blood requests.
* Manage the entire donation process from request to completion.
* Track and update current blood units in the blood bank.
* Send reminders for recovery periods between donations.
* Maintain user profiles and donation history.
* Dashboard and statistical reports for admins.

---

## 🛠️ Technology Stack

* **ASP.NET Core 8** with **Razor Pages**
* **Entity Framework Core**
* **SQL Server / PostgreSQL**
* (Optional) Google Maps API, Chart.js

---

## 📈 Future Improvements

* Add authentication via email or OTP
* Interactive map for nearby donors
* Real-time emergency notification system
* Open API for mobile application integration

---

## 📂 Planning Project Structure

```
VitaFlow.sln
├── VitaFlow.Web             # Razor Pages project
├── VitaFlow.Core            # Domain models, interfaces
├── VitaFlow.Infrastructure  # EF Core, database access
└── VitaFlow.Services        # Business logic
```

---

## 📄 License

This project is developed for educational and research purposes. Contributions and forks are welcome!