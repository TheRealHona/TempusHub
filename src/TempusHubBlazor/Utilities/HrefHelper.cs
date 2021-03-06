﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TempusHubBlazor.Utilities
{
    public static class HrefHelper
    {
        public static string GetMapInfoPath(string mapName) => "/map/" + mapName;
        public static string GetPlayerInfoPath(int playerId) => GetPlayerInfoPath(playerId.ToString());
        public static string GetPlayerInfoPath(string playerId) => "/player/" + playerId;
        public static string GetServerInfoPath(string server) => "/server/" + server;
        public static string GetDemoInfoPath(int demoId) => "/demo/" + demoId;
        public static string GetRunInfoPath(int runId) => "/run/" + runId;

    }
}
