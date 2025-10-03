/*
 * SusEquip Equipment Management System
 * Copyright (c) 2025 Mark Parking (mpark@dtu.dk)
 * All rights reserved. READ-ONLY SHOWCASE LICENSE.
 * See LICENSE file for full terms.
 */

using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.Extensions.Options;
using MudBlazor;
using MudBlazor.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SusEquip.Data;
using SusEquip.Data.Services;
using SusEquip.Data.Services.Decorators;
using SusEquip.Data.Services.Validation;
using SusEquip.Data.Services.ErrorHandling;
using SusEquip.Data.Models;
using SusEquip.Data.Utilities;
using SusEquip.Data.Interfaces;
using SusEquip.Data.Interfaces.Services;
using SusEquip.Data.Repositories;
using SusEquip.Data.Commands;
using SusEquip.Data.Commands.Equipment;
using SusEquip.Data.Commands.Handlers;
using SusEquip.Data.Exceptions;
using SusEquip.Data.Factories;
using SusEquip.Data.Factories.Abstract;
using SusEquip.Data.Factories.Methods;
using SusEquip.Data.Builders;
using SusEquip.Middleware;
using System.Web.Services.Description;

var builder = WebApplication.CreateBuilder(args);

// Configure the app to serve static files in non-development environments
builder.WebHost.UseStaticWebAssets();

// Get the connection string and handle potential null values
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                          ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddRazorComponents();
builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = true; // Enable detailed errors for debugging
});
builder.Services.AddMudServices();
builder.Services.AddMemoryCache(); // Add memory caching for performance
builder.Services.AddSingleton<DatabaseHelper>(provider => new DatabaseHelper(connectionString));

// Register existing concrete services (but not as the interface implementations)
builder.Services.AddSingleton<EquipmentService>();
builder.Services.AddSingleton<OLDEquipmentService>();

// Register Repository Pattern implementations
builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
builder.Services.AddScoped<IOldEquipmentRepository, OldEquipmentRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Cache Service
builder.Services.AddSingleton<ICacheService, CacheService>();

// Register Validation Strategies (temporarily excluding SerialNumberValidationStrategy due to circular dependency)
// builder.Services.AddTransient<IValidationStrategy, SerialNumberValidationStrategy>(); // TODO: Fix circular dependency
builder.Services.AddTransient<IValidationStrategy, PCNameValidationStrategy>();
builder.Services.AddTransient<IValidationStrategy, DateValidationStrategy>();
builder.Services.AddTransient<IValidationStrategy, StatusValidationStrategy>();

// Register Validation Service
builder.Services.AddScoped<IValidationService, ValidationService>();

// Register Equipment Service with Decorator Chain (Synchronous)
// The decorator chain: Logging -> Caching -> Core Service
builder.Services.AddScoped<IEquipmentServiceSync>(provider =>
{
    // Get required services
    var memoryCache = provider.GetRequiredService<IMemoryCache>();
    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
    
    // 1. Start with core service (concrete EquipmentService)
    var coreService = provider.GetRequiredService<EquipmentService>();
    
    // 2. Wrap with caching
    var cachingLogger = loggerFactory.CreateLogger<CachingEquipmentService>();
    var cachingService = new CachingEquipmentService(coreService, memoryCache, cachingLogger);
    
    // 3. Wrap with logging (outermost decorator)
    var loggingLogger = loggerFactory.CreateLogger<LoggingEquipmentService>();
    return new LoggingEquipmentService(cachingService, loggingLogger);
});

// Register Equipment Service with Decorator Chain (Async)
// Decorator chain order: Performance Monitoring -> Logging -> Caching -> Core Service
builder.Services.AddScoped<IEquipmentService>(provider =>
{
    // Get required services
    var memoryCache = provider.GetRequiredService<IMemoryCache>();
    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
    
    // 1. Start with core service - use the existing EquipmentService wrapped in async adapter
    var coreEquipmentService = provider.GetRequiredService<EquipmentService>();
    var syncService = provider.GetRequiredService<IEquipmentServiceSync>();
    IEquipmentService coreService = new EquipmentServiceSyncToAsyncAdapter(syncService);
    
    // 2. Wrap with caching (inner decorator)
    var cachingLogger = loggerFactory.CreateLogger<SusEquip.Data.Services.Decorators.CachingEquipmentServiceAsync>();
    var cachingService = new SusEquip.Data.Services.Decorators.CachingEquipmentServiceAsync(coreService, memoryCache, cachingLogger);
    
    // 3. Wrap with logging (middle decorator)
    var loggingLogger = loggerFactory.CreateLogger<SusEquip.Data.Services.Decorators.LoggingEquipmentServiceAsync>();
    var loggingService = new SusEquip.Data.Services.Decorators.LoggingEquipmentServiceAsync(cachingService, loggingLogger);
    
    // 4. Wrap with performance monitoring (outermost decorator)
    var perfLogger = loggerFactory.CreateLogger<SusEquip.Data.Services.Decorators.PerformanceMonitoringEquipmentService>();
    return new SusEquip.Data.Services.Decorators.PerformanceMonitoringEquipmentService(loggingService, perfLogger);
});

