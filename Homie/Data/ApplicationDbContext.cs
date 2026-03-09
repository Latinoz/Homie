using Homie;
using Homie.Areas.Series.Models;
using Homie.Areas.Cigars.Models;
using Homie.Areas.Battletech.Models;
using Homie.Areas.Finances.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Homie.Areas.Identity.Models;
using Homie.Models;

namespace Homie.Data.Models {

    public class ApplicationDbContext : IdentityDbContext<User>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<MoviesModel> MoviesEF { get; set; }
        public DbSet<CigarsModel> CigarsEF { get; set; }
        public DbSet<Format> FormatsEF { get; set; }
        public DbSet<BTMechsModel> BtEF { get; set; }
        public DbSet<BTPilotsModel> BtPilotEF { get; set; } 

        public DbSet<FileModel> Files { get; set; }
        public DbSet<Image> Picture { get; set; }

        // --- Модуль Финансы ---
        public DbSet<CurrencyModel> Currencies { get; set; }
        public DbSet<ExchangeRateModel> ExchangeRates { get; set; }
        public DbSet<InstrumentModel> Instruments { get; set; }
        public DbSet<OperationTypeModel> OperationTypes { get; set; }
        public DbSet<BankModel> Banks { get; set; }
        public DbSet<BrokerModel> Brokers { get; set; }
        public DbSet<WalletModel> Wallets { get; set; }
        public DbSet<CryptoExchangeModel> CryptoExchanges { get; set; }
        public DbSet<AccountModel> FinanceAccounts { get; set; }
        public DbSet<DepositModel> Deposits { get; set; }
        public DbSet<InvestmentPositionModel> InvestmentPositions { get; set; }
        public DbSet<CryptoAssetModel> CryptoAssets { get; set; }
        public DbSet<PreciousMetalModel> PreciousMetals { get; set; }
        public DbSet<OperationModel> FinanceOperations { get; set; }
        public DbSet<InflationModel> Inflation { get; set; }
        public DbSet<PriceHistoryModel> PriceHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Индексы ---
            modelBuilder.Entity<CurrencyModel>()
                .HasIndex(c => c.UserUid);

            modelBuilder.Entity<ExchangeRateModel>()
                .HasIndex(e => new { e.CurrencyId, e.Date, e.UserUid })
                .IsUnique();
            modelBuilder.Entity<ExchangeRateModel>()
                .HasIndex(e => e.UserUid);

            modelBuilder.Entity<InstrumentModel>()
                .HasIndex(i => i.UserUid);
            modelBuilder.Entity<InstrumentModel>()
                .HasIndex(i => new { i.Code, i.UserUid });

            modelBuilder.Entity<OperationTypeModel>()
                .HasIndex(o => o.UserUid);

            modelBuilder.Entity<BankModel>()
                .HasIndex(b => b.UserUid);

            modelBuilder.Entity<BrokerModel>()
                .HasIndex(b => b.UserUid);

            modelBuilder.Entity<WalletModel>()
                .HasIndex(w => w.UserUid);

            modelBuilder.Entity<CryptoExchangeModel>()
                .HasIndex(c => c.UserUid);

            modelBuilder.Entity<AccountModel>()
                .HasIndex(a => a.UserUid);

            modelBuilder.Entity<DepositModel>()
                .HasIndex(d => d.UserUid);

            modelBuilder.Entity<InvestmentPositionModel>()
                .HasIndex(ip => ip.UserUid);

            modelBuilder.Entity<CryptoAssetModel>()
                .HasIndex(ca => ca.UserUid);

            modelBuilder.Entity<PreciousMetalModel>()
                .HasIndex(pm => pm.UserUid);

            modelBuilder.Entity<OperationModel>()
                .HasIndex(o => o.UserUid);
            modelBuilder.Entity<OperationModel>()
                .HasIndex(o => new { o.Date, o.UserUid });

