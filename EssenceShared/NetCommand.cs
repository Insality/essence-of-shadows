﻿using System;
using Newtonsoft.Json;

namespace EssenceShared {
    public enum NetCommandType {
        SAY,
        UPDATE_GAMESTATE,
        UPDATE_PLAYERSTATE,
        CONNECT,
        DISCONNECT,
        CALL_PLAYER_METHOD
    }

    public class NetCommand {
        public NetCommand(NetCommandType type, string data) {
            CreateTime = DateTime.Now;
            Type = type;
            Data = data;
        }

        public DateTime CreateTime { get; private set; }
        /** Data - строка с необходимыми полями для указанного типа команды
        * Должна быть в формате json, если команда update Game/Player-state */
        public string Data { get; private set; }
        public NetCommandType Type { get; private set; }

        public string Serialize() {
            return JsonConvert.SerializeObject(this);
        }

        public static NetCommand Deserialize(string json) {
            return JsonConvert.DeserializeObject<NetCommand>(json);
        }
    }
}