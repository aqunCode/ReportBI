using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using Newtonsoft.Json.Linq;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Services.Service;

public class BinaryTreeServices : IBinaryTreeServices
{

    /// <summary>
    /// 仓储字段
    /// </summary>
    private SqlSugarScopeProvider repository;
    /// <summary>
    /// sql处理引擎
    /// </summary>
    private IDbEngineServices dbEngine;

    public BinaryTreeServices(ISqlSugarClient _sqlSugarClient
                                , IDbEngineServices dbService)
    {
        repository = (_sqlSugarClient as SqlSugarScope).GetConnectionScope("bidb");
        this.dbEngine = dbService;
    }


    public async Task<(string,string)> AnalysisRelation(BIWorkbookInput input)
    {
        List<string> labelNames = new();

        //计算筛选器中的节点数量
        foreach(var fItem in input.FilterItems)
        {
            
            if(fItem.ColumnType == "1")
            {
                labelNames.Add(fItem.LabelName);
            }
            else if (fItem.ColumnType == "2") // 代表自定义字段
            {
                var customerField = await repository.Queryable<BiCustomerField>()
                    .Where(x => x.Id == fItem.Id)
                    .Take(1)
                    .ToListAsync();
                if (!customerField.Any())
                    return ("ERROR 自定义字段查询失败",null);

                string function = customerField.First().FieldFunction ;
                AnalysisStr(function, labelNames);
            }
            else
            {
                labelNames.Add(fItem.LabelName);
            }
        }

        //计算行和列的节点数量
        foreach (var cItem in input.CalcItems)
        {
            if (cItem.ColumnType == "1")
            {
                labelNames.Add(cItem.LabelName);
            }
            else if(cItem.ColumnType == "2") // 代表自定义字段
            {
                var customerField = await repository.Queryable<BiCustomerField>()
                    .Where(x => x.Id == cItem.NodeId)
                    .Take(1)
                    .ToListAsync();
                if (!customerField.Any())
                    return ("ERROR 自定义字段查询失败", null);

                string function = customerField.First().FieldFunction;
                AnalysisStr(function, labelNames);
            }
            else
            {
                labelNames.Add(cItem.LabelName);
            }
        }

        //计算行和列的节点数量
        input.MarkItems = input.MarkItems ?? new List<BiMarkField>();
        foreach (var mItem in input.MarkItems)
        {
            if (mItem.ColumnType == "1")
            {
                labelNames.Add(mItem.LabelName);
            }
            else if (mItem.ColumnType == "2") // 代表自定义字段
            {
                var customerField = await repository.Queryable<BiCustomerField>()
                    .Where(x => x.Id == mItem.NodeId)
                    .Take(1)
                    .ToListAsync();
                if (!customerField.Any())
                    return ("ERROR 自定义字段查询失败", null);

                string function = customerField.First().FieldFunction;
                AnalysisStr(function, labelNames);
            }
            else
            {
                labelNames.Add(mItem.LabelName);
            }
        }

        // 对重复的表名进行去重
        labelNames = labelNames.Distinct().ToList();

        // 获取节点和关系
        var allNodes = await repository.Queryable<BiDatasetNode>()
            .Where(x => x.DatasetCode == input.DatasetId)
            .ToListAsync();
        var nodes = allNodes
            .Where(x => labelNames.Contains(x.NodeLabel) && x.DatasetCode == input.DatasetId)
            .ToList();
        var relations = await repository.Queryable<BIRelation>()
            .Where(x => x.DatasetCode == input.DatasetId)
            .ToListAsync();

        var relationSql = await binaryTree(nodes, allNodes, relations);
        return ("OK", relationSql);
    }
    /// <summary>
    /// 分析自定义函数中的所有labelName，并添加
    /// </summary>
    private void AnalysisStr(string function, List<string> labelNames)
    {
        int index = 0;
        while (index != -1)
        {
            index = function.IndexOf('[', index);
            if(index != -1)
            {
                var fieldSet = function.Substring(index, function.IndexOf(']', index)- index);
                labelNames.Add(fieldSet.Substring(fieldSet.IndexOf('(') + 1, fieldSet.IndexOf(')')- fieldSet.IndexOf('(')-1));
                index++;
            }
        }
    }

