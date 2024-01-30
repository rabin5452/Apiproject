using Microsoft.EntityFrameworkCore;
using Practiseproject.Model;

namespace Practiseproject.Data
{
    public class ProjectDBContext:DbContext
    {
        public ProjectDBContext(DbContextOptions options):base(options) 
        {
            
        }
        public DbSet<Student> Students { get; set; }
       
    }
}
