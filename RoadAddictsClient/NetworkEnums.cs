using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoadAddictsClient
{
    public enum UnconnectedMessageType
    {
        MyGameIsNowUnavailable,
        JoinRequest,
        JoinRequestReply,
        IsGameStillAvailableRequest,
        IsGameStillAvailableResponse
    }

    public enum JoinRequestStatus
    {
        NoReply,
        PositiveReply,
        NegativeReply
    }

    public enum ConnectedMessageType
    {
        Chat,
        NetworkCommands,
        PeerConnectionInstructions,
        StartGame,
        EndRoundInfo,
        PlayerConnected,
        PlayerDisconnected,
        ChatMessageFromPlayerToHost,
        ChatMessageFromHostToPlayers,
        Ping,
        PingReply
    }
}
