using Microsoft.AspNetCore.Mvc;

namespace dotnetStudy.Controllers;

[ApiController]
[Route("[controller]")]
public class HomeController : ControllerBase
{
    private readonly ILogger<HomeController> _logger;

    private DateTime _firstTime = DateTime.MinValue;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 获取标记字段所有值
    /// </summary>
    [HttpPost]
    [ActionName("getValue")]
    public async Task<string> getValue()
    {

        return "succcess  " + _firstTime;
    }

}

