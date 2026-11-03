using System;
using System.Security.Cryptography;

namespace PokerGame.Core.Cards
{
    /// <summary>
    /// Cryptographically secure <see cref="IRandomSource"/> backed by
    /// <see cref="RandomNumberGenerator"/> (OS entropy: /dev/urandom on Android and Linux).
    /// <para>
    /// Uses rejection sampling to avoid modulo bias, so every value in the range
    /// is exactly equally likely.
    /// </para>
    /// </summary>
    public sealed class CryptoRandomSource : IRandomSource, IDisposable
    {
        private static readonly Lazy<CryptoRandomSource> _shared =
            new Lazy<CryptoRandomSource>(() => new CryptoRandomSource());

        private readonly RandomNumberGenerator _rng;

        /// <summary>
        /// A process-wide shared instance. Never dispose it.
        /// </summary>
        public static CryptoRandomSource Shared => _shared.Value;

        /// <summary>Creates a new source with its own generator.</summary>
        public CryptoRandomSource()
        {
            _rng = RandomNumberGenerator.Create();
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxExclusive"/> is not positive.</exception>
        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxExclusive), maxExclusive, "Upper bound must be positive.");
            }

            if (maxExclusive == 1)
            {
                return 0;
            }

            uint range = (uint)maxExclusive;

            // Largest multiple of 'range' that fits in uint. Values at or above it
            // are rejected, otherwise the lower residues would be slightly more likely.
            uint limit = uint.MaxValue - (uint.MaxValue % range);

            Span<byte> buffer = stackalloc byte[sizeof(uint)];
            uint value;

            do
            {
                _rng.GetBytes(buffer);
                value = BitConverter.ToUInt32(buffer);
            }
            while (value >= limit);

            return (int)(value % range);
        }

        /// <summary>Releases the underlying generator.</summary>
        public void Dispose()
        {
            _rng.Dispose();
        }
    }
}