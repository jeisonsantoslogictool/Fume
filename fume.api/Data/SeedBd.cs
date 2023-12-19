using fume.api.Helpers;
using fume.shared.Enttities;
using fume.shared.Enums;

namespace fume.api.Data
{
    public class SeedBd
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public SeedBd(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;

        }

        public async Task seedAsync()
        {
            await _context.Database.EnsureCreatedAsync();
            await CheckCountriesAsync();
            await CheckCategoriesAsync();
            await CheckRolesAsync();
            await CheckUserAsync("1010", "Juan", "Zuluaga", "zulu@yopmail.com", "322 311 4620", "Calle Luna Calle Sol", UserType.Admin);

        }


        private async Task CheckCategoriesAsync()
        {
            if(!_context.categories.Any())
            {
                _context.categories.Add(new Category { Name = "Extra" });
                _context.categories.Add(new Category { Name = "Ultra" });
                _context.categories.Add(new Category { Name = "Infinity" });
                
                await _context.SaveChangesAsync();

            }
        }

        private async Task CheckRolesAsync()
        {
            await _userHelper.CheckRoleAsync(UserType.Admin.ToString());
            await _userHelper.CheckRoleAsync(UserType.User.ToString());
        }

        private async Task<User> CheckUserAsync(string document, string firstName, string lastName, string email, string phone, string address, UserType userType)
        {
            var user = await _userHelper.GetUserAsync(email);
            if (user == null)
            {
                user = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    UserName = email,
                    PhoneNumber = phone,
                    Address = address,
                    Document = document,
                    City = _context.Cities.FirstOrDefault(),
                    UserType = userType,
                };

                await _userHelper.AddUserAsync(user, "123456");
                await _userHelper.AddUserToRoleAsync(user, userType.ToString());
            }

            return user;
        }


        private async Task CheckCountriesAsync()
        {
            if (!_context.Countries.Any())
            {
                _context.Countries.Add(new Country
                {
                    Name = "Rep.Dom",
                    States = new List<State>
                    {
                        new State
                        {
                            Name = "Espaillat",
                            Cities = new List<City>
                            {
                                new City {Name = "Moca"}
                            }
                        }
                    }

                });
                _context.Countries.Add(new Country { Name = "Mexico" });
                _context.Countries.Add(new Country { Name = "Ecuador" });
                _context.Countries.Add(new Country { Name = "El salvador" });
                _context.Countries.Add(new Country { Name = "China" });
                await _context.SaveChangesAsync();
            }
        }
    }
}
