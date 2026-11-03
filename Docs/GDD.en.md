# Game Design Document (GDD)

## Royal Flop — Social Casino Texas Hold’em (Android)

> **Working project name:** Royal Flop  
> **Document version:** 1.0  
> **Platform:** Android (API 24+) | **Engine:** Unity 2022.3 LTS | **Language:** C#  
> **Age rating target:** 18+ (Simulated Gambling)

---

## 0. Vision and Design Pillars

### 0.1 Vision Statement

A fast, smooth, social Texas Hold’em mobile game that combines the feel of a real poker table with progression, competition, and social interaction, **without any cash-out functionality or real-money wins or losses**.

### 0.2 Design Pillars

| Pillar | Description | Success metric |
| --- | --- | --- |
| **Speed and responsiveness** | Each mobile hand should take less than 60 seconds. | Average hand duration ≤ 45 seconds |
| **Fairness and transparency** | Shuffling and card dealing are fully server-side. | Zero confirmed cheating reports |
| **Social belonging** | Players connect with friends and their clubs. | ≥ 30% of players have at least one friend |
| **Meaningful progression** | Every session should provide a sense of progress. | ≥ 80% of sessions include XP or a quest |
| **Healthy economy** | Free-to-play players must be able to keep playing. | Average time to bankruptcy for an F2P player ≥ 48 hours |

### 0.3 Target Audience

| Segment | Description |
| --- | --- |
| Age | 18–55 |
| Core profile | Poker enthusiasts; casual to mid-core players |
| Play pattern | 3–5 sessions per day, 10–25 minutes per session |
| Initial markets | Turkey, Brazil, India, Europe, and the Middle East, subject to local laws |

---

## 1. Core Gameplay Loop

### 1.1 Macro Loop

```text
Enter game → Receive Daily Bonus → Lobby → Select table/tournament
      ↑                                                   ↓
Purchase/watch ad ← Low on chips ← Rewards/XP ← Play several hands
```

### 1.2 Stage Details

| Stage | Player behavior | Systems involved | Design goal |
| --- | --- | --- | --- |
| **1. Entry** | Log in as Guest / Google / Facebook | Authentication, Daily Bonus | Reach the lobby in under 10 seconds |
| **2. Lobby** | View tables, quests, and events | Matchmaking, Quests, Events | Clearly communicate the next action |
| **3. Table selection** | Quick Seat or manual selection | Level Gate, Buy-in | Sit at a table in fewer than 3 taps |
| **4. Play** | Fold / Check / Call / Raise / All-in | Game State Machine, Hand Evaluator | Fast decisions with a 15-second timer |
| **5. Reward** | Receive the pot, XP, and quest progress | Economy, Progression | Immediate visual and audio feedback |
| **6. Repeat** | Stay at the same table or switch tables | Retention Hooks | Encourage “one more hand” |

### 1.3 Hand Loop (Micro Loop)

1. **Waiting / Seating** → 2. **Post Blinds** → 3. **Deal Hole Cards** → 4. **Pre-flop Betting** → 5. **Flop** → 6. **Turn** → 7. **River** → 8. **Showdown** → 9. **Payout** → 10. **Cleanup**

> These stages map directly to the **Game State Machine** in Phase 1 (State Pattern and events such as `OnStateChanged` and `OnPotAwarded`).

### 1.4 Retention Hooks

- **Short-term:** Daily Quests, Daily Bonus, Spin Wheel
- **Mid-term:** Level-ups, unlocking new tables, Battle Pass
- **Long-term:** Seasonal Leaderboards, Club Events, cosmetic-item collections

---

## 2. Game Modes

### 2.1 Cash Game (Virtual Chips)

| Feature | Value |
| --- | --- |
| Seats | 2 (Heads-up), 6 (Short-handed), 9 (Full Ring) |
| Buy-in | Minimum 20 to maximum 100 big blinds |
| Entry/exit | Open entry and exit; rebuys allowed |
| Rake (virtual fee) | 3% of each pot, subject to a cap (used as a Chip Sink) |
| Bots | If there are not enough real players after 30 seconds, bots are added |

**Table tiers**

