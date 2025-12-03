# FitnessApp Setup Script
# Run this script after installing .NET 8 SDK

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  FitnessApp Setup Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if .NET SDK is installed
Write-Host "[1/5] Checking .NET SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: .NET SDK is not installed!" -ForegroundColor Red
    Write-Host "Please install .NET 8 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Red
    exit 1
}
Write-Host "  .NET SDK version: $dotnetVersion" -ForegroundColor Green

# Navigate to project directory
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $projectRoot
Write-Host "  Working directory: $projectRoot" -ForegroundColor Green
Write-Host ""

# Restore packages
Write-Host "[2/5] Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to restore packages!" -ForegroundColor Red
    exit 1
}
Write-Host "  Packages restored successfully!" -ForegroundColor Green
Write-Host ""

# Build solution
Write-Host "[3/5] Building solution..." -ForegroundColor Yellow
dotnet build --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "  Build succeeded!" -ForegroundColor Green
Write-Host ""

# Install EF Core tools if needed
Write-Host "[4/5] Setting up Entity Framework tools..." -ForegroundColor Yellow
dotnet tool install --global dotnet-ef 2>$null
if ($LASTEXITCODE -ne 0) {
    dotnet tool update --global dotnet-ef 2>$null
}
Write-Host "  EF Core tools ready!" -ForegroundColor Green
Write-Host ""

# Create and apply migrations
Write-Host "[5/5] Setting up database..." -ForegroundColor Yellow
$apiProject = "src\FitnessApp.API"
$infraProject = "src\FitnessApp.Infrastructure"

# Check if migrations exist
$migrationsPath = Join-Path $infraProject "Migrations"
if (-not (Test-Path $migrationsPath)) {
    Write-Host "  Creating initial migration..." -ForegroundColor Cyan
    dotnet ef migrations add InitialCreate --project $infraProject --startup-project $apiProject
}

Write-Host "  Applying migrations to database..." -ForegroundColor Cyan
dotnet ef database update --project $infraProject --startup-project $apiProject
if ($LASTEXITCODE -ne 0) {
    Write-Host "WARNING: Database update failed. You may need to configure your connection string." -ForegroundColor Yellow
} else {
    Write-Host "  Database setup complete!" -ForegroundColor Green
}
Write-Host ""

# Success message
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Setup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "To run the application:" -ForegroundColor White
Write-Host "  dotnet run --project src\FitnessApp.API" -ForegroundColor Yellow
Write-Host ""
Write-Host "Then open in browser:" -ForegroundColor White
Write-Host "  https://localhost:5001/swagger" -ForegroundColor Yellow
Write-Host ""
