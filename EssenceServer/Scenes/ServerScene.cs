﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using CocosSharp;
using EssenceShared;
using EssenceShared.Entities;
using EssenceShared.Entities.Objects;
using EssenceShared.Entities.Players;
using EssenceShared.Game;
using EssenceShared.Game.Generators;
using EssenceShared.Scenes;

namespace EssenceServer.Scenes {
    /// <summary>
    ///     Основная сцена на сервере. Запускает игровой слой и занимается управлением состояние сервера
    /// </summary>
    internal class ServerScene : CCScene {
        private readonly GameLayer _caveGameLayer;
        private readonly GameLayer _cityGameLayer;
        private readonly List<EnemyManager> _enemiesManager = new List<EnemyManager>();
        private readonly GameLayer _gameLayer;
        private readonly GameLayer _townGameLayer;
        public List<AccountState> Accounts = new List<AccountState>();
        public Dictionary<Locations, GameLayer> LocationsDict;

        public ServerScene(CCWindow window) : base(window) {
            LocationsDict = new Dictionary<Locations, GameLayer>();

            _gameLayer = new GameLayer {Tag = Tags.Server, Location = Locations.Desert};
            AddChild(_gameLayer);
            LocationsDict.Add(Locations.Desert, _gameLayer);

            _townGameLayer = new GameLayer {Tag = Tags.Server, Location = Locations.Town};
            AddChild(_townGameLayer);
            LocationsDict.Add(Locations.Town, _townGameLayer);

            _cityGameLayer = new GameLayer {Tag = Tags.Server, Location = Locations.City};
            AddChild(_cityGameLayer);
            LocationsDict.Add(Locations.City, _cityGameLayer);


            _caveGameLayer = new GameLayer {Tag = Tags.Server, Location = Locations.Cave};
            AddChild(_caveGameLayer);
            LocationsDict.Add(Locations.Cave, _caveGameLayer);

            Log.Print("Game has started, waiting for players");
            Schedule(UpdateNetwork, Settings.NetworkFreqUpdate);
            Schedule(UpdateLogic);
        }

        public GameLayer GetGameLayer(Locations location) {
            return LocationsDict[location];
        }

        public override void OnEnter() {
            base.OnEnter();
            InitMap();

            //            _enemiesManager.Add(new EnemyManager(_cityGameLayer));
            // Adding test enemies:
            AddTestEnemies();
            _enemiesManager.Add(new EnemyManager(_gameLayer));
            _enemiesManager.Add(new EnemyManager(_caveGameLayer));
            _enemiesManager.Add(new EnemyManager(_cityGameLayer));

            // Adding event:
            Log.Print("Adding event to ChangeLocation");
            EosEvent.ChangeLocation +=
                (sender, args) => Server.SendMap((sender as Player).Id, (sender as Player).AccState.Location);
        }

        private void AddTestEnemies() {
            _gameLayer.AddEntity(new Gate(Util.GetUniqueId()) {
                PositionX = -10,
                PositionY = -10,
                TeleportTo = Locations.Town
            });

            for (int i = 0; i < 10; i++)
                _townGameLayer.AddEntity(new GoldStack(Util.GetUniqueId()) {
                    PositionX = CCRandom.Next(100, 1400),
                    PositionY = CCRandom.Next(100, 1400)
                });
            _townGameLayer.AddEntity(new Gate(Util.GetUniqueId()) {
                PositionX = 500,
                PositionY = 500,
                TeleportTo = Locations.Desert
            });
            _townGameLayer.AddEntity(new Gate(Util.GetUniqueId()) {
                PositionX = 700,
                PositionY = 600,
                TeleportTo = Locations.City
            });
            _townGameLayer.AddEntity(new Gate(Util.GetUniqueId()) {
                PositionX = 900,
                PositionY = 500,
                TeleportTo = Locations.Cave
            });
            _townGameLayer.AddEntity(new Smith(Util.GetUniqueId()) {
                PositionX = 100,
                PositionY = 700,
            });


            _cityGameLayer.AddEntity(new Gate(Util.GetUniqueId()) {
                PositionX = -10,
                PositionY = -10,
                TeleportTo = Locations.Town
            });
            _caveGameLayer.AddEntity(new Gate(Util.GetUniqueId()) {
                PositionX = -10,
                PositionY = -10,
                TeleportTo = Locations.Town
            });
        }

        /// <summary>
        ///     Считывает карту и возвращает её как массив строк
        /// </summary>
        public List<string> ParseMap(string map) {
            string s = File.ReadAllText(map);
            var tileMap = new List<string>(s.Split('\n'));

            for (int i = 0; i < tileMap.Count; i++) {
                tileMap[i] = tileMap[i].TrimEnd('\r');
            }
            // Переворачиваем её сверху вниз
            tileMap.Reverse();
            return tileMap;
        }

        /// <summary>
        ///     Возвращает текущее игровое состояние для указанного игрока
        ///     В состояние помещаются сущности, находящиеся на определенном расстоянии от игрока
        /// </summary>
        public GameState GetGameState(string playerId) {
            var gs = new GameState();

            Player pl = GetPlayer(playerId);

            if (pl != null) {
                AccountState accState = Accounts.Find(x => x.HeroId == playerId);
                if (accState != null) {
                    gs.Account = accState;

                    List<Entity> entities = GetGameLayer(accState.Location).Entities.ToList();
                    foreach (Entity entity in entities) {
                        if (pl.DistanceTo(entity.Position) < 800)
                            gs.Entities.Add(EntityState.ParseEntity(entity));
                    }
                }
            }

            return gs;
        }

        /// <summary>
        ///     Обновляет состояние игрока, полученное от клиента
        ///     Обновляется только его позиция
        /// </summary>
        internal void AppendPlayerState(EntityState es) {
            Entity player = GetPlayer(es.Id);

            if (player != null) {
                player.PositionX = es.PositionX;
                player.PositionY = es.PositionY;
                player.FlipX = es.FlipX;
            }
        }

        private void UpdateLogic(float dt) {
            foreach (GameLayer gameLayer in LocationsDict.Values) {
                gameLayer.Update(dt);
            }

            foreach (EnemyManager enemyManager in _enemiesManager) {
                enemyManager.Update();
            }
        }

        public void UpdateNetwork(float dt) {
            Server.SendGameStateToAll();
        }

        private void InitMap() {
//            _gameLayer.CreateNewMap(Generator.GenerateLevel(11, 10, LevelType.Desert));
//            _townGameLayer.CreateNewMap(Generator.GenerateLevel(11, 10, LevelType.Town));
//            _cityGameLayer.CreateNewMap(Generator.GenerateLevel(11, 10, LevelType.City));
            _caveGameLayer.CreateNewMap(Generator.GenerateLevel(11, 10, LevelType.Cave));
            _gameLayer.CreateNewMap(ParseMap("DesertMap.txt"));
            _townGameLayer.CreateNewMap(ParseMap("TownMap.txt"));
            _cityGameLayer.CreateNewMap(ParseMap("CityMap.txt"));
            _caveGameLayer.CreateNewMap(ParseMap("CaveMap.txt"));
        }

        internal Player GetPlayer(string id) {
            Player player = null;
            foreach (GameLayer gameLayer in LocationsDict.Values) {
                player = gameLayer.FindEntityById(id) as Player;
                if (player != null)
                    break;
            }
            return player;
        }
    }
}