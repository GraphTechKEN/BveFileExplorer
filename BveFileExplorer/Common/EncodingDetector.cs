using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace BveFileExplorer
{
    /// <summary>
    /// BVE のファイルヘッダー(1行目)からエンコードを判定する。
    /// <para>仕様(bvets.net 車両ファイル/マップファイル ほか共通):</para>
    /// <para>・ファイルの1行目にファイル形式を記述する 例: "BveTs Vehicle 2.00", "BveTs Map 2.02"</para>
    /// <para>・ファイル形式の後に、コロン( : )、文字エンコーディングの順に記述する。コロンの前後に空白は入れられない</para>
    /// <para>・文字エンコーディングを省略した場合は UTF-8</para>
    /// <para>例: "BveTs Vehicle 2.00:shift_jis" → Shift_JIS / "BveTs Map 2.02" → UTF-8</para>
    /// </summary>
    internal static class EncodingDetector
    {
        /// <summary>エンコード選択コンボボックスのインデックス(0:未判定・その他)</summary>
        public const int ModeUnknown = 0;
        /// <summary>エンコード選択コンボボックスのインデックス(1:UTF-8)</summary>
        public const int ModeUtf8 = 1;
        /// <summary>エンコード選択コンボボックスのインデックス(2:Shift_JIS)</summary>
        public const int ModeShiftJis = 2;

        private const int CodePageShiftJis = 932;
        private const int CodePageUtf8 = 65001;

        public static Encoding Utf8 => Encoding.GetEncoding("utf-8");
        public static Encoding ShiftJis => Encoding.GetEncoding("shift_jis");

        /// <summary>ヘッダーの判定結果</summary>
        public sealed class HeaderInfo
        {
            /// <summary>読み込みに使うエンコード(判定できない場合は UTF-8)</summary>
            public Encoding Encoding { get; internal set; } = Utf8;

            /// <summary>ヘッダーに記述されたエンコード名(省略時は空文字)</summary>
            public string DeclaredName { get; internal set; } = "";

            /// <summary>コンボボックスのインデックス(0:その他, 1:UTF-8, 2:Shift_JIS)</summary>
            public int Mode => ModeOf(Encoding);

            /// <summary>仕様から外れた記述があった場合の警告(なければ空文字)</summary>
            public string Warning { get; internal set; } = "";
        }

        // "BveTs Vehicle 2.00 : shift_jis" のように前後に空白があっても拾い、警告を出す
        private static readonly Regex HeaderPattern = new Regex(
            @"^(?<format>BveTs\b[^:]*?)(?<sp1>\s*):(?<sp2>\s*)(?<enc>[A-Za-z0-9_\-\.]*)",
            RegexOptions.IgnoreCase);

        /// <summary>
        /// ファイル1行目のヘッダーからエンコードを判定する(既定:UTF-8)。
        /// </summary>
        /// <param name="encMode">0:その他, 1:UTF-8, 2:Shift_JIS</param>
        public static Encoding Detect(string filePath, out int encMode)
        {
            HeaderInfo info = Analyze(filePath);
            encMode = info.Mode;
            return info.Encoding;
        }

        /// <summary>
        /// ファイル1行目のヘッダーを解析する。ファイルが読めない場合は UTF-8。
        /// </summary>
        public static HeaderInfo Analyze(string filePath)
        {
            string firstLine;
            try
            {
                // ヘッダーは ASCII なので、どのエンコードでも1行目は読める(BOM は自動で除去される)
                using (StreamReader sr = new StreamReader(filePath))
                {
                    firstLine = sr.ReadLine();
                }
            }
            catch (IOException) { return new HeaderInfo(); }
            catch (UnauthorizedAccessException) { return new HeaderInfo(); }
            catch (ArgumentException) { return new HeaderInfo(); }
            catch (NotSupportedException) { return new HeaderInfo(); }

            return ParseHeader(firstLine);
        }

        /// <summary>ヘッダー行(1行目)を解析する</summary>
        public static HeaderInfo ParseHeader(string headerLine)
        {
            HeaderInfo info = new HeaderInfo();
            string line = (headerLine ?? "").Trim();

            if (!line.StartsWith("BveTs", StringComparison.OrdinalIgnoreCase))
            {
                info.Warning = "1行目にファイルヘッダー(BveTs ...)がありません。UTF-8 として読み込みます。";
                return info;
            }

            Match m = HeaderPattern.Match(line);
            if (!m.Success)
            {
                // コロンなし = エンコード省略 → UTF-8 (仕様どおり)
                return info;
            }

            string name = m.Groups["enc"].Value;
            info.DeclaredName = name;

            if (m.Groups["sp1"].Length > 0 || m.Groups["sp2"].Length > 0)
            {
                info.Warning = "ヘッダーのコロン( : )の前後に空白があります(仕様外)。";
            }

            if (name.Length == 0)
            {
                info.Warning = AppendWarning(info.Warning, "コロンの後にエンコード名がありません。UTF-8 として読み込みます。");
                return info;
            }

            Encoding enc = ResolveEncoding(name);
            if (enc == null)
            {
                info.Warning = AppendWarning(info.Warning, "未対応のエンコード名です: " + name + "。UTF-8 として読み込みます。");
                return info;
            }

            info.Encoding = enc;
            return info;
        }

        /// <summary>エンコード名から Encoding を得る(未対応は null)</summary>
        private static Encoding ResolveEncoding(string name)
        {
            switch (name.ToLowerInvariant())
            {
                // シフト JIS の表記ゆれ
                case "shift_jis":
                case "shift-jis":
                case "shiftjis":
                case "sjis":
                case "x-sjis":
                case "cp932":
                case "windows-31j":
                    return ShiftJis;
                case "utf-8":
                case "utf8":
                    return Utf8;
            }

            try
            {
                return Encoding.GetEncoding(name);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        /// <summary>
        /// ヘッダー行のエンコード指定を enc に合わせて書き換える("BveTs Scenario 2.00:shift_jis" 等)。
        /// ヘッダー行でない場合はそのまま返す。
        /// </summary>
        public static string RewriteHeader(string headerLine, Encoding enc)
        {
            string line = (headerLine ?? "").Trim();
            if (!line.StartsWith("BveTs", StringComparison.OrdinalIgnoreCase)) return headerLine;

            Match m = HeaderPattern.Match(line);
            string format = m.Success ? m.Groups["format"].Value.TrimEnd() : line;
            return format + ":" + enc.WebName;
        }

        private static string AppendWarning(string current, string add) => string.IsNullOrEmpty(current) ? add : current + " " + add;

        /// <summary>コンボボックスのインデックス(1:UTF-8, 2:Shift_JIS)から Encoding を得る</summary>
        public static Encoding FromMode(int encMode) => encMode == ModeShiftJis ? ShiftJis : Utf8;

        /// <summary>Encoding からコンボボックスのインデックス(0:その他, 1:UTF-8, 2:Shift_JIS)を得る</summary>
        public static int ModeOf(Encoding enc)
        {
            if (enc == null) return ModeUtf8;
            if (enc.CodePage == CodePageShiftJis) return ModeShiftJis;
            if (enc.CodePage == CodePageUtf8) return ModeUtf8;
            return ModeUnknown;
        }
    }
}
