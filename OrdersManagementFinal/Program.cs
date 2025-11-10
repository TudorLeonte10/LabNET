using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Week4.Common.Mapping;
using Week4.Common.Middleware;
using Week4.Data;
using Week4.Features.Orders;
using Week4.Features.Orders.Commands;
using Week4.Features.Orders.DTOs;
using Week4.Validators;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

builder.Services.AddAutoMapper(typeof(AdvancedOrderMappingProfile));
builder.Services.AddMemoryCache();

builder.Services.AddMediatR(options =>
{
    options.RegisterServicesFromAssembly(typeof(CreateOrderHandler).Assembly);
});

builder.Services.AddScoped<IValidator<CreateOrderProfileRequest>, CreateOrderProfileValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderProfileValidator>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Orders API",
        Version = "v1",
        Description = "Minimal API + MediatR version"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCorrelationMiddleware();


app.MapOrdersEndpoints();

app.Run();
