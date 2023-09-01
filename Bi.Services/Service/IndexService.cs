using Bi.Core.Caches;
using Bi.Core.Const;
using Bi.Core.Extensions;
using Bi.Core.Helpers;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Services.IService;
using SqlSugar;

namespace Bi.Services.Service;

public class IndexService : IIndexService
{
    /// <summary>
    /// redis 缓存
    /// </summary>
    private ICache redisCache;
    /// <summary>
    /// 数据库连接
    /// </summary>
    private SqlSugarScope repository;

    public IndexService(ICache redisCache,
                        ISqlSugarClient _sqlSugarClient)
    {
        this.redisCache = redisCache.UseKeyPrefix("[BI-execute]");
        this.repository = _sqlSugarClient as SqlSugarScope;
    }

    public async Task<BiRecord> getRecord(IndexInput input)
    {
        var beginDate = getBeginDate(input.DateType);
        BiRecord record = new();
        var dataStr = DateTime.Now.ToString("yyyyMMdd");
        var authoritys = await GetModelList(input.CurrentUser);
        var list = await getWorkbookList(authoritys);
        record.ModelCount = authoritys.Count();
        // 工作簿总数
        record.WorkbookCount = list.Count();
        // 点击总数
        foreach (var item in list)
        {
            var keyList = await redisCache.KeysLikeAsync(string.Concat(item.Id,"_"));
            foreach(var key in keyList)
            {
                if (DateTime.ParseExact(key.Split('_')[1], "yyyyMMdd", null) < beginDate)
                    continue;
                var memberAndValue = await redisCache.SortedSetRangeByScoreWithScoresAsync(key,false);
                foreach(var member in memberAndValue)
                {
                    record.ClickCount += member.Score;
                }
            }
        }
        // 新增工作簿
        record.AddCount = list.Where(x => x.CreateDate >= beginDate).Count();
        repository.Close();
        return record;
    }

    public async Task<List<BiFrequency>> getTopFive(IndexInput input)
    {
        List<BiFrequency> frequencys = new();
        Dictionary<string, double> dic = new();
        var beginDate = getBeginDate(input.DateType);
        var list = await getWorkbookList(input.CurrentUser);
        double clickCount = 0;
        foreach (var item in list)
        {
            var keyList = await redisCache.KeysLikeAsync(string.Concat(item.Id, "_"));
            foreach(var key in keyList)
            {
                if (DateTime.ParseExact(key.Split('_')[1], "yyyyMMdd", null) < beginDate)
                    continue;
                var memberAndValue = await redisCache.SortedSetRangeByScoreWithScoresAsync(key, false);
                foreach (var member in memberAndValue)
                {
                    clickCount += member.Score;
                }
            }
            dic.Add(item.Id,clickCount);
            clickCount = 0;
        }
        var res = dic.OrderByDescending(x => x.Value).Take(5);
        foreach(var item in res)
        {
            var tmp = list.Where(x => x.Id == item.Key).First();
            frequencys.Add(new BiFrequency
            {
                ModelName = tmp.Opt2,
                WorkBookName = tmp.WorkBookName,
                ClickCount = item.Value,
                WorkBooId = tmp.Id
            });
        }
        repository.Close();
        return frequencys;
    }

    public async Task<List<BiChartRecord>> getTopChartRecord(IndexInput input)
    {
        var beginDate = getBeginDate(input.DateType);
        DateTime beginDateTmp = beginDate.DeepClone();
        List<BiChartRecord> chartRecords = new();
        double clickCount = 0;
        // 首先初始化返回数据格式
        switch (input.DateType)
        {
            case "month":
                for(int i = 0; i<6; i++)
                {
                    chartRecords.Add(new BiChartRecord
                    {
                        DateStr = beginDateTmp.ToString("yyyyMM"),
                        ClickCount = clickCount
                    });
                    beginDateTmp = beginDateTmp.AddMonths(1);
                }
                break;
            case "week":
                for (int i = 0; i < 7; i++)
                {
                    chartRecords.Add(new BiChartRecord
                    {
                        DateStr = beginDateTmp.ToString("yyyyMMdd"),
                        ClickCount = clickCount
                    });
                    beginDateTmp = beginDateTmp.AddDays(7);
                }
                break;
            case "day":
            default:
                for (int i = 0; i < 7; i++)
                {
                    chartRecords.Add(new BiChartRecord
                    {
                        DateStr = beginDateTmp.ToString("yyyyMMdd"),
                        ClickCount = clickCount
                    });
                    beginDateTmp = beginDateTmp.AddDays(1);
                }
                break;
        }

        var keyList = await redisCache.KeysLikeAsync(string.Concat(input.Id, "_"));
        foreach (var key in keyList)
        {
            clickCount = 0;
            var keyDate = DateTime.ParseExact(key.Split('_')[1], "yyyyMMdd", null);

            if (keyDate < beginDate)
                continue;
            var memberAndValue = await redisCache.SortedSetRangeByScoreWithScoresAsync(key, false);
            foreach (var member in memberAndValue)
            {
                clickCount += member.Score;
            }
            BiChartRecord tmp;
            switch (input.DateType)
            {
                case "month":
                    tmp = chartRecords.Where(x => keyDate >= DateTime.ParseExact(x.DateStr, "yyyyMM", null)
                                            && keyDate < DateTime.ParseExact(x.DateStr, "yyyyMM", null).AddMonths(1)).First();
                    break;
                case "week":
                    tmp = chartRecords.Where(x => keyDate >= DateTime.ParseExact(x.DateStr, "yyyyMMdd", null)
                                            && keyDate < DateTime.ParseExact(x.DateStr, "yyyyMMdd", null).AddDays(7)).First();
                    break;
                case "day":
                default:
                    tmp = chartRecords.Where(x => keyDate >= DateTime.ParseExact(x.DateStr, "yyyyMMdd", null)
                                            && keyDate < DateTime.ParseExact(x.DateStr, "yyyyMMdd", null).AddDays(1)).First();
                    break;
            }
            tmp.ClickCount += clickCount;
        }
        chartRecords = chartRecords.OrderBy(x=>x.DateStr).ToList();
        repository.Close();
        return chartRecords;
    }

