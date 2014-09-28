﻿using CocosSharp;
using EssenceShared;

namespace EssenceClient {
    internal class Program {
        private static void Main(string[] args) {
            Log.Print("Starting Essence Client");

            var application = new CCApplication(false, new CCSize(Settings.ScreenWidth, Settings.ScreenHeight)) {
                ApplicationDelegate = new Client()
            };
            application.StartGame();
        }
    }
}