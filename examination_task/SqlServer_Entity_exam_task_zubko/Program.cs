using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public abstract class Property
{
    public int Id { get; set; }
    public string District { get; set; }
    public double Area { get; set; }
    public int Rooms { get; set; }
    public int Floor { get; set; }
    public double Price { get; set; }
    public string Address { get; set; }
    public abstract string GetDescription();
}

public class Apartment : Property
{
    public override string GetDescription()
    {
        return $"Адреса: {Address}, район: {District}, площа: {Area} кв.м, кімнат: {Rooms}, поверх: {Floor}, ціна: {Price} грн.";
    }
}

public class ClientRequest
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string RequestType { get; set; } // обмін, купівля або продаж
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime RequestDate { get; set; }
}

public class RealEstateContext : DbContext
{
    public DbSet<Property> Properties { get; set; }
    public DbSet<ClientRequest> ClientRequests { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=RealEstateDB;Trusted_Connection=True;");
    }
}

public interface IPropertyService
{
    void AddProperty(Property property);
    void RemoveProperty(int propertyId);
    IEnumerable<Property> GetAllProperties();
    IEnumerable<Property> GetFilteredProperties(Func<Property, bool> filter);
    void UpdateProperty(int propertyId, Property newProperty);
}

public interface IClientService
{
    void AddClientRequest(ClientRequest request);
    void RemoveClientRequest(int requestId);
    IEnumerable<ClientRequest> GetAllClientRequests();
    ClientRequest GetClientBySurname(string surname);
    ClientRequest GetClientByPhoneNumber(string phoneNumber);
}

public class PropertyService : IPropertyService
{
    private readonly RealEstateContext _context;

    public PropertyService(RealEstateContext context)
    {
        _context = context;
    }

    public void AddProperty(Property property)
    {
        _context.Properties.Add(property);
        _context.SaveChanges();
    }

    public void RemoveProperty(int propertyId)
    {
        var property = _context.Properties.FirstOrDefault(p => p.Id == propertyId);
        if (property != null)
        {
            _context.Properties.Remove(property);
            _context.SaveChanges();
        }
    }

    public IEnumerable<Property> GetAllProperties()
    {
        return _context.Properties.ToList();
    }

    public IEnumerable<Property> GetFilteredProperties(Func<Property, bool> filter)
    {
        return _context.Properties.Where(filter).ToList();
    }

    public void UpdateProperty(int propertyId, Property newProperty)
    {
        var property = _context.Properties.FirstOrDefault(p => p.Id == propertyId);
        if (property != null)
        {
            property.District = newProperty.District;
            property.Area = newProperty.Area;
            property.Rooms = newProperty.Rooms;
            property.Floor = newProperty.Floor;
            property.Price = newProperty.Price;
            property.Address = newProperty.Address;
            _context.SaveChanges();
        }
    }
}

public class ClientService : IClientService
{
    private readonly RealEstateContext _context;
    public ClientService(RealEstateContext context)
    {
        _context = context;
    }

    public void AddClientRequest(ClientRequest request)
    {
        _context.ClientRequests.Add(request);
        _context.SaveChanges();
    }

    public void RemoveClientRequest(int requestId)
    {
        var request = _context.ClientRequests.FirstOrDefault(r => r.Id == requestId);
        if (request != null)
        {
            _context.ClientRequests.Remove(request);
            _context.SaveChanges();
        }
    }

    public IEnumerable<ClientRequest> GetAllClientRequests()
    {
        return _context.ClientRequests.ToList();
    }

    public ClientRequest GetClientBySurname(string surname)
    {
        return _context.ClientRequests.FirstOrDefault(r => r.FullName.Contains(surname));
    }

    public ClientRequest GetClientByPhoneNumber(string phoneNumber)
    {
        return _context.ClientRequests.FirstOrDefault(r => r.PhoneNumber == phoneNumber);
    }
}

