using CentCom.Common.Models;
using CentCom.Server.Extensions;
using System;
using Xunit;

namespace CentCom.Test
{
    public class Ban_GetCanonicalKeyShould
    {
        [Fact]
        public void GetCanonicalKey_FromRaw_ReturnTrue()
        {
            var rawKey = "B o bbahbrown";
            Assert.True("bobbahbrown" == BanExtensions.GetCanonicalKey(rawKey));
        }

        [Fact]
        public void GetCanonicalKey_NullArgument_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => BanExtensions.GetCanonicalKey(null));
        }
    }
}
