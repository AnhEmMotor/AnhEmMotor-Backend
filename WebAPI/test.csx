using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Infrastructure.DBContexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Infrastructure;

var builder = new ConfigurationBuilder().SetBasePath("C:\Users\nqvqu\Downloads\Hoctap\HK6\Dự án cuối môn\AEMOTO\AnhEmMotor-Backend\WebAPI").AddJsonFile("appsettings.Development.json", optional: true);
var config = builder.Build();
var services = new ServiceCollection();
services.AddDbContext<ApplicationDBContext>(options => options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
var sp = services.BuildServiceProvider();
var db = sp.GetRequiredService<ApplicationDBContext>();

var counts = db.Products.GroupBy(p => p.Name).Select(g => new { Name = g.Key, Count = g.Count() }).Where(x => x.Count > 1).ToList();
foreach(var c in counts) { Console.WriteLine($"{c.Name}: {c.Count}"); }
