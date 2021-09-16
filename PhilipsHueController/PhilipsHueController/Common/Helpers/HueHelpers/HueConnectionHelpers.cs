﻿using PhilipsHueController.Helpers;
using Q42.HueApi;
using Q42.HueApi.Models.Bridge;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhilipsHueController
{
    public static class HueConnectionHelpers
    {
        private static LocalHueClient LocalHueClient;

        public static LocalHueClient GetLocalHueClient()
        {
            if(LocalHueClient == null)
            {
                LoadConfiguredBridge();
            }

            return LocalHueClient;
        }

        public static async Task<bool> ConfigureBridge(string ipAddress)
        {
            try
            {
                LocalHueClient = new LocalHueClient(ipAddress);
                var appKey = await LocalHueClient.RegisterAsync("PhilipsHueController", "MyPC");

                LocalHueClient.Initialize(appKey);

                ConfigHelpers.AddOrUpdateAppSetting("AppKey", appKey);
                ConfigHelpers.AddOrUpdateAppSetting("BridgeIpAddress", ipAddress);

                return true;
            }
            catch (LinkButtonNotPressedException)
            {
                return false;
            }
        }

        public static void LoadConfiguredBridge()
        {
            var bridgeIpAddress = ConfigHelpers.GetSettingByKey("BridgeIpAddress");
            var appKey = ConfigHelpers.GetSettingByKey("AppKey");

            LocalHueClient = new LocalHueClient(bridgeIpAddress);
            LocalHueClient.Initialize(appKey);
        }

        public async static Task<Bridge> GetConnectedBridge()
        {
            return await LocalHueClient.GetBridgeAsync();
        }

        public static bool IsApplicationRegistered()
        {
            var appKey = ConfigHelpers.GetSettingByKey("AppKey");
            return appKey != null;
        }

        public static async Task<List<LocatedBridge>> GetNearbyBridges()
        {
            var locator = new HttpBridgeLocator();

            await locator.LocateBridgesAsync(TimeSpan.FromSeconds(5));
            return await HueBridgeDiscovery.CompleteDiscoveryAsync(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30));
        }
    }
}