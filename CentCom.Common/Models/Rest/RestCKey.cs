using CentCom.Common.Abstract;

namespace CentCom.Common.Models.Rest;

public record RestCKey(string CanonicalKey) : ICKey;