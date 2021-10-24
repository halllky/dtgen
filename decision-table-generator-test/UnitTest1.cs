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
            dt.AddCondition("����1:y,n");
            dt.AddCondition("����2:y,n");
            dt.AddCondition("����3:1,2,3,4");
            dt.AddAction("���Ҍ���1");
            dt.AddAction("���Ҍ���2");
            dt.AddConstraint("IF ����1 == y THEN ����3 IN 1,2,4");

            var expected1 = @"����1,y,y,y,y,y,y,n,n,n,n,n,n,n,n
����2,y,y,y,n,n,n,y,y,y,y,n,n,n,n
����3,1,2,4,1,2,4,1,2,3,4,1,2,3,4
���Ҍ���1,,,,,,,,,,,,,,
���Ҍ���2,,,,,,,,,,,,,,";
            var actual1 = dt.ToString(",", DecisionTable.Direction.Horizontal);
            Assert.AreEqual(expected1, actual1);

            dt.ClearConstraint();
            dt.AddConstraint("if ����1 !in n then ����3 = 4");
            var expected2 = @"����1|����2|����3|���Ҍ���1|���Ҍ���2
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
