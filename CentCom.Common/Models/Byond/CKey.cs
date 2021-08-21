using CentCom.Common.Abstract;

namespace CentCom.Common.Models.Byond
{
    public record CKey(string Key) : ICKey
    {
        public string CanonicalKey => KeyUtilities.GetCanonicalKey(Key);

        public static implicit operator CKey(string key) => new CKey(key);
    }
}