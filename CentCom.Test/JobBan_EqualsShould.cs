using CentCom.Common.Models;
using Xunit;

namespace CentCom.Test
{
    public class JobBan_EqualsShould
    {
        [Fact]
        public void Equals_SameJobBan_ReturnTrue()
        {
            var jobA = new JobBan()
            {
                BanId = 18922,
                Job = "ooc"
            };

            var jobB = new JobBan()
            {
                BanId = 18922,
                Job = "ooc"
            };

            Assert.True(jobA == jobB, "Two jobs equal by internal values should be equal");
            Assert.True(jobA.GetHashCode() == jobB.GetHashCode(), "Two jobs equal by internal values should have the same hashcode");
        }

        /// <summary>
        /// This test is important for comparing bans in the database versus
        /// bans that have been parsed, we shouldn't consider ban id for
        /// job ban equality as semantically it would never be practiced.
        /// </summary>
        [Fact]
        public void Equals_SameJobBan_DifferentID_ReturnTrue()
        {
            var jobA = new JobBan()
            {
                BanId = 18922,
                Job = "ooc"
            };

            var jobB = new JobBan()
            {
                BanId = 0,
                Job = "ooc"
            };

            Assert.True(jobA == jobB, "Two jobs equal by job, even with differing IDs, should be equal");
            Assert.True(jobA.GetHashCode() == jobB.GetHashCode(), "Two jobs equal by job, even with differing ids, should have the same hashcode");
        }
    }
}
