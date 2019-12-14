using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VectSharp
{
    //Reference: http://stevehanov.ca/blog/?id=143, https://developer.apple.com/fonts/TrueType-Reference-Manual/, https://docs.microsoft.com/en-us/typography/opentype/spec/
    public class TrueTypeFile
    {
        private static readonly Dictionary<string, TrueTypeFile> FontCache = new Dictionary<string, TrueTypeFile>();
        private static readonly Dictionary<Stream, TrueTypeFile> StreamFontCache = new Dictionary<Stream, TrueTypeFile>();
        internal uint ScalarType { get; }
        internal ushort NumTables { get; }
        internal ushort SearchRange { get; }
        internal ushort EntrySelector { get; }
        internal ushort RangeShift { get; }
        internal Dictionary<string, TrueTypeTableOffset> TableOffsets { get; } = new Dictionary<string, TrueTypeTableOffset>();
        internal Dictionary<string, ITrueTypeTable> Tables { get; } = new Dictionary<string, ITrueTypeTable>();


        /// <summary>
        /// A stream pointing to the TrueType file source (either on disk or in memory). Never dispose this stream directly; if you really need to, call <see cref="Destroy"/> instead.
        /// </summary>
        public Stream FontStream { get; }

        /// <summary>
        /// Remove this TrueType file from the cache, clear the tables and release the <see cref="FontStream"/>.
        /// Only call this when the actual file that was used to create this object needs to be changed!
        /// </summary>
        public void Destroy()
        {
            string keyString = null;

            foreach (KeyValuePair<string, TrueTypeFile> kvp in FontCache)
            {
                if (kvp.Value == this)
                {
                    keyString = kvp.Key;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(keyString))
            {
                FontCache.Remove(keyString);
            }

            Stream keyStream = null;

            foreach (KeyValuePair<Stream, TrueTypeFile> kvp in StreamFontCache)
            {
                if (kvp.Value == this)
                {
                    keyStream = kvp.Key;
                }
            }

            if (keyStream != null)
            {
                StreamFontCache.Remove(keyStream);
            }

            this.FontStream.Dispose();
            this.TableOffsets.Clear();
            this.Tables.Clear();
        }

        internal TrueTypeFile(Dictionary<string, ITrueTypeTable> tables)
        {
            this.Tables = tables;

            this.ScalarType = (uint)65536;
            this.NumTables = (ushort)tables.Count;
            this.EntrySelector = (ushort)Math.Floor(Math.Log(tables.Count, 2));
            this.SearchRange = (ushort)(16 * (1 << this.EntrySelector));
            this.RangeShift = (ushort)(tables.Count * 16 - this.SearchRange);

            uint offset = 12 + 16 * (uint)tables.Count;

            uint fontChecksum = this.ScalarType;
            fontChecksum += ((uint)this.NumTables << 16) + (uint)this.SearchRange;
            fontChecksum += ((uint)this.EntrySelector << 16) + (uint)this.RangeShift;

            Dictionary<string, byte[]> allBytes = new Dictionary<string, byte[]>();

            foreach (KeyValuePair<string, ITrueTypeTable> kvp in Tables)
            {
                if (kvp.Key == "head")
                {
                    ((TrueTypeHeadTable)kvp.Value).ChecksumAdjustment = 0;
                }

                byte[] tableBytes = kvp.Value.GetBytes();
                allBytes.Add(kvp.Key, tableBytes);
                int length = tableBytes.Length;

                uint checksum = 0;

                for (int i = 0; i < tableBytes.Length; i += 4)
                {
                    byte b1 = tableBytes[i];
                    byte b2 = (i + 1 < tableBytes.Length) ? tableBytes[i + 1] : (byte)0;
                    byte b3 = (i + 2 < tableBytes.Length) ? tableBytes[i + 2] : (byte)0;
                    byte b4 = (i + 3 < tableBytes.Length) ? tableBytes[i + 3] : (byte)0;

                    checksum += (uint)((b1 << 24) + (b2 << 16) + (b3 << 8) + b4);
                }

                TableOffsets.Add(kvp.Key, new TrueTypeTableOffset(checksum, offset, (uint)length));

                fontChecksum += checksum;

                fontChecksum += (uint)(((byte)kvp.Key[0] << 24) + ((byte)kvp.Key[1] << 16) + ((byte)kvp.Key[2] << 8) + (byte)kvp.Key[3]);
                fontChecksum += offset;
                fontChecksum += checksum;
                fontChecksum += (uint)length;

                offset += (uint)length;

                switch (length % 4)
                {
                    case 1:
                        offset += 3;
                        break;
                    case 2:
                        offset += 2;
                        break;
                    case 3:
                        offset++;
                        break;
                }
            }

            ((TrueTypeHeadTable)this.Tables["head"]).ChecksumAdjustment = 0xB1B0AFBA - fontChecksum;


            FontStream = new MemoryStream();
            FontStream.WriteUInt(this.ScalarType);
            FontStream.WriteUShort(this.NumTables);
            FontStream.WriteUShort(this.SearchRange);
            FontStream.WriteUShort(this.EntrySelector);
            FontStream.WriteUShort(this.RangeShift);

            foreach (KeyValuePair<string, ITrueTypeTable> kvp in Tables)
            {
                FontStream.Write(Encoding.ASCII.GetBytes(kvp.Key), 0, 4);
                FontStream.WriteUInt(this.TableOffsets[kvp.Key].Checksum);
                FontStream.WriteUInt(this.TableOffsets[kvp.Key].Offset);
                FontStream.WriteUInt(this.TableOffsets[kvp.Key].Length);
            }

            foreach (KeyValuePair<string, ITrueTypeTable> kvp in Tables)
            {
                FontStream.Write(allBytes[kvp.Key], 0, allBytes[kvp.Key].Length);
                switch (allBytes[kvp.Key].Length % 4)
                {
                    case 1:
                        FontStream.WriteByte(0);
                        FontStream.WriteByte(0);
                        FontStream.WriteByte(0);
                        break;
                    case 2:
                        FontStream.WriteByte(0);
                        FontStream.WriteByte(0);
                        break;
                    case 3:
                        FontStream.WriteByte(0);
                        break;
                }
            }

            FontStream.Seek(0, SeekOrigin.Begin);
        }

        internal static TrueTypeFile CreateTrueTypeFile(string fileName)
        {
            if (!FontCache.ContainsKey(fileName))
            {
                FontCache.Add(fileName, new TrueTypeFile(fileName));
            }

            return FontCache[fileName];
        }

        internal static TrueTypeFile CreateTrueTypeFile(Stream fontStream)
        {
            if (!StreamFontCache.ContainsKey(fontStream))
            {
                StreamFontCache.Add(fontStream, new TrueTypeFile(fontStream));
            }

            return StreamFontCache[fontStream];
        }

        private TrueTypeFile(string fileName) : this(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {

        }

        private TrueTypeFile(Stream fs)
        {
            FontStream = fs;
            this.ScalarType = fs.ReadUInt();
            this.NumTables = fs.ReadUShort();
            this.SearchRange = fs.ReadUShort();
            this.EntrySelector = fs.ReadUShort();
            this.RangeShift = fs.ReadUShort();

            for (int i = 0; i < NumTables; i++)
            {
                this.TableOffsets.Add(fs.ReadString(4), new TrueTypeTableOffset(fs.ReadUInt(), fs.ReadUInt(), fs.ReadUInt()));
            }

            fs.Seek(this.TableOffsets["head"].Offset, SeekOrigin.Begin);

            Tables.Add("head", new TrueTypeHeadTable()
            {
                Version = fs.ReadFixed(),
                FontRevision = fs.ReadFixed(),
                ChecksumAdjustment = fs.ReadUInt(),
                MagicNumber = fs.ReadUInt(),
                Flags = fs.ReadUShort(),
                UnitsPerEm = fs.ReadUShort(),
                Created = fs.ReadDate(),
                Modified = fs.ReadDate(),
                XMin = fs.ReadShort(),
                YMin = fs.ReadShort(),
                XMax = fs.ReadShort(),
                YMax = fs.ReadShort(),
                MacStyle = fs.ReadUShort(),
                LowestRecPPEM = fs.ReadUShort(),
                FontDirectionInt = fs.ReadShort(),
                IndexToLocFormat = fs.ReadShort(),
                GlyphDataFormat = fs.ReadShort()
            });

            fs.Seek(this.TableOffsets["hhea"].Offset, SeekOrigin.Begin);

            Tables.Add("hhea", new TrueTypeHHeaTable()
            {
                Version = fs.ReadFixed(),
                Ascent = fs.ReadShort(),
                Descent = fs.ReadShort(),
                LineGap = fs.ReadShort(),
                AdvanceWidthMax = fs.ReadUShort(),
                MinLeftSideBearing = fs.ReadShort(),
                MinRightSideBearing = fs.ReadShort(),
                XMaxExtent = fs.ReadShort(),
                CaretSlopeRise = fs.ReadShort(),
                CaretSlopeRun = fs.ReadShort(),
                CaretOffset = fs.ReadShort()
            });

            fs.ReadShort();
            fs.ReadShort();
            fs.ReadShort();
            fs.ReadShort();

            ((TrueTypeHHeaTable)Tables["hhea"]).MetricDataFormat = fs.ReadShort();
            ((TrueTypeHHeaTable)Tables["hhea"]).NumOfLongHorMetrics = fs.ReadUShort();

            fs.Seek(this.TableOffsets["maxp"].Offset, SeekOrigin.Begin);

            Tables.Add("maxp", new TrueTypeMaxpTable()
            {
                Version = fs.ReadFixed(),
                NumGlyphs = fs.ReadUShort()
            });

            if (((TrueTypeMaxpTable)Tables["maxp"]).Version.Bits == 65536)
            {
                ((TrueTypeMaxpTable)Tables["maxp"]).MaxPoints = fs.ReadUShort();
                ((TrueTypeMaxpTable)Tables["maxp"]).MaxContours = fs.ReadUShort();
                ((TrueTypeMaxpTable)Tables["maxp"]).MaxComponentPoints = fs.ReadUShort();
                ((TrueTypeMaxpTable)Tables["maxp"]).MaxComponentContours = fs.ReadUShort();
                ((TrueTypeMaxpTable)Tables["maxp"]).MaxZones = fs.ReadUShort();
                ((TrueTypeMaxpTable)Tables["maxp"]).MaxTwilightPoints = fs.ReadUShort();
                ((TrueTypeMaxpTable)Tables["maxp"]).MaxStorage = fs.ReadUShort();
                ((TrueTypeMaxpTable)Tables["maxp"]).MaxFunctionDefs = fs.ReadUShort();
                ((TrueTypeMaxpTable)Tables["maxp"]).MaxInstructionDefs = fs.ReadUShort();
                ((TrueTypeMaxpTable)Tables["maxp"]).MaxStackElements = fs.ReadUShort();
                ((TrueTypeMaxpTable)Tables["maxp"]).MaxSizeOfInstructions = fs.ReadUShort();
                ((TrueTypeMaxpTable)Tables["maxp"]).MaxComponentElements = fs.ReadUShort();
                ((TrueTypeMaxpTable)Tables["maxp"]).MaxComponentDepth = fs.ReadUShort();
            }

            int totalGlyphs = ((TrueTypeMaxpTable)Tables["maxp"]).NumGlyphs;
            int numMetrics = ((TrueTypeHHeaTable)Tables["hhea"]).NumOfLongHorMetrics;

            fs.Seek(this.TableOffsets["hmtx"].Offset, SeekOrigin.Begin);
            Tables.Add("hmtx", new TrueTypeHmtxTable() { HMetrics = new LongHorFixed[numMetrics], LeftSideBearing = new short[totalGlyphs - numMetrics] });

            for (int i = 0; i < numMetrics; i++)
            {
                ((TrueTypeHmtxTable)Tables["hmtx"]).HMetrics[i] = new TrueTypeFile.LongHorFixed(fs.ReadUShort(), fs.ReadShort());
            }

            for (int i = 0; i < totalGlyphs - numMetrics; i++)
            {
                ((TrueTypeHmtxTable)Tables["hmtx"]).LeftSideBearing[i] = fs.ReadShort();
            }

            fs.Seek(this.TableOffsets["cmap"].Offset, SeekOrigin.Begin);
            Tables.Add("cmap", new TrueTypeCmapTable() { Version = fs.ReadUShort(), NumberSubTables = fs.ReadUShort() });

            ((TrueTypeCmapTable)Tables["cmap"]).SubTables = new CmapSubTable[((TrueTypeCmapTable)Tables["cmap"]).NumberSubTables];
            ((TrueTypeCmapTable)Tables["cmap"]).ActualCmapTables = new ICmapTable[((TrueTypeCmapTable)Tables["cmap"]).NumberSubTables];

            for (int i = 0; i < ((TrueTypeCmapTable)Tables["cmap"]).NumberSubTables; i++)
            {
                ((TrueTypeCmapTable)Tables["cmap"]).SubTables[i] = new TrueTypeFile.CmapSubTable(fs.ReadUShort(), fs.ReadUShort(), fs.ReadUInt());
            }


            for (int i = 0; i < ((TrueTypeCmapTable)Tables["cmap"]).NumberSubTables; i++)
            {
                fs.Seek(((TrueTypeCmapTable)Tables["cmap"]).SubTables[i].Offset + TableOffsets["cmap"].Offset, SeekOrigin.Begin);

                ushort format = fs.ReadUShort();
                ushort length = fs.ReadUShort();
                ushort language = fs.ReadUShort();

                if (format == 0)
                {
                    byte[] glyphIndexArray = new byte[256];
                    fs.Read(glyphIndexArray, 0, 256);
                    ((TrueTypeCmapTable)Tables["cmap"]).ActualCmapTables[i] = new CmapTable0(format, length, language, glyphIndexArray);
                }
                else if (format == 4)
                {
                    ((TrueTypeCmapTable)Tables["cmap"]).ActualCmapTables[i] = new CmapTable4(format, length, language, fs);
                }
            }

            if (this.TableOffsets.ContainsKey("OS/2"))
            {
                fs.Seek(this.TableOffsets["OS/2"].Offset, SeekOrigin.Begin);
                Tables.Add("OS/2", new TrueTypeOS2Table()
                {
                    Version = fs.ReadUShort(),
                    XAvgCharWidth = fs.ReadShort(),
                    UsWeightClass = fs.ReadUShort(),
                    UsWidthClass = fs.ReadUShort(),
                    FsType = fs.ReadShort(),
                    YSubscriptXSize = fs.ReadShort(),
                    YSubscriptYSize = fs.ReadShort(),
                    YSubscriptXOffset = fs.ReadShort(),
                    YSubscriptYOffset = fs.ReadShort(),
                    YSuperscriptXSize = fs.ReadShort(),
                    YSuperscriptYSize = fs.ReadShort(),
                    YSuperscriptXOffset = fs.ReadShort(),
                    YSuperscriptYOffset = fs.ReadShort(),
                    YStrikeoutSize = fs.ReadShort(),
                    YStrikeoutPosition = fs.ReadShort(),
                    SFamilyClass = (byte)fs.ReadByte(),
                    SFamilySubClass = (byte)fs.ReadByte(),
                    Panose = new TrueTypeOS2Table.PANOSE((byte)fs.ReadByte(), (byte)fs.ReadByte(), (byte)fs.ReadByte(), (byte)fs.ReadByte(), (byte)fs.ReadByte(), (byte)fs.ReadByte(), (byte)fs.ReadByte(), (byte)fs.ReadByte(), (byte)fs.ReadByte(), (byte)fs.ReadByte()),
                    UlUnicodeRange = new uint[] { fs.ReadUInt(), fs.ReadUInt(), fs.ReadUInt(), fs.ReadUInt() },
                    AchVendID = new byte[] { (byte)fs.ReadByte(), (byte)fs.ReadByte(), (byte)fs.ReadByte(), (byte)fs.ReadByte() },
                    FsSelection = fs.ReadUShort(),
                    FsFirstCharIndex = fs.ReadUShort(),
                    FsLastCharIndex = fs.ReadUShort()
                });
            }

            if (this.TableOffsets.ContainsKey("name"))
            {
                fs.Seek(this.TableOffsets["name"].Offset, SeekOrigin.Begin);
                Tables.Add("name", new TrueTypeNameTable(this.TableOffsets["name"].Offset, fs));
            }

            fs.Seek(this.TableOffsets["loca"].Offset, SeekOrigin.Begin);
            Tables.Add("loca", new TrueTypeLocaTable(fs, ((TrueTypeMaxpTable)this.Tables["maxp"]).NumGlyphs, ((TrueTypeHeadTable)this.Tables["head"]).IndexToLocFormat == 0));

            if (this.TableOffsets.ContainsKey("cvt "))
            {
                fs.Seek(this.TableOffsets["cvt "].Offset, SeekOrigin.Begin);
                Tables.Add("cvt ", new TrueTypeRawTable(fs, this.TableOffsets["cvt "].Length));
            }

            if (this.TableOffsets.ContainsKey("prep"))
            {
                fs.Seek(this.TableOffsets["prep"].Offset, SeekOrigin.Begin);
                Tables.Add("prep", new TrueTypeRawTable(fs, this.TableOffsets["prep"].Length));
            }

            if (this.TableOffsets.ContainsKey("fpgm"))
            {
                fs.Seek(this.TableOffsets["fpgm"].Offset, SeekOrigin.Begin);
                Tables.Add("fpgm", new TrueTypeRawTable(fs, this.TableOffsets["fpgm"].Length));
            }

            Glyph[] glyphs = new Glyph[((TrueTypeMaxpTable)this.Tables["maxp"]).NumGlyphs];

            for (int i = 0; i < ((TrueTypeMaxpTable)this.Tables["maxp"]).NumGlyphs; i++)
            {
                uint offset = ((TrueTypeLocaTable)Tables["loca"]).GetOffset(i);
                uint length = ((TrueTypeLocaTable)Tables["loca"]).Lengths[i];

                if (length > 0)
                {
                    fs.Seek(this.TableOffsets["glyf"].Offset + offset, SeekOrigin.Begin);
                    glyphs[i] = Glyph.Parse(fs);
                }
                else
                {
                    glyphs[i] = new EmptyGlyph();
                }

            }

            Tables.Add("glyf", new TrueTypeGlyfTable() { Glyphs = glyphs });
        }

        /// <summary>
        /// Create a subset of the TrueType file, containing only the glyphs for the specified characters.
        /// </summary>
        /// <param name="charactersToInclude">A string containing the characters for which the glyphs should be included.</param>
        /// <param name="consolidateAt32">If true, the character map is rearranged so that the included glyphs start at the unicode U+0032 control point.</param>
        /// <param name="outputEncoding">If <paramref name="consolidateAt32"/> is true, entries will be added to this dictionary mapping the original characters to the new map (that starts at U+0033).</param>
        /// <returns></returns>
        public TrueTypeFile SubsetFont(string charactersToInclude, bool consolidateAt32 = false, Dictionary<char, char> outputEncoding = null)
        {
            if (!this.HasCmap4Table())
            {
                return this;
            }
            else
            {

                TrueTypeHeadTable head = (TrueTypeHeadTable)this.Tables["head"];

                TrueTypeHHeaTable originalHhea = (TrueTypeHHeaTable)this.Tables["hhea"];
                TrueTypeMaxpTable originalMaxp = (TrueTypeMaxpTable)this.Tables["maxp"];
                TrueTypeCmapTable originalCmap = (TrueTypeCmapTable)this.Tables["cmap"];
                TrueTypeLocaTable originalLoca = (TrueTypeLocaTable)this.Tables["loca"];
                TrueTypeGlyfTable originalGlyf = (TrueTypeGlyfTable)this.Tables["glyf"];

                CmapTable4 cmap4 = null;
                int platformId = -1;
                int platformSpecificID = -1;

                for (int i = 0; i < originalCmap.ActualCmapTables.Length; i++)
                {
                    if (originalCmap.ActualCmapTables[i] != null && originalCmap.ActualCmapTables[i].Format == 4 && originalCmap.SubTables[i].PlatformID == 3 && originalCmap.SubTables[i].PlatformSpecificID == 1)
                    {
                        cmap4 = (CmapTable4)originalCmap.ActualCmapTables[i];
                        platformId = originalCmap.SubTables[i].PlatformID;
                        platformSpecificID = originalCmap.SubTables[i].PlatformSpecificID;
                        break;
                    }
                }

                List<int> characterCodes = new List<int>();

                for (int i = 0; i < charactersToInclude.Length; i++)
                {
                    characterCodes.Add((int)charactersToInclude[i]);
                }

                characterCodes.Sort();

                List<(int, int)> segments = new List<(int, int)>();

                for (int i = 0; i < characterCodes.Count; i++)
                {
                    if (segments.Count > 0 && segments.Last().Item1 + segments.Last().Item2 == characterCodes[i])
                    {
                        segments[segments.Count - 1] = (segments[segments.Count - 1].Item1, segments[segments.Count - 1].Item2 + 1);
                    }
                    else
                    {
                        segments.Add((characterCodes[i], 1));
                    }
                }

                segments.Add((cmap4.StartCode.Last(), cmap4.EndCode.Last() - cmap4.StartCode.Last() + 1));

                int totalGlyphIndexCount = characterCodes.Count + 1;

                segments.Sort((a, b) => a.Item1 + a.Item2 - b.Item1 - b.Item2);

                ushort entrySelector = (ushort)Math.Floor(Math.Log(segments.Count, 2));


                CmapTable4 newCmap4 = new CmapTable4()
                {
                    Format = 4,
                    Length = (ushort)(16 + 8 * segments.Count),
                    Language = cmap4.Language,
                    SegCountX2 = (ushort)(2 * segments.Count),
                    SearchRange = (ushort)(2 * (1 << entrySelector)),
                    EntrySelector = entrySelector,
                    RangeShift = (ushort)(2 * segments.Count - (2 * (1 << entrySelector))),
                    EndCode = new ushort[segments.Count],
                    ReservedPad = cmap4.ReservedPad,
                    StartCode = new ushort[segments.Count],
                    IdDelta = new ushort[segments.Count],
                    IdRangeOffset = new ushort[segments.Count],
                    GlyphIndexArray = new ushort[0]
                };

                List<char> includedGlyphs = new List<char>
                {
                    (char)0
                };

                List<ushort> glyphIndexArray = new List<ushort>();

                if (!consolidateAt32)
                {
                    for (int i = 0; i < segments.Count; i++)
                    {
                        newCmap4.EndCode[i] = (ushort)(segments[i].Item1 + segments[i].Item2 - 1);
                        newCmap4.StartCode[i] = (ushort)segments[i].Item1;

                        newCmap4.IdRangeOffset[i] = 0;

                        if (i < segments.Count - 1)
                        {
                            newCmap4.IdDelta[i] = (ushort)(includedGlyphs.Count - newCmap4.StartCode[i]);

                            for (int j = newCmap4.StartCode[i]; j <= newCmap4.EndCode[i]; j++)
                            {
                                includedGlyphs.Add((char)j);
                            }
                        }
                        else
                        {
                            newCmap4.IdDelta[i] = 1;
                            newCmap4.IdRangeOffset[i] = 0;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < segments.Count - 1; i++)
                    {
                        newCmap4.EndCode[i] = (ushort)(32 + includedGlyphs.Count + segments[i].Item2 - 1);
                        newCmap4.StartCode[i] = (ushort)(32 + includedGlyphs.Count);
                        newCmap4.IdRangeOffset[i] = 0;

                        if (i < segments.Count - 1)
                        {
                            newCmap4.IdDelta[i] = (ushort)(includedGlyphs.Count - newCmap4.StartCode[i]);

                            for (int j = segments[i].Item1; j < segments[i].Item1 + segments[i].Item2; j++)
                            {
                                includedGlyphs.Add((char)j);
                            }
                        }
                        else
                        {
                            newCmap4.IdDelta[i] = 1;
                            newCmap4.IdRangeOffset[i] = 0;
                        }
                    }

                    {
                        int i = segments.Count - 1;
                        newCmap4.EndCode[i] = (ushort)(segments[i].Item1 + segments[i].Item2 - 1);
                        newCmap4.StartCode[i] = (ushort)segments[i].Item1;
                        newCmap4.IdRangeOffset[i] = 0;

                        if (i < segments.Count - 1)
                        {
                            newCmap4.IdDelta[i] = (ushort)(includedGlyphs.Count - newCmap4.StartCode[i]);

                            for (int j = newCmap4.StartCode[i]; j <= newCmap4.EndCode[i]; j++)
                            {
                                includedGlyphs.Add((char)j);
                            }
                        }
                        else
                        {
                            newCmap4.IdDelta[i] = 1;
                            newCmap4.IdRangeOffset[i] = 0;
                        }
                    }

                    if (outputEncoding != null)
                    {
                        for (int i = 1; i < includedGlyphs.Count; i++)
                        {
                            outputEncoding.Add(includedGlyphs[i], (char)(32 + i));
                        }
                    }
                }


                newCmap4.GlyphIndexArray = glyphIndexArray.ToArray();
                newCmap4.Length += (ushort)(newCmap4.GlyphIndexArray.Length * 2);

                TrueTypeCmapTable cmap = new TrueTypeCmapTable() { ActualCmapTables = new ICmapTable[] { newCmap4 }, NumberSubTables = 1, Version = 0, SubTables = new CmapSubTable[] { new CmapSubTable((ushort)platformId, (ushort)platformSpecificID, 12) } };


                List<Glyph> glyphs = new List<Glyph>();
                List<int> originalGlyphIndices = new List<int>();

                for (int i = 0; i < includedGlyphs.Count; i++)
                {
                    int index = this.GetGlyphIndex(includedGlyphs[i]);
                    originalGlyphIndices.Add(index);
                    glyphs.Add(originalGlyf.Glyphs[index].Clone());
                }

                for (int i = 0; i < glyphs.Count; i++)
                {
                    if (glyphs[i] is CompositeGlyph)
                    {
                        for (int j = 0; j < ((CompositeGlyph)glyphs[i]).GlyphIndex.Length; j++)
                        {
                            if (originalGlyphIndices.Contains(((CompositeGlyph)glyphs[i]).GlyphIndex[j]))
                            {
                                ((CompositeGlyph)glyphs[i]).GlyphIndex[j] = (ushort)originalGlyphIndices.IndexOf(((CompositeGlyph)glyphs[i]).GlyphIndex[j]);
                            }
                            else
                            {
                                originalGlyphIndices.Add(((CompositeGlyph)glyphs[i]).GlyphIndex[j]);
                                glyphs.Add(originalGlyf.Glyphs[((CompositeGlyph)glyphs[i]).GlyphIndex[j]]);
                                ((CompositeGlyph)glyphs[i]).GlyphIndex[j] = (ushort)originalGlyphIndices.IndexOf(((CompositeGlyph)glyphs[i]).GlyphIndex[j]);
                            }
                        }
                    }
                }

                TrueTypeGlyfTable glyf = new TrueTypeGlyfTable() { Glyphs = glyphs.ToArray() };

                TrueTypeLocaTable loca = new TrueTypeLocaTable(glyphs.Count, head.IndexToLocFormat == 0);

                for (int i = 0; i < glyphs.Count + 1; i++)
                {
                    if (i == 0)
                    {
                        loca.SetOffset(0, 0);
                        loca.Lengths[i] = (uint)(glyphs[i].GetBytes()).Length;
                    }
                    else
                    {
                        if (i < glyphs.Count)
                        {
                            loca.Lengths[i] = (uint)(glyphs[i].GetBytes()).Length;
                        }

                        loca.SetOffset(i, loca.GetOffset(i - 1) + loca.Lengths[i - 1]);
                    }
                }

                TrueTypeHHeaTable hhea = new TrueTypeHHeaTable()
                {
                    Version = originalHhea.Version,
                    Ascent = originalHhea.Ascent,
                    Descent = originalHhea.Descent,
                    LineGap = originalHhea.LineGap,
                    AdvanceWidthMax = originalHhea.AdvanceWidthMax,
                    MinLeftSideBearing = originalHhea.MinLeftSideBearing,
                    MinRightSideBearing = originalHhea.MinRightSideBearing,
                    XMaxExtent = originalHhea.XMaxExtent,
                    CaretSlopeRise = originalHhea.CaretSlopeRise,
                    CaretSlopeRun = originalHhea.CaretSlopeRun,
                    CaretOffset = originalHhea.CaretOffset,
                    MetricDataFormat = originalHhea.MetricDataFormat,
                    NumOfLongHorMetrics = (ushort)glyphs.Count
                };


                LongHorFixed[] metrics = new LongHorFixed[glyphs.Count];

                for (int i = 0; i < glyphs.Count; i++)
                {
                    metrics[i] = this.GetGlyphMetrics(originalGlyphIndices[i]);
                }

                TrueTypeHmtxTable hmtx = new TrueTypeHmtxTable()
                {
                    LeftSideBearing = new short[0],
                    HMetrics = metrics
                };

                TrueTypeMaxpTable maxp = new TrueTypeMaxpTable()
                {
                    Version = originalMaxp.Version,
                    NumGlyphs = (ushort)glyphs.Count,
                    MaxPoints = originalMaxp.MaxPoints,
                    MaxContours = originalMaxp.MaxContours,
                    MaxComponentPoints = originalMaxp.MaxComponentPoints,
                    MaxComponentContours = originalMaxp.MaxComponentContours,
                    MaxZones = originalMaxp.MaxZones,
                    MaxTwilightPoints = originalMaxp.MaxTwilightPoints,
                    MaxStorage = originalMaxp.MaxStorage,
                    MaxFunctionDefs = originalMaxp.MaxFunctionDefs,
                    MaxInstructionDefs = originalMaxp.MaxInstructionDefs,
                    MaxStackElements = originalMaxp.MaxStackElements,
                    MaxSizeOfInstructions = originalMaxp.MaxSizeOfInstructions,
                    MaxComponentElements = originalMaxp.MaxComponentElements,
                    MaxComponentDepth = originalMaxp.MaxComponentDepth
                };

                Dictionary<string, ITrueTypeTable> newTables = new Dictionary<string, ITrueTypeTable>() {
                    {"head", head },
                    {"hhea", hhea },
                    {"loca", loca },
                    {"maxp", maxp },
                    {"cmap", cmap },
                    {"glyf", glyf },
                    {"hmtx", hmtx }
                };

                if (Tables.ContainsKey("cvt "))
                {
                    TrueTypeRawTable cvt = (TrueTypeRawTable)Tables["cvt "];
                    newTables.Add("cvt ", cvt);
                }

                if (Tables.ContainsKey("prep"))
                {
                    TrueTypeRawTable prep = (TrueTypeRawTable)Tables["prep"];
                    newTables.Add("prep", prep);
                }
                if (Tables.ContainsKey("fpgm"))
                {
                    TrueTypeRawTable fpgm = (TrueTypeRawTable)Tables["fpgm"];
                    newTables.Add("fpgm", fpgm);
                }
                if (Tables.ContainsKey("OS/2"))
                {
                    TrueTypeOS2Table os2 = (TrueTypeOS2Table)Tables["OS/2"];
                    newTables.Add("OS/2", os2);
                }
                if (Tables.ContainsKey("name"))
                {
                    TrueTypeNameTable name = (TrueTypeNameTable)Tables["name"];
                    newTables.Add("name", name);
                }
                TrueTypeFile newFile = new TrueTypeFile(newTables);

                return newFile;
            }

        }

        internal class TrueTypeGlyfTable : ITrueTypeTable
        {
            public Glyph[] Glyphs;

            public byte[] GetBytes()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    for (int i = 0; i < Glyphs.Length; i++)
                    {
                        byte[] glyph = Glyphs[i].GetBytes();
                        ms.Write(glyph, 0, glyph.Length);
                    }
                    return ms.ToArray();
                }
            }

        }

        internal abstract class Glyph
        {
            public short NumberOfContours { get; set; }
            public short XMin { get; set; }
            public short YMin { get; set; }
            public short XMax { get; set; }
            public short YMax { get; set; }

            public abstract byte[] GetBytes();
            public abstract Glyph Clone();
            public static Glyph Parse(Stream sr)
            {
                short numOfContours = sr.ReadShort();
                if (numOfContours >= 0)
                {
                    return new SimpleGlyph(sr, numOfContours);
                }
                else
                {
                    return new CompositeGlyph(sr, numOfContours);
                }
            }

            public abstract TrueTypePoint[][] GetGlyphPath(double size, int emSize, Glyph[] glyphCollection);
        }

        internal class EmptyGlyph : Glyph
        {
            public override byte[] GetBytes()
            {
                return new byte[0];
            }

            public override Glyph Clone()
            {
                return new EmptyGlyph();
            }

            public override TrueTypePoint[][] GetGlyphPath(double size, int emSize, Glyph[] glyphCollection)
            {
                return new TrueTypePoint[0][];
            }
        }
        internal class CompositeGlyph : Glyph
        {
            public ushort[] Flags { get; set; }
            public ushort[] GlyphIndex { get; set; }
            public byte[][] Argument1 { get; set; }
            public byte[][] Argument2 { get; set; }
            public byte[][] TransformationOption { get; set; }
            public ushort NumInstructions { get; set; }
            public byte[] Instructions { get; set; }

            private CompositeGlyph() { }
            public override Glyph Clone()
            {
                CompositeGlyph tbr = new CompositeGlyph()
                {
                    NumberOfContours = this.NumberOfContours,
                    XMin = this.XMin,
                    YMin = this.YMin,
                    XMax = this.XMax,
                    YMax = this.YMax,

                    Flags = new ushort[this.Flags.Length],

                    GlyphIndex = new ushort[this.GlyphIndex.Length],
                    Argument1 = new byte[this.Argument1.Length][],
                    Argument2 = new byte[this.Argument2.Length][],
                    TransformationOption = new byte[this.TransformationOption.Length][],
                    NumInstructions = this.NumInstructions
                };
                this.Flags.CopyTo(tbr.Flags, 0);

                this.GlyphIndex.CopyTo(tbr.GlyphIndex, 0);

                for (int i = 0; i < this.Argument1.Length; i++)
                {
                    tbr.Argument1[i] = (byte[])this.Argument1[i].Clone();
                }


                for (int i = 0; i < this.Argument2.Length; i++)
                {
                    tbr.Argument2[i] = (byte[])this.Argument2[i].Clone();
                }


                for (int i = 0; i < this.TransformationOption.Length; i++)
                {
                    tbr.TransformationOption[i] = (byte[])this.TransformationOption[i].Clone();
                }



                if (this.Instructions != null)
                {
                    tbr.Instructions = new byte[this.Instructions.Length];
                    this.Instructions.CopyTo(tbr.Instructions, 0);
                }
                else
                {
                    tbr.Instructions = null;
                }

                return tbr;
            }
            public CompositeGlyph(Stream sr, short numberOfContours) : base()
            {
                this.NumberOfContours = numberOfContours;
                this.XMin = sr.ReadShort();
                this.YMin = sr.ReadShort();
                this.XMax = sr.ReadShort();
                this.YMax = sr.ReadShort();

                List<ushort> flags = new List<ushort>();
                List<ushort> glyphIndex = new List<ushort>();
                List<byte[]> argument1 = new List<byte[]>();
                List<byte[]> argument2 = new List<byte[]>();
                List<byte[]> transformationOption = new List<byte[]>();

                bool moreComponents = true;

                while (moreComponents)
                {
                    flags.Add(sr.ReadUShort());

                    moreComponents = (flags.Last() & 0x0020) != 0;
                    glyphIndex.Add(sr.ReadUShort());

                    if ((flags.Last() & 0x0001) != 0)
                    {
                        argument1.Add(new byte[] { (byte)sr.ReadByte(), (byte)sr.ReadByte() });
                        argument2.Add(new byte[] { (byte)sr.ReadByte(), (byte)sr.ReadByte() });
                    }
                    else
                    {
                        argument1.Add(new byte[] { (byte)sr.ReadByte() });
                        argument2.Add(new byte[] { (byte)sr.ReadByte() });
                    }

                    if ((flags.Last() & 0x0008) != 0)
                    {
                        transformationOption.Add(new byte[] { (byte)sr.ReadByte(), (byte)sr.ReadByte() });
                    }
                    else if ((flags.Last() & 0x0040) != 0)
                    {
                        transformationOption.Add(new byte[] { (byte)sr.ReadByte(), (byte)sr.ReadByte(), (byte)sr.ReadByte(), (byte)sr.ReadByte() });
                    }
                    else if ((flags.Last() & 0x0080) != 0)
                    {
                        transformationOption.Add(new byte[] { (byte)sr.ReadByte(), (byte)sr.ReadByte(), (byte)sr.ReadByte(), (byte)sr.ReadByte(), (byte)sr.ReadByte(), (byte)sr.ReadByte(), (byte)sr.ReadByte(), (byte)sr.ReadByte() });
                    }
                    else
                    {
                        transformationOption.Add(new byte[] { });
                    }
                }

                this.Flags = flags.ToArray();
                this.GlyphIndex = glyphIndex.ToArray();
                this.Argument1 = argument1.ToArray();
                this.Argument2 = argument2.ToArray();
                this.TransformationOption = transformationOption.ToArray();

                if ((flags.Last() & 0x0100) != 0)
                {
                    this.NumInstructions = sr.ReadUShort();
                    this.Instructions = new byte[this.NumInstructions];
                    sr.Read(this.Instructions, 0, this.NumInstructions);
                }
                else
                {
                    this.NumInstructions = 0;
                    this.Instructions = null;
                }
            }

            public override byte[] GetBytes()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.WriteShort(this.NumberOfContours);
                    ms.WriteShort(this.XMin);
                    ms.WriteShort(this.YMin);
                    ms.WriteShort(this.XMax);
                    ms.WriteShort(this.YMax);

                    for (int i = 0; i < this.Flags.Length; i++)
                    {
                        ms.WriteUShort(this.Flags[i]);
                        ms.WriteUShort(this.GlyphIndex[i]);
                        ms.Write(this.Argument1[i], 0, this.Argument1[i].Length);
                        ms.Write(this.Argument2[i], 0, this.Argument2[i].Length);
                        ms.Write(this.TransformationOption[i], 0, this.TransformationOption[i].Length);
                    }

                    if (this.NumInstructions > 0)
                    {
                        ms.WriteUShort(this.NumInstructions);
                        ms.Write(this.Instructions, 0, this.NumInstructions);
                    }

                    if (ms.Length % 2 != 0)
                    {
                        ms.WriteByte(0);
                    }

                    return ms.ToArray();
                }
            }

            public override TrueTypePoint[][] GetGlyphPath(double size, int emSize, Glyph[] glyphCollection)
            {
                List<short> argument1 = new List<short>();
                List<short> argument2 = new List<short>();

                for (int i = 0; i < this.Argument1.Length; i++)
                {
                    if (this.Argument1[i].Length == 1)
                    {
                        argument1.Add(this.Argument1[i][0]);
                    }
                    else if (this.Argument1[i].Length == 2)
                    {
                        argument1.Add((short)((this.Argument1[i][0] << 8) + this.Argument1[i][1]));
                    }
                }

                for (int i = 0; i < this.Argument2.Length; i++)
                {
                    if (this.Argument2[i].Length == 1)
                    {
                        argument2.Add(this.Argument2[i][0]);
                    }
                    else if (this.Argument2[i].Length == 2)
                    {
                        argument2.Add((short)((this.Argument2[i][0] << 8) + this.Argument2[i][1]));
                    }
                }

                List<double[]> transformationOption = new List<double[]>();

                for (int i = 0; i < this.TransformationOption.Length; i++)
                {
                    if (this.TransformationOption[i].Length == 0)
                    {
                        transformationOption.Add(new double[] { });
                    }
                    else if (this.TransformationOption[i].Length == 2)
                    {
                        double val = (double)((this.TransformationOption[i][0] << 8) + this.TransformationOption[i][1]) / (1 << 14);
                        transformationOption.Add(new double[] { val });
                    }
                    else if (this.TransformationOption[i].Length == 4)
                    {
                        double val1 = (double)((this.TransformationOption[i][0] << 8) + this.TransformationOption[i][1]) / (1 << 14);
                        double val2 = (double)((this.TransformationOption[i][2] << 8) + this.TransformationOption[i][3]) / (1 << 14);
                        transformationOption.Add(new double[] { val1, val2 });
                    }
                    else if (this.TransformationOption[i].Length == 8)
                    {
                        double val1 = (double)((this.TransformationOption[i][0] << 8) + this.TransformationOption[i][1]) / (1 << 14);
                        double val2 = (double)((this.TransformationOption[i][2] << 8) + this.TransformationOption[i][3]) / (1 << 14);
                        double val3 = (double)((this.TransformationOption[i][4] << 8) + this.TransformationOption[i][5]) / (1 << 14);
                        double val4 = (double)((this.TransformationOption[i][6] << 8) + this.TransformationOption[i][7]) / (1 << 14);
                        transformationOption.Add(new double[] { val1, val2, val3, val4 });
                    }
                }

                List<TrueTypePoint[]> tbr = new List<TrueTypePoint[]>();


                for (int i = 0; i < this.GlyphIndex.Length; i++)
                {
                    TrueTypePoint[][] componentContours = glyphCollection[this.GlyphIndex[i]].GetGlyphPath(size, emSize, glyphCollection);

                    double[,] transformMatrix = new double[,] { { 1, 0 }, { 0, 1 } };

                    if ((Flags[i] & 0x0008) != 0)
                    {
                        transformMatrix[0, 0] = transformationOption[i][0];
                        transformMatrix[1, 1] = transformationOption[i][0];
                    }
                    else if ((Flags[i] & 0x0040) != 0)
                    {
                        transformMatrix[0, 0] = transformationOption[i][0];
                        transformMatrix[1, 1] = transformationOption[i][1];
                    }
                    else if ((Flags[i] & 0x0080) != 0)
                    {
                        transformMatrix[0, 0] = transformationOption[i][0];
                        transformMatrix[0, 1] = transformationOption[i][1];
                        transformMatrix[1, 0] = transformationOption[i][2];
                        transformMatrix[1, 1] = transformationOption[i][3];
                    }

                    double deltaX = 0;
                    double deltaY = 0;

                    if ((Flags[i] & 0x0002) != 0)
                    {
                        deltaX = argument1[i] * size / emSize;
                        deltaY = argument2[i] * size / emSize;

                        if ((Flags[i] & 0x0800) != 0 && (Flags[i] & 0x1000) == 0)
                        {
                            deltaX *= Magnitude(Multiply(transformMatrix, new double[] { 1, 0 }));
                            deltaY *= Magnitude(Multiply(transformMatrix, new double[] { 0, 1 }));
                        }
                    }
                    else
                    {
                        TrueTypePoint reference = GetNthElementWhere(tbr, argument1[i], el => el.IsDefinedPoint);
                        TrueTypePoint destination = GetNthElementWhere(componentContours, argument2[i], el => el.IsDefinedPoint);

                        deltaX = reference.X - destination.X;
                        deltaY = reference.Y - destination.Y;

                        //Note: this would be the sensible behaviour (i.e. make sure that reference and destination coincide after the transform), but apparently it is not the correct one.
                        /*double[] transfDestination = Multiply(transformMatrix, new double[] { destination.X, destination.Y });

                        deltaX = reference.X - transfDestination[0];
                        deltaY = reference.Y - transfDestination[1];*/
                    }

                    for (int j = 0; j < componentContours.Length; j++)
                    {
                        for (int k = 0; k < componentContours[j].Length; k++)
                        {
                            double[] transformed = Multiply(transformMatrix, new double[] { componentContours[j][k].X, componentContours[j][k].Y });

                            componentContours[j][k] = new TrueTypePoint(transformed[0] + deltaX, transformed[1] + deltaY, componentContours[j][k].IsOnCurve, componentContours[j][k].IsDefinedPoint);
                        }
                    }

                    tbr.AddRange(componentContours);
                }

                return tbr.ToArray();
            }
        }

        private static double Magnitude(double[] vector)
        {
            double tbr = 0;

            for (int i = 0; i < vector.Length; i++)
            {
                tbr += vector[i] * vector[i];
            }

            return Math.Sqrt(tbr);
        }

        private static double[] Multiply(double[,] matrix, double[] vector)
        {
            double[] tbr = new double[2];

            tbr[0] = matrix[0, 0] * vector[0] + matrix[0, 1] * vector[1];
            tbr[1] = matrix[1, 0] * vector[0] + matrix[1, 1] * vector[1];

            return tbr;
        }

        private static T GetNthElementWhere<T>(IEnumerable<IEnumerable<T>> array, int n, Func<T, bool> condition)
        {
            int index = 0;

            foreach (IEnumerable<T> arr1 in array)
            {
                foreach (T el in arr1)
                {
                    if (condition(el))
                    {
                        if (index == n)
                        {
                            return el;
                        }
                        index++;
                    }
                }
            }

            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Represents a point in a TrueType path description.
        /// </summary>
        public struct TrueTypePoint
        {
            /// <summary>
            /// The horizontal coordinate of the point.
            /// </summary>
            public double X;

            /// <summary>
            /// The vertical coordinate of the point.
            /// </summary>
            public double Y;

            /// <summary>
            /// Whether the point is a point on the curve, or a control point of a quadratic Bezier curve.
            /// </summary>
            public bool IsOnCurve;

            internal bool IsDefinedPoint;

            internal TrueTypePoint(double x, double y, bool onCurve, bool isDefinedPoint)
            {
                this.X = x;
                this.Y = y;
                this.IsOnCurve = onCurve;
                this.IsDefinedPoint = isDefinedPoint;
            }
        }

        internal class SimpleGlyph : Glyph
        {
            public ushort[] EndPtsOfContours { get; set; }
            public ushort InstructionLength { get; set; }
            public byte[] Instructions { get; set; }
            public byte[] Flags { get; set; }
            public byte[] XCoordinates { get; set; }
            public byte[] YCoordinates { get; set; }

            private SimpleGlyph() { }
            public override Glyph Clone()
            {
                SimpleGlyph tbr = new SimpleGlyph
                {
                    NumberOfContours = this.NumberOfContours,
                    XMin = this.XMin,
                    YMin = this.YMin,
                    XMax = this.XMax,
                    YMax = this.YMax,

                    EndPtsOfContours = new ushort[this.EndPtsOfContours.Length],
                    InstructionLength = this.InstructionLength,
                    Instructions = new byte[this.Instructions.Length],
                    Flags = new byte[this.Flags.Length],
                    XCoordinates = new byte[this.XCoordinates.Length],
                    YCoordinates = new byte[this.YCoordinates.Length]
                };

                this.EndPtsOfContours.CopyTo(tbr.EndPtsOfContours, 0);
                this.Instructions.CopyTo(tbr.Instructions, 0);
                this.Flags.CopyTo(tbr.Flags, 0);
                this.XCoordinates.CopyTo(tbr.XCoordinates, 0);
                this.YCoordinates.CopyTo(tbr.YCoordinates, 0);

                return tbr;
            }

            public SimpleGlyph(Stream sr, short numberOfContours) : base()
            {
                this.NumberOfContours = numberOfContours;
                this.XMin = sr.ReadShort();
                this.YMin = sr.ReadShort();
                this.XMax = sr.ReadShort();
                this.YMax = sr.ReadShort();

                this.EndPtsOfContours = new ushort[this.NumberOfContours];
                for (int i = 0; i < this.NumberOfContours; i++)
                {
                    this.EndPtsOfContours[i] = sr.ReadUShort();
                }

                this.InstructionLength = sr.ReadUShort();
                this.Instructions = new byte[this.InstructionLength];
                for (int i = 0; i < this.InstructionLength; i++)
                {
                    this.Instructions[i] = (byte)sr.ReadByte();
                }

                List<byte> logicalFlags = new List<byte>();
                List<byte> flags = new List<byte>();

                int totalPoints = this.EndPtsOfContours[this.NumberOfContours - 1] + 1;

                int countedPoints = 0;

                while (countedPoints < totalPoints)
                {
                    flags.Add((byte)sr.ReadByte());
                    logicalFlags.Add(flags.Last());
                    countedPoints++;
                    if ((flags.Last() & 0x08) != 0)
                    {
                        byte repeats = (byte)sr.ReadByte();
                        for (int i = 0; i < repeats; i++)
                        {
                            logicalFlags.Add(flags.Last());
                            countedPoints++;
                        }
                        flags.Add(repeats);
                    }
                }

                this.Flags = flags.ToArray();

                List<byte> xCoordinates = new List<byte>();

                for (int i = 0; i < totalPoints; i++)
                {
                    bool isByte = (logicalFlags[i] & 0x02) != 0;

                    if (isByte)
                    {
                        xCoordinates.Add((byte)sr.ReadByte());
                    }
                    else if ((logicalFlags[i] & 0x10) == 0)
                    {
                        xCoordinates.Add((byte)sr.ReadByte());
                        xCoordinates.Add((byte)sr.ReadByte());
                    }
                }

                this.XCoordinates = xCoordinates.ToArray();

                List<byte> yCoordinates = new List<byte>();

                List<int> yCoordinateLengths = new List<int>();

                for (int i = 0; i < totalPoints; i++)
                {
                    bool isByte = (logicalFlags[i] & 0x04) != 0;

                    if (isByte)
                    {
                        yCoordinates.Add((byte)sr.ReadByte());
                        yCoordinateLengths.Add(1);
                    }
                    else if ((logicalFlags[i] & 0x20) == 0)
                    {
                        yCoordinates.Add((byte)sr.ReadByte());
                        yCoordinates.Add((byte)sr.ReadByte());
                        yCoordinateLengths.Add(2);
                    }
                    else
                    {
                        yCoordinateLengths.Add(0);
                    }
                }

                this.YCoordinates = yCoordinates.ToArray();
            }

            public override byte[] GetBytes()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.WriteShort(this.NumberOfContours);
                    ms.WriteShort(this.XMin);
                    ms.WriteShort(this.YMin);
                    ms.WriteShort(this.XMax);
                    ms.WriteShort(this.YMax);

                    for (int i = 0; i < this.EndPtsOfContours.Length; i++)
                    {
                        ms.WriteUShort(this.EndPtsOfContours[i]);
                    }

                    ms.WriteUShort(this.InstructionLength);
                    ms.Write(this.Instructions, 0, this.InstructionLength);
                    ms.Write(this.Flags, 0, this.Flags.Length);
                    ms.Write(this.XCoordinates, 0, this.XCoordinates.Length);
                    ms.Write(this.YCoordinates, 0, this.YCoordinates.Length);

                    if (ms.Length % 2 != 0)
                    {
                        ms.WriteByte(0);
                    }

                    return ms.ToArray();
                }
            }

            public override TrueTypePoint[][] GetGlyphPath(double size, int emSize, Glyph[] glyphCollection)
            {
                List<TrueTypePoint[]> contours = new List<TrueTypePoint[]>();

                List<TrueTypePoint> currentContour = new List<TrueTypePoint>();


                List<byte> logicalFlags = new List<byte>();

                int totalPoints = this.EndPtsOfContours[this.NumberOfContours - 1] + 1;

                int countedPoints = 0;

                int index = 0;

                while (countedPoints < totalPoints)
                {
                    logicalFlags.Add(this.Flags[index]);
                    index++;
                    countedPoints++;
                    if ((logicalFlags.Last() & 0x08) != 0)
                    {
                        byte repeats = this.Flags[index];
                        index++;
                        for (int i = 0; i < repeats; i++)
                        {
                            logicalFlags.Add(logicalFlags.Last());
                            countedPoints++;
                        }
                    }
                }

                List<short> xCoordinates = new List<short>();

                index = 0;

                for (int i = 0; i < totalPoints; i++)
                {
                    bool isByte = (logicalFlags[i] & 0x02) != 0;

                    if (isByte)
                    {
                        if ((logicalFlags[i] & 0x10) != 0)
                        {
                            xCoordinates.Add(this.XCoordinates[index]);
                        }
                        else
                        {
                            xCoordinates.Add((short)(-this.XCoordinates[index]));
                        }

                        index++;
                    }
                    else if ((logicalFlags[i] & 0x10) == 0)
                    {
                        xCoordinates.Add((short)((this.XCoordinates[index] << 8) + this.XCoordinates[index + 1]));
                        index += 2;
                    }
                    else
                    {
                        xCoordinates.Add(0);
                    }
                }

                List<short> yCoordinates = new List<short>();

                index = 0;

                for (int i = 0; i < totalPoints; i++)
                {
                    bool isByte = (logicalFlags[i] & 0x04) != 0;

                    if (isByte)
                    {
                        if ((logicalFlags[i] & 0x20) != 0)
                        {
                            yCoordinates.Add(this.YCoordinates[index]);
                        }
                        else
                        {
                            yCoordinates.Add((short)(-this.YCoordinates[index]));
                        }
                        index++;
                    }
                    else if ((logicalFlags[i] & 0x20) == 0)
                    {
                        yCoordinates.Add((short)((this.YCoordinates[index] << 8) + this.YCoordinates[index + 1]));
                        index += 2;
                    }
                    else
                    {
                        yCoordinates.Add(0);
                    }
                }

                int[] previousPoint = new int[2] { 0, 0 };

                for (int i = 0; i < totalPoints; i++)
                {
                    int absoluteX = xCoordinates[i] + previousPoint[0];
                    int absoluteY = yCoordinates[i] + previousPoint[1];

                    previousPoint[0] = absoluteX;
                    previousPoint[1] = absoluteY;

                    bool onCurve = (logicalFlags[i] & 0x01) != 0;

                    if (onCurve)
                    {
                        currentContour.Add(new TrueTypePoint(size * absoluteX / emSize, size * absoluteY / emSize, onCurve, true));
                    }
                    else
                    {
                        if (currentContour.Count > 0)
                        {
                            if (currentContour.Last().IsOnCurve)
                            {
                                currentContour.Add(new TrueTypePoint(size * absoluteX / emSize, size * absoluteY / emSize, onCurve, true));
                            }
                            else
                            {
                                double newX = size * absoluteX / emSize;
                                double newY = size * absoluteY / emSize;

                                currentContour.Add(new TrueTypePoint((newX + currentContour.Last().X) * 0.5, (newY + currentContour.Last().Y) * 0.5, true, false));
                                currentContour.Add(new TrueTypePoint(newX, newY, onCurve, true));
                            }
                        }
                        else
                        {
                            currentContour.Add(new TrueTypePoint(size * absoluteX / emSize, size * absoluteY / emSize, onCurve, true));
                        }
                    }

                    if (this.EndPtsOfContours.Contains((ushort)i))
                    {
                        if (!currentContour[0].IsOnCurve)
                        {
                            if (currentContour.Last().IsOnCurve)
                            {
                                currentContour.Insert(0, new TrueTypePoint(currentContour.Last().X, currentContour.Last().Y, currentContour.Last().IsOnCurve, false));
                            }
                            else
                            {
                                currentContour.Insert(0, new TrueTypePoint((currentContour[0].X + currentContour.Last().X) * 0.5, (currentContour[0].Y + currentContour.Last().Y) * 0.5, true, false));
                            }
                        }

                        if (!currentContour.Last().IsOnCurve)
                        {
                            currentContour.Add(new TrueTypePoint(currentContour[0].X, currentContour[0].Y, currentContour[0].IsOnCurve, false));
                        }

                        contours.Add(currentContour.ToArray());
                        currentContour = new List<TrueTypePoint>();
                    }
                }

                return contours.ToArray();
            }
        }

        internal class TrueTypeLocaTable : ITrueTypeTable
        {
            public ushort[] ShortOffsets { get; }
            public uint[] IntOffsets { get; }
            public uint[] Lengths { get; }

            public uint GetOffset(int index)
            {
                if (IntOffsets == null)
                {
                    return (uint)ShortOffsets[index] * 2;
                }
                else
                {
                    return IntOffsets[index];
                }
            }

            public void SetOffset(int index, uint value)
            {
                if (IntOffsets == null)
                {
                    ShortOffsets[index] = (ushort)(value / 2);
                }
                else
                {
                    IntOffsets[index] = value;
                }
            }

            public TrueTypeLocaTable(int numGlyphs, bool isShort)
            {
                this.Lengths = new uint[numGlyphs];

                if (isShort)
                {
                    this.ShortOffsets = new ushort[numGlyphs + 1];
                }
                else
                {
                    this.IntOffsets = new uint[numGlyphs + 1];
                }
            }

            public TrueTypeLocaTable(Stream sr, int numGlyphs, bool isShort)
            {
                this.Lengths = new uint[numGlyphs];

                if (isShort)
                {
                    this.ShortOffsets = new ushort[numGlyphs + 1];
                    for (int i = 0; i < numGlyphs + 1; i++)
                    {
                        this.ShortOffsets[i] = sr.ReadUShort();
                    }

                    for (int i = 0; i < numGlyphs; i++)
                    {
                        this.Lengths[i] = 2 * ((uint)this.ShortOffsets[i + 1] - (uint)this.ShortOffsets[i]);
                    }
                }
                else
                {
                    this.IntOffsets = new uint[numGlyphs + 1];
                    for (int i = 0; i < numGlyphs + 1; i++)
                    {
                        this.IntOffsets[i] = sr.ReadUInt();
                    }

                    for (int i = 0; i < numGlyphs; i++)
                    {
                        this.Lengths[i] = this.IntOffsets[i + 1] - this.IntOffsets[i];
                    }
                }

            }

            public byte[] GetBytes()
            {
                using (MemoryStream ms = new MemoryStream())
                {

                    if (IntOffsets == null)
                    {
                        for (int i = 0; i < ShortOffsets.Length; i++)
                        {
                            ms.WriteUShort(ShortOffsets[i]);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < IntOffsets.Length; i++)
                        {
                            ms.WriteUInt(IntOffsets[i]);
                        }
                    }

                    return ms.ToArray();
                }
            }
        }

        internal class TrueTypeRawTable : ITrueTypeTable
        {
            public byte[] Data { get; }

            public TrueTypeRawTable(Stream sr, uint length)
            {
                this.Data = new byte[length];
                sr.Read(this.Data, 0, (int)length);
            }

            public byte[] GetBytes()
            {
                return Data;
            }
        }

        internal bool HasCmap4Table()
        {
            foreach (ICmapTable cmap in ((TrueTypeCmapTable)Tables["cmap"]).ActualCmapTables)
            {
                if (cmap is CmapTable4)
                {
                    return true;
                }

            }
            return false;
        }

        /// <summary>
        /// Obtains the font family name from the TrueType file.
        /// </summary>
        /// <returns>The font family name, if available; <see langword="null"/> otherwise.</returns>
        public string GetFontFamilyName()
        {
            TrueTypeNameTable name = (TrueTypeNameTable)this.Tables["name"];

            for (int i = 0; i < name.Count; i++)
            {
                if (name.NameRecords[i].NameID == 16)
                {
                    return name.Name[i];
                }
            }

            for (int i = 0; i < name.Count; i++)
            {
                if (name.NameRecords[i].NameID == 1)
                {
                    return name.Name[i];
                }
            }

            return null;
        }


        /// <summary>
        /// Obtains the PostScript font name from the TrueType file.
        /// </summary>
        /// <returns>The PostScript font name, if available; <see langword="null"/> otherwise.</returns>
        public string GetFontName()
        {
            TrueTypeNameTable name = (TrueTypeNameTable)this.Tables["name"];

            for (int i = 0; i < name.Count; i++)
            {
                if (name.NameRecords[i].NameID == 6)
                {
                    return name.Name[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether the typeface is Italic or Oblique or not.
        /// </summary>
        /// <returns>A <see cref="bool"/> indicating whether the typeface is Italic or Oblique or not.</returns>
        public bool IsItalic()
        {
            TrueTypeOS2Table os2 = (TrueTypeOS2Table)this.Tables["OS/2"];

            return (os2.FsSelection & 1) == 1;
        }

        /// <summary>
        /// Determines whether the typeface is Oblique or not.
        /// </summary>
        /// <returns>A <see cref="bool"/> indicating whether the typeface is Oblique or not.</returns>
        public bool IsOblique()
        {
            TrueTypeOS2Table os2 = (TrueTypeOS2Table)this.Tables["OS/2"];

            return (os2.FsSelection & 512) != 0;
        }

        /// <summary>
        /// Determines whether the typeface is Bold or not.
        /// </summary>
        /// <returns>A <see cref="bool"/> indicating whether the typeface is Bold or not</returns>
        public bool IsBold()
        {
            TrueTypeOS2Table os2 = (TrueTypeOS2Table)this.Tables["OS/2"];

            return (os2.FsSelection & 32) != 0;
        }

        /// <summary>
        /// Determines whether the typeface is fixed-pitch (aka monospaces) or not.
        /// </summary>
        /// <returns>A <see cref="bool"/> indicating whether the typeface is fixed-pitch (aka monospaces) or not.</returns>
        public bool IsFixedPitch()
        {
            TrueTypeOS2Table os2 = (TrueTypeOS2Table)this.Tables["OS/2"];

            return os2.Panose.BProportion == 9;
        }

        /// <summary>
        /// Determines whether the typeface is serifed or not.
        /// </summary>
        /// <returns>A <see cref="bool"/> indicating whether the typeface is serifed or not.</returns>
        public bool IsSerif()
        {
            TrueTypeOS2Table os2 = (TrueTypeOS2Table)this.Tables["OS/2"];

            return os2.SFamilyClass == 1 || os2.SFamilyClass == 2 || os2.SFamilyClass == 3 || os2.SFamilyClass == 4 || os2.SFamilyClass == 5 || os2.SFamilyClass == 7;
        }

        /// <summary>
        /// Determines whether the typeface is a script typeface or not.
        /// </summary>
        /// <returns>A <see cref="bool"/> indicating whether the typeface is a script typeface or not.</returns>
        public bool IsScript()
        {
            TrueTypeOS2Table os2 = (TrueTypeOS2Table)this.Tables["OS/2"];

            return os2.SFamilyClass == 10;
        }

        /// <summary>
        /// Determines the index of the glyph corresponding to a certain character.
        /// </summary>
        /// <param name="glyph">The character whose glyph is sought.</param>
        /// <returns>The index of the glyph in the TrueType file.</returns>
        public int GetGlyphIndex(char glyph)
        {
            foreach (ICmapTable cmap in ((TrueTypeCmapTable)Tables["cmap"]).ActualCmapTables)
            {
                if (cmap is CmapTable4)
                {
                    return cmap.GetGlyphIndex(glyph);
                }
            }

            foreach (ICmapTable cmap in ((TrueTypeCmapTable)Tables["cmap"]).ActualCmapTables)
            {
                if (cmap is CmapTable0)
                {
                    return cmap.GetGlyphIndex(glyph);
                }
            }

            return -1;
        }

        internal int GetGlyphWidth(int glyphIndex)
        {
            if (((TrueTypeHmtxTable)this.Tables["hmtx"]).HMetrics.Length > glyphIndex)
            {
                return ((TrueTypeHmtxTable)this.Tables["hmtx"]).HMetrics[glyphIndex].AdvanceWidth;
            }
            else
            {
                return ((TrueTypeHmtxTable)this.Tables["hmtx"]).HMetrics.Last().AdvanceWidth;
            }
        }

        internal LongHorFixed GetGlyphMetrics(int glyphIndex)
        {
            if (((TrueTypeHmtxTable)this.Tables["hmtx"]).HMetrics.Length > glyphIndex)
            {
                return ((TrueTypeHmtxTable)this.Tables["hmtx"]).HMetrics[glyphIndex];
            }
            else
            {
                return new LongHorFixed(((TrueTypeHmtxTable)this.Tables["hmtx"]).HMetrics.Last().AdvanceWidth, ((TrueTypeHmtxTable)this.Tables["hmtx"]).LeftSideBearing[glyphIndex - ((TrueTypeHmtxTable)this.Tables["hmtx"]).HMetrics.Length]);
            }
        }

        /// <summary>
        /// Get the path that describes the shape of a glyph.
        /// </summary>
        /// <param name="glyphIndex">The index of the glyph whose path is sought.</param>
        /// <param name="size">The font size to be used for the font coordinates.</param>
        /// <returns>An array of contours, each of which is itself an array of TrueType points.</returns>
        public TrueTypePoint[][] GetGlyphPath(int glyphIndex, double size)
        {
            return ((TrueTypeGlyfTable)this.Tables["glyf"]).Glyphs[glyphIndex].GetGlyphPath(size, ((TrueTypeHeadTable)this.Tables["head"]).UnitsPerEm, ((TrueTypeGlyfTable)this.Tables["glyf"]).Glyphs);
        }

        /// <summary>
        /// Get the path that describes the shape of a glyph.
        /// </summary>
        /// <param name="glyph">The glyph whose path is sought.</param>
        /// <param name="size">The font size to be used for the font coordinates.</param>
        /// <returns>An array of contours, each of which is itself an array of TrueType points.</returns>
        public TrueTypePoint[][] GetGlyphPath(char glyph, double size)
        {
            return ((TrueTypeGlyfTable)this.Tables["glyf"]).Glyphs[GetGlyphIndex(glyph)].GetGlyphPath(size, ((TrueTypeHeadTable)this.Tables["head"]).UnitsPerEm, ((TrueTypeGlyfTable)this.Tables["glyf"]).Glyphs);
        }

        /// <summary>
        /// Computes the advance width of a glyph, in thousandths of em unit.
        /// </summary>
        /// <param name="glyph">The glyph whose advance width is to be computed.</param>
        /// <returns>The advance width of the glyph in thousandths of em unit.</returns>
        public double Get1000EmGlyphWidth(char glyph)
        {
            return Get1000EmGlyphWidth(GetGlyphIndex(glyph));
        }

        /// <summary>
        /// Computes the advance width of a glyph, in thousandths of em unit.
        /// </summary>
        /// <param name="glyphIndex">The index of the glyph whose advance width is to be computed.</param>
        /// <returns>The advance width of the glyph in thousandths of em unit.</returns>
        public double Get1000EmGlyphWidth(int glyphIndex)
        {
            int w = GetGlyphWidth(glyphIndex);

            return w * 1000 / ((TrueTypeHeadTable)this.Tables["head"]).UnitsPerEm;
        }

        /// <summary>
        /// Computes the font ascent, in thousandths of em unit.
        /// </summary>
        /// <returns>The font ascent in thousandths of em unit.</returns>
        public double Get1000EmAscent()
        {
            return ((TrueTypeHHeaTable)this.Tables["hhea"]).Ascent * 1000.0 / ((TrueTypeHeadTable)this.Tables["head"]).UnitsPerEm;
        }


        /// <summary>
        /// Computes the font descent, in thousandths of em unit.
        /// </summary>
        /// <returns>The font descent in thousandths of em unit.</returns>
        public double Get1000EmDescent()
        {
            return ((TrueTypeHHeaTable)this.Tables["hhea"]).Descent * 1000.0 / ((TrueTypeHeadTable)this.Tables["head"]).UnitsPerEm;
        }

        /// <summary>
        /// Computes the maximum height over the baseline of the font, in thousandths of em unit.
        /// </summary>
        /// <returns>The maximum height over the baseline of the font in thousandths of em unit.</returns>
        public double Get1000EmYMax()
        {
            return ((TrueTypeHeadTable)this.Tables["head"]).YMax * 1000.0 / ((TrueTypeHeadTable)this.Tables["head"]).UnitsPerEm;
        }

        /// <summary>
        /// Computes the maximum depth below the baseline of the font, in thousandths of em unit.
        /// </summary>
        /// <returns>The maximum depth below the baseline of the font in thousandths of em unit.</returns>
        public double Get1000EmYMin()
        {
            return ((TrueTypeHeadTable)this.Tables["head"]).YMin * 1000.0 / ((TrueTypeHeadTable)this.Tables["head"]).UnitsPerEm;
        }

        /// <summary>
        /// Computes the maximum distance to the right of the glyph origin of the font, in thousandths of em unit.
        /// </summary>
        /// <returns>The maximum distance to the right of the glyph origin of the font in thousandths of em unit.</returns>
        public double Get1000EmXMax()
        {
            return ((TrueTypeHeadTable)this.Tables["head"]).XMax * 1000.0 / ((TrueTypeHeadTable)this.Tables["head"]).UnitsPerEm;
        }

        /// <summary>
        /// Computes the maximum distance to the left of the glyph origin of the font, in thousandths of em unit.
        /// </summary>
        /// <returns>The maximum distance to the left of the glyph origin of the font in thousandths of em unit.</returns>
        public double Get1000EmXMin()
        {
            return ((TrueTypeHeadTable)this.Tables["head"]).XMin * 1000.0 / ((TrueTypeHeadTable)this.Tables["head"]).UnitsPerEm;
        }

        /// <summary>
        /// Represents the left- and right-side bearings of a glyph.
        /// </summary>
        public struct Bearings
        {
            /// <summary>
            /// The left-side bearing of the glyph.
            /// </summary>
            public int LeftSideBearing;

            /// <summary>
            /// The right-side bearing of the glyph.
            /// </summary>
            public int RightSideBearing;

            internal Bearings(int lsb, int rsb)
            {
                LeftSideBearing = lsb;
                RightSideBearing = rsb;
            }
        }

        internal Bearings Get1000EmGlyphBearings(int glyphIndex)
        {
            LongHorFixed metrics = GetGlyphMetrics(glyphIndex);

            int lsb = metrics.LeftSideBearing;

            Glyph glyph = ((TrueTypeGlyfTable)this.Tables["glyf"]).Glyphs[glyphIndex];

            int rsb = metrics.AdvanceWidth - (lsb + glyph.XMax - glyph.XMin);

            return new Bearings(lsb * 1000 / ((TrueTypeHeadTable)this.Tables["head"]).UnitsPerEm, rsb * 1000 / ((TrueTypeHeadTable)this.Tables["head"]).UnitsPerEm);
        }


        /// <summary>
        /// Computes the left- and right- side bearings of a glyph, in thousandths of em unit.
        /// </summary>
        /// <param name="glyph">The glyph whose bearings are to be computed.</param>
        /// <returns>The left- and right- side bearings of the glyph in thousandths of em unit</returns>
        public Bearings Get1000EmGlyphBearings(char glyph)
        {
            return Get1000EmGlyphBearings(GetGlyphIndex(glyph));
        }

        /// <summary>
        /// Represents the maximum heigth above and depth below the baseline of a glyph.
        /// </summary>
        public struct VerticalMetrics
        {
            /// <summary>
            /// The maximum depth below the baseline of the glyph.
            /// </summary>
            public int YMin;

            /// <summary>
            /// The maximum height above the baseline of the glyph.
            /// </summary>
            public int YMax;

            internal VerticalMetrics(int yMin, int yMax)
            {
                this.YMin = yMin;
                this.YMax = yMax;
            }
        }

        internal VerticalMetrics Get1000EmGlyphVerticalMetrics(int glyphIndex)
        {
            Glyph glyph = ((TrueTypeGlyfTable)this.Tables["glyf"]).Glyphs[glyphIndex];

            return new VerticalMetrics(glyph.YMin * 1000 / ((TrueTypeHeadTable)this.Tables["head"]).UnitsPerEm, glyph.YMax * 1000 / ((TrueTypeHeadTable)this.Tables["head"]).UnitsPerEm);
        }

        /// <summary>
        /// Computes the vertical metrics of a glyph, in thousandths of em unit.
        /// </summary>
        /// <param name="glyph">The glyph whose vertical metrics are to be computed.</param>
        /// <returns>The vertical metrics of a glyph, in thousandths of em unit.</returns>
        public VerticalMetrics Get1000EmGlyphVerticalMetrics(char glyph)
        {
            return Get1000EmGlyphVerticalMetrics(GetGlyphIndex(glyph));
        }

        internal interface ITrueTypeTable
        {
            byte[] GetBytes();
        }

        internal interface ICmapTable
        {
            ushort Format { get; }
            ushort Length { get; }
            ushort Language { get; }

            int GetGlyphIndex(char glyph);

            byte[] GetBytes();
        }

        internal class CmapTable4 : ICmapTable
        {
            public ushort Format { get; set; }
            public ushort Length { get; set; }
            public ushort Language { get; set; }
            public ushort SegCountX2 { get; set; }
            public ushort SearchRange { get; set; }
            public ushort EntrySelector { get; set; }
            public ushort RangeShift { get; set; }
            public ushort[] EndCode { get; set; }
            public ushort ReservedPad { get; set; }
            public ushort[] StartCode { get; set; }
            public ushort[] IdDelta { get; set; }
            public ushort[] IdRangeOffset { get; set; }
            public ushort[] GlyphIndexArray { get; set; }

            public CmapTable4() { }

            public CmapTable4(ushort format, ushort length, ushort language, Stream sr)
            {
                this.Format = format;
                this.Length = length;
                this.Language = language;
                this.SegCountX2 = sr.ReadUShort();

                int segCount = this.SegCountX2 / 2;

                this.SearchRange = sr.ReadUShort();
                this.EntrySelector = sr.ReadUShort();
                this.RangeShift = sr.ReadUShort();

                this.EndCode = new ushort[segCount];
                for (int i = 0; i < segCount; i++)
                {
                    this.EndCode[i] = sr.ReadUShort();
                }

                this.ReservedPad = sr.ReadUShort();

                this.StartCode = new ushort[segCount];
                for (int i = 0; i < segCount; i++)
                {
                    this.StartCode[i] = sr.ReadUShort();
                }

                this.IdDelta = new ushort[segCount];
                for (int i = 0; i < segCount; i++)
                {
                    this.IdDelta[i] = sr.ReadUShort();
                }

                this.IdRangeOffset = new ushort[segCount];
                for (int i = 0; i < segCount; i++)
                {
                    this.IdRangeOffset[i] = sr.ReadUShort();
                }

                int numGlyphIndices = (this.Length - 16 + 8 * segCount) / 2;

                this.GlyphIndexArray = new ushort[numGlyphIndices];

                for (int i = 0; i < numGlyphIndices; i++)
                {
                    this.GlyphIndexArray[i] = sr.ReadUShort();
                }
            }

            public int GetGlyphIndex(char glyph)
            {
                int code = (int)glyph;

                int endCodeInd = -1;

                for (int i = 0; i < EndCode.Length; i++)
                {
                    if (EndCode[i] >= code)
                    {
                        endCodeInd = i;
                        break;
                    }
                }

                if (StartCode[endCodeInd] <= code)
                {
                    if (IdRangeOffset[endCodeInd] != 0)
                    {
                        int glyphIndexIndex = IdRangeOffset[endCodeInd] / 2 + (code - StartCode[endCodeInd]) - (IdRangeOffset.Length - endCodeInd);

                        if (GlyphIndexArray[glyphIndexIndex] != 0)
                        {
                            return (IdDelta[endCodeInd] + GlyphIndexArray[glyphIndexIndex]) % 65536;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        return (code + IdDelta[endCodeInd]) % 65536;
                    }
                }
                else
                {
                    return 0;
                }
            }

            public byte[] GetBytes()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.WriteUShort(this.Format);
                    ms.WriteUShort(this.Length);
                    ms.WriteUShort(this.Language);
                    ms.WriteUShort(this.SegCountX2);
                    ms.WriteUShort(this.SearchRange);
                    ms.WriteUShort(this.EntrySelector);
                    ms.WriteUShort(this.RangeShift);
                    for (int i = 0; i < this.EndCode.Length; i++)
                    {
                        ms.WriteUShort(this.EndCode[i]);
                    }
                    ms.WriteUShort(this.ReservedPad);

                    for (int i = 0; i < this.StartCode.Length; i++)
                    {
                        ms.WriteUShort(this.StartCode[i]);
                    }
                    for (int i = 0; i < this.IdDelta.Length; i++)
                    {
                        ms.WriteUShort(this.IdDelta[i]);
                    }
                    for (int i = 0; i < this.IdRangeOffset.Length; i++)
                    {
                        ms.WriteUShort(this.IdRangeOffset[i]);
                    }
                    for (int i = 0; i < this.GlyphIndexArray.Length; i++)
                    {
                        ms.WriteUShort(this.GlyphIndexArray[i]);
                    }

                    return ms.ToArray();
                }
            }
        }

        internal class CmapTable0 : ICmapTable
        {
            public ushort Format { get; }
            public ushort Length { get; }
            public ushort Language { get; }

            public byte[] GlyphIndexArray { get; }

            public CmapTable0(ushort format, ushort length, ushort language, byte[] glyphIndexArray)
            {
                this.Format = format;
                this.Length = length;
                this.Language = language;
                this.GlyphIndexArray = glyphIndexArray;
            }

            public int GetGlyphIndex(char glyph)
            {
                return GlyphIndexArray[(byte)glyph];
            }

            public byte[] GetBytes()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.WriteUShort(this.Format);
                    ms.WriteUShort(this.Length);
                    ms.WriteUShort(this.Language);
                    ms.Write(this.GlyphIndexArray, 0, this.GlyphIndexArray.Length);
                    return ms.ToArray();
                }
            }
        }

        internal struct CmapSubTable
        {
            public ushort PlatformID;
            public ushort PlatformSpecificID;
            public uint Offset;

            public CmapSubTable(ushort platformID, ushort platformSpecificID, uint offset)
            {
                this.PlatformID = platformID;
                this.PlatformSpecificID = platformSpecificID;
                this.Offset = offset;
            }
        }

        internal class TrueTypeCmapTable : ITrueTypeTable
        {
            public ushort Version;
            public ushort NumberSubTables;
            public CmapSubTable[] SubTables;
            public ICmapTable[] ActualCmapTables;

            public byte[] GetBytes()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.WriteUShort(this.Version);
                    ms.WriteUShort(this.NumberSubTables);

                    for (int i = 0; i < this.SubTables.Length; i++)
                    {
                        ms.WriteUShort(this.SubTables[i].PlatformID);
                        ms.WriteUShort(this.SubTables[i].PlatformSpecificID);
                        ms.WriteUInt(this.SubTables[i].Offset);
                    }

                    for (int i = 0; i < this.ActualCmapTables.Length; i++)
                    {
                        byte[] bytes = this.ActualCmapTables[i].GetBytes();
                        ms.Write(bytes, 0, bytes.Length);
                    }

                    return ms.ToArray();
                }
            }
        }

        internal class TrueTypeNameTable : ITrueTypeTable
        {
            public byte[] GetBytes()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.WriteUShort(this.Format);
                    ms.WriteUShort(this.Count);
                    ms.WriteUShort(this.StringOffset);

                    for (int i = 0; i < this.NameRecords.Length; i++)
                    {
                        byte[] bytes = this.NameRecords[i].GetBytes();
                        ms.Write(bytes, 0, bytes.Length);
                    }

                    ms.Write(this.RawBytes, 0, this.RawBytes.Length);

                    return ms.ToArray();
                }
            }

            public ushort Format;
            public ushort Count;
            public ushort StringOffset;
            public NameRecord[] NameRecords;
            public string[] Name;
            private readonly byte[] RawBytes;

            public struct NameRecord
            {
                public ushort PlatformID;
                public ushort PlatformSpecificID;
                public ushort LanguageID;
                public ushort NameID;
                public ushort Length;
                public ushort Offset;

                public NameRecord(ushort platformID, ushort platformSpecificID, ushort languageID, ushort nameID, ushort length, ushort offset)
                {
                    this.PlatformID = platformID;
                    this.PlatformSpecificID = platformSpecificID;
                    this.LanguageID = languageID;
                    this.NameID = nameID;
                    this.Length = length;
                    this.Offset = offset;
                }

                public byte[] GetBytes()
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.WriteUShort(this.PlatformID);
                        ms.WriteUShort(this.PlatformSpecificID);
                        ms.WriteUShort(this.LanguageID);
                        ms.WriteUShort(this.NameID);
                        ms.WriteUShort(this.Length);
                        ms.WriteUShort(this.Offset);
                        return ms.ToArray();
                    }
                }
            }

            public TrueTypeNameTable(uint tableOffset, Stream sr)
            {
                this.Format = sr.ReadUShort();
                this.Count = sr.ReadUShort();
                this.StringOffset = sr.ReadUShort();
                this.NameRecords = new NameRecord[this.Count];
                this.Name = new string[this.Count];

                int maxOffsetItem = -1;

                for (int i = 0; i < this.Count; i++)
                {
                    this.NameRecords[i] = new NameRecord(sr.ReadUShort(), sr.ReadUShort(), sr.ReadUShort(), sr.ReadUShort(), sr.ReadUShort(), sr.ReadUShort());
                    if (maxOffsetItem < 0 || this.NameRecords[i].Offset > this.NameRecords[maxOffsetItem].Offset || (this.NameRecords[i].Offset == this.NameRecords[maxOffsetItem].Offset && this.NameRecords[i].Length > this.NameRecords[maxOffsetItem].Length))
                    {
                        maxOffsetItem = i;
                    }
                }

                this.RawBytes = new byte[this.NameRecords[maxOffsetItem].Offset + this.NameRecords[maxOffsetItem].Length];
                sr.Seek(tableOffset + this.StringOffset, SeekOrigin.Begin);
                sr.Read(this.RawBytes, 0, this.RawBytes.Length);

                for (int i = 0; i < this.Count; i++)
                {
                    sr.Seek(tableOffset + this.NameRecords[i].Offset + this.StringOffset, SeekOrigin.Begin);
                    byte[] stringBytes = new byte[this.NameRecords[i].Length];
                    sr.Read(stringBytes, 0, this.NameRecords[i].Length);

                    if (this.NameRecords[i].PlatformID == 0)
                    {
                        this.Name[i] = Encoding.BigEndianUnicode.GetString(stringBytes);
                    }
                    else if (this.NameRecords[i].PlatformID == 1 && this.NameRecords[i].PlatformSpecificID == 0)
                    {
                        this.Name[i] = GetMacRomanString(stringBytes);
                    }
                    else if (this.NameRecords[i].PlatformID == 3 && (this.NameRecords[i].PlatformSpecificID == 1 || this.NameRecords[i].PlatformSpecificID == 0))
                    {
                        this.Name[i] = Encoding.BigEndianUnicode.GetString(stringBytes);
                    }
                    else
                    {
                        this.Name[i] = "Unsupported encoding: " + this.NameRecords[i].PlatformID.ToString() + "/" + this.NameRecords[i].PlatformSpecificID.ToString();
                    }

                }
            }

            private static readonly char[] MacRomanChars = new char[] { '\u0020', '\u0021', '\u0022', '\u0023', '\u0024', '\u0025', '\u0026', '\u0027', '\u0028', '\u0029', '\u002a', '\u002b', '\u002c', '\u002d', '\u002e', '\u002f', '\u0030', '\u0031', '\u0032', '\u0033', '\u0034', '\u0035', '\u0036', '\u0037', '\u0038', '\u0039', '\u003a', '\u003b', '\u003c', '\u003d', '\u003e', '\u003f', '\u0040', '\u0041', '\u0042', '\u0043', '\u0044', '\u0045', '\u0046', '\u0047', '\u0048', '\u0049', '\u004a', '\u004b', '\u004c', '\u004d', '\u004e', '\u004f', '\u0050', '\u0051', '\u0052', '\u0053', '\u0054', '\u0055', '\u0056', '\u0057', '\u0058', '\u0059', '\u005a', '\u005b', '\u005c', '\u005d', '\u005e', '\u005f', '\u0060', '\u0061', '\u0062', '\u0063', '\u0064', '\u0065', '\u0066', '\u0067', '\u0068', '\u0069', '\u006a', '\u006b', '\u006c', '\u006d', '\u006e', '\u006f', '\u0070', '\u0071', '\u0072', '\u0073', '\u0074', '\u0075', '\u0076', '\u0077', '\u0078', '\u0079', '\u007a', '\u007b', '\u007c', '\u007d', '\u007e', '\u007f', '\u00c4', '\u00c5', '\u00c7', '\u00c9', '\u00d1', '\u00d6', '\u00dc', '\u00e1', '\u00e0', '\u00e2', '\u00e4', '\u00e3', '\u00e5', '\u00e7', '\u00e9', '\u00e8', '\u00ea', '\u00eb', '\u00ed', '\u00ec', '\u00ee', '\u00ef', '\u00f1', '\u00f3', '\u00f2', '\u00f4', '\u00f6', '\u00f5', '\u00fa', '\u00f9', '\u00fb', '\u00fc', '\u2020', '\u00b0', '\u00a2', '\u00a3', '\u00a7', '\u2022', '\u00b6', '\u00df', '\u00ae', '\u00a9', '\u2122', '\u00b4', '\u00a8', '\u2260', '\u00c6', '\u00d8', '\u221e', '\u00b1', '\u2264', '\u2265', '\u00a5', '\u00b5', '\u2202', '\u2211', '\u220f', '\u03c0', '\u222b', '\u00aa', '\u00ba', '\u03a9', '\u00e6', '\u00f8', '\u00bf', '\u00a1', '\u00ac', '\u221a', '\u0192', '\u2248', '\u2206', '\u00ab', '\u00bb', '\u2026', '\u00a0', '\u00c0', '\u00c3', '\u00d5', '\u0152', '\u0153', '\u2013', '\u2014', '\u201c', '\u201d', '\u2018', '\u2019', '\u00f7', '\u25ca', '\u00ff', '\u0178', '\u2044', '\u20ac', '\u2039', '\u203a', '\ufb01', '\ufb02', '\u2021', '\u00b7', '\u201a', '\u201e', '\u2030', '\u00c2', '\u00ca', '\u00c1', '\u00cb', '\u00c8', '\u00cd', '\u00ce', '\u00cf', '\u00cc', '\u00d3', '\u00d4', '\uf8ff', '\u00d2', '\u00da', '\u00db', '\u00d9', '\u0131', '\u02c6', '\u02dc', '\u00af', '\u02d8', '\u02d9', '\u02da', '\u00b8', '\u02dd', '\u02db', '\u02c7' };

            private static string GetMacRomanString(byte[] bytes)
            {
                StringBuilder bld = new StringBuilder(bytes.Length);

                for (int i = 0; i < bytes.Length; i++)
                {
                    if (bytes[i] >= 32)
                    {
                        bld.Append(MacRomanChars[bytes[i] - 32]);
                    }
                    else
                    {
                        bld.Append((char)bytes[i]);
                    }
                }

                return bld.ToString();

            }
        }

        internal class TrueTypeOS2Table : ITrueTypeTable
        {
            public ushort Version;
            public short XAvgCharWidth;
            public ushort UsWeightClass;
            public ushort UsWidthClass;
            public short FsType;
            public short YSubscriptXSize;
            public short YSubscriptYSize;
            public short YSubscriptXOffset;
            public short YSubscriptYOffset;
            public short YSuperscriptXSize;
            public short YSuperscriptYSize;
            public short YSuperscriptXOffset;
            public short YSuperscriptYOffset;
            public short YStrikeoutSize;
            public short YStrikeoutPosition;
            public byte SFamilyClass;
            public byte SFamilySubClass;
            public PANOSE Panose;
            public uint[] UlUnicodeRange;
            public byte[] AchVendID;
            public ushort FsSelection;
            public ushort FsFirstCharIndex;
            public ushort FsLastCharIndex;

            public byte[] GetBytes()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.WriteUShort(this.Version);
                    ms.WriteShort(this.XAvgCharWidth);
                    ms.WriteUShort(this.UsWeightClass);
                    ms.WriteUShort(this.UsWidthClass);
                    ms.WriteShort(this.FsType);
                    ms.WriteShort(this.YSubscriptXSize);
                    ms.WriteShort(this.YSubscriptYSize);
                    ms.WriteShort(this.YSubscriptXOffset);
                    ms.WriteShort(this.YSubscriptYOffset);
                    ms.WriteShort(this.YSuperscriptXSize);
                    ms.WriteShort(this.YSuperscriptYSize);
                    ms.WriteShort(this.YSuperscriptXOffset);
                    ms.WriteShort(this.YSuperscriptYOffset);
                    ms.WriteShort(this.YStrikeoutSize);
                    ms.WriteShort(this.YStrikeoutPosition);
                    ms.WriteByte(this.SFamilyClass);
                    ms.WriteByte(this.SFamilySubClass);
                    ms.Write(this.Panose.GetBytes(), 0, 10);
                    for (int i = 0; i < this.UlUnicodeRange.Length; i++)
                    {
                        ms.WriteUInt(this.UlUnicodeRange[i]);
                    }
                    ms.Write(this.AchVendID, 0, this.AchVendID.Length);
                    ms.WriteUShort(this.FsSelection);
                    ms.WriteUShort(this.FsFirstCharIndex);
                    ms.WriteUShort(this.FsLastCharIndex);

                    return ms.ToArray();
                }
            }

            public struct PANOSE
            {
                public byte BFamilyType;
                public byte BSerifStyle;
                public byte BWeight;
                public byte BProportion;
                public byte BContrast;
                public byte BStrokeVariation;
                public byte BArmStyle;
                public byte BLetterform;
                public byte BMidline;
                public byte BXHeight;

                public PANOSE(byte bFamilyType, byte bSerifStyle, byte bWeight, byte bProportion, byte bContrast, byte bStrokeVariation, byte bArmStyle, byte bLetterform, byte bMidline, byte bXHeight)
                {
                    this.BFamilyType = bFamilyType;
                    this.BSerifStyle = bSerifStyle;
                    this.BWeight = bWeight;
                    this.BProportion = bProportion;
                    this.BContrast = bContrast;
                    this.BStrokeVariation = bStrokeVariation;
                    this.BArmStyle = bArmStyle;
                    this.BLetterform = bLetterform;
                    this.BMidline = bMidline;
                    this.BXHeight = bXHeight;
                }

                public byte[] GetBytes()
                {
                    return new byte[] { BFamilyType, BSerifStyle, BWeight, BProportion, BContrast, BStrokeVariation, BArmStyle, BLetterform, BMidline, BXHeight };
                }
            }
        }

        internal class TrueTypeHmtxTable : ITrueTypeTable
        {
            public LongHorFixed[] HMetrics;
            public short[] LeftSideBearing;

            public byte[] GetBytes()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    for (int i = 0; i < HMetrics.Length; i++)
                    {
                        ms.WriteUShort(HMetrics[i].AdvanceWidth);
                        ms.WriteShort(HMetrics[i].LeftSideBearing);
                    }
                    for (int i = 0; i < LeftSideBearing.Length; i++)
                    {
                        ms.WriteShort(LeftSideBearing[i]);
                    }

                    return ms.ToArray();
                }
            }
        }

        internal class TrueTypeMaxpTable : ITrueTypeTable
        {
            public Fixed Version;
            public ushort NumGlyphs;
            public ushort MaxPoints;
            public ushort MaxContours;
            public ushort MaxComponentPoints;
            public ushort MaxComponentContours;
            public ushort MaxZones;
            public ushort MaxTwilightPoints;
            public ushort MaxStorage;
            public ushort MaxFunctionDefs;
            public ushort MaxInstructionDefs;
            public ushort MaxStackElements;
            public ushort MaxSizeOfInstructions;
            public ushort MaxComponentElements;
            public ushort MaxComponentDepth;

            public byte[] GetBytes()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.WriteFixed(this.Version);
                    ms.WriteUShort(this.NumGlyphs);
                    ms.WriteUShort(this.MaxPoints);
                    ms.WriteUShort(this.MaxContours);
                    ms.WriteUShort(this.MaxComponentPoints);
                    ms.WriteUShort(this.MaxComponentContours);
                    ms.WriteUShort(this.MaxZones);
                    ms.WriteUShort(this.MaxTwilightPoints);
                    ms.WriteUShort(this.MaxStorage);
                    ms.WriteUShort(this.MaxFunctionDefs);
                    ms.WriteUShort(this.MaxInstructionDefs);
                    ms.WriteUShort(this.MaxStackElements);
                    ms.WriteUShort(this.MaxSizeOfInstructions);
                    ms.WriteUShort(this.MaxComponentElements);
                    ms.WriteUShort(this.MaxComponentDepth);
                    return ms.ToArray();
                }
            }
        }

        internal class TrueTypeHeadTable : ITrueTypeTable
        {
            public Fixed Version;
            public Fixed FontRevision;
            public uint ChecksumAdjustment;
            public uint MagicNumber;
            public ushort Flags;
            public ushort UnitsPerEm;
            public DateTime Created;
            public DateTime Modified;
            public short XMin;
            public short YMin;
            public short XMax;
            public short YMax;
            public ushort MacStyle;
            public ushort LowestRecPPEM;
            public short FontDirectionInt;
            public short IndexToLocFormat;
            public short GlyphDataFormat;

            public byte[] GetBytes()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.WriteFixed(this.Version);
                    ms.WriteFixed(this.FontRevision);
                    ms.WriteUInt(this.ChecksumAdjustment);
                    ms.WriteUInt(this.MagicNumber);
                    ms.WriteUShort(this.Flags);
                    ms.WriteUShort(this.UnitsPerEm);
                    ms.WriteDate(this.Created);
                    ms.WriteDate(this.Modified);
                    ms.WriteShort(this.XMin);
                    ms.WriteShort(this.YMin);
                    ms.WriteShort(this.XMax);
                    ms.WriteShort(this.YMax);
                    ms.WriteUShort(this.MacStyle);
                    ms.WriteUShort(this.LowestRecPPEM);
                    ms.WriteShort(this.FontDirectionInt);
                    ms.WriteShort(this.IndexToLocFormat);
                    ms.WriteShort(this.GlyphDataFormat);
                    return ms.ToArray();
                }
            }

        }

        internal class TrueTypeHHeaTable : ITrueTypeTable
        {
            public Fixed Version;
            public short Ascent;
            public short Descent;
            public short LineGap;
            public ushort AdvanceWidthMax;
            public short MinLeftSideBearing;
            public short MinRightSideBearing;
            public short XMaxExtent;
            public short CaretSlopeRise;
            public short CaretSlopeRun;
            public short CaretOffset;
            public short MetricDataFormat;
            public ushort NumOfLongHorMetrics;

            public byte[] GetBytes()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.WriteFixed(this.Version);
                    ms.WriteShort(this.Ascent);
                    ms.WriteShort(this.Descent);
                    ms.WriteShort(this.LineGap);
                    ms.WriteUShort(this.AdvanceWidthMax);
                    ms.WriteShort(this.MinLeftSideBearing);
                    ms.WriteShort(this.MinRightSideBearing);
                    ms.WriteShort(this.XMaxExtent);
                    ms.WriteShort(this.CaretSlopeRise);
                    ms.WriteShort(this.CaretSlopeRun);
                    ms.WriteShort(this.CaretOffset);
                    ms.WriteShort(0);
                    ms.WriteShort(0);
                    ms.WriteShort(0);
                    ms.WriteShort(0);
                    ms.WriteShort(this.MetricDataFormat);
                    ms.WriteUShort(this.NumOfLongHorMetrics);
                    return ms.ToArray();
                }
            }
        }

        internal struct TrueTypeTableOffset
        {
            public uint Checksum;
            public uint Offset;
            public uint Length;

            public TrueTypeTableOffset(uint checksum, uint offset, uint length)
            {
                this.Checksum = checksum;
                this.Offset = offset;
                this.Length = length;
            }
        }

        internal struct LongHorFixed
        {
            public ushort AdvanceWidth;
            public short LeftSideBearing;

            public LongHorFixed(ushort advanceWidth, short leftSideBearing)
            {
                this.AdvanceWidth = advanceWidth;
                this.LeftSideBearing = leftSideBearing;
            }
        }

        internal struct Fixed
        {
            public int Bits;
            public int BitShifts;

            public Fixed(int bits, int bitShifts)
            {
                this.Bits = bits;
                this.BitShifts = bitShifts;
            }
        }
    }

    internal static class ReadUtils
    {
        public static uint ReadUInt(this Stream sr)
        {
            return ((uint)sr.ReadByte() << 24) | ((uint)sr.ReadByte() << 16) | ((uint)sr.ReadByte() << 8) | (uint)sr.ReadByte();
        }

        public static void WriteUInt(this Stream sr, uint val)
        {
            sr.Write(new byte[] { (byte)(val >> 24), (byte)((val >> 16) & 255), (byte)((val >> 8) & 255), (byte)(val & 255) }, 0, 4);
        }

        public static int ReadInt(this Stream sr)
        {
            return (sr.ReadByte() << 24) | (sr.ReadByte() << 16) | (sr.ReadByte() << 8) | sr.ReadByte();
        }

        public static void WriteInt(this Stream sr, int val)
        {
            sr.WriteUInt((uint)val);
        }

        public static ushort ReadUShort(this Stream sr)
        {
            return (ushort)(((uint)sr.ReadByte() << 8) | (uint)sr.ReadByte());
        }

        public static void WriteUShort(this Stream sr, ushort val)
        {
            sr.Write(new byte[] { (byte)(val >> 8), (byte)(val & 255) }, 0, 2);
        }

        public static short ReadShort(this Stream sr)
        {
            ushort result = sr.ReadUShort();
            short tbr;
            if ((result & 0x8000) != 0)
            {
                tbr = (short)(result - (1 << 16));
            }
            else
            {
                tbr = (short)result;
            }
            return tbr;
        }

        public static void WriteShort(this Stream sr, short val)
        {
            sr.WriteUShort((ushort)val);
        }

        public static string ReadString(this Stream sr, int length)
        {
            StringBuilder bld = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                bld.Append((char)sr.ReadByte());
            }
            return bld.ToString();
        }

        public static TrueTypeFile.Fixed ReadFixed(this Stream sr)
        {
            return new TrueTypeFile.Fixed(sr.ReadInt(), 16);
        }

        public static void WriteFixed(this Stream sr, TrueTypeFile.Fixed val)
        {
            sr.WriteInt(val.Bits);
        }

        public static DateTime ReadDate(this Stream sr)
        {
            long macTime = sr.ReadUInt() * 0x100000000 + sr.ReadUInt();
            return new DateTime(1904, 1, 1).AddTicks(macTime * 1000 * TimeSpan.TicksPerMillisecond);
        }

        public static void WriteDate(this Stream sr, DateTime date)
        {
            long macTime = date.Subtract(new DateTime(1904, 1, 1)).Ticks / 1000 / TimeSpan.TicksPerMillisecond;
            sr.WriteUInt((uint)(macTime >> 32));
            sr.WriteUInt((uint)(macTime & 0xFFFFFFFF));
        }
    }

}
