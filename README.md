# 🚗 Dingger

**Dingger** is the ultimate "Tinder for Cars" – a revolutionary platform designed to make trading, buying, and selling cars easier, faster, and much more fun. Swipe right on your dream ride, match with other car enthusiasts, and make trading a breeze!

## 🌟 Why Dingger?
Trading cars traditionally involves endless browsing, negotiating, and searching. Dingger simplifies the process:
- **Swipe to Like/Dislike**: Just like your favorite dating apps, swipe right if you're interested in a car, or swipe left to pass.
- **Mutual Matches**: If you like a car and the owner likes yours, it's a match! You can then proceed to discuss a potential trade.
- **Your Digital Garage**: Easily create a profile and upload your own vehicles to see who's interested.

## 🚀 Features
- **Interactive Swipe Interface**: Engaging left/right swiping mechanics with smooth animations.
- **Smart Matching System**: Instantly notifies you of mutual trade interests.
- **Seamless Car Management**: Create, Read, Update, and Delete your personal car listings directly from your profile.
- **High-Quality Image Uploads**: Upload and showcase your cars securely using modern cloud storage (Azure Blob Storage).
- **Secure Authentication**: Built-in user authentication to keep your data, cars, and matches safe.

## 🛠 Tech Stack
- **Frontend**: Blazor Web (InteractiveServer), responsive styling with Tailwind CSS.
- **Backend**: Robust ASP.NET Core RESTful API.
- **Database**: Entity Framework Core with PostgreSQL (Supabase).
- **Storage**: Azure Blob Storage for cloud image hosting.

## 📁 Project Structure
- `FrontEnd/` - The Blazor application containing the swipe UI, pages, and client-side logic.
- `BackEnd/` - The ASP.NET Core API containing controllers, services, EF Core DbContext, and matching logic.
- `Shared/` - Shared models and DTOs.
- `BackEnd.Tests/` - Unit tests ensuring the stability of backend logic.

## ⚙️ Setup & Installation

### Prerequisites
- .NET 10 SDK
- Visual Studio 2026 (or any modern C# IDE / VS Code)

### Database Setup
The database is automatically created on first run. No manual setup is needed! 
*To trigger manual updates, open the `BackEnd` directory in a terminal and run:*
`dotnet ef database update`

### Running the Application
1. **Start the Backend API:**
   Navigate to the `BackEnd/` directory and run:
   ```bash
   dotnet run
   ```
   *The API will run on: https://localhost:7065*

2. **Start the Frontend (Blazor):**
   Navigate to the `FrontEnd/` directory and run:
   ```bash
   dotnet run
   ```
   *The UI will run on: https://localhost:7140*

The frontend will automatically connect to the backend API.

## 🤝 Contributing
Want to help make car trading even better? Contributions are always welcome! Feel free to open an issue or submit a pull request.