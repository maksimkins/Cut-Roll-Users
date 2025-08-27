using Cut_Roll_Users.Api.Common.Extensions.ServiceCollection;
using Cut_Roll_Users.Api.Common.Extensions.WebApplication;
using Cut_Roll_Users.Api.Common.Extensions.WebApplicationBuilder;

var builder = WebApplication.CreateBuilder(args);

builder.SetupVariables();
builder.InitMessageBroker();

builder.Services.InitDbContext(builder.Configuration);
builder.Services.InitAuth(builder.Configuration);
builder.Services.InitSwagger();
builder.Services.InitCors();

builder.Services.RegisterDependencyInjection();

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await app.UpdateDbContext();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.UseCors("AllowAllOrigins");

app.UseAuthentication();
app.UseAuthorization();


app.Run();
