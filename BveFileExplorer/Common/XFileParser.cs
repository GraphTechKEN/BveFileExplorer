using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace BveFileExplorer
{
    /// <summary>3次元ベクトル(簡易)</summary>
    public struct XVector3
    {
        public double X, Y, Z;
        public XVector3(double x, double y, double z) { X = x; Y = y; Z = z; }
    }

    /// <summary>.x ファイルのマテリアル</summary>
    public sealed class XMaterial
    {
        public double R = 1, G = 1, B = 1, A = 1;
        public double Power;
        public double SpecularR, SpecularG, SpecularB;
        public double EmissiveR, EmissiveG, EmissiveB;
        /// <summary>テクスチャファイル名(.x ファイルからの相対パス。なければ null)</summary>
        public string TextureFile;
    }

    /// <summary>.x ファイルのメッシュ1つ分(座標は Frame の変換適用済み、DirectX の左手系のまま)</summary>
    public sealed class XMesh
    {
        public List<XVector3> Positions = new List<XVector3>();
        /// <summary>頂点ごとのテクスチャ座標(なければ空)</summary>
        public List<(double U, double V)> TexCoords = new List<(double U, double V)>();
        /// <summary>面(頂点インデックスの配列。3角形以上)</summary>
        public List<int[]> Faces = new List<int[]>();
        /// <summary>面ごとのマテリアル番号(Materials のインデックス)。マテリアルがなければ空</summary>
        public List<int> FaceMaterials = new List<int>();
        public List<XMaterial> Materials = new List<XMaterial>();
    }

    /// <summary>.x ファイルの読込結果</summary>
    public sealed class XModel
    {
        public string FilePath;
        public List<XMesh> Meshes = new List<XMesh>();
        public List<string> Warnings = new List<string>();
    }

    public sealed class XFileFormatException : Exception
    {
        public XFileFormatException(string message) : base(message) { }
    }

    /// <summary>
    /// DirectX .x ファイル(テキスト形式)の簡易パーサー。
    /// <para>対応: Frame / FrameTransformMatrix / Mesh / MeshMaterialList / Material / TextureFilename / MeshTextureCoords</para>
    /// <para>非対応: バイナリ形式・圧縮形式、アニメーション、スキンメッシュ(読み飛ばす)</para>
    /// <para>区切り記号(, ;)の書き方は出力ソフトによってまちまちなので、個数で数値を読み進める。</para>
    /// </summary>
    public sealed class XFileParser
    {
        private List<string> _tokens;
        private int _pos;
        private readonly Dictionary<string, XMaterial> _namedMaterials = new Dictionary<string, XMaterial>(StringComparer.OrdinalIgnoreCase);
        private XModel _model;

        public static XModel Load(string filePath)
        {
            return new XFileParser().Parse(filePath);
        }

        private XModel Parse(string filePath)
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            if (bytes.Length < 16 || Encoding.ASCII.GetString(bytes, 0, 4) != "xof ")
            {
                throw new XFileFormatException(".x ファイルのヘッダー(xof)がありません。");
            }

            string format = Encoding.ASCII.GetString(bytes, 8, 4);
            if (!format.Equals("txt ", StringComparison.OrdinalIgnoreCase))
            {
                throw new XFileFormatException("テキスト形式以外の .x (" + format.Trim() + ")には対応していません。");
            }

            _model = new XModel { FilePath = filePath };
            _tokens = Tokenize(DecodeText(bytes, 16));
            _pos = 0;

            while (!AtEnd)
            {
                ParseObject(XMatrix.Identity);
            }

            if (_model.Meshes.Count == 0)
            {
                _model.Warnings.Add("メッシュが見つかりませんでした。");
            }
            return _model;
        }

        /// <summary>UTF-8 として正しければ UTF-8、そうでなければ Shift_JIS(日本語のテクスチャ名対策)</summary>
        private static string DecodeText(byte[] bytes, int offset)
        {
            try
            {
                return new UTF8Encoding(false, true).GetString(bytes, offset, bytes.Length - offset);
            }
            catch (DecoderFallbackException)
            {
                return EncodingDetector.ShiftJis.GetString(bytes, offset, bytes.Length - offset);
            }
        }

        #region 字句解析

        /// <summary>
        /// トークン分割。{ } と文字列("...")と語(識別子・数値・GUID)を返す。
        /// , ; と空白は区切りとして捨て、// と # から行末はコメントとして捨てる。
        /// </summary>
        private static List<string> Tokenize(string text)
        {
            var tokens = new List<string>();
            var sb = new StringBuilder();
            int i = 0;
            int n = text.Length;

            void Flush()
            {
                if (sb.Length > 0)
                {
                    tokens.Add(sb.ToString());
                    sb.Clear();
                }
            }

            while (i < n)
            {
                char c = text[i];

                if (c == '"')
                {
                    Flush();
                    int end = text.IndexOf('"', i + 1);
                    if (end < 0) end = n;
                    // 文字列は先頭に " を付けて区別する
                    tokens.Add("\"" + text.Substring(i + 1, end - i - 1));
                    i = end + 1;
                    continue;
                }
                if (c == '#' || (c == '/' && i + 1 < n && text[i + 1] == '/'))
                {
                    Flush();
                    while (i < n && text[i] != '\n') i++;
                    continue;
                }
                if (c == '{' || c == '}')
                {
                    Flush();
                    tokens.Add(c.ToString());
                    i++;
                    continue;
                }
                if (char.IsWhiteSpace(c) || c == ',' || c == ';')
                {
                    Flush();
                    i++;
                    continue;
                }
                sb.Append(c);
                i++;
            }
            Flush();
            return tokens;
        }

        private bool AtEnd => _pos >= _tokens.Count;

        private string Peek() => AtEnd ? null : _tokens[_pos];

        private string Next()
        {
            if (AtEnd) throw new XFileFormatException("ファイルが途中で終わっています。");
            return _tokens[_pos++];
        }

        private void Expect(string token)
        {
            string t = Next();
            if (t != token) throw new XFileFormatException("「" + token + "」が必要な位置に「" + t + "」があります(トークン " + _pos + ")。");
        }

        private double ReadDouble()
        {
            string t = Next();
            if (!double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
            {
                throw new XFileFormatException("数値が必要な位置に「" + t + "」があります(トークン " + _pos + ")。");
            }
            return v;
        }

        private int ReadInt()
        {
            double v = ReadDouble();
            return (int)v;
        }

        /// <summary>直前の { に対応する } まで読み飛ばす</summary>
        private void SkipBlockContents()
        {
            int depth = 1;
            while (depth > 0)
            {
                string t = Next();
                if (t == "{") depth++;
                else if (t == "}") depth--;
            }
        }

        /// <summary>
        /// "Type [Name] {" を読み、Type を返す(Name は name に)。
        /// "{ Name }" 形式の参照の場合は "{" を返し、name に参照名を入れる。
        /// </summary>
        private string ReadObjectHeader(out string name)
        {
            name = null;
            string type = Next();
            if (type == "{")
            {
                // { Name } 形式の参照
                name = Next();
                if (name == "}") { name = null; return "{"; }
                // { Name <GUID> } の場合もあるので } まで読む
                while (Next() != "}") { }
                return "{";
            }

            if (Peek() != "{")
            {
                name = Next();
            }
            // GUID 等が付く場合に備えて { まで進める
            while (Peek() != null && Peek() != "{") Next();
            Expect("{");
            return type;
        }

        #endregion

        #region 構文解析

        private void ParseObject(XMatrix parent)
        {
            string type = ReadObjectHeader(out string name);
            if (type == "{") return; // トップレベルの参照は無視

            switch (type.ToLowerInvariant())
            {
                case "template":
                    SkipBlockContents();
                    break;
                case "frame":
                    ParseFrame(parent);
                    break;
                case "mesh":
                    ParseMesh(parent);
                    break;
                case "material":
                    XMaterial mat = ParseMaterialBody();
                    if (!string.IsNullOrEmpty(name)) _namedMaterials[name] = mat;
                    break;
                default:
                    // Header, AnimationSet など
                    SkipBlockContents();
                    break;
            }
        }

        private void ParseFrame(XMatrix parent)
        {
            XMatrix world = parent;
            while (Peek() != "}")
            {
                string type = ReadObjectHeader(out string _);
                switch (type.ToLowerInvariant())
                {
                    case "{":
                        break;
                    case "frametransformmatrix":
                        var local = new double[16];
                        for (int i = 0; i < 16; i++) local[i] = ReadDouble();
                        Expect("}");
                        world = XMatrix.Multiply(new XMatrix(local), parent);
                        break;
                    case "frame":
                        ParseFrame(world);
                        break;
                    case "mesh":
                        ParseMesh(world);
                        break;
                    default:
                        SkipBlockContents();
                        break;
                }
            }
            Expect("}");
        }

        private void ParseMesh(XMatrix world)
        {
            var mesh = new XMesh();

            int nVertices = ReadInt();
            for (int i = 0; i < nVertices; i++)
            {
                var v = new XVector3(ReadDouble(), ReadDouble(), ReadDouble());
                mesh.Positions.Add(world.Transform(v));
            }

            int nFaces = ReadInt();
            for (int i = 0; i < nFaces; i++)
            {
                int n = ReadInt();
                var face = new int[n];
                for (int k = 0; k < n; k++) face[k] = ReadInt();
                mesh.Faces.Add(face);
            }

            while (Peek() != "}")
            {
                string type = ReadObjectHeader(out string _);
                switch (type.ToLowerInvariant())
                {
                    case "{":
                        break;
                    case "meshmateriallist":
                        ParseMaterialList(mesh);
                        break;
                    case "meshtexturecoords":
                        int nCoords = ReadInt();
                        for (int i = 0; i < nCoords; i++) mesh.TexCoords.Add((ReadDouble(), ReadDouble()));
                        Expect("}");
                        break;
                    default:
                        // MeshNormals, MeshVertexColors, VertexDuplicationIndices, SkinWeights など
                        SkipBlockContents();
                        break;
                }
            }
            Expect("}");

            Validate(mesh);
            _model.Meshes.Add(mesh);
        }

        private void ParseMaterialList(XMesh mesh)
        {
            int nMaterials = ReadInt();
            int nIndexes = ReadInt();
            var indexes = new int[nIndexes];
            for (int i = 0; i < nIndexes; i++) indexes[i] = ReadInt();

            while (Peek() != "}")
            {
                string type = ReadObjectHeader(out string name);
                if (type == "{")
                {
                    // { MaterialName } 参照
                    if (name != null && _namedMaterials.TryGetValue(name, out XMaterial refMat))
                    {
                        mesh.Materials.Add(refMat);
                    }
                    else
                    {
                        _model.Warnings.Add("マテリアル「" + name + "」が見つかりません。");
                        mesh.Materials.Add(new XMaterial());
                    }
                }
                else if (type.Equals("Material", StringComparison.OrdinalIgnoreCase))
                {
                    XMaterial mat = ParseMaterialBody();
                    if (!string.IsNullOrEmpty(name)) _namedMaterials[name] = mat;
                    mesh.Materials.Add(mat);
                }
                else
                {
                    SkipBlockContents();
                }
            }
            Expect("}");

            if (mesh.Materials.Count < nMaterials)
            {
                _model.Warnings.Add("マテリアル数が宣言(" + nMaterials + ")より少なくなっています。");
                while (mesh.Materials.Count < nMaterials) mesh.Materials.Add(new XMaterial());
            }

            // 面ごとのマテリアル番号(個数が面数より少ない場合は最後の番号を使う)
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                int m = nIndexes == 0 ? 0 : indexes[Math.Min(i, nIndexes - 1)];
                mesh.FaceMaterials.Add(m);
            }
        }

        private XMaterial ParseMaterialBody()
        {
            var mat = new XMaterial
            {
                R = ReadDouble(), G = ReadDouble(), B = ReadDouble(), A = ReadDouble(),
                Power = ReadDouble(),
                SpecularR = ReadDouble(), SpecularG = ReadDouble(), SpecularB = ReadDouble(),
                EmissiveR = ReadDouble(), EmissiveG = ReadDouble(), EmissiveB = ReadDouble(),
            };

            while (Peek() != "}")
            {
                string type = ReadObjectHeader(out string _);
                if (type.Equals("TextureFilename", StringComparison.OrdinalIgnoreCase))
                {
                    string s = Next();
                    mat.TextureFile = s.StartsWith("\"") ? s.Substring(1).Replace("\\\\", "\\") : s;
                    // 残り(通常は } のみ)を読み飛ばす
                    while (Next() != "}") { }
                }
                else if (type != "{")
                {
                    SkipBlockContents();
                }
            }
            Expect("}");
            return mat;
        }

        /// <summary>インデックスの範囲外などを検出して警告に記録し、不正な面を取り除く</summary>
        private void Validate(XMesh mesh)
        {
            int removed = 0;
            for (int i = mesh.Faces.Count - 1; i >= 0; i--)
            {
                int[] f = mesh.Faces[i];
                bool bad = f.Length < 3;
                foreach (int idx in f)
                {
                    if (idx < 0 || idx >= mesh.Positions.Count) bad = true;
                }
                if (bad)
                {
                    mesh.Faces.RemoveAt(i);
                    if (i < mesh.FaceMaterials.Count) mesh.FaceMaterials.RemoveAt(i);
                    removed++;
                }
            }
            if (removed > 0) _model.Warnings.Add("不正な面を " + removed + " 個読み飛ばしました。");

            if (mesh.TexCoords.Count > 0 && mesh.TexCoords.Count != mesh.Positions.Count)
            {
                _model.Warnings.Add("テクスチャ座標の数が頂点数と一致しません。テクスチャ座標を無視します。");
                mesh.TexCoords.Clear();
            }
            for (int i = 0; i < mesh.FaceMaterials.Count; i++)
            {
                if (mesh.FaceMaterials[i] < 0 || mesh.FaceMaterials[i] >= mesh.Materials.Count)
                {
                    mesh.FaceMaterials[i] = 0;
                    if (mesh.Materials.Count == 0) mesh.Materials.Add(new XMaterial());
                }
            }
        }

        #endregion
    }

    /// <summary>4x4 行列(DirectX と同じ行ベクトル形式: v' = v * M)</summary>
    public struct XMatrix
    {
        private readonly double[] _m;

        public XMatrix(double[] m) { _m = m; }

        public static XMatrix Identity => new XMatrix(new double[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 });

        public static XMatrix Multiply(XMatrix a, XMatrix b)
        {
            var r = new double[16];
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    double s = 0;
                    for (int k = 0; k < 4; k++) s += a._m[row * 4 + k] * b._m[k * 4 + col];
                    r[row * 4 + col] = s;
                }
            }
            return new XMatrix(r);
        }

        public XVector3 Transform(XVector3 v)
        {
            double x = v.X * _m[0] + v.Y * _m[4] + v.Z * _m[8] + _m[12];
            double y = v.X * _m[1] + v.Y * _m[5] + v.Z * _m[9] + _m[13];
            double z = v.X * _m[2] + v.Y * _m[6] + v.Z * _m[10] + _m[14];
            double w = v.X * _m[3] + v.Y * _m[7] + v.Z * _m[11] + _m[15];
            if (w != 0 && w != 1) { x /= w; y /= w; z /= w; }
            return new XVector3(x, y, z);
        }
    }
}
