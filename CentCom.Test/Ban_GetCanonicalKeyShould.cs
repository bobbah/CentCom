using System;
using CentCom.Common;
using Xunit;

namespace CentCom.Test;

public class Ban_GetCanonicalKeyShould
{
    [Fact]
    public void GetCanonicalKey_FromRaw_ReturnTrue()
    {
        var rawKey = "B o bbahbrown";
        Assert.True("bobbahbrown" == KeyUtilities.GetCanonicalKey(rawKey));
    }

    [Fact]
    public void GetCanonicalKey_NullArgument_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => KeyUtilities.GetCanonicalKey(null));
    }
}