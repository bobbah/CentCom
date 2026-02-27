using CentCom.Common;

namespace CentCom.Test;

public class Ban_GetCanonicalKeyShould
{
    [Test]
    public async Task GetCanonicalKey_FromRaw_ReturnTrue()
    {
        const string rawKey = "B o bbahbrown";
        await Assert.That(KeyUtilities.GetCanonicalKey(rawKey)).IsEqualTo("bobbahbrown");
    }

    [Test]
    public void GetCanonicalKey_NullArgument_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => KeyUtilities.GetCanonicalKey(null));
    }
}