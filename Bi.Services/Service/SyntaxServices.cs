
using Bi.Core.Extensions;
using Bi.Entities.Entity;
using Bi.Services.IService;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Bi.Services.Service;

public class SyntaxServices : ISyntaxServices
{
    /// <summary>
    /// 自定义字段语法中的列名数据类型
    /// </summary>
    private Dictionary<string, SyntaxDataType> dic;
    /// <summary>
    /// 关键字列表
    /// </summary>
    private string[] keywords = new string[] { "IF","AND","OR", "THEN", "ELSE", "END", "ELSEIF", "MAX", "MIN", "SUM", "AVG", "COUNT", "COUNTDISTINCT", "INT","CHAR","DATE","ABS", "FIXED",",", ":", "{", "}" };
    /// <summary>
    /// 运算符列表
    /// </summary>
    private string[] operators = new string[] { "+", "-", "*", "/","=",">","<",">=","<=","||" };
    /// <summary>
    /// 模板列表
    /// </summary>
    private List<SyntaxModel> models = new();
    /// <summary>
    /// 语法分析字符列表
    /// </summary>
    List<SyntaxModelItem> longMemery = new();
    /// <summary>
    /// 判断是中文还是英文
    /// </summary>
    private string language = "ENGLISH";
    /// <summary>
    /// 自定义字段的数据类型判断
    /// </summary>
    private string dataType = "Number";
    /// <summary>
    /// 自定义字段名称（用来当fixed函数做别名）
    /// </summary>
    private string fieldCode;
    /// <summary>
    /// 判断递归层级
    /// </summary>
    private int level = 0;
    /// <summary>
    /// 解析过的sql
    /// </summary>
    private StringBuilder sb = new();
    /// <summary>
    /// 语法规则匹配集
    /// </summary>
    List<(string, string)> syntaxs = new();
    /// <summary>
    /// 解释执行的词组
    /// </summary>
    private string[] arr;
    /// <summary>
    /// 解释执行到第几个了
    /// </summary>
    private int index = 0;
    /// <summary>
    /// 数据源类型
    /// </summary>
    private string sourceType;
    /// <summary>
    /// 异常信息
    /// </summary>
    private string message = "OK";
    /// <summary>
    /// 数据库连接
    /// </summary>
    private IDbEngineServices dbEngineService;