public class Program
{
    public static void Main()
    {
    using var dbContext = new RealEstateContext();
    dbContext.Database.EnsureCreated();
        IPropertyService propertyService = new PropertyService(dbContext);
        IClientService clientService = new ClientService(dbContext);

        // Заповнення бази даних прикладами нерухомості та заявок клієнтів
        propertyService.AddProperty(new Apartment { Id = 1, District = "Подільський", Area = 70, Rooms = 2, Floor = 5, Price = 70000, Address = "вул. Подільська, 5" });
        propertyService.AddProperty(new Apartment { Id = 2, District = "Голосіївський", Area = 90, Rooms = 3, Floor = 7, Price = 90000, Address = "вул. Голосіївська, 12" });
        propertyService.AddProperty(new Apartment { Id = 3, District = "Печерський", Area = 120, Rooms = 4, Floor = 3, Price = 120000, Address = "вул. Печерська, 22" });

        clientService.AddClientRequest(new ClientRequest { Id = 1, FullName = "Іван Іванов", RequestType = "купівля", Address = "вул. Подільська, 5", PhoneNumber = "0661234567", RequestDate = DateTime.Now });
        clientService.AddClientRequest(new ClientRequest { Id = 2, FullName = "Петро Петров", RequestType = "обмін", Address = "вул. Голосіївська, 12", PhoneNumber = "0671234567", RequestDate = DateTime.Now });
        clientService.AddClientRequest(new ClientRequest { Id = 3, FullName = "Сергій Сергієнко", RequestType = "продаж", Address = "вул. Печерська, 22", PhoneNumber = "0681234567", RequestDate = DateTime.Now });

        // Цикл для обробки вводу користувача
        while (true)
        {
            Console.WriteLine("Виберіть операцію:");
            Console.WriteLine("1. Переглянути всі заявки");
            Console.WriteLine("2. Пошук клієнта за прізвищем");
            Console.WriteLine("3. Пошук клієнта за номером телефона");
            Console.WriteLine("4. Додати заявку клієнта");
            Console.WriteLine("5. Видалити заявку клієнта");
            Console.WriteLine("0. Вихід");

            string input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    Console.WriteLine("Всі заявки:");
                    foreach (var request in clientService.GetAllClientRequests())
                    {
                        Console.WriteLine($"ID: {request.Id}, ПІБ: {request.FullName}, Тип заявки: {request.RequestType}, Адреса: {request.Address}, Телефон: {request.PhoneNumber}, Дата заявки: {request.RequestDate}");
                    }
                    break;

                case "2":
                    Console.WriteLine("Введіть прізвище клієнта:");
                    string surname = Console.ReadLine();
                    var clientBySurname = clientService.GetClientBySurname(surname);
                    if (clientBySurname != null)
                    {
                        Console.WriteLine($"ID: {clientBySurname.Id}, ПІБ: {clientBySurname.FullName}, Тип заявки: {clientBySurname.RequestType}, Адреса: {clientBySurname.Address}, Телефон: {clientBySurname.PhoneNumber}, Дата заявки: {clientBySurname.RequestDate}");
                    }
                    else
                    {
                        Console.WriteLine("Клієнта з таким прізвищем не знайдено.");
                    }
                    break;

                case "3":
                    Console.WriteLine("Введіть номер телефона клієнта:");
                    string phoneNumber = Console.ReadLine();
                    var clientByPhone = clientService.GetClientByPhoneNumber(phoneNumber);
                    if (clientByPhone != null)
                    {
                        Console.WriteLine($"ID: {clientByPhone.Id}, ПІБ: {clientByPhone.FullName}, Тип заявки: {clientByPhone.RequestType}, Адреса: {clientByPhone.Address}, Телефон: {clientByPhone.PhoneNumber}, Дата заявки: {clientByPhone.RequestDate}");
                    }
                    else
                    {
                        Console.WriteLine("Клієнта з таким номером телефона не знайдено.");
                    }
                    break;

                case "4":
                    Console.WriteLine("Введіть дані для нової заявки клієнта (ПІБ, тип заявки, адреса, номер телефона):");
                    string[] requestData = Console.ReadLine().Split(",");
                    clientService.AddClientRequest(new ClientRequest { Id = clientService.GetAllClientRequests().Count() + 1, FullName = requestData[0].Trim(), RequestType = requestData[1].Trim(), Address = requestData[2].Trim(), PhoneNumber = requestData[3].Trim(), RequestDate = DateTime.Now });
                    Console.WriteLine("Заявка клієнта додана.");
                    break;

                case "5":
                    Console.WriteLine("Введіть ID заявки клієнта, яку потрібно видалити:");
                    int requestId = int.Parse(Console.ReadLine());
                    clientService.RemoveClientRequest(requestId);
                    Console.WriteLine("Заявка клієнта видалена.");
                    break;
                case "0":
                    Console.WriteLine("До побачення!");
                    return;

                default:
                    Console.WriteLine("Невідома операція, спробуйте ще раз.");
                    break;
            }
            Console.WriteLine();
        }
    }
}