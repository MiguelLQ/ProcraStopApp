

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcraStop.Infrastructure.Stress.Persistence.Entities;

namespace ProcraStop.Infrastructure.Stress.Persistence.Configs
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
