using DataNex.Data;
using DataNex.Model.Enums;
using DataNex.Model.Models;
using Microsoft.EntityFrameworkCore;

namespace DataNexApi
{
    public class ApplicationDataSeeder
    { 
        public async Task Seed(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await SeedData(context);
            }
        }

        private async Task SeedData(ApplicationDbContext context)
        {
            //Create dnadmin
            var dnadmin = await context.Users.FirstOrDefaultAsync(x=>x.Id == AppBase.DnAdmin);
        
            if(dnadmin == null)
            {
                var userToAdd = new User()
                {
                    Id = AppBase.DnAdmin,
                    Name = "dnadmin",
                    UserName = "dnadmin",
                    PasswordHash = "$2a$11$EwNeaOAFF8eyD2dMXxS/1uPEqMKPlTWNRFa9HR0c7bAgSAPT8wELy",
                    UserRole = UserRolesEnum.DnAdmin,
                    UserAdded = AppBase.DnAdmin,
                };

                context.Users.Add(userToAdd);
                context.SaveChanges();
            }
        }


    }
}
