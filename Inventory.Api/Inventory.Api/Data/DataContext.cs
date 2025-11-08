using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base (options)
        {
            
        }
    }
}
