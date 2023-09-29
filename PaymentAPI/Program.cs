using Microsoft.EntityFrameworkCore;
using PaymentAPI.DataAccess;
using PaymentAPI.Model;

namespace PaymentAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddHttpClient();

        builder.Services.AddControllers();
        builder.Services.AddDbContext<PaymentContext>(
            options => options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddTransient<Payment>();
        builder.Services.AddControllersWithViews();
        builder.Services.AddLogging(builder =>
        {

            builder.AddConsole();
            builder.AddDebug();
        });
        builder.Services.AddControllersWithViews();
        builder.Services.AddMvc();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

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
    }
}