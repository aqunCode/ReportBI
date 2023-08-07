using Bi.Core.Interfaces;
using Bi.Entities.Input;

namespace Bi.Services.IService;

public interface IDataSetRelationServices : IDependency
{
    Task<double> addAsync(DataSetRelationInput input);
}
