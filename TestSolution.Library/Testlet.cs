namespace TestSolution.Library
{
    public class Testlet
    {
        public const int COUNT_PRETEST = 4;
        public const int COUNT_OPERATIONAL = 6;
        public const int COUNT_OUT_OF_ORDER = 2;
        public const int COUNT_IN_ORDER = COUNT_PRETEST + COUNT_OPERATIONAL - COUNT_OUT_OF_ORDER;

        public readonly string TestSolutionId;
        private readonly List<Item> Items;
        private static readonly Random random = new();

        public Testlet(string testSolutionId, List<Item> items)
        {
            ItemsValidation(items);

            TestSolutionId = testSolutionId;
            Items = items;
        }

        public List<Item> Randomize()
        {
            List<Item> result = new();
            List<Item> tempCollecion = new(Items);

            for (int i = 0; i < COUNT_OUT_OF_ORDER; i++)
            {
                result.Add(tempCollecion
                    .Where(x => x.ItemType == ItemTypeEnum.Pretest)
                    .ToArray()[random.Next(COUNT_PRETEST - i)]);
                tempCollecion.Remove(result.Last());
            }

            result.AddRange(tempCollecion
                .Select(x => new { element = x, rank = random.Next() })
                .OrderBy(x => x.rank)
                .Select(x => x.element));
            return result;
        }

        private static void ItemsValidation(List<Item> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (items.Count(x => x.ItemType == ItemTypeEnum.Pretest) != COUNT_PRETEST)
                throw new ArgumentException($"Count of pretest items not equal COUNT_PRETEST");
            if (items.Count(x => x.ItemType == ItemTypeEnum.Operational) != COUNT_OPERATIONAL)
                throw new ArgumentException($"Count of operational items not equal COUNT_OPERATIONAL");
            if (items.Select(x => x.ItemId).Distinct().Count() != COUNT_PRETEST + COUNT_OPERATIONAL)
                throw new ArgumentException($"ItemIds not unique");
        }
    }
}