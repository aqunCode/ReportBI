﻿using Bi.Core.Interfaces;
using Bi.Core.Models;
using Bi.Entities.Entity;
using Bi.Entities.Input;
using Bi.Entities.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Services.IService;

public interface IMenuButtonService : IDependency
{
    Task<IEnumerable<AuthMenuResponse>> GetListTreeCurrentUserAsync(CurrentUser currentUser);
    Task<PageEntity<IEnumerable<MenuButtonResponse>>> GetPageListTreeAsync(PageEntity<MenuButtonInput> input);
    Task<double> addAsync(MenuButtonInput input);
    Task<double> deleteAsync(MenuButtonInput input);
    Task<double> ModifyAsync(MenuButtonInput input);
    Task<PageEntity<IEnumerable<MenuButtonEntity>>> getEntityListAsync(PageEntity<MenuButtonInput> inputs);
    Task<IEnumerable<MenuButtonTree>> getMenuTree();
}
