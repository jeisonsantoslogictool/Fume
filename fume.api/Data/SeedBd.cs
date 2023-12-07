using fume.shared.Enttities;

namespace fume.api.Data
{
    public class SeedBd
    {
        private readonly DataContext _context;

        public SeedBd(DataContext context)
        {
            _context = context;
        }

        public async Task seedAsync()
        {
            await _context.Database.EnsureCreatedAsync();
            await CheckCountriesAsync();
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
