using Bogus;
using RestaurantAPI.Entities;

namespace RestaurantAPI
{
    public class DataGenerator
    {
        public static void Seed(RestaurantDbContext dbContext)
        {
            var locale = "pl";

            var adressGenerator = new Faker<Address>()
                .RuleFor(a => a.City, f => f.Address.City())
                .RuleFor(a => a.Street, f => f.Address.StreetName())
                .RuleFor(a => a.PostalCode, f => f.Address.ZipCode());




            var restaurantGenerator = new Faker<Restaurant>()
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.Description, f => f.Commerce.ProductName())
                .RuleFor(a => a.Category, f => f.Commerce.Department())
                .RuleFor(a => a.HasDelivery, f => f.Random.Bool())
                .RuleFor(a => a.ContactEmail, f => f.Internet.Email())
                .RuleFor(a => a.ContactNumber, f => f.Phone.PhoneNumber())
                .RuleFor(a => a.Address, f => adressGenerator.Generate());


            var restaurants = restaurantGenerator.Generate();

            dbContext.AddRange(restaurants);
            //dbContext.SaveChanges();

        }

    }
}


