using System.Reflection;
using App.Metrics.Formatters.Prometheus;
using FluentNHibernate.Cfg;
using FluentValidation;
using GlavLib.App.Db;
using GlavLib.App.Extensions;
using GlavLib.App.Validation;
using GlavLib.Basics.MultiLang;
using GlavLib.Basics.Serialization;
using GlavLib.Sandbox.API;
using GlavLib.Sandbox.API.Db;
using GlavLib.Sandbox.API.Routes;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

var app = WebApplication.CreateBuilder(args)
                        .AddSerilog("Spectr.Payments.API")
                        .ConfigureBuilder(builder =>
                        {
                            builder.Services.Add_GlavLib_Sandbox_API();

                            var sessionFactory = Fluently.Configure()
                                                         .UsePostgreSQL()
                                                         .UseDefaults()
                                                         .AddFluentMappings("GlavLib.Sandbox.API")
                                                         .BuildSessionFactory();

                            builder.Services.AddSingleton(sessionFactory);

                            var languageContext = new LanguageContextBuilder()
                                                  .FromDirectory("./LanguagePacks", "*.yaml")
                                                  .Build();
                            builder.Services.AddSingleton(languageContext);

                            builder.Services.AddFluentValidationAutoValidation(configuration => //
                            {
                                configuration.OverrideDefaultResultFactoryWith<ErrorResponseFactory>();
                            });

                            builder.Services.AddValidatorsFromAssembly(Assembly.Load("GlavLib.App"));
                            builder.Services.AddValidatorsFromAssembly(Assembly.Load("GlavLib.Sandbox.API"));
                            builder.Services.AddRequestTimeouts();
                        })
                        .ConfigureJsonOptions(options => options.SerializerOptions.GlavConfiguration())
                        .AddAppMetrics()
                        .Bootstrap()
                        .Build();

DapperConventions.Setup();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMetricsEndpoint(new MetricsPrometheusTextOutputFormatter());

app.MapHealthChecks("/healthz")
   .AllowAnonymous();

app.UseRequestTimeouts();

var apiGroup = app.MapGroup("/api")
                  .WithOpenApi()
                  .AddAutoValidation();


app.MapUserRoutes(apiGroup);

app.Run();

public partial class Program;