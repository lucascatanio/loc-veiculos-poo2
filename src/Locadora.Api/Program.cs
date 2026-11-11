using Locadora.Api.Mapeamento;
using Locadora.Api.Middleware;
using Locadora.Api.Relogio;
using Locadora.Application;
using Locadora.Infra;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExcecaoHandler>();

builder.Services.AddSingleton(CriarRelogio(builder));

builder.Services.AddApplication();
builder.Services.AddInfra(builder.Configuration.GetConnectionString("Locadora")
    ?? throw new InvalidOperationException("ConnectionStrings:Locadora não configurada."));

MapsterConfig.Registrar();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();

static TimeProvider CriarRelogio(WebApplicationBuilder builder)
{
    var deslocamentoHoras = builder.Configuration.GetValue<double>("Relogio:DeslocamentoHoras");

    return builder.Environment.IsDevelopment() && deslocamentoHoras != 0
        ? new RelogioDeslocado(TimeSpan.FromHours(deslocamentoHoras))
        : TimeProvider.System;
}
