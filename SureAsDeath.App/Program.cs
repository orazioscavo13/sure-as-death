using System.Reflection;
using SureAsDeath.App.Authorizations;
using SureAsDeath.App.Hangfire;
using Hangfire;
using SureAsDeath.App.Hangfire.Jobs.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHangfireServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHangfireDashboard("/hangfire");

/*, new DashboardOptions
{
    Authorization = new[] { new RemoteAuthorization() }
});
*/

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", (IRetrieveDataJob service) => service.SyncData(CancellationToken.None));

app.Run();