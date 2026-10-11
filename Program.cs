using APIregistroDeVenda.Context;
using APIregistroDeVenda.Repository;
using APIregistroDeVenda.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var mysqlConnection =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        mysqlConnection,
        ServerVersion.AutoDetect(mysqlConnection)));

// Repositórios
builder.Services.AddScoped<
    IVendaComissaoRepository,
    VendaComissaoRepository>();

builder.Services.AddScoped<
    IMovimentacaoEstoqueRepository,
    MovimentacaoEstoqueRepository>();

// Relógio
builder.Services.AddSingleton<IRelogio, Relogio>();

// Services
builder.Services.AddScoped<ComissaoService>();
builder.Services.AddScoped<MovimentacaoEstoqueService>();
builder.Services.AddScoped<JurosService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();