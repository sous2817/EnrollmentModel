using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Semio.ClientService.Data.Intelligence.Enrollment
{
    /// <summary>
    /// Holds a collection of values representing a desired rate
    /// for a desired month.
    /// 
    /// Work still required to ensure invariants between the 
    /// underlying rate collection and rate rules.
    /// </summary>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class VariableRateCollection
    {
        #region Private Fields

        #endregion

        #region Public Properties - Part of Refactor

        /// <summary>
        /// Gets or sets the <see cref="VariableRate"/> with the specified key.
        /// </summary>
        /// <value></value>
        public VariableRate this[int key]
        {
            get { return Rates[key]; }
            set { Rates[key] = value; }
        }

        /// <summary>
        /// Gets the variable rates.
        /// </summary>
        /// <value>The variable rates.</value>
        public IEnumerable<VariableRate> VariableRates => Rates.AsEnumerable();

        #endregion

        /// <summary>
        /// List of month-to-rate values.
        /// </summary>
        public List<VariableRate> Rates { get; set; }

        /// <summary>
        /// Returns if the rate list is empty.
        /// </summary>
        public bool IsEmpty => Rates.Count == 0;

        /// <summary>
        /// 
        /// </summary>
        public VariableRateCollection()
        {
            Rates = new List<VariableRate>();
        }

        /// <summary>
        /// Initializes a rate collection with a default variable rate.
        /// </summary>
        /// <param name="month"></param>
        /// <param name="rate"></param>
        public VariableRateCollection(int month, double rate)
        {
            Rates = new List<VariableRate> { new VariableRate { Month = month, Rate = rate}};
        }

        /// <summary>
        /// Initializes the collection from a dictionary of month
        /// and rate values.
        /// </summary>
        /// <param name="rateTable"></param>
        public VariableRateCollection(Dictionary<int, double> rateTable)
        {
            Rates = new List<VariableRate>(
                rateTable.Select(rate => new VariableRate { Month = rate.Key, Rate = rate.Value }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        public double GetVariableRate(int month)
        {
            if (Rates.Count == 0) return 0;

            var rateTable = Rates.ToDictionary(r => r.Month, r => r.Rate);
            var index = GetIndex(rateTable.Keys, month);
            var rate = 0.0;
            rateTable.TryGetValue(index, out rate);

            return rate;
        }

        private IEnumerable<double> GetRateList(double lastMonth)
        {
            for (var i = 1; i <= lastMonth; i++)
                yield return GetVariableRate(i);
        }

        private int GetIndex(IEnumerable<int> numberList, int highValue)
        {
            var possibleValues = numberList.Where(num => num <= highValue);
            return (possibleValues.Count() == 0) ? 0 : possibleValues.Max();
        }
    }
}