    public SyntaxServices(IDbEngineServices dbService)
    {
        this.dbEngineService = dbService;

    }
    // 调用的语法解析
    public (string, string) syntaxFuction(string fieldFunction, string sourceType, string fieldCode, Dictionary<string, SyntaxDataType> dic)
    {
        sb.Clear();
        index = 0;
        message = "OK";
        level = 0;
        language = "ENGLISH";
        dataType = "Number";
        this.dic = dic;
        longMemery.Clear();

        this.fieldCode = fieldCode;
        this.sourceType = sourceType;
        // 语法格式化  MAX ( INT ( LEDRPT.RPT_UNIT_TRACKOUT_DETAIL.DDAY ) ) 
        formatting(fieldFunction);
        if (message != "OK")
            return (message, "ERROR");

        // 检查数据类型
        checkDataType();
        if (message != "OK")
            return (message, "ERROR");


        // 检查函数是否结束
        while (checkEnd())
        {
            routerCompiler();
            if (message != "OK")
                return (message, "ERROR");
            index++;
        }

        if (message == "OK")
            return (sb.ToString().Replace('"', '\''), dataType);
        else
            return (message, "ERROR");
    }
    // function routing
    private void routerCompiler()
    {
        switch (arr[index])
        {
            case "IF":
                level++;
                message = routerIf();
                break;
            case "MAX":
                level++;
                message = routerMax();
                break;
            case "SUM":
                level++;
                message = routerSum();
                break;
            case "MIN":
                level++;
                message = routerMin();
                break;
            case "COUNT":
                level++;
                message = routerCount();
                break;
            case "COUNTDISTINCT":
                level++;
                message = routerCountDistinct();
                break;
            case "{": // FIXED 函数
                level++;
                message = routerFixed();
                break;
            case "INT":
                level++;
                message = routerInt();
                break;
            case "ABS":
                level++;
                message = routerAbs();
                break;
            case "DATE":
                level++;
                message = routerDate();
                break;
            case "CHAR":
                level++;
                message = routerChar();
                break;
            case "(":
            case "（":
                sb.Append("(");
                break;
            case ")":
            case "）":
                sb.Append(")");
                break;
            default:    // 代表直接写常量值或者默认字段名称
                sb.Append(" ");
                sb.Append(arr[index]);
                sb.Append(" ");
                break;
        }
    }
    // IF function
    private string routerIf()
    {
        if (!checkStatus())
            return message;
        //获取关键字列表
        string[] ifArr = getKeyArr("IF");
        string[] optimize = getKeyArr("IFOPTIMIZE");
        //获取关键字列表记录对象
        List<string> shortMemery = getMemery();
        //切换语法规则匹配集
        cutSyntaxs("IF");


        bool flag = false;
        // 寻找当前方法的结尾符 'END'
        int endIndex = getCloseSymbol("END");
        if (endIndex == -1)
            return "ERROR IF 函数找不到 END 结尾符";
        for (; index <= endIndex; index++)
        {
            if (ifArr.IndexOf(arr[index]) != -1)
            {
                // 记录当前执行的关键字
                shortMemery.Add(arr[index]);
                if (optimize.IndexOf(arr[index]) != -1)
                    flag = true;
                else
                    flag = false;

                sb.Append(" ");
                sb.Append(dbEngineService.showFunctionName(arr[index], sourceType));
                sb.Append(" ");

                if (syntaxs.IndexOf((shortMemery[shortMemery.Count - 2], shortMemery[shortMemery.Count - 1])) == -1)
                    return $"ERROR  FIELD: IF函数语法格式错误，({shortMemery[shortMemery.Count - 2]})关键字缺失";
            }
            else
            {
                if (flag)
                {
                    var nextIndex = arr.IndexOf("THEN", index);
                    int between = nextIndex - index;
                    if (between >= 7 && between % 4 == 3 && (arr.IndexOf("AND", index) == -1 || arr.IndexOf("AND", index) > nextIndex))
                    {
                        var tmp1 = arr[index];
                        var tmp2 = arr[index + 1];
                        List<string> tmp3 = new();
                        for (int i = 0; i < between; i += 4)
                        {
                            if (arr[index + i] == tmp1 && arr[index + i + 1] == tmp2)
                            {
                                tmp3.Add(arr[index + i + 2]);
                            }
                            else
                            {
                                routerCompiler();
                                goto end;
                            }
                        }

                        // 语法可优化，拼接sql
                        sb.Append($" {tmp1} ");
                        if (tmp2 == "=")
                            sb.Append(" in (");
                        else
                            sb.Append(" not in (");

                        // 这里再次使用tmp1 当做类型判断
                        tmp1 = tmp3[0].IndexOf('"') == -1 ? "num" : "str";
                        foreach (var item in tmp3)
                        {
                            if (item.IndexOf('"') == -1 && tmp1 == "num" || item.First() == '\'' && item.Last() == '\'' && tmp1 == "str")
                            {
                                sb.Append(' ');
                                sb.Append(item);
                                sb.Append(',');
                            }
                            else
                            {
                                return "ERROR  FIELD: IF 判断字符类型不一致，请输入相同类型";
                            }
                        }
                        sb = sb.Remove(sb.Length - 1);
                        sb.Append(" )");
                        index = nextIndex - 1;
                        flag = false;
                    }
                    else
                    {
                        flag = false;
                        routerCompiler();
                    }
                }
                else
                {
                    routerCompiler();
                }

            }
        }

        // 索引修正（每个函数执行完毕时，所以应该是结尾符的索引）
        index = endIndex;
        level--;

    end: return "OK";
    }
    // MAX function
    private string routerMax()
    {
        if (!checkStatus())
            return message;

        // 寻找当前方法的结尾符 ')'
        int beginIndex = index;
        int endIndex = getCloseParenthesis();
        if (endIndex == -1)
            return "ERROR FIELD:max函数缺失右括号";

        sb.Append(dbEngineService.showFunctionName(arr[index],sourceType));
        index++;

        for (; index <= endIndex; index++)
        {
            routerCompiler();
        }

        // 索引修正（每个函数执行完毕时，所以应该是结尾符的索引）
        index = endIndex;
        level--;
        return "OK";
    }
    // MIN function
    private string routerMin()
    {
        if (!checkStatus())
            return message;

        // 寻找当前方法的结尾符 ')'
        int beginIndex = index;
        int endIndex = getCloseParenthesis();
        if (endIndex == -1)
            return "ERROR FIELD:min函数缺失右括号";

        sb.Append(dbEngineService.showFunctionName(arr[index], sourceType));
        index++;

        for (; index <= endIndex; index++)
        {
            routerCompiler();
        }

        // 索引修正（每个函数执行完毕时，所以应该是结尾符的索引）
        index = endIndex;
        level--;

        return "OK";
    }
    // SUM function
    private string routerSum()
    {
        if (!checkStatus())
            return message;

        // 寻找当前方法的结尾符 ')'
        int beginIndex = index;
        int endIndex = getCloseParenthesis();
        if (endIndex == -1)
            return "ERROR FIELD:SUM函数缺失右括号";

        sb.Append(dbEngineService.showFunctionName(arr[index], sourceType));
        index++;

        for (; index <= endIndex; index++)
        {
            routerCompiler();
        }

        // 索引修正（每个函数执行完毕时，所以应该是结尾符的索引）
        index = endIndex;
        level--;

        return "OK";
    }
    // COUNT function
    private string routerCount()
    {
        if (!checkStatus())
            return message;

        // 寻找当前方法的结尾符 ')'
        int beginIndex = index;
        int endIndex = getCloseParenthesis();
        if (endIndex == -1)
            return "ERROR FIELD:COUNT函数缺失右括号";

        sb.Append(dbEngineService.showFunctionName(arr[index], sourceType));
        index++;

        for (; index <= endIndex; index++)
        {
            routerCompiler();
        }

        // 索引修正（每个函数执行完毕时，所以应该是结尾符的索引）
        index = endIndex;
        level--;

        return "OK";
    }
    // COUNTDISTINCT function
    private string routerCountDistinct()
    {
        if (!checkStatus())
            return message;

        // 寻找当前方法的结尾符 ')'
        int beginIndex = index;
        int endIndex = getCloseParenthesis();
        if (endIndex == -1)
            return "ERROR FIELD:COUNTDISTINCT函数缺失右括号";

        sb.Append(dbEngineService.showFunctionName(arr[index], sourceType));
        index += 2;

        for (; index <= endIndex; index++)
        {
            routerCompiler();
        }

        // 索引修正（每个函数执行完毕时，所以应该是结尾符的索引）
        index = endIndex;
        level--;

        return "OK";
    }
    // FIXED function
    private string routerFixed()
    {
        if (!checkStatus())
            return message;

        string childName = string.Concat(fieldCode, index);
        string selectOutSb = string.Concat(childName, ".value");
        StringBuilder groupSb = new StringBuilder(" GROUP BY ");
        StringBuilder selectInSb = new();
        StringBuilder leftSb = new(string.Concat(childName, " ON "));

        //获取关键字列表
        string[] fixedArr = getKeyArr("FIXED");
        //string[] fixedPivotalArr = getKeyArr("FIXEDPIVOTAL");
        //获取关键字列表记录对象
        List<string> shortMemery = getMemery();
        //切换语法规则匹配集
        cutSyntaxs("FIXED");

        // 寻找当前方法的结尾符 'END'
        int endIndex = getCloseSymbol("}");
        if (endIndex == -1)
            return "ERROR IF 函数找不到 END 结尾符";

        for (; index <= endIndex; index++)
        {
            if (fixedArr.Contains(arr[index]))
            {
                shortMemery.Add(arr[index]);

                if (syntaxs.IndexOf((shortMemery[shortMemery.Count - 2], shortMemery[shortMemery.Count - 1])) == -1)
                    return $"ERROR  FIELD: 函数语法格式错误，({shortMemery[shortMemery.Count - 2]})后关键字缺失";

                if (arr[index] == "{" || arr[index] == "}")
                    continue;
                // 说明到了fixed函数
                if (fixedArr[1] == arr[index])
                {
                    index++;
                    int nextIndex = arr.IndexOf(fixedArr[2], index);
                    if (nextIndex != -1)
                    {
                        for (; index < nextIndex; index++)
                        {
                            groupSb.Append(arr[index]);
                            selectInSb.Append(arr[index]);
                            if (arr[index].Contains('.'))
                            {
                                leftSb.Append(arr[index]);
                                leftSb.Append('=');
                                leftSb.Append(childName);
                                leftSb.Append('.');
                                leftSb.Append(arr[index].Split('.')[1]);
                                leftSb.Append(',');
                            }
                        }
                        leftSb = leftSb.Remove(leftSb.Length - 1);
                    }

                }
                // 说明到了:函数
                if (fixedArr[2] == arr[index])
                {
                    index++;
                    var momentIndex = sb.Length;
                    int nextIndex = arr.IndexOf(fixedArr[3], index);
                    while (index < nextIndex)
                    {
                        routerCompiler();
                        index++;
                    }

                    selectInSb.Append(',');
                    selectInSb.Append(sb.ToString().Substring(momentIndex));
                    selectInSb.Append(" value");

                    sb.Remove(momentIndex);
                }
            }
            else
            {
                routerCompiler();
            }

        }

        // 函数处理完毕，开始insert 返回值sb
        sb.Append(selectOutSb);
        sb.Insert(0, "::");
        sb.Insert(0, groupSb);
        sb.Insert(0, ':');
        sb.Insert(0, selectInSb);
        sb.Insert(0, ':');
        sb.Insert(0, leftSb);
        sb.Insert(0, ':');
        sb.Insert(0, childName);


        // 索引修正（每个函数执行完毕时，所以应该是结尾符的索引）
        index = endIndex;
        level--;

        return "OK";
    }
    // INT function
    private string routerInt()
    {
        if (!checkStatus())
            return message;

        // 寻找当前方法的结尾符 ')'
        int beginIndex = index;
        int endIndex = getCloseParenthesis();
        if (endIndex == -1)
            return "ERROR FIELD:int函数缺失右括号";

        sb.Append(dbEngineService.showFunctionName(arr[index], sourceType));
        index++;

        for (; index <= endIndex; index++)
        {
            routerCompiler();
        }

        // 索引修正（每个函数执行完毕时，所以应该是结尾符的索引）
        index = endIndex;
        level--;

        return "OK";
    }
    // DATE function
    private string routerDate()
    {
        if (!checkStatus())
            return message;

        // 寻找当前方法的结尾符 ')'
        int beginIndex = index;
        int endIndex = getCloseParenthesis();
        if (endIndex == -1)
            return "ERROR FIELD: DATE函数缺失右括号";

        sb.Append(dbEngineService.showFunctionName(arr[index], sourceType));
        index++;

        for (; index <= endIndex; index++)
        {
            routerCompiler();
        }

        // 索引修正（每个函数执行完毕时，所以应该是结尾符的索引）
        index = endIndex;
        level--;

        return "OK";
    }
    // CHAR function
    private string routerChar()
    {
        if (!checkStatus())
            return message;

        // 寻找当前方法的结尾符 ')'
        int beginIndex = index;
        int endIndex = getCloseParenthesis();
        if (endIndex == -1)
            return "ERROR FIELD: CHAR函数缺失右括号";

        sb.Append(dbEngineService.showFunctionName(arr[index], sourceType));
        index++;

        for (; index <= endIndex; index++)
        {
            routerCompiler();
        }

        // 索引修正（每个函数执行完毕时，所以应该是结尾符的索引）
        index = endIndex;
        level--;

        return "OK";
    }
    // ABS function
    private string routerAbs()
    {
        if (!checkStatus())
            return message;

        // 寻找当前方法的结尾符 ')'
        int beginIndex = index;
        int endIndex = getCloseParenthesis();
        if (endIndex == -1)
            return "ERROR FIELD:ABS函数缺失右括号";

        sb.Append(dbEngineService.showFunctionName(arr[index], sourceType));
        index++;

        for (; index <= endIndex; index++)
        {
            routerCompiler();
        }

        // 索引修正（每个函数执行完毕时，所以应该是结尾符的索引）
        index = endIndex;
        level--;

        return "OK";
    }

