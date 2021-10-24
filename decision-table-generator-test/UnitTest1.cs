using DTGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace decision_table_generator_test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var dt = new DecisionTable();
            dt.AddCondition("ğŒ1:y,n");
            dt.AddCondition("ğŒ2:y,n");
            dt.AddCondition("ğŒ3:1,2,3,4");
            dt.AddAction("Šú‘ÒŒ‹‰Ê1");
            dt.AddAction("Šú‘ÒŒ‹‰Ê2");
            dt.AddConstraint("IF ğŒ1 == y THEN ğŒ3 IN 1,2,4");

            var expected1 = @"ğŒ1,y,y,y,y,y,y,n,n,n,n,n,n,n,n
ğŒ2,y,y,y,n,n,n,y,y,y,y,n,n,n,n
ğŒ3,1,2,4,1,2,4,1,2,3,4,1,2,3,4
Šú‘ÒŒ‹‰Ê1,,,,,,,,,,,,,,
Šú‘ÒŒ‹‰Ê2,,,,,,,,,,,,,,";
            var actual1 = dt.ToString(",", DecisionTable.Direction.Horizontal);
            Assert.AreEqual(expected1, actual1);

            dt.ClearConstraint();
            dt.AddConstraint("if ğŒ1 !in n then ğŒ3 = 4");
            var expected2 = @"ğŒ1|ğŒ2|ğŒ3|Šú‘ÒŒ‹‰Ê1|Šú‘ÒŒ‹‰Ê2
y|y|4||
y|n|4||
n|y|1||
n|y|2||
n|y|3||
n|y|4||
n|n|1||
n|n|2||
n|n|3||
n|n|4||";
            var actual2 = dt.ToString("|", DecisionTable.Direction.Vertical);
            Assert.AreEqual(expected2, actual2);
        }
    }
}
