﻿using System;
using System.Collections.Generic;
using EssenceShared.Entities.Players;
using EssenceShared.Game;
using EssenceShared.Scenes;
using Newtonsoft.Json;

namespace EssenceShared {
    public class AccountState {
        private readonly Dictionary<Locations, GameLayer> _locations;
        public Stat Exp;
        public int Gold;
        public string HeroId;
        public int Level;
        public Locations Location = Locations.Town;

        // Меняется сервером когда он учтет смену локации
        [JsonIgnore] public Locations PrevLocation;


        public AccountState(string id, Dictionary<Locations, GameLayer> locations) {
            HeroId = id;
            Gold = 0;
            Exp = new Stat(Settings.StartExp) {Current = 0};
            Level = 1;
            _locations = locations;
            PrevLocation = Location;
        }

        public void SwitchLocation(Locations locationTo) {
            PrevLocation = Location;

            Player player = GetPlayer();
            _locations[Location].RemoveChild(player);
            Location = locationTo;
            _locations[locationTo].AddEntity(player);
        }

        /// <summary>
        ///     Вызывается классом Player
        /// </summary>
        public void Update() {
            if (Exp.Perc == 1){
                LevelUp();
            }
        }

        private void LevelUp() {
            Level++;
            Exp.Current = 0;
            Exp.Maximum = (int) (Settings.ExpMultiplier*Exp.Maximum);

            // Inc stats:
            Player player = GetPlayer();
            player.Hp.Maximum += 30;
            player.AttackDamage += 3;
            player.Hp.Current = GetPlayer().Hp.Maximum;
        }

        private Player GetPlayer() {
            var pl = _locations[Location].FindEntityById(HeroId) as Player;
            return pl;
        }

        public static AccountState LoadAccountState(string AccountId) {
            //TODO: грузить из базы
            throw new NotImplementedException();
        }

        public AccountState SaveAccountState() {
            //TODO: записывать в базу
            throw new NotImplementedException();
        }

        public string Serialize() {
            return JsonConvert.SerializeObject(this);
        }
    }
}