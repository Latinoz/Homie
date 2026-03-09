using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homie.Areas.Finances.Models
{
    /// <summary>Финансовый счёт (банковский, брокерский, кошелёк, криптобиржа)</summary>
    public class AccountModel
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(255)")]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Банк")]
        public int? BankId { get; set; }
        public BankModel Bank { get; set; }

        [Display(Name = "Брокер")]
        public int? BrokerId { get; set; }
        public BrokerModel Broker { get; set; }

        [Display(Name = "Кошелёк")]
        public int? WalletId { get; set; }
        public WalletModel Wallet { get; set; }

        [Display(Name = "Крипто биржа")]
        public int? CryptoExchangeId { get; set; }
        public CryptoExchangeModel CryptoExchange { get; set; }

        [Display(Name = "Тип счёта")]
        public AccountType AccountType { get; set; }

        [Display(Name = "Валюта")]
        public int CurrencyId { get; set; }
        public CurrencyModel Currency { get; set; }

        [Column(TypeName = "varchar(500)")]
        [Display(Name = "Заметки")]
        public string Notes { get; set; }

        [Display(Name = "Активен")]
        public bool IsActive { get; set; } = true;

        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }

        public ICollection<DepositModel> Deposits { get; set; }
    }
}
