using Bi.Entities.Entity;

namespace Bi.Entities.Input;

public class MarkValueInput
{
    /// <summary>
    /// 要获取值的Mark属性
    /// </summary>
    public BiMarkField? MarkField{ get; set; }
    /// <summary>
    /// 加入的筛选器条件
    /// </summary>
    public List<BiFilterField>? FilterFields { get; set; }
}
