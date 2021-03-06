﻿using MultiMiner.Utility.Serialization;
using MultiMiner.Xgminer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MultiMiner.Engine.Configuration
{
    public class EngineConfiguration
    {
        public EngineConfiguration()
        {
            DeviceConfigurations = new List<DeviceConfiguration>();
            CoinConfigurations = new List<CoinConfiguration>();
            XgminerConfiguration = new XgminerConfiguration();
            StrategyConfiguration = new StrategyConfiguration();
        }

        public List<DeviceConfiguration> DeviceConfigurations { get; set; }
        public List<CoinConfiguration> CoinConfigurations { get; set; }
        public XgminerConfiguration XgminerConfiguration { get; set; }
        public StrategyConfiguration StrategyConfiguration { get; set; }

        public void RemoveBlankPoolConfigurations()
        {
            foreach (CoinConfiguration coinConfiguration in CoinConfigurations)
            {
                for (int i = coinConfiguration.Pools.Count - 1; i >= 0; i--)
                {
                    MiningPool pool = coinConfiguration.Pools[i];
                    if (String.IsNullOrEmpty(pool.Host) &&
                        String.IsNullOrEmpty(pool.Username))
                        coinConfiguration.Pools.Remove(pool);
                }
            }
        }

        private string configDirectory;
        public void LoadAllConfigurations(string configDirectory)
        {
            InitializeConfigDirectory(configDirectory);

            LoadCoinConfigurations(configDirectory);
            LoadDeviceConfigurations();
            LoadMinerConfiguration();
            LoadStrategyConfiguration(configDirectory);
        }

        public string StrategyConfigurationsFileName()
        {
            return Path.Combine(configDirectory, "StrategyConfiguration.xml");
        }

        public void SaveStrategyConfiguration(string configDirectory = null)
        {
            InitializeConfigDirectory(configDirectory);

            ConfigurationReaderWriter.WriteConfiguration(StrategyConfiguration, StrategyConfigurationsFileName());
        }

        private void InitializeConfigDirectory(string configDirectory)
        {
            if (!String.IsNullOrEmpty(configDirectory))
                this.configDirectory = configDirectory;
            else if (String.IsNullOrEmpty(this.configDirectory))
                this.configDirectory = ApplicationPaths.AppDataPath();
        }

        public void LoadStrategyConfiguration(string configDirectory)
        {
            InitializeConfigDirectory(configDirectory);

            try
            {
                StrategyConfiguration = ConfigurationReaderWriter.ReadConfiguration<StrategyConfiguration>(StrategyConfigurationsFileName());
            }
            catch (InvalidOperationException ex)
            {
                //legacy settings
                Obsolete.StrategyConfiguration obsoleteSettings = ConfigurationReaderWriter.ReadConfiguration<Obsolete.StrategyConfiguration>(StrategyConfigurationsFileName());
                StrategyConfiguration = new StrategyConfiguration();
                obsoleteSettings.StoreTo(StrategyConfiguration);
                SaveStrategyConfiguration();
            }
        }

        private static string DeviceConfigurationsFileName()
        {
            return Path.Combine(ApplicationPaths.AppDataPath(), "DeviceConfigurations.xml");
        }
        
        public void SaveDeviceConfigurations()
        {
            ConfigurationReaderWriter.WriteConfiguration(DeviceConfigurations, DeviceConfigurationsFileName());
        }

        public void LoadDeviceConfigurations()
        {
            DeviceConfigurations = ConfigurationReaderWriter.ReadConfiguration<List<DeviceConfiguration>>(DeviceConfigurationsFileName());
            RemoveIvalidCoinsFromDeviceConfigurations();
            RemoveDuplicateDeviceConfigurations();
        }

        //this is necessary due to large changes to the class definition and streaming in / deserializing
        //older legacy XML
        public void RemoveDuplicateDeviceConfigurations()
        {
            DeviceConfigurations = DeviceConfigurations
                .GroupBy(c => new { c.Kind, c.RelativeIndex, c.Driver, c.Path, c.Serial })
                .Select(c => c.First())
                .ToList();
        }

        public string CoinConfigurationsFileName()
        {
            return Path.Combine(configDirectory, "CoinConfigurations.xml");
        }

        public void LoadCoinConfigurations(string configDirectory)
        {
            InitializeConfigDirectory(configDirectory);

            CoinConfigurations = ConfigurationReaderWriter.ReadConfiguration<List<CoinConfiguration>>(CoinConfigurationsFileName());
            RemoveIvalidCoinsFromDeviceConfigurations();
            RemoveBlankPoolConfigurations();
        }

        private void RemoveDisabledCoinsFromDeviceConfigurations()
        {
            foreach (CoinConfiguration coinConfiguration in CoinConfigurations.Where(c => !c.Enabled))
            {
                IEnumerable<DeviceConfiguration> coinDeviceConfigurations = DeviceConfigurations.Where(c => !String.IsNullOrEmpty(c.CoinSymbol) && c.CoinSymbol.Equals(coinConfiguration.Coin.Symbol));
                foreach (DeviceConfiguration coinDeviceConfiguration in coinDeviceConfigurations)
                    coinDeviceConfiguration.CoinSymbol = string.Empty;
            }
        }

        private void RemoveDeletedCoinsFromDeviceConfigurations()
        {
            foreach (DeviceConfiguration deviceConfiguration in DeviceConfigurations)
                if (CoinConfigurations.Count(c => c.Coin.Symbol.Equals(deviceConfiguration.CoinSymbol)) == 0)
                    deviceConfiguration.CoinSymbol = string.Empty;
        }

        private void RemoveIvalidCoinsFromDeviceConfigurations()
        {
            RemoveDisabledCoinsFromDeviceConfigurations();
            RemoveDeletedCoinsFromDeviceConfigurations();
        }

        public void SaveCoinConfigurations(string configDirectory = null)
        {
            InitializeConfigDirectory(configDirectory);

            RemoveBlankPoolConfigurations();
            ConfigurationReaderWriter.WriteConfiguration(CoinConfigurations, CoinConfigurationsFileName());
            RemoveIvalidCoinsFromDeviceConfigurations();
        }

        public void LoadMinerConfiguration()
        {
            XgminerConfiguration.LoadMinerConfiguration();
        }

        public void SaveMinerConfiguration()
        {
            XgminerConfiguration.SaveMinerConfiguration();
        }
    }
}