| Tier | Name | Blinds (SB/BB) | Minimum buy-in | Required level |
| --- | --- | --- | ---: | --- |
| 1 | Rookie | 50 / 100 | 2,000 | 1 |
| 2 | Amateur | 250 / 500 | 10,000 | 5 |
| 3 | Pro | 1,000 / 2,000 | 40,000 | 12 |
| 4 | Expert | 5,000 / 10,000 | 200,000 | 20 |
| 5 | High Roller | 25,000 / 50,000 | 1,000,000 | 30 |
| 6 | VIP Elite | 100,000 / 200,000 | 4,000,000 | 40 + VIP |

**Bot AI behavior:** Three profiles are planned: Tight-Aggressive, Loose-Passive, and Balanced. Bots **must not** appear at tables with real players without being clearly labeled. This is a transparency requirement. Bots do not participate in Leaderboards or Leaderboard rewards.

### 2.2 Tournaments

#### A. Sit & Go (SNG)

| Feature | Value |
| --- | --- |
| Start | As soon as all seats are filled (6 or 9 players) |
| Buy-in | 1–100 times the base BB, depending on tier |
| Starting stack | 1,500–5,000 tournament chips, separate from the main chip balance |
| Blind levels | Increase every 3 minutes (Turbo) or 5 minutes (Regular) |
| Prize structure | 6 players: 50/30/20; 9 players: 45/30/25 (places 1–3) |

#### B. Scheduled Tournaments

| Type | Schedule | Capacity | Prize |
| --- | --- | ---: | --- |
| Daily Freeroll | At a fixed time every day | Unlimited | Chips + XP |
| Evening Main Event | Daily at 20:00 UTC | Up to 500 | Chips + cosmetic item |
| Weekend Special | Friday/Saturday | Up to 2,000 | Special prize + Badge |
| Club Championship | Monthly | Club members | Club reward |

**Important legal/product constraint:** Tournament prizes are limited to virtual chips, cosmetic items, or Badges. Cash, gift cards, and real-world goods must never be offered as prizes.

### 2.3 Private Table

| Feature | Description |
| --- | --- |
| Table creation | Choose Blinds, seat count, timer, and access password |
| Invitations | Six-digit code, Deep Link, or friends list |
| Host controls | Remove a player, pause the game, change Blinds |
| Creation cost | Free for up to 3 hours; longer sessions require VIP or a chip fee |
| Rake | 0%, to strengthen the social aspect |
| Bots | Host can enable or disable bots |
| Restriction | Private tables award 50% less XP to reduce abuse (Chip Dumping) |

---

## 3. Economy Design

### 3.1 Economic Principles

1. Chips are **Soft Currency** and cannot be converted into real money.
2. A **Hard Currency** (Diamonds / Gems) is used to buy cosmetic items and the Battle Pass.
3. The economy should allow F2P players to continue playing at lower tiers through ads and bonuses.

### 3.2 Starting Balance

| Item | Amount |
| --- | ---: |
| Welcome Bonus (registration) | **50,000** chips |
| Tutorial reward | +10,000 chips |
| Starting Gems | 20 Gems |
| **Total starting balance** | **60,000 chips + 20 Gems** |

> With 60,000 chips, a player can afford approximately 30 Rookie-table buy-ins, which should be enough for learning.

### 3.3 Daily Bonus

| Day | Chip reward | Additional reward |
| ---: | ---: | --- |
| 1 | 5,000 | — |
| 2 | 7,500 | — |
| 3 | 10,000 | 5 Gems |
| 4 | 15,000 | — |
| 5 | 20,000 | Spin Ticket |
| 6 | 30,000 | — |
| 7 | 50,000 | 20 Gems + random cosmetic item |

- **Streak reset:** Missing a day resets the streak to Day 1. One free **Streak Freeze** is available per month.
- **Hourly Bonus:** 2,000 chips every 4 hours, up to 3 times per day.
- **Lucky Spin:** One free spin per day; prizes range from 1,000 to 100,000 chips. The probability of each prize must be displayed in the UI (the original design identifies this as a Google Play requirement for paid/randomized chance mechanics).

### 3.4 Chip Packages (IAP)

