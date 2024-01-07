using System.Reflection;
using App.Metrics.Formatters.Prometheus;
using FluentValidation;
using GlavLib.App.Extensions;
using GlavLib.App.Validation;
using GlavLib.Basics.Logging;
using GlavLib.Basics.MultiLang;
using GlavLib.Basics.Serialization;
using GlavLib.Db.Extensions;
using GlavLib.Sandbox.API;
using GlavLib.Sandbox.API.Db;
using GlavLib.Sandbox.API.Routes;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

DapperConventions.Setup();

var appBuilder = WebApplication.CreateBuilder(args);

var configuration = appBuilder.Configuration;

Log.Logger = new LoggerBuilder()
             .Configure(configuration)
             .Build();

Log.Logger.Information("Starting Sandbox");

appBuilder.Services
          .AddDefaults()
          .AddSerilog()
          .Add_GlavLib_Sandbox_API()
          .AddAppMetrics(defaultContextLabel: "sandbox")
          .ConfigureHttpJsonOptions(options => //
          {
              options.SerializerOptions.GlavConfiguration();
          })
          .AddFluentValidationAutoValidation(config => //
          {
              config.OverrideDefaultResultFactoryWith<ErrorResponseFactory>();
          })
          .AddValidatorsFromAssembly(Assembly.Load("GlavLib.App"))
          .AddValidatorsFromAssembly(Assembly.Load("GlavLib.Sandbox.API"))
          .AddNh(config =>
          {
              config.UsePostgreSQL()
                    .UseDefaults()
                    .AddFluentMappings("GlavLib.Sandbox.API");
          })
          .AddMultiLang(builder => //
          {
              builder.AddFromDirectory("./LanguagePacks", "*.yaml");
          });

var app = appBuilder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMetricsEndpoint(new MetricsPrometheusTextOutputFormatter());

app.MapHealthChecks("/healthz")
   .AllowAnonymous();

app.UseRequestTimeouts();

var apiGroup = app.MapGroup("/api");


app.MapUserRoutes(apiGroup);

app.Run();

public partial class Program;