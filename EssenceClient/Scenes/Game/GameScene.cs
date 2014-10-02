﻿using System;
using CocosSharp;
using EssenceShared;
using EssenceShared.Entities;
using EssenceShared.Entities.Players;
using EssenceShared.Scenes;
using IniParser;
using IniParser.Model;
using Lidgren.Network;

namespace EssenceClient.Scenes.Game {
    internal class GameScene: CCScene {
        private readonly ChatLayer _chatLayer;
        private readonly NetGameClient _netGameClient;
        private BackgroundLayer _backgroundLayer;
        private HudLayer _hudLayer;
        private int cameraHight = 700;

        public GameScene(CCWindow window): base(window) {
            Id = "888888888888888";

            _backgroundLayer = new BackgroundLayer();
            AddChild(_backgroundLayer);

            //            _camLayer = new CameraLayer();

            var cameraVisibleBounds = new CCSize(Settings.ScreenWidth, Settings.ScreenHeight);
            var camera = new CCCamera(CCCameraProjection.Projection3D, cameraVisibleBounds, new CCPoint3(800, 600, 10));

            GameLayer = new GameLayer {
                Tag = Tags.Client,
                Camera = camera
            };
            AddChild(GameLayer);

            _chatLayer = new ChatLayer();
            AddChild(_chatLayer);

            _hudLayer = new HudLayer();
            AddChild(_hudLayer);


            var keyListener = new CCEventListenerKeyboard {OnKeyPressed = OnKeyPressed, OnKeyReleased = OnKeyReleased};

            AddEventListener(keyListener, this);

            var mouseListener = new CCEventListenerMouse {OnMouseDown = OnMouseDown};

            AddEventListener(mouseListener, this);

            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile("Config.ini");
            _netGameClient = new NetGameClient(data["Server"]["ip"], this);
            _netGameClient.ConnectToServer();

            Schedule(UpdateNetwork, 0.03f);
            Schedule(Update);
        }

        public Player MyPlayer { get; private set; }


        public string Id { get; private set; }

        public GameLayer GameLayer { get; private set; }

        public void UpdateNetwork(float dt) {
            base.Update(dt);
            UpdateMyState();
        }

        public override void Update(float dt) {
            base.Update(dt);

            if (MyPlayer != null){
                MyPlayer.Control(dt);
            }

            UpdateCamera();
        }

        private void UpdateCamera() {
            if (MyPlayer != null){
                GameLayer.Camera.CenterInWorldspace = new CCPoint3(MyPlayer.Position, cameraHight);
                GameLayer.Camera.TargetInWorldspace = new CCPoint3(MyPlayer.Position, 0);
                //                Camera.CenterInWorldspace = new CCPoint3(MyPlayer.Position, 0);
            }
        }

        public void SetMyId(string id) {
            Log.Print("Set new Id: " + id);
            Id = id;
        }

        public void GetChatMessage(string msg) {
            _chatLayer.Messages.Add(msg);
        }

        private void UpdateMyState() {
            if (MyPlayer != null){
                EntityState myps = EntityState.ParseEntity(MyPlayer);

                var nc = new NetCommand(NetCommandType.UpdatePlayerstate, myps.Serialize());
                _netGameClient.Send(nc, NetDeliveryMethod.Unreliable);
            }
            else{
                Entity myPl = GameLayer.FindEntityById(Id);
                if (myPl != null){
                    MyPlayer = (Player) myPl;
                }
            }
        }

        private void OnKeyPressed(CCEventKeyboard e) {
            Input.OnKeyPress(e.Keys);
        }

        private void OnKeyReleased(CCEventKeyboard e) {
            Input.OnKeyRelease(e.Keys);

            if (e.Keys == CCKeys.S){
                _netGameClient.SendChatMessage("DASDA" + Id);
            }

            if (e.Keys == CCKeys.A){
                var nc = new NetCommand(NetCommandType.CallPlayerMethod, "attack.100.100");
                _netGameClient.Send(nc, NetDeliveryMethod.ReliableOrdered);
            }

            if (e.Keys == CCKeys.Escape){
                Window.DefaultDirector.PopScene();
            }

            if (e.Keys == CCKeys.T){
                Console.WriteLine(MyPlayer.Mask.ToString());
            }
        }

        private void OnMouseDown(CCEventMouse obj) {
            /** get scale coef.*/
            float windowScaleX = Window.WindowSizeInPixels.Width/Settings.ScreenWidth;
            float windowScaleY = Window.WindowSizeInPixels.Height/Settings.ScreenHeight;

            /** Актуальные координаты */
            var mousePosX = (int) (obj.CursorX/windowScaleX);
            var mousePosY = (int) (obj.CursorY/windowScaleY);
            // поправка на камеру:
            if (GameLayer.Camera != null){
                //Correcting by camera
                mousePosX += (int) (GameLayer.Camera.TargetInWorldspace.X - Settings.ScreenWidth/2);
                mousePosY += (int) (GameLayer.Camera.TargetInWorldspace.Y - Settings.ScreenHeight/2);
            }
//            Console.WriteLine("Got pos: " + mousePosX + " " + mousePosY);

            if (obj.MouseButton == CCMouseButton.LeftButton){
                // Стреляем при нажатой левой кнопке
                var nc = new NetCommand(NetCommandType.CallPlayerMethod, "attack." + mousePosX + "." + mousePosY);
                _netGameClient.Send(nc, NetDeliveryMethod.ReliableOrdered);
            }
        }
    }
}