﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TempusHubBlazor.Models.Tempus.PlayerStats
{
    public class ZonedStats
    {
        [JsonProperty(PropertyName = "map")]
        public BaseZoneStats Map { get; set; }
        [JsonProperty(PropertyName = "course")]
        public BaseZoneStats Course { get; set; }
        [JsonProperty(PropertyName = "bonus")]
        public BaseZoneStats Bonus { get; set; }
        [JsonProperty(PropertyName = "Trick")]
        public BaseZoneStats Trick { get; set; }
    }
}