| Package | Price (USD) | Base chips | Bonus | Total chips | Relative value |
| --- | ---: | ---: | ---: | ---: | ---: |
| Starter | $0.99 | 100,000 | — | 100,000 | ×1.0 |
| Small | $2.99 | 330,000 | +10% | 363,000 | ×1.2 |
| Medium | $4.99 | 600,000 | +20% | 720,000 | ×1.4 |
| Large | $9.99 | 1,300,000 | +30% | 1,690,000 | ×1.7 |
| Mega | $19.99 | 2,800,000 | +40% | 3,920,000 | ×2.0 |
| Ultra | $49.99 | 7,500,000 | +50% | 11,250,000 | ×2.3 |
| Whale | $99.99 | 16,000,000 | +60% | 25,600,000 | ×2.6 |

**Special offers**

| Offer | Display condition | Contents |
| --- | --- | --- |
| **First Purchase** | First purchase | Double chips, one time only |
| **Bust-out Offer** | Balance below the minimum buy-in | 500,000-chip package at 50% off, available for 24 hours |
| **Level Up Pack** | Reaching a specified level | Chips + exclusive avatar |
| **Weekend Deal** | Weekend | +10% bonus on all packages |

> Prices should use Google Play Console **Regional Pricing**.

### 3.5 Rewarded Ads for Free Chips

| Placement | Reward | Limit |
| --- | --- | --- |
| Lobby — “Free Chips” | 5,000 chips | 10 times per day, 5-minute interval |
| After Bust (bankruptcy) | 20,000 chips | 3 times per day |
| Double Daily Bonus | ×2 | Once per day |
| Extra Spin | 1 Spin | Twice per day |
| Quest Skip | Instant quest completion | Once per day |

**Suggested ad networks:** AdMob (Mediation), Unity LevelPlay (ironSource), and AppLovin MAX for eCPM optimization.

### 3.6 Economy Sinks and Sources

#### Chip sources

| Source | Estimated share of total inflow |
| --- | ---: |
| Welcome / Tutorial | 3% |
| Daily / Hourly Bonus | 25% |
| Rewarded Ads | 20% |
| Quests and Achievements | 15% |
| Level-up Rewards | 10% |
| Tournament prizes (prize pool from buy-ins) | Neutral (zero-sum) |
| IAP purchases | 27% |

#### Chip sinks

| Sink | Mechanism | Target share |
| --- | --- | ---: |
| **Table Rake** | 3% of the pot (maximum 5 BB) | 40% |
| **Tournament Fee** | 8% of buy-in (chips are removed) | 20% |
| **Cosmetic Shop** | Avatars, tables, and card designs purchased with chips | 15% |
| **Club Donations / Upgrades** | Club upgrades | 8% |
| **Emotes / Gifts** | Gifts sent to friends (with a fee) | 7% |
| **Private Table Creation** | Fee for extended table creation | 5% |
| **VIP Lounge Entry** | Entry to High Roller tables | 5% |

#### Economy health KPIs

| Metric | Target |
| --- | ---: |
| Sink/Source Ratio | 0.85–1.05 |
| Inflation Rate (monthly growth of total chips) | Less than 5% |
| Gini Coefficient of chip distribution | Less than 0.7 |
| Bust Rate (daily bankruptcy percentage) | 15–25% |
| Payer Conversion | 2–4% |
| ARPDAU | $0.15–$0.35 |

**Control tools:** Remote Config for adjusting rewards and Rake without an update; an economy dashboard (Metabase/Grafana); and A/B testing for prices and bonuses.

---

## 4. Progression System

### 4.1 Level and XP

**XP sources**

| Activity | XP |
| --- | ---: |
| Play one Cash hand | 10 |
| Win a hand | +15 |
| Win at Showdown | +10 |
| Strong hand (Flush or better) | +50 to +500 |
| Finish an SNG (Top 3) | 100–300 |
| Complete a Daily Quest | 50–200 |
| Daily Login | 25 |

**XP required for the next level**

```text
XP_required(L) = 100 × L^1.6      (L = current level)
```

| Level | XP required for next level | Approx. cumulative XP |
| --- | ---: | ---: |
| 1 → 2 | 100 | 100 |
| 5 → 6 | 954 | ~2,400 |
| 10 → 11 | 3,981 | ~14,000 |
| 20 → 21 | 12,000 | ~90,000 |
| 30 → 31 | 23,000 | ~280,000 |
| 50 | Initial soft cap | — |

