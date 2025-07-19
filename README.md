# Data Organizer Server

**Data Organizer Server** is the backend part of the [Data Organizer mobile app](https://github.com/Denis-Bredun/Data-Organizer), developed using **ASP.NET Core Web API**. It provides secure endpoints for:

- Generating **transcripts from uploaded audio files** using **Azure Speech-To-Text**
- Producing **summaries** from user text via **OpenAI API**
- Performing CRUD operations on user data, metadata, and notes stored in **Firebase Firestore**

> This project is designed to work in combination with the .NET MAUI-based mobile client.

---

## ⚙️ Prerequisites

Before getting started, make sure you have the following installed:

- [**Visual Studio 2022**](https://visualstudio.microsoft.com/vs/) with the **ASP.NET and web development** workload
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Git](https://git-scm.com/) - to clone the repository.

---

## 📦 Getting Started

### 1. Clone the repository

Use Git to download the project:

- "git clone https://github.com/Denis-Bredun/Data_Organizer_Server.git"

- "cd Data_Organizer_Server"

### 2. Open the solution

- Double-click the `"Data_Organizer_Server.sln"` file  
  **or**  
- Open Visual Studio → **File → Open → Project/Solution** → select `"Data_Organizer_Server.sln"`

---

## 🚀 CI/CD with GitHub Actions

![CI](https://github.com/Denis-Bredun/Data_Organizer_Server/actions/workflows/docker-ci.yml/badge.svg)

This project uses **GitHub Actions** for continuous integration and delivery (CI/CD).

Each push to the `master` branch triggers an automated workflow that:

- Builds the project using **.NET 8 SDK**
- Publishes a Docker image based on the `Dockerfile`
- Pushes the image to **Docker Hub** (or your private registry)
- Injects environment variables securely using **GitHub Secrets**

This ensures consistent builds, fast deployment, and up-to-date Docker images.


> ✅ CI/CD is defined in `.github/workflows/docker-ci.yml`
