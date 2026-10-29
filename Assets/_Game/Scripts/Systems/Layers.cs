namespace ExitClause.Systems
{
    /// <summary>Physics layer indices. The Editor project setup names these layers in the TagManager.</summary>
    public static class Layers
    {
        public const int Default = 0;
        public const int IgnoreRaycast = 2;
        public const int Player = 8;
        public const int Character = 9;   // enemies, civilians
        public const int Vehicle = 10;
        public const int Props = 11;
        public const int Trigger = 12;

        public const int PlayerMask = 1 << Player;
        public const int CharacterMask = 1 << Character;
        public const int VehicleMask = 1 << Vehicle;
        /// <summary>What blocks sight lines and the camera.</summary>
        public const int EnvironmentMask = (1 << Default) | (1 << Vehicle) | (1 << Props);
    }
}
