using Bi.Core.Interfaces;
using Bi.Entities.Input;

namespace Bi.Services.IService;

public interface IDynamicCodeService : IDependency
{
    Task<(string, bool)> syntaxRules(DynamicCodeInput input);
}

