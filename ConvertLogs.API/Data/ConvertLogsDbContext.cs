using ConvertLogs.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ConvertLogs.API.Data
{
    public class ConvertLogsDbContext : DbContext
    {
        public ConvertLogsDbContext(DbContextOptions<ConvertLogsDbContext> options) : base(options) { }

        public DbSet<LogOrigem> LogsOrigem { get; set; }
        public DbSet<LogConvertido> LogsConvertidos { get; set; }

        //Adicionado para auxiliar no projeto de testes
        public virtual async Task<LogOrigem> FindLogByIdAsync(int id)
        {
            return await LogsOrigem.FindAsync(id);
        }
    }
}
