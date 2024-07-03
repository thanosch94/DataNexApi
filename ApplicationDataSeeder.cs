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

            //Create document types
            var documentTypes = await context.DocumentTypes.ToListAsync();

            if(!documentTypes.Any(x=>x.Id == DataSeedIds.Offer))
            {
                var documentType = new DocumentType()
                {
                    Id = DataSeedIds.Offer,
                    Name = "Offer",
                    Abbreviation = "OFR",
                    DocumentTypeGroup  = DocumentTypeGroupEnum.Sales,
                    IsSeeded = true,
                    UserAdded = AppBase.DnAdmin,

                };
                context.DocumentTypes.Add(documentType);
                context.SaveChanges();

            }

            if (!documentTypes.Any(x=>x.Id == DataSeedIds.SalesOrder))
            {
                var documentType = new DocumentType()
                {
                    Id = DataSeedIds.SalesOrder,
                    Name = "Sales Order",
                    Abbreviation = "SO",
                    DocumentTypeGroup = DocumentTypeGroupEnum.Sales,
                    IsSeeded = true,
                    UserAdded = AppBase.DnAdmin,

                };
                context.DocumentTypes.Add(documentType);
                context.SaveChanges();

            }

            if (!documentTypes.Any(x=>x.Id == DataSeedIds.ProformaInvoice))
            {
                var documentType = new DocumentType()
                {
                    Id = DataSeedIds.ProformaInvoice,
                    Name = "Proforma Invoice",
                    Abbreviation = "PI",
                    DocumentTypeGroup = DocumentTypeGroupEnum.Sales,
                    IsSeeded = true,
                    UserAdded = AppBase.DnAdmin,

                };
                context.DocumentTypes.Add(documentType);
                context.SaveChanges();

            }

            if (!documentTypes.Any(x=>x.Id == DataSeedIds.Receipt))
            {
                var documentType = new DocumentType()
                {
                    Id = DataSeedIds.Receipt,
                    Name = "Receipt",
                    Abbreviation = "RCPT",
                    DocumentTypeGroup = DocumentTypeGroupEnum.Sales,
                    IsSeeded = true,
                    UserAdded = AppBase.DnAdmin,

                };
                context.DocumentTypes.Add(documentType);
                context.SaveChanges();

            }

            if (!documentTypes.Any(x=>x.Id == DataSeedIds.Invoice))
            {
                var documentType = new DocumentType()
                {
                    Id = DataSeedIds.Invoice,
                    Name = "Invoice",
                    Abbreviation = "INV",
                    DocumentTypeGroup = DocumentTypeGroupEnum.Sales,
                    IsSeeded = true,
                    UserAdded = AppBase.DnAdmin,

                };
                context.DocumentTypes.Add(documentType);
                context.SaveChanges();

            }

            if (!documentTypes.Any(x=>x.Id == DataSeedIds.SalesDeliveryNote))
            {
                var documentType = new DocumentType()
                {
                    Id = DataSeedIds.SalesDeliveryNote,
                    Name = "Sales Delivery Note",
                    Abbreviation = "SDN",
                    DocumentTypeGroup = DocumentTypeGroupEnum.Sales,
                    IsSeeded = true,
                    UserAdded = AppBase.DnAdmin,

                };
                context.DocumentTypes.Add(documentType);
                context.SaveChanges();

            }

            if (!documentTypes.Any(x=>x.Id == DataSeedIds.PurchaseDeliveryNote))
            {
                var documentType = new DocumentType()
                {
                    Id = DataSeedIds.PurchaseDeliveryNote,
                    Name = "Purchase Delivery Note",
                    Abbreviation = "SDN",
                    DocumentTypeGroup = DocumentTypeGroupEnum.Purchasing,
                    IsSeeded = true,
                    UserAdded = AppBase.DnAdmin,

                };
                context.DocumentTypes.Add(documentType);
                context.SaveChanges();

            }

            if (!documentTypes.Any(x => x.Id == DataSeedIds.PurchaseInvoice))
            {
                var documentType = new DocumentType()
                {
                    Id = DataSeedIds.PurchaseInvoice,
                    Name = "Purchase Invoice",
                    Abbreviation = "PINV",
                    DocumentTypeGroup = DocumentTypeGroupEnum.Purchasing,
                    IsSeeded = true,
                    UserAdded = AppBase.DnAdmin,

                };
                context.DocumentTypes.Add(documentType);
                context.SaveChanges();

            }

            if (!documentTypes.Any(x=>x.Id == DataSeedIds.PurchaseOrder))
            {
                var documentType = new DocumentType()
                {
                    Id = DataSeedIds.PurchaseOrder,
                    Name = "Purchase Order",
                    Abbreviation = "PO",
                    DocumentTypeGroup = DocumentTypeGroupEnum.Purchasing,
                    IsSeeded = true,
                    UserAdded = AppBase.DnAdmin,

                };
                context.DocumentTypes.Add(documentType);
                context.SaveChanges();

            }

            if (!documentTypes.Any(x=>x.Id == DataSeedIds.CreditNote))
            {
                var documentType = new DocumentType()
                {
                    Id = DataSeedIds.CreditNote,
                    Name = "Credit Note",
                    Abbreviation = "CN",
                    DocumentTypeGroup = DocumentTypeGroupEnum.Finance,
                    IsSeeded = true,
                    UserAdded = AppBase.DnAdmin,

                };
                context.DocumentTypes.Add(documentType);
                context.SaveChanges();

            }

            
        }


    }
}
