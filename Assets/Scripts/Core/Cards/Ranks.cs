namespace PokerGame.Core.Cards
{
    /// <summary>
    /// Card ranks. The numeric value equals the pip value (2-10, J=11, Q=12, K=13),
    /// and Ace is high (14). The wheel straight (A-2-3-4-5) must be handled
    /// by the hand evaluator, not by this enum.
    /// <para>
    /// <c>default(Rank)</c> is 0, which is intentionally NOT a defined member,
    /// so an uninitialized <see cref="Card"/> is detectable as invalid.
    /// </para>
    /// </summary>
    public enum Rank : byte
    {
        /// <summary>Two.</summary>
        Two = 2,
        /// <summary>Three.</summary>
        Three = 3,
        /// <summary>Four.</summary>
        Four = 4,
        /// <summary>Five.</summary>
        Five = 5,
        /// <summary>Six.</summary>
        Six = 6,
        /// <summary>Seven.</summary>
        Seven = 7,
        /// <summary>Eight.</summary>
        Eight = 8,
        /// <summary>Nine.</summary>
        Nine = 9,
        /// <summary>Ten.</summary>
        Ten = 10,
        /// <summary>Jack.</summary>
        Jack = 11,
        /// <summary>Queen.</summary>
        Queen = 12,
        /// <summary>King.</summary>
        King = 13,
        /// <summary>Ace (high).</summary>
        Ace = 14
    }
}