            modelBuilder.Entity<InflationModel>()
                .HasIndex(inf => new { inf.Year, inf.Month, inf.UserUid })
                .IsUnique();

            modelBuilder.Entity<PriceHistoryModel>()
                .HasIndex(ph => new { ph.InstrumentId, ph.Date, ph.UserUid })
                .IsUnique();
            modelBuilder.Entity<PriceHistoryModel>()
                .HasIndex(ph => new { ph.InstrumentId, ph.Date });

            // --- Связи ---
            modelBuilder.Entity<AccountModel>()
                .HasOne(a => a.Bank)
                .WithMany()
                .HasForeignKey(a => a.BankId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AccountModel>()
                .HasOne(a => a.Broker)
                .WithMany()
                .HasForeignKey(a => a.BrokerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AccountModel>()
                .HasOne(a => a.Wallet)
                .WithMany()
                .HasForeignKey(a => a.WalletId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AccountModel>()
                .HasOne(a => a.CryptoExchange)
                .WithMany()
                .HasForeignKey(a => a.CryptoExchangeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WalletModel>()
                .HasOne(w => w.Currency)
                .WithMany()
                .HasForeignKey(w => w.CurrencyId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DepositModel>()
                .HasOne(d => d.Account)
                .WithMany(a => a.Deposits)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OperationModel>()
                .HasOne(o => o.Account)
                .WithMany()
                .HasForeignKey(o => o.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OperationModel>()
                .HasOne(o => o.Instrument)
                .WithMany()
                .HasForeignKey(o => o.InstrumentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvestmentPositionModel>()
                .HasOne(ip => ip.Instrument)
                .WithMany()
                .HasForeignKey(ip => ip.InstrumentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvestmentPositionModel>()
                .HasOne(ip => ip.Account)
                .WithMany()
                .HasForeignKey(ip => ip.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CryptoAssetModel>()
                .HasOne(ca => ca.Instrument)
                .WithMany()
                .HasForeignKey(ca => ca.InstrumentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PriceHistoryModel>()
                .HasOne(ph => ph.Instrument)
                .WithMany()
                .HasForeignKey(ph => ph.InstrumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Seed: Типы операций ---
            modelBuilder.Entity<OperationTypeModel>().HasData(
                new OperationTypeModel { Id = 1, Name = "Пополнение депозита", Category = OperationCategory.Deposit },
                new OperationTypeModel { Id = 2, Name = "Снятие с депозита", Category = OperationCategory.Deposit },
                new OperationTypeModel { Id = 3, Name = "Начисление процентов", Category = OperationCategory.Deposit },
                new OperationTypeModel { Id = 4, Name = "Покупка ценных бумаг", Category = OperationCategory.Investment },
                new OperationTypeModel { Id = 5, Name = "Продажа ценных бумаг", Category = OperationCategory.Investment },
                new OperationTypeModel { Id = 6, Name = "Дивиденд", Category = OperationCategory.Investment },
                new OperationTypeModel { Id = 7, Name = "Купон", Category = OperationCategory.Investment },
                new OperationTypeModel { Id = 8, Name = "Комиссия", Category = OperationCategory.Investment },
                new OperationTypeModel { Id = 9, Name = "Налог", Category = OperationCategory.Investment },
                new OperationTypeModel { Id = 10, Name = "Покупка криптовалюты", Category = OperationCategory.Crypto },
                new OperationTypeModel { Id = 11, Name = "Продажа криптовалюты", Category = OperationCategory.Crypto },
                new OperationTypeModel { Id = 12, Name = "Покупка драгметалла", Category = OperationCategory.PreciousMetal },
                new OperationTypeModel { Id = 13, Name = "Продажа драгметалла", Category = OperationCategory.PreciousMetal },
                new OperationTypeModel { Id = 14, Name = "Перевод", Category = OperationCategory.Transfer },
                new OperationTypeModel { Id = 15, Name = "Прочее", Category = OperationCategory.Other }
            );
        }

    }
    
}