    /// <summary>
    /// 根据二叉树最短路径算法获取所需的最少表，并创建连接sql
    /// </summary>
    private async Task<string> binaryTree(List<BiDatasetNode> nodes, List<BiDatasetNode> allNodes, List<BIRelation> relations)
    {
        List<BiDatasetNode> targetNodes = new();
        List<BiDatasetNode> tmpNodes = new();
        StringBuilder relationSql = new StringBuilder(" FROM ");
        // 开始二叉树的最短路径计算
        for(int i = 0; i < nodes.Count-1; i++)
        {
            targetNodes.AddRange(getShortPath(nodes[i], nodes[i+1], allNodes, relations));
        }
        if(!targetNodes.Any() && nodes.Count == 1)
            targetNodes = nodes;

        targetNodes = targetNodes.Distinct().ToList();

        // 开始根据targetNodes来生成sql
        int min = targetNodes.Select(x => Convert.ToInt32(x.TopLevel)).Min();

        // 优先创建根节点
        var root = targetNodes.Where(x => x.TopLevel == min.ToString()).First();
        relationSql.Append(root.TableName);
        relationSql.Append(' ');
        relationSql.Append(root.NodeLabel.Replace(".","").Replace("(", "").Replace(")", ""));

        tmpNodes.Add(root);
        BiDatasetNode sourceNode;
        BiDatasetNode targetNode;
        string sourceRename;
        string targetRename;
        // 最短路径上的所有节点
        var arr2 = targetNodes.Select(x => x.NodeId);
        while (tmpNodes.Any())
        {
            // 根节点（动态的，初始是单个，后面多个）
            var arr1 = tmpNodes.Select(x => x.NodeId);   
            // 找线（多个）
            var tmp = relations.Where(x => arr1.Contains(x.SourceId) && arr2.Contains(x.TargetId)).ToList();
            // 找到线后清除根节点
            tmpNodes.Clear();
            // 遍历线，找子节点
            foreach (var item in tmp)
            {
                // 把子节点赋值给新的根节点（单次单个，遍历后多个）
                tmpNodes.AddRange(targetNodes.Where(x=>x.NodeId == item.TargetId));
                
                // 连接
                relationSql.Append('\n');
                relationSql.Append(' ');
                relationSql.Append(item.IncidenceRelation);
                relationSql.Append(' ');

                targetNode  = (targetNodes.Where(x=>x.NodeId == item.TargetId && x.DatasetCode == item.DatasetCode).ToList()).FirstOrDefault();
                sourceNode = (targetNodes.Where(x => x.NodeId == item.SourceId && x.DatasetCode == item.DatasetCode).ToList()).FirstOrDefault();

                // 表名，别名
                sourceRename = sourceNode.NodeLabel.Replace(".", "").Replace("(", "").Replace(")", "");
                targetRename = targetNode.NodeLabel.Replace(".", "").Replace("(", "").Replace(")", "");

                relationSql.Append(targetNode.TableName);
                relationSql.Append(' ');
                relationSql.Append(targetRename);
                relationSql.Append(' ');

                // on 条件
                JArray jsonArr = JArray.Parse(item.JoinRelational);

                relationSql.Append("ON ");
                foreach (JObject jObj in jsonArr)
                {
                    relationSql.Append(sourceRename);
                    relationSql.Append('.');
                    relationSql.Append(jObj["source"].ToString());
                    relationSql.Append(' ');
                    relationSql.Append(jObj["symbol"].ToString());
                    relationSql.Append(' ');
                    relationSql.Append(targetRename);
                    relationSql.Append('.');
                    relationSql.Append(jObj["target"].ToString());
                    relationSql.Append(' ');
                    relationSql.Append(" AND ");
                }
                relationSql.Remove(relationSql.Length-4,4);
            }
        }
        return relationSql.ToString();
    }
    /// <summary>
    /// 计算二叉树的最短路径
    /// </summary>
    private IEnumerable<BiDatasetNode> getShortPath(BiDatasetNode node1, BiDatasetNode node2, List<BiDatasetNode> allNodes, List<BIRelation> relations)
    {
        // 创建List<BiDatasetNode> 用于记录路径
        List<BiDatasetNode> list = new();

        if (node1.TopLevel == "0")
        {
            list.Add(node1);
        }
        else
        {
            // 循环获取父节点
            list.Add(node1);
            BiDatasetNode sourceNode = node1;
            while (sourceNode.TopLevel != "0")
            {
                sourceNode = allNodes.Where(x => x.NodeId == relations.Where(x => x.TargetId == sourceNode.NodeId).First().SourceId).First();
                list.Add(sourceNode);

                if(sourceNode.NodeId == node2.NodeId)
                    return list;
            }
        }

        // node2 的逻辑
        if (!list.Where(x => x.NodeId == node2.NodeId).Any())
        {
            List<BiDatasetNode> tmp = new();
            tmp.Add(node2);
            BiDatasetNode sourceNode = node2;
            while (sourceNode.TopLevel != "0")
            {
                sourceNode = allNodes.Where(x => x.NodeId == relations.Where(x => x.TargetId == sourceNode.NodeId).First().SourceId).First();
                tmp.Add(sourceNode);
                if (list.Contains(sourceNode))
                {

                    int index = list.IndexOf(sourceNode);
                    // 移除多余的左侧的节点
                    list.RemoveRange(index, list.Count - index);
                    list.AddRange(tmp);
                    return list;
                }
            }
        }
        return list;
    }
}
