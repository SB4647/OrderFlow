using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderFlow.Payments.Infrastructure.Persistence;

public sealed class PaymentsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<PaymentsDbContext>
{
    public PaymentsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=orderflow_payments;Username=orderflow;Password=orderflow")
            .Options;

        return new PaymentsDbContext(options);
    }
}
