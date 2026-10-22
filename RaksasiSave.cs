using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RaksasiSaveEditor
{
    public enum FieldType
    {
        Int,
        Float,
        Bool01
    }

    /// <summary>
    /// Describes one ["attrs"] field: its key, value type, category (used as
    /// the tab it lives on), and an example value taken from the PC save
    /// editor author's confirmed-safe list.
    /// </summary>
    public class RaksasiField
    {
        public string Key { get; }
        public FieldType Type { get; }
        public string Category { get; }
        public string Example { get; }
        public string Note { get; }

        public RaksasiField(string key, FieldType type, string category, string example, string note = "")
        {
            Key = key;
            Type = type;
            Category = category;
            Example = example;
            Note = note;
        }
    }

    /// <summary>
    /// The full known ["attrs"] field table, in the same order the PC save
    /// editor author listed them, grouped into categories. Categories double
    /// as the editor's tab names.
    /// </summary>
    public static class RaksasiFields
    {
        public static readonly List<string> CategoryOrder = new List<string>
        {
            "Resources", "Vitals", "Recovery", "Offense", "Defense", "Movement", "Talents", "Meta"
        };

        public static readonly List<RaksasiField> All = new List<RaksasiField>
        {
            // -- Resources / currency ------------------------------------------------
            new RaksasiField("MONEY",      FieldType.Int,  "Resources", "60000"),
            new RaksasiField("CUR_SOUL",   FieldType.Int,  "Resources", "9430"),
            new RaksasiField("KEY",        FieldType.Int,  "Resources", "254", "Dungeon keys"),
            new RaksasiField("MASTER_KEY", FieldType.Bool01, "Resources", "1"),

            // -- Core vitals ------------------------------------------------------------
            new RaksasiField("HP",            FieldType.Int, "Vitals", "8000000"),
            new RaksasiField("MAX_HP_RAW",    FieldType.Int, "Vitals", "8000000"),
            new RaksasiField("SP",            FieldType.Int, "Vitals", "8000000"),
            new RaksasiField("MAX_SP",        FieldType.Int, "Vitals", "8000000"),
            new RaksasiField("STAMINA",       FieldType.Int, "Vitals", "8000000"),
            new RaksasiField("MAX_STAMINA",   FieldType.Int, "Vitals", "8000000"),
            new RaksasiField("ARMOR",         FieldType.Int, "Vitals", "8000000"),
            new RaksasiField("MAX_ARMOR_RAW", FieldType.Int, "Vitals", "8000000"),
            new RaksasiField("SHIELD",        FieldType.Int, "Vitals", "15"),

            // -- Recovery / regen rates ---------------------------------------------------
            new RaksasiField("SP_RCV_RAW",         FieldType.Int,   "Recovery", "2000"),
            new RaksasiField("SP_RCV_FACTOR",      FieldType.Float, "Recovery", "2"),
            new RaksasiField("STAMINA_RCV",        FieldType.Int,   "Recovery", "25000"),
            new RaksasiField("HEAL_FACTOR",        FieldType.Float, "Recovery", "-0.5", "Can be negative"),
            new RaksasiField("ITEM_HEAL_FACTOR",   FieldType.Float, "Recovery", "1"),
            new RaksasiField("MANA_SHIELD_FACTOR", FieldType.Float, "Recovery", "1"),

            // -- Combat offense --------------------------------------------------------------
            new RaksasiField("ATTACK_RAW",        FieldType.Int,   "Offense", "3000"),
            new RaksasiField("ATTACK_FACTOR",     FieldType.Float, "Offense", "6000.1"),
            new RaksasiField("COLD_ATK",          FieldType.Int,   "Offense", "10"),
            new RaksasiField("FIRE_ATK",          FieldType.Int,   "Offense", "90"),
            new RaksasiField("ACID_ATK",          FieldType.Int,   "Offense", "80"),
            new RaksasiField("THUNDER_ATK",       FieldType.Int,   "Offense", "55"),
            new RaksasiField("BLEED_ATK",         FieldType.Int,   "Offense", "10"),
            new RaksasiField("BREAK_HIT_DMG_INC", FieldType.Float, "Offense", "1"),
            new RaksasiField("BREAK_ATK_DMG_INC", FieldType.Float, "Offense", "0.2"),
            new RaksasiField("RIPOSTE_DMG_INC",   FieldType.Float, "Offense", "0.5"),
            new RaksasiField("DAMAGE_INC",        FieldType.Float, "Offense", "0.8"),
            new RaksasiField("DAMAGE_DEC",        FieldType.Float, "Offense", "0.05", "Damage taken multiplier"),
            new RaksasiField("ELITE_RATE",        FieldType.Int,   "Offense", "35"),

            // -- Defense / status immunity ------------------------------------------------------
            new RaksasiField("DODGE_RAW",            FieldType.Int,    "Defense", "18"),
            new RaksasiField("DODGE_FACTOR",         FieldType.Float,  "Defense", "3.3"),
            new RaksasiField("ANTI_SLOW",            FieldType.Bool01, "Defense", "1"),
            new RaksasiField("ANTI_CHAOS",           FieldType.Bool01, "Defense", "1"),
            new RaksasiField("ANTI_ACID",            FieldType.Bool01, "Defense", "1"),
            new RaksasiField("ANTI_CHARM",           FieldType.Bool01, "Defense", "1"),
            new RaksasiField("ANTI_PARALYSIS",       FieldType.Bool01, "Defense", "1"),
            new RaksasiField("ANTI_BLEED",           FieldType.Bool01, "Defense", "1"),
            new RaksasiField("ANTI_BURN",            FieldType.Bool01, "Defense", "1"),
            new RaksasiField("CURSE_IMMUNITY",       FieldType.Bool01, "Defense", "1"),
            new RaksasiField("DEAD_PROTECT",         FieldType.Bool01, "Defense", "1"),
            new RaksasiField("ENDURE",               FieldType.Bool01, "Defense", "1"),
            new RaksasiField("ST_NO_RECV_KNOCKDOWN", FieldType.Bool01, "Defense", "1"),

            // -- Movement / utility ----------------------------------------------------------------
            new RaksasiField("RUN_SPEED_RAW", FieldType.Int,   "Movement", "68"),
            new RaksasiField("RUN_COST",      FieldType.Int,   "Movement", "10"),
            new RaksasiField("SPEED",         FieldType.Int,   "Movement", "138"),
            new RaksasiField("ODOMETER",      FieldType.Float, "Movement", "3703.1498881687", "High precision"),
            new RaksasiField("SIGHT",         FieldType.Int,   "Movement", "90"),
            new RaksasiField("NO_COLLIDE",    FieldType.Int,   "Movement", "0", "Untested at 1"),
            new RaksasiField("ST_MOV_TGR",    FieldType.Int,   "Movement", "3"),

            // -- Talents / one-shot unlock toggles ----------------------------------------------------
            new RaksasiField("BRAVE_BADGE",                FieldType.Bool01, "Talents", "1"),
            new RaksasiField("MIND_EYE",                   FieldType.Bool01, "Talents", "1"),
            new RaksasiField("RESTOCK",                    FieldType.Bool01, "Talents", "1"),
            new RaksasiField("TALENT_HUGE_BAG",             FieldType.Bool01, "Talents", "1"),
            new RaksasiField("TALENT_EXTRA_SHIELD_WEAPON", FieldType.Bool01, "Talents", "1"),
            new RaksasiField("TALENT_SHIELD_MASTER",       FieldType.Bool01, "Talents", "1"),
            new RaksasiField("LANTERN_KEEPER_NO_COST",     FieldType.Bool01, "Talents", "1"),
            new RaksasiField("RESURRECTION_DOLL",          FieldType.Bool01, "Talents", "1"),
            new RaksasiField("RESURRECTION_HERB",          FieldType.Bool01, "Talents", "1"),
            new RaksasiField("RESURRECTION_SOUL_PENDANT",  FieldType.Bool01, "Talents", "1"),
            new RaksasiField("EXTRA_WEAPON",               FieldType.Bool01, "Talents", "1"),

            // -- Meta / counters ------------------------------------------------------------------------
            new RaksasiField("SUMMON_COUNT",           FieldType.Int,   "Meta", "6"),
            new RaksasiField("KILL_COUNT",             FieldType.Int,   "Meta", "65"),
            new RaksasiField("SHOP_KEEPER_SPAWN_RATE", FieldType.Int,   "Meta", "1000"),
            new RaksasiField("MONEY_LOOT_DOUBLE_RATE", FieldType.Float, "Meta", "0.5"),
            new RaksasiField("BONUS_SOUL_RAW",         FieldType.Int,   "Meta", "0"),
            new RaksasiField("RESURRECTION_EVIL",      FieldType.Int,   "Meta", "5", "Not on the PC editor's list"),
        };

        public static IEnumerable<RaksasiField> InCategory(string category) =>
            All.Where(f => f.Category == category);
    }

    /// <summary>
    /// Loads, edits and re-checksums a Devil Slayer Raksasi (PS4/PC) save.
    ///
    /// File layout:
    ///   bytes[0:32]  = 32 ASCII hex chars = checksum of bytes[32:]
    ///   bytes[32:]   = Lua "return { ... }" text (outer table's [1]/[2]/[4]/[5]
    ///                  entries are each *strings* containing another
    ///                  serialized Lua chunk - the player's stats live inside
    ///                  the chunk at outer key [1], under d[9].attrs / d[9].baseline)
    ///
    /// Checksum: SHA-256(body + fixed 60-byte key) with a length-field quirk
    /// (the padding's bit-length only counts `body`, not the appended key),
    /// then the 32-byte digest is folded to 16 bytes via digest[i] ^ digest[i+16],
    /// and hex-encoded. Reverse engineered from slua.dll's
    /// get_hash_string -> get_hash_bytes -> calc_sha_256 (arnan_security.c).
    /// </summary>
    public class RaksasiSave
    {
        public byte[] Data { get; private set; }

        private static readonly byte[] SaveKey =
            Encoding.ASCII.GetBytes("Hi! We're GlassesCat team. Please don't crack our games. :)\0");

        private static readonly uint[] K =
        {
            0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
            0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
            0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
            0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
            0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
            0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
            0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
            0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2,
        };

        private static readonly uint[] H0 =
        {
            0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19
        };

        public static RaksasiSave Load(string path)
        {
            return new RaksasiSave { Data = File.ReadAllBytes(path) };
        }

        public void SaveToFile(string path)
        {
            FixChecksum();
            File.WriteAllBytes(path, Data);
        }

        public bool VerifyChecksum()
        {
            if (Data == null || Data.Length < 32)
            {
                return false;
            }
            string stored = Encoding.ASCII.GetString(Data, 0, 32);
            byte[] body = new byte[Data.Length - 32];
            Buffer.BlockCopy(Data, 32, body, 0, body.Length);
            return string.Equals(stored, ComputeChecksum(body), StringComparison.Ordinal);
        }

        public void FixChecksum()
        {
            byte[] body = new byte[Data.Length - 32];
            Buffer.BlockCopy(Data, 32, body, 0, body.Length);
            byte[] hashBytes = Encoding.ASCII.GetBytes(ComputeChecksum(body));
            Buffer.BlockCopy(hashBytes, 0, Data, 0, 32);
        }

        public static string ComputeChecksum(byte[] body)
        {
            byte[] msg = new byte[body.Length + SaveKey.Length];
            Buffer.BlockCopy(body, 0, msg, 0, body.Length);
            Buffer.BlockCopy(SaveKey, 0, msg, body.Length, SaveKey.Length);

            ulong bitLen = (ulong)body.Length * 8; // quirk: excludes the appended key's length

            int padLen = msg.Length + 1;
            while (padLen % 64 != 56)
            {
                padLen++;
            }
            padLen += 8;

            byte[] padded = new byte[padLen];
            Buffer.BlockCopy(msg, 0, padded, 0, msg.Length);
            padded[msg.Length] = 0x80;

            byte[] lenBytes = BitConverter.GetBytes(bitLen);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lenBytes);
            }
            Buffer.BlockCopy(lenBytes, 0, padded, padLen - 8, 8);

            uint[] h = (uint[])H0.Clone();
            for (int off = 0; off < padded.Length; off += 64)
            {
                h = Compress(h, padded, off);
            }

            byte[] digest = new byte[32];
            for (int i = 0; i < 8; i++)
            {
                byte[] b = BitConverter.GetBytes(h[i]);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(b);
                }
                Buffer.BlockCopy(b, 0, digest, i * 4, 4);
            }

            var sb = new StringBuilder(32);
            for (int i = 0; i < 16; i++)
            {
                sb.Append(((byte)(digest[i] ^ digest[i + 16])).ToString("x2"));
            }
            return sb.ToString();
        }

        private static uint RRot(uint x, int n) => (x >> n) | (x << (32 - n));

        private static uint[] Compress(uint[] hIn, byte[] data, int offset)
        {
            uint[] w = new uint[64];
            for (int i = 0; i < 16; i++)
            {
                int p = offset + i * 4;
                w[i] = ((uint)data[p] << 24) | ((uint)data[p + 1] << 16) | ((uint)data[p + 2] << 8) | data[p + 3];
            }
            for (int i = 16; i < 64; i++)
            {
                uint s0 = RRot(w[i - 15], 7) ^ RRot(w[i - 15], 18) ^ (w[i - 15] >> 3);
                uint s1 = RRot(w[i - 2], 17) ^ RRot(w[i - 2], 19) ^ (w[i - 2] >> 10);
                w[i] = w[i - 16] + s0 + w[i - 7] + s1;
            }

            uint a = hIn[0], b = hIn[1], c = hIn[2], d = hIn[3], e = hIn[4], f = hIn[5], g = hIn[6], hh = hIn[7];
            for (int i = 0; i < 64; i++)
            {
                uint s1 = RRot(e, 6) ^ RRot(e, 11) ^ RRot(e, 25);
                uint ch = (e & f) ^ (~e & g);
                uint t1 = hh + s1 + ch + K[i] + w[i];
                uint s0 = RRot(a, 2) ^ RRot(a, 13) ^ RRot(a, 22);
                uint maj = (a & b) ^ (a & c) ^ (b & c);
                uint t2 = s0 + maj;
                hh = g; g = f; f = e; e = d + t1; d = c; c = b; b = a; a = t1 + t2;
            }

            return new[]
            {
                hIn[0] + a, hIn[1] + b, hIn[2] + c, hIn[3] + d,
                hIn[4] + e, hIn[5] + f, hIn[6] + g, hIn[7] + hh
            };
        }

        // ---------------------------------------------------------------------------
        // Block-scoped attrs read/write - offset-safe, insert-if-missing
        // ---------------------------------------------------------------------------
        // Fields are stored as: [\"KEY\"]=VALUE,\<newline>  (literal backslash-quote,
        // since the whole chunk is itself embedded as a Lua string). VALUE can be
        // an int, a float, or negative - matched up to the next comma so any
        // width/precision works.
        //
        // The attrs table is SPARSE: an early/fresh save only lists fields that
        // have actually been touched. SetAttr() inserts a field that doesn't
        // exist yet instead of failing.
        //
        // There are two unrelated "attrs"-named tables in the file: the real
        // player entity's, and a minor companion/pet sub-entity. Individual
        // field names inside attrs/baseline aren't reliable disambiguators -
        // they vary by character/save state (even DODGE_RAW and ATTACK_RAW
        // have been observed missing). The one structural fact that holds is
        // that the PLAYER's entity has BOTH an "attrs" table AND a "baseline"
        // table as siblings; the companion only ever has "attrs" alone. So we
        // find the entity block that contains both, not any specific field.

        private static Regex FieldRegex(string key) =>
            new Regex(@"\[\\""" + Regex.Escape(key) + @"\\""\]=(-?[0-9]+\.?[0-9]*)(,)");

        private (int Start, int End) FindPlayerEntityBlock()
        {
            var baselineBlocks = FindAllBlocks("baseline");
            foreach (var attrs in FindAllBlocks("attrs"))
            {
                var entity = FindEnclosingNumericBlock(attrs.Start);
                bool hasBaseline = baselineBlocks.Any(b => b.Start >= entity.Start && b.End <= entity.End);
                if (hasBaseline)
                {
                    return entity;
                }
            }

            throw new InvalidOperationException(
                "Could not find the player's entity block (an entity with both an \"attrs\" table " +
                "and a \"baseline\" table). Is this really a Raksasi save with a saved character?");
        }

        public (int Start, int End) FindPlayerAttrsBlock()
        {
            var entity = FindPlayerEntityBlock();
            foreach (var block in FindAllBlocks("attrs"))
            {
                if (block.Start >= entity.Start && block.End <= entity.End)
                {
                    return block;
                }
            }
            throw new InvalidOperationException("No \"attrs\" block found inside the player's entity block.");
        }

        public (int Start, int End) FindPlayerBaselineBlock()
        {
            var entity = FindPlayerEntityBlock();
            foreach (var block in FindAllBlocks("baseline"))
            {
                if (block.Start >= entity.Start && block.End <= entity.End)
                {
                    return block;
                }
            }
            throw new InvalidOperationException("No \"baseline\" block found inside the player's entity block.");
        }

        /// <summary>
        /// Finds the smallest [&lt;digits&gt;]={...} block (an entity entry like
        /// d[9]={...}) that contains the given byte position. Used to scope
        /// "baseline" lookups to the same entity as "attrs" regardless of
        /// which one the game happened to write first.
        /// </summary>
        private (int Start, int End) FindEnclosingNumericBlock(int position)
        {
            string s = Encoding.ASCII.GetString(Data);
            var re = new Regex(@"\[\d+\]=\{");
            (int Start, int End)? best = null;

            foreach (Match m in re.Matches(s))
            {
                int contentStart = m.Index + m.Length;
                int depth = 1;
                int j = contentStart;
                while (depth > 0 && j < Data.Length)
                {
                    if (Data[j] == (byte)'{') depth++;
                    else if (Data[j] == (byte)'}') depth--;
                    j++;
                }
                int contentEnd = j - 1;

                if (contentStart <= position && position <= contentEnd)
                {
                    if (best == null || (contentEnd - contentStart) < (best.Value.End - best.Value.Start))
                    {
                        best = (contentStart, contentEnd);
                    }
                }
            }

            if (best == null)
            {
                throw new InvalidOperationException(
                    "Could not find an enclosing entity block (e.g. d[9]={...}) around this position.");
            }
            return best.Value;
        }

        private List<(int Start, int End)> FindAllBlocks(string blockKey)
        {
            var results = new List<(int, int)>();
            byte[] marker = Encoding.ASCII.GetBytes("[\\\"" + blockKey + "\\\"]={");
            int idx = 0;
            while (true)
            {
                int found = IndexOfBytes(Data, marker, idx);
                if (found == -1)
                {
                    break;
                }
                int contentStart = found + marker.Length;
                int depth = 1;
                int j = contentStart;
                while (depth > 0)
                {
                    if (Data[j] == (byte)'{') depth++;
                    else if (Data[j] == (byte)'}') depth--;
                    j++;
                }
                results.Add((contentStart, j - 1));
                idx = j;
            }
            return results;
        }

        private static int IndexOfBytes(byte[] haystack, byte[] needle, int start)
        {
            int limit = haystack.Length - needle.Length;
            for (int i = start; i <= limit; i++)
            {
                bool match = true;
                for (int k = 0; k < needle.Length; k++)
                {
                    if (haystack[i + k] != needle[k])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>Returns the raw numeric text for a field within a block, or null if absent.</summary>
        public string GetAttr(string key, (int Start, int End) block)
        {
            string scope = Encoding.ASCII.GetString(Data, block.Start, block.End - block.Start);
            Match m = FieldRegex(key).Match(scope);
            return m.Success ? m.Groups[1].Value : null;
        }

        /// <summary>
        /// Sets a field's value within a block (updates in place if present,
        /// inserts a new line if not). Mutates Data and returns the block's
        /// new (Start, End) - the caller MUST use the returned block for any
        /// further operations, since insertion/replacement shifts offsets.
        /// Does NOT touch the checksum - call FixChecksum() after all edits.
        /// </summary>
        public (int Start, int End) SetAttr(string key, string value, (int Start, int End) block)
        {
            string scope = Encoding.ASCII.GetString(Data, block.Start, block.End - block.Start);
            Match m = FieldRegex(key).Match(scope);

            if (m.Success)
            {
                int absStart = block.Start + m.Index;
                int absEnd = absStart + m.Length;
                byte[] newField = Encoding.ASCII.GetBytes("[\\\"" + key + "\\\"]=" + value + ",");

                byte[] newData = new byte[Data.Length - (absEnd - absStart) + newField.Length];
                Array.Copy(Data, 0, newData, 0, absStart);
                Array.Copy(newField, 0, newData, absStart, newField.Length);
                Array.Copy(Data, absEnd, newData, absStart + newField.Length, Data.Length - absEnd);

                int delta = newField.Length - (absEnd - absStart);
                Data = newData;
                return (block.Start, block.End + delta);
            }
            else
            {
                string indent = SniffIndent(block);
                byte[] newLine = Encoding.ASCII.GetBytes("\\\n" + indent + "[\\\"" + key + "\\\"]=" + value + ",");

                byte[] newData = new byte[Data.Length + newLine.Length];
                Array.Copy(Data, 0, newData, 0, block.Start);
                Array.Copy(newLine, 0, newData, block.Start, newLine.Length);
                Array.Copy(Data, block.Start, newData, block.Start + newLine.Length, Data.Length - block.Start);

                Data = newData;
                return (block.Start, block.End + newLine.Length);
            }
        }

        private string SniffIndent((int Start, int End) block)
        {
            string scope = Encoding.ASCII.GetString(Data, block.Start, block.End - block.Start);
            Match m = Regex.Match(scope, @"\\\n([ ]+)\[\\""");
            return m.Success ? m.Groups[1].Value : "        ";
        }

        /// <summary>
        /// Sets a field on the player's attrs table, and mirrors it onto the
        /// baseline table too if (and only if) that field already exists
        /// there - keeps buffable stats like ATTACK_RAW consistent instead
        /// of reverting on the next stat recalculation.
        /// </summary>
        public void SetFieldEverywhere(string key, string value)
        {
            var attrs = FindPlayerAttrsBlock();
            SetAttr(key, value, attrs);

            var baseline = FindPlayerBaselineBlock();
            if (GetAttr(key, baseline) != null)
            {
                SetAttr(key, value, baseline);
            }
        }
    }
}