    public async Task<List<BiModelRecord>> getModelRecord(IndexInput input)
    {
        var authoritys = await GetModelList(input.CurrentUser);

        var list = await getWorkbookList(authoritys);

        var result = list.GroupBy(x=>x.Opt2)
            .ToDictionary(group => group.Key, group => group.Count());

        List<BiModelRecord> records = new ();
        foreach (var item in authoritys) 
        {
            int counts = 0;
            result.TryGetValue(item.Name,out counts);
            records.Add(new BiModelRecord
            {
                ModelName = item.Title,
                Counts = counts
            });
        }
        repository.Close();
        return records;
    }

    private DateTime getBeginDate(string dateType)
    {
        // 根据时间 DateType 获取要计算的开始时间
        DateTime beginDate;
        switch (dateType)
        {
            case "month":
                var sixAgoDate = DateTime.Today.AddMonths(-5);
                beginDate = new DateTime(sixAgoDate.Year, sixAgoDate.Month, 1);
                break;
            case "week":
                int dayAdd = ((int)DateTime.Today.DayOfWeek - 1);
                dayAdd = dayAdd == -1 ? 6 : dayAdd;
                beginDate = DateTime.Today.AddDays(-7 * 6 - dayAdd);
                break;
            case "day":
                beginDate = DateTime.Today.AddDays(-6);
                break;
            default:
                beginDate = DateTime.MinValue;
                break;
        }
        return beginDate;
    }

    private async  Task<List<BIWorkbook>> getWorkbookList(CurrentUser currentUser)
    {
        List<BIWorkbook> list = new();
        var allAuthority = await GetModelList(currentUser);
        var db = repository.GetConnectionScope("bidb");
        foreach (var authority in allAuthority)
        {
            list.AddRange(await db.Queryable<BIWorkbook>().Where(x=>x.Opt2 == authority.Name).ToListAsync());
        }
        db.Close();
        return list;
    }

    private async Task<List<BIWorkbook>> getWorkbookList(IEnumerable<MenuButtonEntity> allAuthority)
    {
        List<BIWorkbook> list = new();
        var db = repository.GetConnectionScope("bidb");
        foreach (var authority in allAuthority)
        {
            list.AddRange(await db.Queryable<BIWorkbook>().Where(x => x.Opt2 == authority.Name).ToListAsync());
        }
        db.Close();
        return list;
    }

    private async Task<IEnumerable<MenuButtonEntity>> GetModelList(CurrentUser currentUser)
    {
        List<Authority> list = new();
        var account = currentUser?.Account ?? "";
        SqlSugarScopeProvider db;
        //var urls = ConfigHelper.Get<string>("Urls");
        db = repository.GetConnectionScope("bidb");

        // 获取当前用户权限信息
        var userInfo = await repository.Queryable<CurrentUser>().FirstAsync(x => x.Account == account && x.Enabled == 1);
        if (userInfo == null)
            return null;
        string[] arr = userInfo.RoleIds.Split(',');
        var roles = await repository.Queryable<RoleAuthorizeEntity>().Where(x => arr.Contains(x.RoleId) && x.Enabled == 1).ToListAsync();

        List<string> ids = new();
        foreach (var role in roles)
        {
            ids.AddRange(role.MenuButtonId.Split(','));
        }
        IEnumerable<string> enums = ids.Distinct();

        List<MenuButtonEntity> menus = new();
        if (AppSettings.IsAdministrator(userInfo.Account) == 1)
            menus = await repository.Queryable<MenuButtonEntity>().Where(x => x.Enabled == 1).ToListAsync();
        else
            menus = await repository.Queryable<MenuButtonEntity>().Where(x => enums.Contains(x.Id) && x.Enabled == 1).ToListAsync();

        // 模型总数
        var root = menus.Where(x => x.Category == 1 && x.Name == "preview-bi");
        if (root.Any())
        {
            var rootId = root.First().Id;
            var templates = menus.Where(x => x.ParentId == rootId && x.Name.Contains("Template")).ToList();
            //添加公共模型
            templates.Add(new MenuButtonEntity
            {
                Name = "commonTemplate",
                Title= "公共模型"
            });
            return templates;
        }
        db.Close();
        return new List<MenuButtonEntity>();
    }
}
