

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcraStop.Modules.Stress.Infrastructure.Persistences.Entities;

namespace ProcraStop.Modules.Stress.Infrastructure.Persistences.Config
{
    public class StressRecordConfig : IEntityTypeConfiguration<StressRecordEntity>
    {
        public void Configure(EntityTypeBuilder<StressRecordEntity> builder)
        {
            builder.ToTable("StressRecords");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Level).IsRequired();
            builder.Property(x => x.Source)
                .HasConversion<int>()   //  EF guarda como int pero usas enum
                .IsRequired();
            builder.Property(x => x.Timestamp).IsRequired();
        }
    }
}
