using System;
using System.Collections.Generic;
using RoguelikeDarling.Core.Storage;

namespace RoguelikeDarling.Core.GameSpecific.Menus.TileMap
{
    public sealed class TileMapMenuComponent
    {
        public bool IsVisible { get; set; } = true;

        public bool NeedsRefresh { get; set; } = true;

        public DateTime LastRefreshUtc { get; set; } = DateTime.MinValue;

        public List<TileMapMenuItem> Items { get; } = new List<TileMapMenuItem>();
    }
}
