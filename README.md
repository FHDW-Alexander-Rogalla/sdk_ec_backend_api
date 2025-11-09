# SDK EC Backend API

Backend API for the E-Commerce Software Development Kit project, built with .NET 9.0 and ASP.NET Core.

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop) (recommended)
- **OR** [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Optional: PowerShell (for Windows users)

## Quick Start

The Supabase database credentials are already configured in `appsettings.Development.json`, so no additional setup is required.

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

The API will be accessible at:
- **HTTPS**: https://localhost:7129
- **HTTP**: http://localhost:5139 (redirects to HTTPS)

---

### Option 2: Local Development without Docker

#### HTTP Mode

```powershell
cd Sdk_EC_Backend
dotnet run --launch-profile http
```

The API runs on: **http://localhost:5139**

#### HTTPS Mode

```powershell
cd Sdk_EC_Backend
dotnet run --launch-profile https
```

The API runs on:
- **HTTPS**: https://localhost:7129
- **HTTP**: http://localhost:5139

---

## API Documentation

The API provides automatically generated Swagger documentation in development mode:

- **Swagger UI**: http://localhost:5139/swagger (or https://localhost:7129/swagger with HTTPS)

### Available Endpoints

- `GET /api/products` - List all products
- `GET /api/products/{id}` - Get a single product
- `POST /api/products` - Create a new product
- `PUT /api/products/{id}` - Update a product
- `DELETE /api/products/{id}` - Delete a product

---

## CORS Configuration

The backend allows requests from the following origins by default:
- `http://localhost:4200` (Angular Development Server)
- `https://localhost:4200`

For other origins, modify `Program.cs`.

---

## Troubleshooting

### Container won't start with HTTPS

**Problem**: `Unable to configure HTTPS endpoint. No server certificate was specified`

**Solution**: Ensure that:
1. The certificate was exported correctly (`${env:USERPROFILE}\.aspnet\https\aspnetapp.pfx` exists)
2. The password in the environment variable is correct
3. The volume mount is correct (`-v` parameter)

### Supabase connection failed

**Problem**: API starts, but database access fails

**Solution**: The Supabase credentials are pre-configured in `appsettings.Development.json`. If you need to use different credentials, you can:
- Modify `appsettings.Development.json` directly, or
- Set the `SUPABASE_KEY` environment variable when running the container

### Port already in use

**Problem**: `Address already in use`

**Solution**:
```powershell
# Check which process is using the port
netstat -ano | findstr :5139

# Container with same name already running? Stop it:
docker stop sdk-ec-backend
```

---

## Development

### Project Structure

```
Sdk_EC_Backend/
├── Controllers/        # API Controllers
├── Models/            # Data models and DTOs
├── Services/          # Business logic and external services
├── Configuration/     # Configuration classes
├── Program.cs         # Main entry point
└── Dockerfile         # Docker build configuration
```

### Adding New Dependencies

```powershell
dotnet add package PackageName
```

Then rebuild the Docker image.

---

## License

[Insert your license here]

## Contact

[Insert your contact information here]
```