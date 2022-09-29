using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

public static class Packets
{
    public enum PacketType
    {
        Introduction,
        ObjectCreation,
        WorldData
    }
}
