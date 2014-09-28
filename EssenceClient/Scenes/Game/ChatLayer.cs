﻿using System.Collections.Generic;
using System.Linq;
using CocosSharp;
using EssenceShared;

namespace EssenceClient.Scenes.Game {
    internal class ChatLayer: CCLayerColor {
        public List<string> Messages;
        private CCLabel _label;

        public ChatLayer() {
            Layer.Scale = 0.3f;
            Messages = new List<string>();

            AnchorPoint = CCPoint.AnchorLowerLeft;
            PositionX = 200;
            PositionY = 0;

            Log.Print("cht init");
        }

        protected override void AddedToScene() {
            base.AddedToScene();

            _label = new CCLabel("Chat", "kongtext", 28) {
                Color = CCColor3B.White,
                AnchorPoint = CCPoint.AnchorLowerLeft,
                Position = Layer.Position
            };

            AddChild(_label);

            Schedule(Update);
        }

        public override void Update(float dt) {
            base.Update(dt);

            if (Messages.Count > 0)
                _label.Text = "Message: " + Messages.Last();
        }
    }
}