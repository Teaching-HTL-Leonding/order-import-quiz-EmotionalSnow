using System;
using System.IO;
using System.Linq;
using EFCore.BulkExtensions;
using OrderImport;

var factory = new OrderImportContextFactory();
using var context = factory.CreateDbContext(Array.Empty<string>());

switch (args[0].ToLower())
{
    case "import":
        Import();
        break;
                
    case "clean":
        Clean();
        break;
                
    case "check":
        Check();
        break;
    
    default:
        Clean();
        Import();
        Check();
        break;
}

async void Import()
{
    var customers = await File.ReadAllLinesAsync(args[1]);
    foreach (string line in customers.Skip(1).ToList())
    {
        var parts = line.Split("\t");
        context.Customers.Add(new Customer { Name = parts[0], CreditLimit = decimal.Parse(parts[1]) });
    }
    
    var orders = await File.ReadAllLinesAsync(args[2]);
    foreach (string line in orders.Skip(1).ToList())
    {
        var parts = line.Split("\t");
        var customer = context.Customers.First(customer => customer.Name == parts[0]);
        context.Orders.Add(new Order { CustomerId = customer.Id, OrderDate = DateTime.Parse(parts[1]), OrderValue = int.Parse(parts[2]) });
    }
    
    await context.SaveChangesAsync();
}

void Clean()
{
    context.TruncateAsync<Customer>();
    context.TruncateAsync<Order>();

    /*
    Without EFCore.BulkExtensions (NuGet pkg):
    
    context.Customers.RemoveRange(context.Customers);
    context.Orders.RemoveRange(context.Orders);
    */
}

void Check()
{
    var exceedingCustomers = context.Orders
        .GroupBy(order => order.CustomerId)
        .Select(group => new {
            Customer = context.Customers.First(customer => customer.Id == group.Key),
            TotalValue = group.Sum(order => order.OrderValue)
        })
        .Where(groupAggregate => groupAggregate.TotalValue > groupAggregate.Customer.CreditLimit)
        .Select(groupAggregate => groupAggregate.Customer.Name)
        .ToList();

    foreach (var name in exceedingCustomers)
    {
        Console.WriteLine(name);
    }
}
