using apiPermissions.Context;
using apiPermissions.Elastic;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Elastic
builder.Services.AddSingleton<ServiceElastic>();

//Serilog

// Obtener URL de Elasticsearch desde la configuración
var elasticUri = builder.Configuration["Serilog:Elasticsearch:Uri"] ?? "http://localhost:9200";

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
	.WriteTo.Console()  // Muestra logs en la consola
	.WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)  // Guarda logs en archivos diarios
	.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUri))
	{
		AutoRegisterTemplate = true,  // Registra automáticamente el índice en Elasticsearch
		IndexFormat = "log-api-{0:yyyy.MM.dd}",  // Nombre del índice en formato diario
		FailureCallback = (logEvent, ex) => Console.WriteLine($"Error al enviar log a Elasticsearch: {ex.Message}"),
		EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog
	})
	.Enrich.FromLogContext()
	.CreateLogger();



builder.Host.UseSerilog();

var app = builder.Build();

// Aplicar Migraciones Automáticas
using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
