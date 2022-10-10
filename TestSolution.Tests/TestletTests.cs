using NUnit.Framework;
using TestSolution.Library;

namespace TestSolution.Tests
{
    [TestFixture]
    public class TestletTests
    {
        /// <summary>
        /// All elements from base list are existed in the result and result is not null
        /// </summary>
        [Test]
        public void Randomize_BaseTests()
        {
            var baseItems = GenerateBaseItems(Testlet.COUNT_PRETEST, Testlet.COUNT_OPERATIONAL);
            var testlet = new Testlet("", baseItems);
            List<Item> items = testlet.Randomize();
            Assert.IsNotNull(items);
            Assert.IsTrue(items.All(x => baseItems.Single(y => y.ItemId == x.ItemId) != null));
        }

        /// <summary>
        /// Count Pretest and Operational items should be COUNT_PRETEST and COUNT_OPERATIONAL
        /// </summary>
        [TestCase(Testlet.COUNT_PRETEST - 1, Testlet.COUNT_OPERATIONAL, ExpectedResult = "Count of pretest items not equal COUNT_PRETEST")]
        [TestCase(Testlet.COUNT_PRETEST + 1, Testlet.COUNT_OPERATIONAL, ExpectedResult = "Count of pretest items not equal COUNT_PRETEST")]
        [TestCase(Testlet.COUNT_PRETEST, Testlet.COUNT_OPERATIONAL - 1, ExpectedResult = "Count of operational items not equal COUNT_OPERATIONAL")]
        [TestCase(Testlet.COUNT_PRETEST, Testlet.COUNT_OPERATIONAL + 1, ExpectedResult = "Count of operational items not equal COUNT_OPERATIONAL")]
        public string? Randomize_IncorrectListComposition(int countPretest, int countOperational)
        {
            var baseItems = GenerateBaseItems(countPretest, countOperational);
            return Assert.Throws<ArgumentException>(() => new Testlet("", baseItems))?.Message;
        }

        /// <summary>
        /// Paramentr items should be not NULL
        /// </summary>
        [Test]
        public void Randomize_EmptyList()
        {
            #pragma warning disable CS8625
            var exception = Assert.Throws<ArgumentNullException>(() => new Testlet("", items: null));
            #pragma warning restore CS8625
            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'items')"));
        }

        /// <summary>
        /// Elements from first and second positions are Pretest only
        /// </summary>
        [Test]
        public void Randomize_IsFirstPartCorrect()
        {
            var baseItems = GenerateBaseItems(Testlet.COUNT_PRETEST, Testlet.COUNT_OPERATIONAL);
            var countIterations = 100;
            var testlet = new Testlet("", baseItems);

            for (int iteration = 0; iteration < countIterations; iteration++)
            {
                List<Item> items = testlet.Randomize();
                for (int i = 0; i < Testlet.COUNT_OUT_OF_ORDER; i++)
                {
                    Assert.IsTrue(items[i].ItemType == ItemTypeEnum.Pretest);
                }
            }
        }

        /// <summary>
        /// Every Pretest elements should be in first positions with probability (1/count of Pretest elements)
        /// </summary>
        [Test]
        public void Randomize_IsDistributionFirstPartCorrect()
        {
            var baseItems = GenerateBaseItems(Testlet.COUNT_PRETEST, Testlet.COUNT_OPERATIONAL);
            var countIterations = 1000;
            var allowableErorr = 0.005;
            var testlet = new Testlet("", baseItems);
            var countPosition = new List<double[]>();
            for (int i = 0; i < Testlet.COUNT_PRETEST; i++)
            {
                countPosition.Add(new double[Testlet.COUNT_OUT_OF_ORDER]);
            }

            for (int iteration = 0; iteration < countIterations; iteration++)
            {
                List<Item> items = testlet.Randomize();
                for (int i = 0; i < Testlet.COUNT_OUT_OF_ORDER; i++)
                {
                    countPosition[Convert.ToInt32(items[i].ItemId)][i]++;
                }
            }

            foreach (var item in countPosition)
                for (int i = 0; i < Testlet.COUNT_OUT_OF_ORDER; i++)
                    item[i] /= countIterations;

            var targetProbability = 1.0 / Testlet.COUNT_PRETEST;
            for (int i = 0; i < Testlet.COUNT_PRETEST; i++)
            {
                Warn.Unless(countPosition[i].All(x => Math.Pow(targetProbability - x, 2) < allowableErorr));
            }
        }

        /// <summary>
        /// Every Operational elements should be in any second positions with probability (1/count of all elements without out of orders element)
        /// and every Pretest elements should be in any second positions with probability (1/count of all elements without out of orders element) x (1/COUNT out of orders positions)
        /// </summary>
        [Test]
        public void Randomize_IsDistributionSecondPartCorrect()
        {
            var baseItems = GenerateBaseItems(Testlet.COUNT_PRETEST, Testlet.COUNT_OPERATIONAL);
            var countIterations = 1000;
            var allowableErorr = 0.005;
            var countPosition = new List<double[]>();
            foreach (var item in baseItems)
            {
                countPosition.Add(new double[Testlet.COUNT_IN_ORDER]);
            }

            var testlet = new Testlet("", baseItems);
            for (int iteration = 0; iteration < countIterations; iteration++)
            {
                List<Item> items = testlet.Randomize();
                for (int i = 0; i < items.Count - Testlet.COUNT_OUT_OF_ORDER; i++)
                {
                    countPosition[Convert.ToInt32(items[i + Testlet.COUNT_OUT_OF_ORDER].ItemId)][i]++;
                }
            }

            foreach (var item in countPosition)
                for (int i = 0; i < item.Length; i++)
                    item[i] /= countIterations;

            var targetProbabilityInOrder = 1.0 / (Testlet.COUNT_IN_ORDER * Testlet.COUNT_OUT_OF_ORDER);
            var targetProbabilityOutOrder = 1.0 / Testlet.COUNT_IN_ORDER;
            for (int i = 0; i < countPosition.Count; i++)
            {
                if (i < Testlet.COUNT_PRETEST)
                {
                    Warn.Unless(countPosition[i].All(x => Math.Pow(targetProbabilityInOrder - x, 2) < allowableErorr));
                }
                else
                {
                    Warn.Unless(countPosition[i].All(x => Math.Pow(targetProbabilityOutOrder - x, 2) < allowableErorr));
                }
            }
        }

        private static List<Item> GenerateBaseItems(int countPretest, int countOperational)
        {
            var result = new List<Item>(countPretest + countOperational);
            for (int i = 0; i < countPretest + countOperational; i++)
            {
                result.Add(new Item()
                {
                    ItemId = i.ToString(),
                    ItemType = (i < countPretest) ? ItemTypeEnum.Pretest : ItemTypeEnum.Operational
                });
            }
            return result;
        }
    }
}