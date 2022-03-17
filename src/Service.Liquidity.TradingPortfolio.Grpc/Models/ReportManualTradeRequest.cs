using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Service.Liquidity.TradingPortfolio.Grpc.Models
{
    [DataContract]
    public class ReportManualTradeRequest
    {
        [DataMember(Order = 1)] public string BrokerId { get; set; }
        [DataMember(Order = 2)] public string WalletName { get; set; }
        [DataMember(Order = 3)] public string AssociateSymbol { get; set; }
        [DataMember(Order = 4)] public decimal Price { get; set; }
        [DataMember(Order = 5)] public decimal BaseVolume { get; set; }
        [DataMember(Order = 6)] public decimal QuoteVolume { get; set; }
        [DataMember(Order = 7)] public string Comment { get; set; }
        [DataMember(Order = 8)] public string User { get; set; }
        [DataMember(Order = 9)] public string FeeAsset { get; set; }
        [DataMember(Order = 10)] public decimal FeeVolume { get; set; }
        [DataMember(Order = 11)] public string BaseAsset { get; set; }
        [DataMember(Order = 12)] public string QuoteAsset { get; set; }

        public bool IsValid(out string message)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(BrokerId))
            {
                errors.Add($"{nameof(BrokerId)} can't be empty");
            }

            if (string.IsNullOrWhiteSpace(WalletName))
            {
                errors.Add($"{nameof(WalletName)} can't be empty");
            }

            if (string.IsNullOrWhiteSpace(AssociateSymbol))
            {
                errors.Add($"{nameof(AssociateSymbol)} can't be empty");
            }

            if (string.IsNullOrWhiteSpace(BaseAsset))
            {
                errors.Add($"{nameof(BaseAsset)} can't be empty");
            }

            if (string.IsNullOrWhiteSpace(QuoteAsset))
            {
                errors.Add($"{nameof(QuoteAsset)} can't be empty");
            }

            if (string.IsNullOrWhiteSpace(Comment))
            {
                errors.Add($"{nameof(Comment)} can't be empty");
            }

            if (string.IsNullOrWhiteSpace(User))
            {
                errors.Add($"{nameof(User)} can't be empty");
            }

            if (Price == 0)
            {
                errors.Add($"{nameof(Price)} can't be 0");
            }

            if (BaseVolume == 0)
            {
                errors.Add($"{nameof(BaseVolume)} can't be 0");
            }

            if (QuoteVolume == 0)
            {
                errors.Add($"{nameof(QuoteVolume)} can't be 0");
            }

            if (BaseVolume > 0 && QuoteVolume > 0 || BaseVolume < 0 && QuoteVolume < 0)
            {
                errors.Add($"{nameof(BaseVolume)} and {nameof(QuoteVolume)} can't be with same symbol");

            }

            message = string.Join("; ", errors);

            return errors.Count == 0;
        }
    }
}