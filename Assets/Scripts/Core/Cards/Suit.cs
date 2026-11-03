namespace PokerGame.Core.Cards
{
    /// <summary>
    /// The four suits of a standard French-suited deck.
    /// Backed by <see cref="byte"/> to keep <see cref="Card"/> tiny.
    /// Values are contiguous (0-3) so they can be used to compute a card index.
    /// </summary>
    public enum Suit : byte
    {
        /// <summary>Clubs (♣).</summary>
        Clubs = 0,

        /// <summary>Diamonds (♦).</summary>
        Diamonds = 1,

        /// <summary>Hearts (♥).</summary>
        Hearts = 2,

        /// <summary>Spades (♠).</summary>
        Spades = 3
    }
}