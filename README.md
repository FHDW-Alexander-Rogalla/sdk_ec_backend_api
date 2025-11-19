# SDK EC Backend API

Backend API for the E-Commerce Software Development Kit project, built with .NET 9.0 and ASP.NET Core.

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop) (recommended)
- **OR** [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Optional: PowerShell (for Windows users)

## Quick Start

The Supabase database credentials are already configured in `appsettings.Development.json`.

### ⚠️ Important: JWT Secret Configuration

To enable authentication, you **must** configure the Supabase JWT Secret:

1. **Get your JWT Secret from Supabase Dashboard:**
   - Go to https://supabase.com/dashboard/project/YOUR_PROJECT_ID/settings/api
   - Copy the **JWT Secret** (not the anon key!)

2. **Set the JWT Secret in `appsettings.Development.json`:**
   ```json
   "Supabase": {
     "JwtSecret": "YOUR_ACTUAL_JWT_SECRET_HERE"
   }
   ```

   **OR** set it as an environment variable:
   ```powershell
   $env:SUPABASE_JWT_SECRET="your-actual-jwt-secret"
   ```

Without this configuration, authenticated endpoints (like `/api/cart`) will not work!

### Option 1: Docker (Recommended)

#### HTTP Mode (Default)

```powershell
# 1. Navigate to the project directory
cd Sdk_EC_Backend

# 2. Build the Docker image
docker build -t sdk-ec-backend:latest .

# 3. Start the container (HTTP on port 5139)
docker run --rm -p 5139:5139 --name sdk-ec-backend sdk-ec-backend:latest
```

The API will be accessible at: **http://localhost:5139**

---



#### HTTPS Mode

**One-time certificate setup (only needed once):**

```powershell
# 1. Remove old certificate (if exists)
dotnet dev-certs https --clean

# 2. Create directory for certificate
New-Item -ItemType Directory -Force -Path ${env:USERPROFILE}\.aspnet\https

# 3. Create and export new development certificate
dotnet dev-certs https -ep ${env:USERPROFILE}\.aspnet\https\aspnetapp.pfx -p YourSecurePassword123

# 4. Trust the certificate (Windows/macOS)
dotnet dev-certs https --trust
```

**Start container with HTTPS:**

```powershell
# 1. Navigate to the project directory
cd Sdk_EC_Backend

# 2. Build the Docker image
docker build -t sdk-ec-backend:latest .

# 3. Start container with HTTPS
docker run --rm -p 5139:5139 -p 7129:7129 `
  -e USE_HTTPS=true `
  -e ASPNETCORE_Kestrel__Certificates__Default__Password="YourSecurePassword123" `
  -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx `
  -v ${env:USERPROFILE}\.aspnet\https:/https:ro `
  --name sdk-ec-backend `
  sdk-ec-backend:latest
```