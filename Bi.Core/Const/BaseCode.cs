using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Core.Const;

public class BaseCode
{
    public static string toChinesCode(double code)
    {
        switch(code)
        {
            case BaseErrorCode.Successful:
                return "执行成功";
            case BaseErrorCode.Wrong_AccountPassword:
                return "账号密码有误";
            case BaseErrorCode.Invalid_Account:
                return "无效输入账号";
            default:
                return "编码未设定";
        }
    }
}
