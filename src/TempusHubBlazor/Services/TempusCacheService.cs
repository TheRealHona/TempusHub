﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TempusHubBlazor.Data;
using TempusHubBlazor.Models;
using TempusHubBlazor.Models.Tempus.Rank;
using TempusHubBlazor.Models.Tempus.Responses;

namespace TempusHubBlazor.Services
{
    public class TempusCacheService
    {
        private readonly Timer _updateTimer;
        private readonly TempusDataService TempusDataService;
        public RecentActivityModel RecentActivity { get; private set; }
        public RecentActivityWithZonedData RecentActivityWithZonedData { get; private set; } = new RecentActivityWithZonedData();
        public List<TopPlayerOnline> TopPlayersOnline { get; private set; } = new List<TopPlayerOnline>();

        public TempusCacheService(TempusDataService tempusDataService)
        {
            TempusDataService = tempusDataService;
            UpdateAllCachedDataAsync().GetAwaiter().GetResult();
            _updateTimer = new Timer(async callback => await UpdateAllCachedDataAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1.5));
        }
        
        private async Task UpdateAllCachedDataAsync()
        {
            var tasks = new List<Task>
            {
                 UpdateRecentActivityAsync().ContinueWith((taskOutput) => UpdateRecentActivityWithZonedDataAsync()),
                 UpdateTopOnlinePlayersAsync()
            };

            await Task.WhenAll(tasks);
        }
        private async Task UpdateRecentActivityAsync()
        {
            RecentActivity = await TempusDataService.GetRecentActivityAsync();
        }
        private async Task UpdateRecentActivityWithZonedDataAsync()
        {
            var recentActivityWithZonedData = new RecentActivityWithZonedData();

            var tasks = new List<Task>();
            tasks.AddRange(RecentActivity.MapRecords.Select(async x => recentActivityWithZonedData.MapWR.Add(await TempusDataService.PopulateRecordDataAsync(x))));
            tasks.AddRange(RecentActivity.CourseRecords.Select(async x => recentActivityWithZonedData.CourseWR.Add(await TempusDataService.PopulateRecordDataAsync(x))));
            tasks.AddRange(RecentActivity.BonusRecords.Select(async x => recentActivityWithZonedData.BonusWR.Add(await TempusDataService.PopulateRecordDataAsync(x))));
            tasks.AddRange(RecentActivity.MapTopTimes.Select(async x => recentActivityWithZonedData.MapTT.Add(await TempusDataService.PopulateRecordDataAsync(x))));
            await Task.WhenAll(tasks);

            RecentActivityWithZonedData = recentActivityWithZonedData;
        }
        private async Task UpdateTopOnlinePlayersAsync()
        {
            var tempTopPlayersOnline = new List<TopPlayerOnline>();

            // Get online servers
            var servers = (await TempusDataService.GetServerStatusAsync()).Where(x => x != null).ToArray();

            // Get all valid online users
            var users = servers.Where(x => x.GameInfo != null &&
                                           (x.GameInfo != null || x.ServerInfo != null ||
                                            x.GameInfo.Users != null) &&
                                           x.GameInfo.Users.Count != 0)
                .SelectMany(x => x.GameInfo.Users).Where(x => x?.Id != null).ToArray();

            // Get the user IDs as strings
            var userIdStrings = (users.Where(user => user?.Id != null).Select(user => user.Id.ToString())).ToList();

            // Query all at once for all users ranks
            var rankTasks = new List<Task<Rank>>();
            rankTasks.AddRange(userIdStrings.Select(TempusDataService.GetUserRankAsync));
            var ranks = await Task.WhenAll(rankTasks);

            // Get the users that actually have a rank (exclude unranks), and select the higher rank
            // Dictionary<User, BestRank>
            var rankedUsers = ranks.ToDictionary(rank => users.First(x => x.Id == rank.PlayerInfo.Id), rank =>
                rank.ClassRankInfo.DemoRank.Rank <= rank.ClassRankInfo.SoldierRank.Rank
                    ? rank.ClassRankInfo.DemoRank.Rank
                    : rank.ClassRankInfo.SoldierRank.Rank);

            // Sort by the best rank
            var output = rankedUsers.OrderBy(x => x.Value);


            foreach (var (player, rank) in output)
            {
                if (player == null || rank > 200) continue;
                var server = servers
                    .FirstOrDefault(x =>
                        x.GameInfo?.Users != null &&
                        x.GameInfo.Users.Count(z => z.Id.HasValue && z.Id == player.Id) != 0);
                if (server == null || player.Id == null) continue;

                tempTopPlayersOnline.Add(new TopPlayerOnline
                {
                    Rank = rank,
                    SteamId = player.SteamId,
                    Name = player.Name,
                    Server = server
                });
            }

            TopPlayersOnline = tempTopPlayersOnline;
        }
    }
}