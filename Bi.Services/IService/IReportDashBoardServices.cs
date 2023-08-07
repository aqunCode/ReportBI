using Bi.Core.Interfaces;
using Bi.Entities.Input;
using Bi.Entities.Response;
using MagicOnion;
using Newtonsoft.Json.Linq;

namespace Bi.Services.IService;

public interface IReportDashBoardServices : IDependency {

     UnaryResult<(bool res, string msg)> insert(DashBoardInput input, bool master = true);

    UnaryResult<DashBoardOutput> preview(string reportCode);

    UnaryResult<string> getChartData(ChartInput input);
    /// <summary>
    /// 数据二次处理
    /// </summary>
    /// <param name="result"></param>
    /// <param name="autoTurn"></param>
    /// <param name="setCode"></param>
    /// <returns></returns>
    UnaryResult<JToken> turnData(string setCode,string result, AutoTurn autoTurn);
}