**Level-up rewards:** Chips (`5,000 × Level`), Gems at levels 5, 10, 25, and 50, and an unlock for a new feature.

### 4.2 Table and Feature Unlocks

| Level | Unlock |
| ---: | --- |
| 1 | Rookie table, bot matches |
| 3 | Entry-level Sit & Go |
| 5 | Amateur table, friend system |
| 8 | Full text chat, basic Emotes |
| 10 | Private Table creation |
| 12 | Pro table, club access |
| 15 | Scheduled Tournaments |
| 20 | Expert table |
| 25 | Create a club |
| 30 | High Roller table |
| 40 | VIP Elite (also requires VIP status) |

### 4.3 Achievements

| Category | Example | Reward |
| --- | --- | --- |
| **Hand-based** | “Royal Flush” — make a Royal Flush | 500 Gems... (rare) → 50 Gems |
| **Volume** | Play 1,000 hands | 50,000 chips |
| **Skill** | Win 10 hands in a row | 100,000 chips |
| **Social** | Add 10 friends | Exclusive avatar |
| **Tournament** | Win an SNG for the first time | Badge + 25 Gems |
| **Collection** | Collect 5 cosmetic items | Profile frame |

Each Achievement has three tiers: Bronze, Silver, and Gold.

### 4.4 Quests

| Type | Quantity | Example | Reward |
| --- | ---: | --- | --- |
| **Daily** | 3 | “Play 10 hands”, “Win 3 hands” | 5,000–20,000 chips + XP |
| **Weekly** | 3 | “Enter 2 tournaments” | 100,000 chips + Gems |
| **Event Quest** | Temporary | “Play 50 hands during the Halloween event” | Limited cosmetic item |
| **Club Quest** | Group | “The club plays 5,000 hands in total” | Reward for all members |

---

## 5. Social Features

### 5.1 Friends System

| Feature | Description |
| --- | --- |
| Add friend | By ID, QR Code, or Facebook/Google Contacts (with user consent) |
| Friends list | Online status and the table a friend is playing at |
| Join Table | Join a friend's table directly if a seat is available |
| Chip gifts | Up to 5 gifts per day (1,000 chips each); received gifts are subject to a cap |
| Block / Report | Block users and report misconduct |
| Limit | Maximum 200 friends |

### 5.2 In-game Chat

| Type | Feature |
| --- | --- |
| **Table Chat** | Text chat with a profanity filter |
| **Quick Chat** | Predefined multilingual phrases |
| **Emotes** | Short animations; some may be paid |
| **Private Message** | Direct messages between friends |
| **Club Chat** | Group chat for club members |

**Safety:** Automated filtering, user reports, temporary mute, moderation logs for review, and an option to disable chat in settings.

### 5.3 Clubs / Groups

| Feature | Value |
| --- | --- |
| Initial capacity | 30 members (upgradeable to 100) |
| Roles | Leader, Co-Leader, Member |
| Creation requirement | Level 25 + 500,000-chip fee |
| **Club Level** | Increases through members’ accumulated XP |
| **Club Perks** | Daily Bonus increase (up to +20%), exclusive Emote |
| **Club Vault** | Chip donation pool for upgrades (a Sink) |
| **Club Wars** | Weekly competition between clubs based on total XP |
| **Private Club Table** | Table reserved for club members |

### 5.4 Leaderboards

| Type | Metric | Period |
| --- | --- | --- |
| **Global Weekly** | Most chips won | Weekly |
| **Friends** | Comparison among friends | Weekly |
| **Tournament Points** | Tournament points | Monthly (Season) |
| **Club Ranking** | Club XP | Weekly |
| **Biggest Pot / Best Hand** | Records | All-time |

End-of-season rewards: chips, exclusive cosmetic items, and Badges. **Bots are excluded from Leaderboards.**

---

## 6. Monetization Strategy

### 6.1 Virtual Chip Sales

As specified in Section 3.4. Target: 60% of IAP revenue from Special Offers and mid-tier packages.

### 6.2 Cosmetic Items