    #region
    // check the return value data type of the custom syntax
    private void checkDataType()
    {

        // first step: filter out incorrect character preferentially 
        foreach (var item in arr)
        {
            ItemType itype = checkItemType(item);
            if(itype == ItemType.Error)
            {
                message = $"ERROR :【{item}】为无效字符，请确认正确性";
                //message = $"ERROR :【{item}】invalid character，Please verify if this is correct";
                return;
            }

            SyntaxDataType dataType;
            if (itype == ItemType.Num)
                dataType = SyntaxDataType.Number;
            else if (itype == ItemType.Character)
                dataType = SyntaxDataType.String;
            else if (itype == ItemType.ColumnName)
                dataType = dic[item];
            else
                dataType = SyntaxDataType.Arbitrary;

            longMemery.Add(new SyntaxModelItem
            {
                Key = item,
                IType = itype,
                DataType = dataType
            });
        }

        //initializes the syntax validation template
        initOracleSyntaxModel();
        // the second step， Verity the relationship between "ItemType" types  (functional model comparsion)
        SyntaxModelItem res = checkModel();
        this.dataType = res.DataType.ToString();
        // 执行完重置索引
        index = 0;
    }

    // (underline)here is the syntax check section ------------------------------------------------------

    /// <summary>
    /// Template Initialization
    /// </summary>
    private void initOracleSyntaxModel()
    {
        models.Clear();
        List<SyntaxModelItem> items = new();
        #region 0 负数情况
        items.Add(new SyntaxModelItem { Key = "-", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S0-1", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        #endregion
        #region 1、Single template 单个字符串/单个数字/单个列
        items = new();
        items.Add(new SyntaxModelItem{Key = null,IType = ItemType.Character, DataType = SyntaxDataType.String });
        models.Add(new SyntaxModel { Id = "S1-1", ModelItems = items, IType = ItemType.Character, DataType = SyntaxDataType.String });

        items = new();
        items.Add(new SyntaxModelItem{Key = null,IType = ItemType.Num, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S1-2", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });

        items = new();
        items.Add(new SyntaxModelItem{Key = null,IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S1-3", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items = new();
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        models.Add(new SyntaxModel { Id = "S1-4", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        items = new();
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime });
        models.Add(new SyntaxModel { Id = "S1-5", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime });
        #endregion
        #region 2、简单运算模板 + - * /
        items = new(); // 数字+数字 = 数字
        items.Add(new SyntaxModelItem{Key = null,IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem{Key = "+",IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem{Key = null,IType = ItemType.Num, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S2-1", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items = new(); // 数字+列数字 = 列数字
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem { Key = "+", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S2-2", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items = new(); // 列数字+数字 = 列数字
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem { Key = "+", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S2-3", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items = new(); // 列数字+列数字 = 列数字
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem { Key = "+", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S2-4", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });

        items = new(); // 数字-数字 = 数字
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem { Key = "-", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S2-5", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items = new(); // 数字-列数字 = 列数字
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem { Key = "-", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S2-6", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items = new(); // 列数字-数字 = 列数字
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem { Key = "-", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S2-7", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items = new(); // 列数字-列数字 = 列数字
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem { Key = "-", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S2-8", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items = new(); // 时间-时间 = 列数字
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime });
        items.Add(new SyntaxModelItem { Key = "-", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime });
        models.Add(new SyntaxModel { Id = "S2-9", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });

        items = new();
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem { Key = "*", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S2-10", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items = new();
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem { Key = "*", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S2-11", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items = new();
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem { Key = "*", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S2-12", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items = new();
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem { Key = "*", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S2-13", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items = new();
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem { Key = "/", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S2-14", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items = new();
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem { Key = "/", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S2-15", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items = new();
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem { Key = "/", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S2-16", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items = new();
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem { Key = "/", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S2-17", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        #endregion
        #region 3 IF函数模板
        //数字
        items = new()
        {
            new SyntaxModelItem { Key = "IF", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number, Circ = 1 },
            new SyntaxModelItem { Key = " = > < >= <= ", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary, Circ = 1 },
            new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number, Circ = 1 },
            new SyntaxModelItem { Key = " AND OR ", IType = ItemType.Keyword, Circ = 2, CircNum = 4 },
            new SyntaxModelItem { Key = "THEN", IType = ItemType.Keyword, Circ = 1 },
            new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number, Circ = 1 },
            new SyntaxModelItem { Key = "ELSEIF", IType = ItemType.Keyword, Circ = 2, CircNum = 7 },
            new SyntaxModelItem { Key = "ELSE", IType = ItemType.Keyword,Ignore = 1,IgnoreNum = 2 },
            new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = "END", IType = ItemType.Keyword }
        };
        models.Add(new SyntaxModel { Id = "S3-1", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });

        items = new()
        {
            new SyntaxModelItem { Key = "IF", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number, Circ = 1 },
            new SyntaxModelItem { Key = " = > < >= <= ", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary, Circ = 1 },
            new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number, Circ = 1 },
            new SyntaxModelItem { Key = " AND OR ", IType = ItemType.Keyword, Circ = 2, CircNum = 4 },
            new SyntaxModelItem { Key = "THEN", IType = ItemType.Keyword, Circ = 1 },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String, Circ = 1 },
            new SyntaxModelItem { Key = "ELSEIF", IType = ItemType.Keyword, Circ = 2, CircNum = 7 },
            new SyntaxModelItem { Key = "ELSE", IType = ItemType.Keyword ,Ignore = 1,IgnoreNum = 2},
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = "END", IType = ItemType.Keyword }
        };
        models.Add(new SyntaxModel { Id = "S3-2", ModelItems = items, IType = ItemType.Character, DataType = SyntaxDataType.String });

        items = new()
        {
            new SyntaxModelItem { Key = "IF", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number, Circ = 1 },
            new SyntaxModelItem { Key = " = > < >= <= ", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary, Circ = 1 },
            new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number, Circ = 1 },
            new SyntaxModelItem { Key = " AND OR ", IType = ItemType.Keyword, Circ = 2, CircNum = 4 },
            new SyntaxModelItem { Key = "THEN", IType = ItemType.Keyword, Circ = 1 },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number, Circ = 1 },
            new SyntaxModelItem { Key = "ELSEIF", IType = ItemType.Keyword, Circ = 2, CircNum = 7 },
            new SyntaxModelItem { Key = "ELSE", IType = ItemType.Keyword ,Ignore = 1,IgnoreNum = 2},
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = "END", IType = ItemType.Keyword }
        };
        models.Add(new SyntaxModel { Id = "S3-3", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });

        items = new()
        {
            new SyntaxModelItem { Key = "IF", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number, Circ = 1 },
            new SyntaxModelItem { Key = " = > < >= <= ", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary, Circ = 1 },
            new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number, Circ = 1 },
            new SyntaxModelItem { Key = " AND OR ", IType = ItemType.Keyword, Circ = 2, CircNum = 4 },
            new SyntaxModelItem { Key = "THEN", IType = ItemType.Keyword, Circ = 1 },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String, Circ = 1 },
            new SyntaxModelItem { Key = "ELSEIF", IType = ItemType.Keyword, Circ = 2, CircNum = 7 },
            new SyntaxModelItem { Key = "ELSE", IType = ItemType.Keyword ,Ignore = 1,IgnoreNum = 2},
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = "END", IType = ItemType.Keyword }
        };
        models.Add(new SyntaxModel { Id = "S3-4", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });

        items = new()
        {
            new SyntaxModelItem { Key = "IF", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number, Circ = 1 },
            new SyntaxModelItem { Key = " = > < >= <= ", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary, Circ = 1 },
            new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number, Circ = 1 },
            new SyntaxModelItem { Key = " AND OR ", IType = ItemType.Keyword, Circ = 2, CircNum = 4 },
            new SyntaxModelItem { Key = "THEN", IType = ItemType.Keyword, Circ = 1 },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime, Circ = 1 },
            new SyntaxModelItem { Key = "ELSEIF", IType = ItemType.Keyword, Circ = 2, CircNum = 7 },
            new SyntaxModelItem { Key = "ELSE", IType = ItemType.Keyword ,Ignore = 1,IgnoreNum = 2},
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime },
            new SyntaxModelItem { Key = "END", IType = ItemType.Keyword }
        };
        models.Add(new SyntaxModel { Id = "S3-5", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime });

        // 字符
        items = new()
        {
            new SyntaxModelItem { Key = "IF", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String, Circ = 1 },
            new SyntaxModelItem { Key = " = > < >= <= ", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary, Circ = 1 },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String, Circ = 1 },
            new SyntaxModelItem { Key = " AND OR ", IType = ItemType.Keyword, Circ = 2, CircNum = 4 },
            new SyntaxModelItem { Key = "THEN", IType = ItemType.Keyword, Circ = 1 },
            new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number, Circ = 1 },
            new SyntaxModelItem { Key = "ELSEIF", IType = ItemType.Keyword, Circ = 2, CircNum = 7 },
            new SyntaxModelItem { Key = "ELSE", IType = ItemType.Keyword ,Ignore = 1,IgnoreNum = 2},
            new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = "END", IType = ItemType.Keyword }
        };
        models.Add(new SyntaxModel { Id = "S3-6", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });

        items = new()
        {
            new SyntaxModelItem { Key = "IF", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String, Circ = 1 },
            new SyntaxModelItem { Key = " = > < >= <= ", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary, Circ = 1 },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String, Circ = 1 },
            new SyntaxModelItem { Key = " AND OR ", IType = ItemType.Keyword, Circ = 2, CircNum = 4 },
            new SyntaxModelItem { Key = "THEN", IType = ItemType.Keyword, Circ = 1 },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String, Circ = 1 },
            new SyntaxModelItem { Key = "ELSEIF", IType = ItemType.Keyword, Circ = 2, CircNum = 7 },
            new SyntaxModelItem { Key = "ELSE", IType = ItemType.Keyword ,Ignore = 1,IgnoreNum = 2},
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = "END", IType = ItemType.Keyword }
        };
        models.Add(new SyntaxModel { Id = "S3-7", ModelItems = items, IType = ItemType.Character, DataType = SyntaxDataType.String });

        items = new()
        {
            new SyntaxModelItem { Key = "IF", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String, Circ = 1 },
            new SyntaxModelItem { Key = " = > < >= <= ", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary, Circ = 1 },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String, Circ = 1 },
            new SyntaxModelItem { Key = " AND OR ", IType = ItemType.Keyword, Circ = 2, CircNum = 4 },
            new SyntaxModelItem { Key = "THEN", IType = ItemType.Keyword, Circ = 1 },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number, Circ = 1 },
            new SyntaxModelItem { Key = "ELSEIF", IType = ItemType.Keyword, Circ = 2, CircNum = 7 },
            new SyntaxModelItem { Key = "ELSE", IType = ItemType.Keyword ,Ignore = 1,IgnoreNum = 2},
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = "END", IType = ItemType.Keyword }
        };
        models.Add(new SyntaxModel { Id = "S3-8", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });

