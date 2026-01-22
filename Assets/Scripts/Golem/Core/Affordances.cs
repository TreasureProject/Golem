namespace Golem
{
    /// <summary>
    /// Standard affordance types that define what actions can be performed on objects.
    /// Agents use these to understand what they can do with discovered objects.
    /// </summary>
    public static class Affordances
    {
        // Seating
        public const string Sit = "sit";
        public const string Stand = "stand";

        // Doors and barriers
        public const string Open = "open";
        public const string Close = "close";
        public const string Enter = "enter";
        public const string Exit = "exit";

        // Interaction
        public const string Use = "use";
        public const string Play = "play";
        public const string Examine = "examine";
        public const string Talk = "talk";

        // Items
        public const string PickUp = "pickup";
        public const string Drop = "drop";

        // Leaning/posing
        public const string Lean = "lean";
        public const string LookAt = "lookat";
    }

    /// <summary>
    /// Standard object types for categorization.
    /// </summary>
    public static class ObjectTypes
    {
        public const string Seat = "seat";
        public const string Door = "door";
        public const string Arcade = "arcade";
        public const string Display = "display";
        public const string Container = "container";
        public const string Terminal = "terminal";
        public const string Item = "item";
        public const string NPC = "npc";
        public const string Zone = "zone";
        public const string Waypoint = "waypoint";
    }
}