| Category | Example | Currency | Example price |
| --- | --- | --- | --- |
| **Avatar / Frame** | Themed avatars | Chips / Gems | 50,000 chips – 200 Gems |
| **Table Theme** | Classic green felt, Neon, Royal | Gems | 150–500 Gems |
| **Card Back / Deck** | Golden deck, minimalist deck | Gems | 100–400 Gems |
| **Chip Style** | Special chip designs | Chips | 100,000+ |
| **Emotes** | Animation pack | Gems | 80–200 Gems |
| **Dealer Skin** | Virtual dealer | Gems | 300 Gems |

> **No Pay-to-Win:** Cosmetic items must not provide gameplay advantages (for example, revealing opponents’ information).

### 6.3 Battle Pass (Season Pass)

| Feature | Free Track | Premium Track ($4.99) | Premium+ ($9.99) |
| --- | --- | --- | --- |
| Tier count | 50 | 50 | 50 + 10 tier skips |
| Rewards | Chips, small amounts of Gems | More chips, exclusive cosmetics, Gems | Premium rewards + legendary item |
| Season duration | 60 days | 60 days | 60 days |
| Progression | Seasonal XP from play and Quests | Same | Same |

- Tiers can be purchased with Gems.
- Each season has a theme (for example, “Vegas Nights”).
- Seasonal items will not be available in the shop during the following season (controlled scarcity).

### 6.4 Advertising

| Type | Placement | Rule |
| --- | --- | --- |
| **Rewarded Video** | Lobby, Bust, Bonus | Always optional |
| **Interstitial** | After leaving a table or finishing a tournament, never during a hand | Maximum once every 5 minutes and 4 times per session |
| **Banner** | Not used during gameplay | — |
| **No Ads Pack** | $4.99 or VIP | Removes Interstitial ads; Rewarded ads remain |

### 6.5 VIP Subscription (Optional)

| Tier | Monthly price | Benefits |
| --- | ---: | --- |
| VIP Silver | $4.99 | +20% Daily Bonus, removes Interstitial ads |
| VIP Gold | $9.99 | +40% Bonus, exclusive table, special Emote |

### 6.6 Revenue Forecast (Initial Assumptions)

| Metric | Target |
| --- | ---: |
| Day-1 Retention | ≥ 40% |
| Day-7 Retention | ≥ 18% |
| Day-30 Retention | ≥ 8% |
| Payer Conversion | 2–4% |
| ARPPU | $15–40 |
| IAP / Ads revenue share | 70% / 30% |

### 6.7 Compliance Requirements

| Requirement | Action |
| --- | --- |
| **No Cash-out** | No way to convert Chips/Gems into money or real-world goods |
| **Disclaimer** | Display: “This game is for entertainment. You cannot win real money. Success in a virtual game does not mean success in real-world gambling.” |
| **Age rating** | Complete IARC with the Simulated Gambling option; apply an 18+ restriction |
| **Age Gate** | Confirm age on first launch |
| **Privacy** | Privacy Policy, GDPR, CCPA, and Data Safety Form |
| **Loot Boxes** | Display odds/probabilities |
| **Responsible Play** | Play-time reminders, Self-Exclusion, and spending limits |
| **Geographic restrictions** | Review each country's laws and do not release in prohibited regions |

> **Recommendation:** Before launch, review Google Play’s “Gambling, Games, and Contests” policy and the laws of each target market with a legal adviser. This document is not legal advice.

---

## 7. Technical Architecture

### 7.1 High-Level Client–Server Architecture

```text
┌────────────────────────┐         ┌──────────────────────────────┐
│ Unity Client (Android) │         │          Backend             │
│ ─ View (UI, Animations)│◄──WSS──►│ ─ Authoritative Game Server  │
│ ─ Controller          │         │ ─ Table Manager              │
│ ─ Model (Pure C# Core)│◄──HTTPS►│ ─ Matchmaking                │
│ ─ Network Adapter     │         │ ─ Tournament Engine          │
└────────────────────────┘         │ Meta Services (REST)         │
                                   │ ─ Auth, Profile, Shop, IAP   │
                                   │ ─ Social, Quests, Economy    │
                                   └──────────┬───────────────────┘
                                              │
                           ┌──────────────────┼──────────────────┐
                        PostgreSQL           Redis          Analytics/Logs
                     (Persistent data)  (Session, Cache)  (BigQuery / ClickHouse)
```