        items = new()
        {
            new SyntaxModelItem { Key = "IF", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String, Circ = 1 },
            new SyntaxModelItem { Key = " = > < >= <= ", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary, Circ = 1 },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String, Circ = 1 },
            new SyntaxModelItem { Key = " AND OR ", IType = ItemType.Keyword, Circ = 2, CircNum = 4 },
            new SyntaxModelItem { Key = "THEN", IType = ItemType.Keyword, Circ = 1 },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String, Circ = 1 },
            new SyntaxModelItem { Key = "ELSEIF", IType = ItemType.Keyword, Circ = 2, CircNum = 7 },
            new SyntaxModelItem { Key = "ELSE", IType = ItemType.Keyword ,Ignore = 1,IgnoreNum = 2},
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = "END", IType = ItemType.Keyword }
        };
        models.Add(new SyntaxModel { Id = "S3-9", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });

        items = new()
        {
            new SyntaxModelItem { Key = "IF", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String, Circ = 1 },
            new SyntaxModelItem { Key = " = > < >= <= ", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary, Circ = 1 },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String, Circ = 1 },
            new SyntaxModelItem { Key = " AND OR ", IType = ItemType.Keyword, Circ = 2, CircNum = 4 },
            new SyntaxModelItem { Key = "THEN", IType = ItemType.Keyword, Circ = 1 },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime, Circ = 1 },
            new SyntaxModelItem { Key = "ELSEIF", IType = ItemType.Keyword, Circ = 2, CircNum = 7 },
            new SyntaxModelItem { Key = "ELSE", IType = ItemType.Keyword ,Ignore = 1,IgnoreNum = 2},
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime },
            new SyntaxModelItem { Key = "END", IType = ItemType.Keyword }
        };
        models.Add(new SyntaxModel { Id = "S3-10", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime });
        #endregion
        #region 4 MAX 函数模板

        items = new()
        {
            new SyntaxModelItem { Key = "MAX", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S4-1", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items = new()
        {
            new SyntaxModelItem { Key = "MAX", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S4-2", ModelItems = items, IType = ItemType.Character, DataType = SyntaxDataType.String });
        items = new()
        {
            new SyntaxModelItem { Key = "MAX", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S4-3", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items = new()
        {
            new SyntaxModelItem { Key = "MAX", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S4-4", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        items = new()
        {
            new SyntaxModelItem { Key = "MAX", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S4-5", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime });
        #endregion
        #region 5 MIN 函数模板

        items = new()
        {
            new SyntaxModelItem { Key = "MIN", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S4-1", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items = new()
        {
            new SyntaxModelItem { Key = "MIN", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S4-2", ModelItems = items, IType = ItemType.Character, DataType = SyntaxDataType.String });
        items = new()
        {
            new SyntaxModelItem { Key = "MIN", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S4-3", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items = new()
        {
            new SyntaxModelItem { Key = "MIN", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S4-4", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        items = new()
        {
            new SyntaxModelItem { Key = "MIN", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S4-5", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime });
        #endregion
        #region 6 SUM 函数模板

        items = new()
        {
            new SyntaxModelItem { Key = "SUM", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S4-1", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items = new()
        {
            new SyntaxModelItem { Key = "SUM", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S4-3", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        #endregion
        #region 7 COUNT 函数模板

        items = new()
        {
            new SyntaxModelItem { Key = "COUNT", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S4-1", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items = new()
        {
            new SyntaxModelItem { Key = "COUNT", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S4-2", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items = new()
        {
            new SyntaxModelItem { Key = "COUNT", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S4-3", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items = new()
        {
            new SyntaxModelItem { Key = "COUNT", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S4-4", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items = new()
        {
            new SyntaxModelItem { Key = "COUNT", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S4-5", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        #endregion
        #region 8 COUNTDISTINCT 函数模板

        items = new()
        {
            new SyntaxModelItem { Key = "COUNTDISTINCT", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S8-1", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items = new()
        {
            new SyntaxModelItem { Key = "COUNTDISTINCT", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S8-2", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items = new()
        {
            new SyntaxModelItem { Key = "COUNTDISTINCT", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S8-3", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items = new()
        {
            new SyntaxModelItem { Key = "COUNTDISTINCT", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S8-4", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items = new()
        {
            new SyntaxModelItem { Key = "COUNTDISTINCT", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S8-5", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        #endregion
        #region 9 INT 函数模板

        items = new()
        {
            new SyntaxModelItem { Key = "INT", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ",", IType = ItemType.Keyword,Ignore = 1,IgnoreNum = 2 },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S9-1", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items = new()
        {
            new SyntaxModelItem { Key = "INT", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ",", IType = ItemType.Keyword,Ignore = 1,IgnoreNum = 2 },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S9-2", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items = new()
        {
            new SyntaxModelItem { Key = "INT", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ",", IType = ItemType.Keyword,Ignore = 1,IgnoreNum = 2 },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S9-3", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items = new()
        {
            new SyntaxModelItem { Key = "INT", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ",", IType = ItemType.Keyword,Ignore = 1,IgnoreNum = 2 },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S9-4", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        #endregion
        #region 10 FIXED 函数模板

        items = new()
        {
            new SyntaxModelItem { Key = "{", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "FIXED", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Arbitrary, Circ = 1 },
            new SyntaxModelItem { Key = ",", IType = ItemType.Keyword, Circ = 2 },
            new SyntaxModelItem { Key = ":", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = "}", IType = ItemType.Keyword }
        };
        models.Add(new SyntaxModel { Id = "S10-1", ModelItems = items, IType = ItemType.Character, DataType = SyntaxDataType.String });
        items = new()
        {
            new SyntaxModelItem { Key = "{", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "FIXED", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Arbitrary, Circ = 1 },
            new SyntaxModelItem { Key = ",", IType = ItemType.Keyword, Circ = 2 },
            new SyntaxModelItem { Key = ":", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = "}", IType = ItemType.Keyword }
        };
        models.Add(new SyntaxModel { Id = "S10-2", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        
        items = new()
        {
            new SyntaxModelItem { Key = "{", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "FIXED", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Arbitrary, Circ = 1 },
            new SyntaxModelItem { Key = ",", IType = ItemType.Keyword, Circ = 2 },
            new SyntaxModelItem { Key = ":", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = "}", IType = ItemType.Keyword }
        };
        models.Add(new SyntaxModel { Id = "S10-3", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        items = new()
        {
            new SyntaxModelItem { Key = "{", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "FIXED", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Arbitrary, Circ = 1 },
            new SyntaxModelItem { Key = ",", IType = ItemType.Keyword, Circ = 2 },
            new SyntaxModelItem { Key = ":", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = "}", IType = ItemType.Keyword }
        };
        models.Add(new SyntaxModel { Id = "S10-4", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items = new()
        {
            new SyntaxModelItem { Key = "{", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "FIXED", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Arbitrary, Circ = 1 },
            new SyntaxModelItem { Key = ",", IType = ItemType.Keyword, Circ = 2 },
            new SyntaxModelItem { Key = ":", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime },
            new SyntaxModelItem { Key = "}", IType = ItemType.Keyword }
        };
        models.Add(new SyntaxModel { Id = "S10-5", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime });
        #endregion
        #region 11 () 括号模板
        items = new()
        {
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket },
            new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket}
        };
        models.Add(new SyntaxModel { Id = "S11-1", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items = new()
        {
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket },
            new SyntaxModelItem { Key = null, IType = ItemType.DateTime, DataType = SyntaxDataType.DateTime },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket}
        };
        models.Add(new SyntaxModel { Id = "S11-1", ModelItems = items, IType = ItemType.DateTime, DataType = SyntaxDataType.DateTime });
        items = new()
        {
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket}
        };
        models.Add(new SyntaxModel { Id = "S11-1", ModelItems = items, IType = ItemType.Character, DataType = SyntaxDataType.String });
        items = new()
        {
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket}
        };
        models.Add(new SyntaxModel { Id = "S11-1", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items = new()
        {
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket}
        };
        models.Add(new SyntaxModel { Id = "S11-1", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime });
        items = new()
        {
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket}
        };
        models.Add(new SyntaxModel { Id = "S11-1", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        #endregion
        #region 12 CHAR 函数模板

        items = new()
        {
            new SyntaxModelItem { Key = "CHAR", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ",", IType = ItemType.Keyword,Ignore = 1,IgnoreNum = 2 },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S12-1", ModelItems = items, IType = ItemType.Character, DataType = SyntaxDataType.String });
        items = new()
        {
            new SyntaxModelItem { Key = "CHAR", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ",", IType = ItemType.Keyword,Ignore = 1,IgnoreNum = 2 },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S12-2", ModelItems = items, IType = ItemType.Character, DataType = SyntaxDataType.String });
        items = new()
        {
            new SyntaxModelItem { Key = "CHAR", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime },
            new SyntaxModelItem { Key = ",", IType = ItemType.Keyword,Ignore = 1,IgnoreNum = 2 },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S12-3", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        items = new()
        {
            new SyntaxModelItem { Key = "CHAR", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ",", IType = ItemType.Keyword,Ignore = 1,IgnoreNum = 2 },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S12-4", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        items = new()
        {
            new SyntaxModelItem { Key = "CHAR", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ",", IType = ItemType.Keyword,Ignore = 1,IgnoreNum = 2 },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S12-5", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        #endregion
        #region 13 DATE 函数模板

        items = new()
        {
            new SyntaxModelItem { Key = "DATE", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ",", IType = ItemType.Keyword,Ignore = 1,IgnoreNum = 2 },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S13-1", ModelItems = items, IType = ItemType.DateTime, DataType = SyntaxDataType.DateTime });
        items = new()
        {
            new SyntaxModelItem { Key = "DATE", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ",", IType = ItemType.Keyword,Ignore = 1,IgnoreNum = 2 },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S13-2", ModelItems = items, IType = ItemType.DateTime, DataType = SyntaxDataType.DateTime });
        items = new()
        {
            new SyntaxModelItem { Key = "DATE", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ",", IType = ItemType.Keyword,Ignore = 1,IgnoreNum = 2 },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S12-4", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime });
        items = new()
        {
            new SyntaxModelItem { Key = "DATE", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ",", IType = ItemType.Keyword,Ignore = 1,IgnoreNum = 2 },
            new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S12-5", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.DateTime });
        #endregion
        #region 14 ABS 函数模板

        items = new()
        {
            new SyntaxModelItem { Key = "ABS", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S14-1", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items = new()
        {
            new SyntaxModelItem { Key = "ABS", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S14-2", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        #endregion
        #region 15 字符拼接 || 
        items = new(); // 数字||数字 = 字符
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem { Key = "||", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S15-1", ModelItems = items, IType = ItemType.Character, DataType = SyntaxDataType.String });
        items = new(); // 数字||字符 = 字符
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem { Key = "||", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String });
        models.Add(new SyntaxModel { Id = "S15-2", ModelItems = items, IType = ItemType.Character, DataType = SyntaxDataType.String });
        items = new(); // 数字||列数字 = 列字符
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem { Key = "||", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S15-3", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        items = new(); // 数字||列字符 = 列字符
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem { Key = "||", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        models.Add(new SyntaxModel { Id = "S15-4", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        items = new(); // 字符||数字 = 字符
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String });
        items.Add(new SyntaxModelItem { Key = "||", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S15-5", ModelItems = items, IType = ItemType.Character, DataType = SyntaxDataType.String });
        items = new(); // 字符||字符 = 字符
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String });
        items.Add(new SyntaxModelItem { Key = "||", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String });
        models.Add(new SyntaxModel { Id = "S15-6", ModelItems = items, IType = ItemType.Character, DataType = SyntaxDataType.String });
        items = new(); // 字符||列数字 = 列字符
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String });
        items.Add(new SyntaxModelItem { Key = "||", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S15-7", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        items = new(); // 字符||列字符 = 列字符
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.Character, DataType = SyntaxDataType.String });
        items.Add(new SyntaxModelItem { Key = "||", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        models.Add(new SyntaxModel { Id = "S15-8", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        items = new(); // 列数字||列数字 = 列字符
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem { Key = "||", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S15-9", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        items = new(); // 列数字||列字符 = 列字符
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        items.Add(new SyntaxModelItem { Key = "||", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        models.Add(new SyntaxModel { Id = "S15-10", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        items = new(); // 列字符||列数字 = 列字符
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        items.Add(new SyntaxModelItem { Key = "||", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        models.Add(new SyntaxModel { Id = "S15-11", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        items = new(); // 列字符||列字符 = 列字符
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        items.Add(new SyntaxModelItem { Key = "||", IType = ItemType.Operators, DataType = SyntaxDataType.Arbitrary });
        items.Add(new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.String });
        models.Add(new SyntaxModel { Id = "S15-12", ModelItems = items, IType = ItemType.Character, DataType = SyntaxDataType.String });
        #endregion
        #region 16 AVG 函数模板

        items = new()
        {
            new SyntaxModelItem { Key = "AVG", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.Num, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S16-1", ModelItems = items, IType = ItemType.Num, DataType = SyntaxDataType.Number });
        items = new()
        {
            new SyntaxModelItem { Key = "AVG", IType = ItemType.Keyword },
            new SyntaxModelItem { Key = "(", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary },
            new SyntaxModelItem { Key = null, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number },
            new SyntaxModelItem { Key = ")", IType = ItemType.Bracket, DataType = SyntaxDataType.Arbitrary }
        };
        models.Add(new SyntaxModel { Id = "S16-2", ModelItems = items, IType = ItemType.ColumnName, DataType = SyntaxDataType.Number });
        #endregion
    }

    private SyntaxModelItem checkModel()
    {
        if (longMemery.Count == 1)
        {
            IEnumerable<SyntaxModel> currentModels = models.Where(x =>
            {
                if (x.ModelItems.Count == 1)
                {
                    var tmp = x.ModelItems.First();
                    if (tmp.Key == null && tmp.IType == longMemery[index].IType)
                        return true;
                }
                return false;
            });
            if (currentModels.Count() == 0)
            {
                message = $"ERROR : 字符索引:{index} 对应字符:{longMemery[index].Key} 附近语法缺失";
                return new SyntaxModelItem
                {
                    IType = ItemType.Error
                };
            }
            else
            {
                return longMemery[0];
            }
        }else
        {
            SyntaxModelItem item = new SyntaxModelItem { IType = ItemType.Error};
            while(index < longMemery.Count - 1)
            {
                item = checkMultiModel();
                if (item.IType == ItemType.Error)
                    return item;
                if(index < longMemery.Count - 1)
                    longMemery[index] = item;
            }
            return item;
        }
    }

    private SyntaxModelItem checkMultiModel()
    {
        int modelIndex = 0;
        List<string> priorityCharacters = new List<string>
        {
            getPriority()
        };
        List<ItemType> modelMemery = new List<ItemType>() 
        {
            longMemery[index].IType
        };
        // gets the template that the current character matches  1+(1+3)
        IEnumerable<SyntaxModel> currentModels = models.Where(x =>
        {
            if (x.ModelItems.Count <= 1)
                return false;
            var tmp = x.ModelItems.First();
            if(tmp.IType == longMemery[index].IType)
            {
                if (tmp.IType == ItemType.Num || tmp.IType == ItemType.Character)
                    return true;
                if (tmp.IType == ItemType.Bracket && tmp.Key == longMemery[index].Key)
                    return true;
                if (tmp.IType == ItemType.Keyword && tmp.Key == longMemery[index].Key)
                    return true;
                if (tmp.IType == ItemType.ColumnName && tmp.DataType == longMemery[index].DataType)
                    return true;
                if (tmp.IType == ItemType.Operators && tmp.Key == "-" && tmp.Key == longMemery[index].Key && x.ModelItems.Count == 2)
                    return true;
            }
            return false;
        }).ToList();
        // throw a err and return it 
        if (currentModels.Count() == 0)
        {
            message = $"ERROR : 字符索引:{index} 对应字符:{longMemery[index].Key} 不符合语法规范，请检查(语法校验模型不匹配)";
            return new SyntaxModelItem
            {
                IType = ItemType.Error
            };
        }
        SyntaxModelItem modelItem;
        int circNum = 0;
        while (++index < longMemery.Count)
        {
            if (currentModels.Count() == 1 && modelIndex == currentModels.First().ModelItems.Count - 1)
            {
                // 当前模板已经契合，需要返回,故需要讲加上去
                index--; 
                break;
            }
                

            modelIndex++;
            //priority = Math.Max(priority,getPriority());
            var nextChar = getPriority();
            priorityCharacters.Add(nextChar);
            modelMemery.Add(longMemery[index].IType);

            switch (nextChar)
            {
                case "KEYWORD":
                    modelItem = checkMultiModel();
                    if (priorityCharacters[priorityCharacters.Count - 2] == "(" && index < longMemery.Count - 1)
                    {
                        index++;
                        // 此时代表的是括号中嵌套函数
                        if (longMemery[index].Key != ")" && longMemery[index].Key != ",")
                        {
                            index--;
                            longMemery[index] = modelItem;
                            modelItem = checkMultiModel();
                        }
                        else { index--; }
                    }
                    break;
                case "(":
                    if (priorityCharacters[priorityCharacters.Count-2] == "KEYWORD")
                        modelItem = longMemery[index];
                    else
                        modelItem = checkMultiModel();
                    break;
                default:// NA
                    modelItem = longMemery[index];
                    if (priorityCharacters[priorityCharacters.Count - 2] == "(" && index < longMemery.Count-1)
                    {
                        index++;
                        if (longMemery[index].Key != ")" && longMemery[index].Key != ",")
                        {
                            index--;
                            modelItem = checkMultiModel();
                        }
                        else { index--; }
                    } //  以下代码优先级从小到大  [|| + -]  =>  [* /]   
                    else if ( (   priorityCharacters[priorityCharacters.Count - 2] == "+" 
                               || priorityCharacters[priorityCharacters.Count - 2] == "-"
                               || priorityCharacters[priorityCharacters.Count - 2] == "||") 
                               && index < longMemery.Count - 1)
                    {
                        index++;
                        if (getPriority() == "*" || getPriority() == "/")
                        {
                            index--;
                            modelItem = checkMultiModel();
                        }
                        else { index--; }
                    }
                    else if(nextChar == "-" && priorityCharacters.Count != 2)
                    {
                        /*if (longMemery[index+1].IType == ItemType.Num
                            && longMemery[index-1].IType != ItemType.Num 
                            && !( longMemery[index-1].IType != ItemType.ColumnName 
                                  && dic.ContainsKey(longMemery[index - 1].Key)
                                  &&  dic[longMemery[index-1].Key] == SyntaxDataType.Number)){
                            
                        }*/
                        modelItem = checkMultiModel();

                    }
                    break;
            }
            if (modelItem.IType == ItemType.Error)
                return modelItem;

            currentModels = currentModels.Where(x =>
            {
                if (modelIndex >= x.ModelItems.Count)
                    return false;
                var tmp = x.ModelItems[modelIndex];
                if (tmp.IType == modelItem.IType)
                {
                    if (tmp.IType == ItemType.Num || tmp.IType == ItemType.Character || tmp.IType == ItemType.DateTime)
                        return true;
                    if (tmp.IType == ItemType.ColumnName && (tmp.DataType == modelItem.DataType|| tmp.DataType == SyntaxDataType.Arbitrary))
                        return true;
                    if (tmp.IType == ItemType.Bracket && tmp.Key == modelItem.Key)
                        return true;
                    if (tmp.IType == ItemType.Operators && (tmp.Key == modelItem.Key || tmp.Key.Contains(' ') && tmp.Key.Contains(modelItem.Key)))
                        return true;
                    if (tmp.IType == ItemType.Keyword && (tmp.Key == modelItem.Key || tmp.Key.Contains(' ') && tmp.Key.Contains(modelItem.Key)))
                    {
                        // 对于最后一个循环关键字 （循环）
                        if (tmp.Circ == 2)
                            circNum = tmp.CircNum;
                        return true;
                    }
                    // 对于最后一个循环关键字 （不循环） if 函数 和 fiexd 函数 最后一个循环的关键字的下个字符都是关键字
                    if (tmp.IType == ItemType.Keyword && tmp.Circ == 2)
                    {
                        circNum = -1;
                        if (x.ModelItems[modelIndex+1].Key == modelItem.Key)
                            return true;
                    }
                }
                // 对于忽略字段，跳过忽略的字段数 目前有int  if  char date 函数的忽略字段之后都是括号或者关键字
                if (x.ModelItems[modelIndex- circNum].Ignore == 1)
                {
                    circNum = circNum+ x.ModelItems[modelIndex - circNum].IgnoreNum * -1;
                    if (x.ModelItems[modelIndex - circNum].Key == modelItem.Key)
                        return true;
                }
                return false;
            }).ToList();

            modelIndex -= circNum;
            circNum = 0;

            // throw a err and return it 
            if (currentModels.Count() == 0)
            {
                
                message = $"ERROR : 字符索引:{index} 对应字符:{longMemery[index].Key} 附近语法缺失(语法校验模型不匹配),当前模型:{getCurrentStrModel(modelMemery)}";
                return new SyntaxModelItem
                {
                    IType = ItemType.Error
                };
            }
        }
        if(currentModels.Count() == 1 && currentModels.First().ModelItems.Count-1 > modelIndex)
        {
            message = $"ERROR : 字符索引:{index} 语法缺失:{currentModels.First().ModelItems[modelIndex+1].Key??":缺少值"}";
        }

        return new SyntaxModelItem
        {
            DataType = currentModels.First().DataType,
            IType = currentModels.First().IType
        };

    }
    // 将当前记忆的模型转成字符串返回前台
    private string getCurrentStrModel(List<ItemType> modelMemery)
    {
        StringBuilder sb = new StringBuilder("[");
        foreach(var item in modelMemery)
        {
            sb.Append(item.ToString());
            sb.Append(", ");
        }
        sb.Remove(sb.Length-2, 2);
        sb.Append("]");
        return sb.ToString();
    }

    public string getPriority()
    {//"IF","AND","OR", "THEN", "ELSE", "END", "ELSEIF", "MAX", "MIN", "SUM", "AVG", "COUNT", "COUNTDISTINCT", "INT", "FIXED",",", ":", "{", "}" 
        string[] keyWordArr = new string[] {"MAX","MIN", "SUM", "IF", "{", "AVG", "STDEV", "COUNT", "COUNTDISTINCT", "INT","DATE","CHAR","ABS" };
        string[] arr = new string[] { "+", "-", "*", "/" , "(" ,"||"};
        if (arr.Contains(longMemery[index].Key))
            return longMemery[index].Key;
        else if (keyWordArr.Contains(longMemery[index].Key))
            return "KEYWORD";
        else
            return "NA";
    }

    public bool checkPriority(List<string> keys)
    {
        foreach(var key in keys)
        {
            switch (key, longMemery[index].Key)
            {
                case ("+", "KEYWORD"):
                case ("-", "KEYWORD"):
                case ("*", "KEYWORD"):
                case ("/", "KEYWORD"):
                case ("(", "KEYWORD"):
                case ("NA", "KEYWORD"):
                case ("KEYWORD", "KEYWORD"):
                case ("NA", "("):
                case ("+", "("):
                case ("-", "("):
                case ("*", "("):
                case ("/", "("):
                case ("+", "*"):
                case ("+", "/"):
                case ("-", "*"):
                case ("-", "/"):
                    return true;
                default:
                    return false;
            }
        }
        return false;
    }

    private ItemType checkItemType(string item)
    {
        if (keywords.Contains(item))
            return ItemType.Keyword;
        else if (dic.ContainsKey(item))
            return ItemType.ColumnName;
        else if (operators.Contains(item))
            return ItemType.Operators;
        else if (item == "(" || item == ")")
            return ItemType.Bracket;
        else if (item.IndexOf("'") == 0 && item.LastIndexOf("'") == item.Length - 1)
            return ItemType.Character;
        else if (numberCheck(item))
            return ItemType.Num;
        else
            return ItemType.Error;
    }
    //Determine whether it is a number
    private bool numberCheck(string item)
    {
        return decimal.TryParse(item, out _);
    }
    #endregion

    // (underline)  The following is the public methods section---------------------------------------------------------------------------

    /// <summary>
    /// Format custom syntax and Split characters as Spaces
    /// </summary>
    private void formatting(string fieldFunction)
    {
        List<string> paramList = new List<string>();
        int nextIndex = fieldFunction.IndexOf('\'');
        int endIndex;
        string nextChacter;
        string nextReplace;
        int deviation = 0;
        while (nextIndex != -1)
        {
            if(nextIndex == fieldFunction.Length)
            {
                message = $"ERROR : 索引:{nextIndex+ deviation} 缺失单引号";
                return;
            }
            else
                endIndex = fieldFunction.IndexOf('\'', nextIndex + 1);

            if (endIndex <= -1) 
            {
                message = $"ERROR : 索引:{nextIndex+ deviation} 缺失单引号";
                return;
            }

            nextChacter = fieldFunction.Substring(nextIndex, endIndex - nextIndex + 1);
            paramList.Add(nextChacter);
            nextReplace = $"[param{paramList.Count}]";
            deviation += nextChacter.Length - nextReplace.Length;
            fieldFunction = fieldFunction.Replace(paramList[paramList.Count-1], nextReplace);

            nextIndex = fieldFunction.IndexOf('\'', endIndex - nextChacter.Length + nextReplace.Length+1);
        }

        arr = fieldFunction.Replace("\n", " ")
                            .Replace("\r", " ")
                            .Replace(":", " : ")
                            .Replace(",", " , ")
                            .Replace("+", " + ")
                            .Replace("-", " - ")
                            .Replace("*", " * ")
                            .Replace("/", " / ")
                            .Replace("||", " || ")
                            .Replace("=", " = ")
                            .Replace("! =", " != ")
                            .Replace("> =", " >= ")
                            .Replace("< =", " <= ")
                            .Replace("(", " ( ")
                            .Replace(")", " ) ")
                            .Replace("{", " { ")
                            .Replace("}", " } ").Split(' ');

        List<string> list = new();
        foreach (var item in arr)
        {
            if (!string.IsNullOrEmpty(item) && item != " ")
            {
                list.Add(item);
            }
        }
        arr = list.ToArray();

        StringBuilder param = new();
        int j = 0;

        for (int i = 0 ; i< paramList.Count; i++)
        {
            param.Clear();
            param.Append("[param");
            param.Append(i+1);
            param.Append("]");
            for (; j < arr.Count(); j++)
            {
                if (arr[j].Contains(param.ToString()))
                {
                    arr[j] = arr[j].Replace(param.ToString(), paramList[i]);
                    break;
                }
            }
        }
    }
    /// <summary>
    /// Gets a list of keywords
    /// </summary>
    private string[] getKeyArr(string key)
    {
        string[] arr;
        switch (key,language)
        {
            case ("IF","ENGLISH"):
                arr = new string[] { "IF", "THEN","ELSEIF" , "ELSE", "END" };
                break;
            case ("IF", "CHINESE"):
                arr = new string[] { "如果", "则", "亦如", "或者", "结束" };
                break;
            case ("IFOPTIMIZE", "ENGLISH"):
                arr = new string[] { "IF",  "ELSEIF"};
                break;
            case ("IFOPTIMIZE", "CHINESE"):
                arr = new string[] { "如果","亦如"};
                break;
            case ("FIXED", "ENGLISH"):
                arr = new string[] { "{", "FIXED", ":" , "}" };
                break;
            case ("FIXED", "CHINESE"):
                arr = new string[] { "{", "聚合", ":", "}" };
                break;
            case ("FIXEDPIVOTAL", "ENGLISH"):
                arr = new string[] { "FIXED", ":" };
                break;
            case ("FIXEDPIVOTAL", "CHINESE"):
                arr = new string[] {  "聚合", ":"};
                break;
            default:
                arr = new string[] { "IF", "THEN", "ELSEIF", "ELSE", "END" };
                break;
        }
        return arr;
    }
    /// <summary>
    /// Toggle syntax rule matching set
    /// </summary>
    private void cutSyntaxs(string key)
    {
        switch (key,language)
        {
            case ("IF","ENGLISH"):
                syntaxs.Clear();
                syntaxs.Add(("BEGIN", "IF"));
                syntaxs.Add(("IF", "THEN"));
                syntaxs.Add(("THEN", "ELSE"));
                syntaxs.Add(("THEN", "ELSEIF"));
                syntaxs.Add(("THEN", "END"));
                syntaxs.Add(("ELSEIF", "THEN"));
                syntaxs.Add(("ELSE", "END"));
                break;
            case ("IF", "CHINESE"):
                syntaxs.Clear();
                syntaxs.Add(("开始", "如果"));
                syntaxs.Add(("如果", "则"));
                syntaxs.Add(("则", "否则"));
                syntaxs.Add(("则", "亦如"));
                syntaxs.Add(("则", "结束"));
                syntaxs.Add(("亦如", "则"));
                syntaxs.Add(("否则", "结束"));
                break;
            case ("FIXED", "ENGLISH"):
                syntaxs.Clear();
                syntaxs.Add(("BEGIN", "{"));
                syntaxs.Add(("{", "FIXED"));
                syntaxs.Add(("FIXED", ":"));
                syntaxs.Add((":", "}"));
                break;
            case ("FIXED", "CHINESE"):
                syntaxs.Clear();
                syntaxs.Add(("开始", "{"));
                syntaxs.Add(("{", "聚合"));
                syntaxs.Add(("聚合", ":"));
                syntaxs.Add((":", "}"));
                break;
            default:
                message = $"ERROR :关键字[ {key} ]语法规则缺失，请补充";
                syntaxs.Clear();
                break;
        }
    }
    /// <summary>
    /// 获取关键字列表记录对象
    /// </summary>
    private List<string> getMemery()
    {
        List<string> shortMemery = new();
        if(language == "CHINESE")
            shortMemery.Add("开始");
        else
            shortMemery.Add("BEGIN");
        return shortMemery;
    }
    /// <summary>
    /// Look for the terminator of the function
    /// </summary>
    private int getCloseSymbol(string symbol)
    {
        int endIndex = arr.IndexOf(symbol, index);
        return endIndex;
    }
    /// <summary>
    /// 寻找函数的右括号定位
    /// </summary>
    private int getCloseParenthesis()
    {
        int openParenthesis = 0;
        int closeParenthesis = 0;
        if (arr[index+1] != "(")
            return -1;
        for (int i = index; i < arr.Length; i++)
        {
            if (arr[i] == "(")
                openParenthesis++;
            else if (arr[i] == ")")
                closeParenthesis++;

            if (closeParenthesis != 0 && openParenthesis == closeParenthesis)
                return i;
        }
        return -1;
    }
    /// <summary>
    /// 检查是否解析到语法结尾
    /// </summary>
    private bool checkEnd()
    {
        // 这里 0 表示初始状态，1 表示进入方法开始执行
        if((level == 1 || level == 0) && index < arr.Length)
        {
            return true;
        }
        else
        {
            level--;
            return false;
        }
    }
    /// <summary>
    /// 判断是否返回
    /// </summary>
    private Boolean checkStatus()
    {
        return message == "OK";
    }

}
