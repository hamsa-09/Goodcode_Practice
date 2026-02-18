using System;
using System.Collections.Generic;
using Assignment_Example_HU.Enums;

namespace Assignment_Example_HU.DTOs
{
    public class GameDto
    {
        public Guid Id { get; set; }
        public Guid SlotId { get; set; }
        public GameType Type { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public int CurrentPlayerCount { get; set; }
        public GameStatus Status { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public List<GamePlayerDto> Players { get; set; } = new List<GamePlayerDto>();
    }

    public class GamePlayerDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = default!;
        public DateTime JoinedAt { get; set; }
    }

    public class CreateGameDto
    {
        public Guid SlotId { get; set; }
        public GameType Type { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
    }

    public class JoinGameDto
    {
        public Guid GameId { get; set; }
    }

    public class LeaveGameDto
    {
        public Guid GameId { get; set; }
    }
}
