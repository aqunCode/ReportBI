using Bi.Entities.Entity;

namespace Bi.Entities.Response;

public class DataCollectResponse : DataCollect {

    /// <summary>
    /// 查询条件列表
    /// </summary>
    public List<DataCollectItem>? DataSetParamDtoList {set;get;}
    
    /// <summary>
    /// 参数列表
    /// </summary>
    public List<String>? SetParamList {set;get;}
}

