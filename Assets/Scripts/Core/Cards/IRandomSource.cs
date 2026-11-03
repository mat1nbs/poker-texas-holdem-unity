namespace PokerGame.Core.Cards
{
    /// <summary>
    /// Abstraction over a source of random integers (Dependency Inversion).
    /// Production code uses <see cref="CryptoRandomSource"/>; tests inject a deterministic fake.
    /// </summary>
    public interface IRandomSource
    {
        /// <summary>
        /// Returns a uniformly distributed integer in [0, <paramref name="maxExclusive"/>).
        /// </summary>
        /// <param name="maxExclusive">Exclusive upper bound; must be greater than zero.</param>
        int NextInt(int maxExclusive);
    }
}