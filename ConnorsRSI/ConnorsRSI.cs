// -------------------------------------------------------------------------------------------------------------------------------------------
//
//    Connors RSI (CRSI) is a technical analysis indicator created by Larry Connors that is actually a composite of three separate components.
//    This is the Connors RSI (CRSI) custom indicator for cTrader based on Automate API.
//
// -------------------------------------------------------------------------------------------------------------------------------------------

using cAlgo.API;
using cAlgo.API.Indicators;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ConnorsRSI : Indicator
    {
        private RelativeStrengthIndex _rsi;
        private RelativeStrengthIndex _upDownRsi;
        private IndicatorDataSeries _upDownSeries;

        [Parameter("RSI Periods", DefaultValue = 3, MinValue = 1)]
        public int RsiPeriods { get; set; }

        [Parameter("UpDown Periods", DefaultValue = 2, MinValue = 1)]
        public int UpDownPeriods { get; set; }

        [Parameter("ROC Periods", DefaultValue = 100, MinValue = 1)]
        public int RocPeriods { get; set; }

        [Output("Main")]
        public IndicatorDataSeries Result { get; set; }

        protected override void Initialize()
        {
            _upDownSeries = CreateDataSeries();

            _rsi = Indicators.RelativeStrengthIndex(Bars.ClosePrices, RsiPeriods);
            _upDownRsi = Indicators.RelativeStrengthIndex(_upDownSeries, UpDownPeriods);
        }

        public override void Calculate(int index)
        {
            var bar = Bars[index];

            int upDownValue = 0;

            if (bar.Close > bar.Open)
            {
                upDownValue = 1;
            }
            else if (bar.Close < bar.Open)
            {
                upDownValue = -1;
            }

            if (index > 0 && (_upDownSeries[index - 1] > 0 && upDownValue == 1) || (_upDownSeries[index - 1] < 0 && upDownValue == -1))
            {
                _upDownSeries[index] = _upDownSeries[index - 1] + upDownValue;
            }
            else
            {
                _upDownSeries[index] = upDownValue;
            }

            Result[index] = (_rsi.Result[index] + _upDownRsi.Result[index] + GetRoc(index)) / 3;
        }

        private double GetRoc(int index)
        {
            var percentChanges = new List<double>();

            for (int barIndex = index; barIndex > index - RocPeriods; barIndex--)
            {
                percentChanges.Add((Bars.ClosePrices[barIndex] - Bars.OpenPrices[barIndex]) / Bars.OpenPrices[barIndex] * 100);
            }

            var currentBarPercentChange = (Bars.ClosePrices[index] - Bars.OpenPrices[index]) / Bars.OpenPrices[index] * 100;
            var numberOfValuesLessThanGivenValue = percentChanges.Where(iValue => iValue <= currentBarPercentChange).Count();

            return 100.0 * numberOfValuesLessThanGivenValue / percentChanges.Count;
        }
    }
}