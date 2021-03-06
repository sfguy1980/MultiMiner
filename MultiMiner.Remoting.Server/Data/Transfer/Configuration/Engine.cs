﻿using MultiMiner.Engine.Configuration;
using System.Collections.Generic;

namespace MultiMiner.Remoting.Server.Data.Transfer.Configuration
{
    public class Engine
    {
        public List<DeviceConfiguration> DeviceConfigurations { get; set; }
        public List<CoinConfiguration> CoinConfigurations { get; set; }
        public XgminerConfiguration XgminerConfiguration { get; set; }
        public StrategyConfiguration StrategyConfiguration { get; set; }
    }
}