// Register core service dependencies
builder.Services.AddScoped<EquipmentServiceAsyncAdapter>();

// Register OLD Equipment Service (simple registration for now)
builder.Services.AddScoped<IOLDEquipmentService>(provider => 
{
    var oldService = provider.GetRequiredService<OLDEquipmentService>();
    return new OLDEquipmentServiceAsyncAdapter(oldService);
});

// Register Command Pattern Infrastructure
builder.Services.AddScoped<ICommandExecutor, CommandExecutor>();

// Register Command Handlers
builder.Services.AddScoped<EquipmentCommandHandler>();

// Register individual command handlers with their interfaces
builder.Services.AddScoped<ICommandHandler<AddEquipmentCommand, EquipmentOperationResult>>(provider => 
    provider.GetRequiredService<EquipmentCommandHandler>());
    
builder.Services.AddScoped<ICommandHandler<DeleteEquipmentCommand, EquipmentOperationResult>>(provider => 
    provider.GetRequiredService<EquipmentCommandHandler>());
    
builder.Services.AddScoped<ICommandHandler<UpdateEquipmentCommand, EquipmentOperationResult>>(provider => 
    provider.GetRequiredService<EquipmentCommandHandler>());
    
builder.Services.AddScoped<ICommandHandler<UpdateEquipmentStatusCommand, EquipmentOperationResult>>(provider => 
    provider.GetRequiredService<EquipmentCommandHandler>());
    
builder.Services.AddScoped<ICommandHandler<BulkImportEquipmentCommand, EquipmentOperationResult>>(provider => 
    provider.GetRequiredService<EquipmentCommandHandler>());

// Register Command-based Equipment Service as alternative (optional)
builder.Services.AddScoped<CommandBasedEquipmentService>();

// NOTE: Equipment Commands are not registered in DI because they require runtime parameters
// They should be instantiated directly with required dependencies when needed

// Register Advanced Factory Patterns
// Basic factory interfaces (assuming they exist)
builder.Services.AddScoped<IEquipmentFactory, EquipmentFactory>();

// Register Abstract Factory pattern components
builder.Services.AddScoped<IEquipmentTypeRegistry, EquipmentTypeRegistry>();
builder.Services.AddScoped<IExtensibleEquipmentFactory, ExtensibleEquipmentFactory>();

// Register the unified Advanced Factory Manager
builder.Services.AddScoped<IAdvancedEquipmentFactory, AdvancedEquipmentFactory>();

// Family factories are created on-demand by AdvancedEquipmentFactory
// but you could register them if needed:
// builder.Services.AddTransient<ServerEquipmentFactory>();
// builder.Services.AddTransient<WorkstationEquipmentFactory>();

// Equipment builders are created on-demand through the factory

// Register Exception Handling Services
builder.Services.AddScoped<IExceptionHandlingService, ExceptionHandlingService>();

// Register other services with interfaces
builder.Services.AddScoped<IDataValidationService, DataValidationService>(); 
builder.Services.AddTransient<DTUPC_NewDevice>();
builder.Services.AddScoped<ICookieService, CookieService>();
builder.Services.AddScoped<IDashboardCacheService, DashboardCacheService>(); // Register with interface
builder.Services.AddMudBlazorDialog();
builder.Services.AddMudBlazorSnackbar();
builder.Services.AddRazorPages(); // Register Razor Pages services 
builder.Services.AddHttpContextAccessor(); // Register IHttpContextAccessor
builder.Services.AddHttpClient();
// Configure ApiSettings from appsettings.json
builder.Services.Configure<SusEquip.Data.Models.ApiSettings>(
    builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddScoped<SusEquip.Data.Interfaces.Services.IApiService, ApiService>(); // Register ApiService with interface
builder.Services.AddSingleton<IConfiguration>(builder.Configuration); // Register IConfiguration


// Register DataContext with the dependency injection container
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
    // Configure other Snackbar properties as needed
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Add global exception handling middleware first
app.UseGlobalExceptionHandling();

if (!app.Environment.IsDevelopment())
{
    // In development, we rely on our custom exception handling middleware
    // In production, we can use both for redundancy
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Middleware to initialize EquipmentUtils with CookieService
app.Use(async (context, next) =>
{
    var cookieService = context.RequestServices.GetRequiredService<ICookieService>();
    EquipmentUtils.Initialize(cookieService);
    await next.Invoke();
});

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