**Key principles**

1. **Server Authoritative:** Shuffling, dealing, winner calculation, and pot settlement happen only on the server.
2. **Shared Core Library:** Pure C# logic (`Card`, `Deck`, `HandEvaluator`, `StateMachine`) is packaged as a `.NET Standard 2.1` Library and shared by the client (preview/UI) and server. This is what Phase 1 builds.
3. **Dumb Client:** The client only renders the game and sends actions; every action is validated.
4. **Event-Driven:** Messages are modeled as events such as `PlayerActed`, `CardsDealt`, and `PotAwarded`.
5. **Secure shuffle:** Use `RandomNumberGenerator` (CSPRNG) with Fisher–Yates, not `System.Random`.

### 7.2 Networking Choice: Photon PUN2 or a Dedicated Server?

| Criterion | Photon PUN2 | Photon Fusion / Quantum | Dedicated server (e.g., Nakama or .NET) |
| --- | --- | --- | --- |
| Development speed | ★★★★★ | ★★★ | ★★ |
| Security (anti-cheat) | ★★ (Master Client) | ★★★★ | ★★★★★ |
| Economy/game-logic control | Weak | Medium | Full |
| Initial cost | Low (CCU-based) | Medium | Infrastructure + maintenance |
| Scalability | Good (within purchased CCU) | Good | Very good with proper design |
| Support status | **Maintenance Mode** (bug fixes only) | Active | Depends on choice |
| Poker suitability | MVP only; not recommended for production | Partial | **Best fit** |

**Important caveats**

- Photon PUN2 uses a Master Client model, meaning one client acts as the host for game logic. In poker, client-side shuffling or winner calculation creates opportunities for cheating and chip manipulation. If PUN2 is used, sensitive logic must be moved to your own server.
- Photon has described PUN2 as being in maintenance mode and recommends Fusion for new projects. Verify Photon’s current product status before making a final decision.

**Proposed phased / hybrid architecture**

| Phase | Approach |
| --- | --- |
| **MVP (Months 1–3)** | Offline game against bots with complete Core Logic (current Phase 1). For multiplayer testing, use PUN2 only as a transport layer. |
| **Soft Launch** | Independent authoritative server (Nakama or .NET + WebSocket) for table logic; Photon/Nakama for realtime communication. |
| **Production** | Move all game logic to the Game Server; if retained, Photon is used only for relay/matchmaking. |

To keep the networking layer replaceable, define an `INetworkService` interface (Dependency Inversion):

```csharp
public interface INetworkService
{
    event Action<TableSnapshot> OnTableUpdated;
    Task ConnectAsync(string token);
    Task SendActionAsync(PlayerAction action);
}
```

### 7.3 Database Structure

**Primary database:** PostgreSQL | **Cache/session:** Redis | **Analytics:** BigQuery or ClickHouse

#### Main tables

**`players`**

| Field | Type | Description |
| --- | --- | --- |
| id | UUID (PK) | Player identifier |
| display_name | VARCHAR(24) | Display name |
| auth_provider / auth_id | VARCHAR | Google / Guest / etc. |
| level / xp | INT / BIGINT | Progression |
| chips | BIGINT | Chip balance |
| gems | INT | Gem balance |
| vip_tier | SMALLINT | VIP tier |
| avatar_id / frame_id | INT | Appearance |
| club_id | UUID (FK, nullable) | Club |
| country_code | CHAR(2) | Country |
| birth_confirmed_18 | BOOL | Age confirmation |
| created_at / last_login_at | TIMESTAMPTZ | Timestamps |
| is_banned | BOOL | Ban status |

**`wallet_transactions`** (immutable, append-only ledger)

| Field | Type | Description |
| --- | --- | --- |
| id | BIGSERIAL | Identifier |
| player_id | UUID (FK) | Player |
| currency | ENUM(chips, gems) | Currency type |
| amount | BIGINT | Positive/negative amount |
| balance_after | BIGINT | Balance after transaction |
| reason | VARCHAR | `hand_win`, `iap`, `rake`, etc. |
| ref_id | VARCHAR | Hand/purchase identifier |
| created_at | TIMESTAMPTZ | Timestamp |

