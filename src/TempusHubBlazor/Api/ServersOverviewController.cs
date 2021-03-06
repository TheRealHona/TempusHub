﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TempusHubBlazor.Models.Api;
using TempusHubBlazor.Models.Tempus.Responses;
using TempusHubBlazor.Services;

namespace TempusHubBlazor.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServersOverviewController : ControllerBase
    {
        private TempusCacheService _tempusCacheService;
        public ServersOverviewController(TempusCacheService tempusCacheService)
        {
            _tempusCacheService = tempusCacheService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ServerStatusModel>> Get()
        {
            if (_tempusCacheService.ServerStatusList == null || _tempusCacheService.ServerStatusList.Count == 0)
            {
                return NoContent();
            }

            return _tempusCacheService.ServerStatusList;
        }
    }
}
