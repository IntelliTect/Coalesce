using GenFu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coalesce.Domain
{
    /// <summary>
    /// Allow for setting the random number generator seed in GenFu to make it deterministic.
    /// </summary>
    public class GenFuBaseValueGenerator:BaseValueGenerator
    {
        private static int? _randomSeed = null;

        /// <summary>
        /// Random number seed. Null if not set and using Environment.TickCount.
        /// Every time this is set, a new Random object will be created.
        /// </summary>
        public static int? RandomSeed
        {
            get
            {
                return _randomSeed;
            }
            set
            {
                _randomSeed = value;
                if (_randomSeed.HasValue)
                {
                    _random = new Random(_randomSeed.Value);
                }
                else
                {
                    _random = new Random(Environment.TickCount);
                }
            }
        }
    }
}