> **Rule:** Never edit a balance directly; update it only through the ledger and an atomic transaction.

**Other tables**

| Table | Purpose |
| --- | --- |
| `purchases` | IAP receipts, validation status, Store Transaction ID |
| `hand_history` | Compressed JSON history for each hand, used for audit and replay |
| `tournaments` / `tournament_entries` | Settings, participants, and rankings |
| `friendships` | `(player_a, player_b, status, created_at)` |
| `clubs` / `club_members` | Clubs, roles, and XP contribution |
| `inventory` | Owned cosmetic items |
| `quests_progress` | Quest and Achievement progress |
| `battle_pass_progress` | Tier and Premium status |
| `daily_bonus_state` | Streak and last claim |
| `reports` / `chat_logs` | Reports and moderation |
| `leaderboards` | Redis Sorted Set for speed; snapshots in the database |

### 7.4 Security and Anti-Cheat

| Threat | Mitigation |
| --- | --- |
| Card cheating | Server-side CSPRNG shuffle; send each player only their own Hole Cards |
| Chip manipulation | Server ledger and atomic transactions |
| Fake IAP | Server-side receipt validation with Google Play Developer API |
| Collusion / Chip Dumping | Pattern analysis in `hand_history`; private-table restrictions |
| External bots | Rate limiting, Play Integrity API, behavioral analysis |
| Replay attack | Short-lived token + a nonce for each action |
| Reverse engineering | IL2CPP, obfuscation, and keeping sensitive logic off the client |

### 7.5 Android Performance Considerations

- **GC-free Update:** Use Object Pooling for cards, chips, and events; avoid LINQ and string concatenation in hot loops.
- **Struct instead of Class** for `Card` (for example, a `byte` or `readonly struct`) and a Hand Evaluator based on Bitmasks / Lookup Tables.
- **Addressables** for loading cosmetic items.
- **Texture Atlas and ASTC Compression;** target an installation size below 150 MB.
- **FPS target:** 60 on mid-range devices; 30 minimum on low-end devices.
- **Network:** Binary protocol (MessagePack/Protobuf), automatic reconnect, and table-state recovery.

### 7.6 Analytics and LiveOps

| Tool | Purpose |
| --- | --- |
| Firebase Analytics + Crashlytics | Events and crash reporting |
| Remote Config | Adjust the economy without an app update |
| A/B Testing | Prices, bonuses, and UI |
| Push Notifications (FCM) | Bonus and tournament reminders |
| Dashboard | Retention, ARPDAU, and Economy KPIs |

---

## 8. Roadmap

| Phase | Scope | Estimated duration |
| --- | --- | --- |
| **Phase 1 (current)** | Card System, Hand Evaluator, State Machine, basic Bot AI, Unit Tests | 4–6 weeks |
| **Phase 2** | UI/UX, animations, offline bot game, local economy | 8 weeks |
| **Phase 3** | Networking, Auth, Matchmaking, online Cash Game | 8–10 weeks |
| **Phase 4** | Tournaments, Social, Clubs, IAP, advertising | 8 weeks |
| **Phase 5** | Soft Launch, economy tuning, Battle Pass, LiveOps | 6–8 weeks |
| **Global Launch** | Marketing and scaling | — |

---

## 9. Risks and Mitigations

| Risk | Likelihood | Mitigation |
| --- | --- | --- |
| Rejection by Google Play | Medium | Carefully follow gambling-related policies, provide the Disclaimer, and obtain the correct IARC rating |
| Multiplayer cheating | High | Use an authoritative server from the beginning |
| Economy inflation | High | Diverse Sinks, Remote Config, and KPI monitoring |
| Strong market competition | High | Differentiate through Clubs, speed, and UX quality |
| Server costs | Medium | Autoscaling and protocol optimization |
| PUN2 dependency | Medium | Abstraction through `INetworkService` |

---

## Document Notes

This document records the current design proposal and its initial targets. Metrics, prices, platform requirements, third-party product status, and legal interpretations must be validated before implementation or release. The figures are design assumptions, not verified market benchmarks or guarantees.
