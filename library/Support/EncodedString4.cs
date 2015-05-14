﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PkmnFoundations.Support
{
	public class EncodedString4
    {
        /// <summary>
        /// Instances an EncodedString4 from its binary representation.
        /// </summary>
        /// <param name="data">This buffer is copied.</param>
        public EncodedString4(byte[] data)
        {
            RawData = data;
        }

        /// <summary>
        /// Instances an EncodedString4 from its binary representation.
        /// </summary>
        /// <param name="data">Buffer to copy from</param>
        /// <param name="start">Offset in buffer</param>
        /// <param name="length">Number of bytes (not chars) to copy</param>
        public EncodedString4(byte[] data, int start, int length)
        {
            if (length < 2) throw new ArgumentOutOfRangeException("length");
            if (data.Length < start + length) throw new ArgumentOutOfRangeException("length");
            if (length % 2 != 0) throw new ArgumentException("length");

            m_size = length;
            byte[] trim = new byte[length];
            Array.Copy(data, start, trim, 0, length);
            AssignData(trim);
        }

        /// <summary>
        /// Instances an EncodedString4 from a Unicode string.
        /// </summary>
        /// <param name="text">text</param>
        /// <param name="length">Length of encoded buffer in bytes (not chars)</param>
        public EncodedString4(String text, int length)
        {
            if (length < 2) throw new ArgumentOutOfRangeException("length");
            if (length % 2 != 0) throw new ArgumentException("length");

            m_size = length;
            Text = text;
        }

        // todo: Use pointers for both of these
		public static string DecodeString(byte[] data, int start, int count)
		{
            if (data.Length < start + count) throw new ArgumentOutOfRangeException("count");
            if (count < 0) throw new ArgumentOutOfRangeException("count");

			StringBuilder sb = new StringBuilder();

            for (int i = start; i < start + count * 2; i += 2)
			{
				ushort gamecode = BitConverter.ToUInt16(data, i);
				if (gamecode == 0xffff) { break; }
				char ch = Generation4TextLookupTable.ContainsKey(gamecode) ?
                    Generation4TextLookupTable[gamecode] :
                    '?';

				sb.Append(ch);
			}

			return sb.ToString();
		}

        public static String DecodeString(byte[] data)
        {
            return DecodeString(data, 0, data.Length >> 1);
        }

        public static byte[] EncodeString(String str, int size)
        {
            int actualLength = (size >> 1) - 1;
            if (str.Length > actualLength) throw new ArgumentOutOfRangeException("size");

            byte[] result = new byte[size];
            MemoryStream m = new MemoryStream(result);

            foreach (char c in str.ToCharArray())
            {
                m.Write(BitConverter.GetBytes(LookupReverse.ContainsKey(c) ? LookupReverse[c] : (ushort)0x01ac), 0, 2);
            }

            m.WriteByte(0xff);
            m.WriteByte(0xff);
            return result;
        }

        private int m_size;
		private byte[] m_raw_data;
		private string m_text;

        public int Size
        {
            get
            {
                return m_size;
            }
            // todo: set
        }

		public string Text
        {
            get
            {
                if (m_text == null && m_raw_data == null) return null;
                if (m_text == null) m_text = DecodeString(m_raw_data);
                return m_text;
            }
            set
            {
                int actualLength = (m_size >> 1) - 1;
                if (value.Length > actualLength) throw new ArgumentException();
                AssignText(value);
            }
        }

		public byte[] RawData
        {
            get
            {
                if (m_raw_data == null && m_text == null) return null;
                if (m_raw_data == null) m_raw_data = EncodeString(m_text, m_size);
                return m_raw_data.ToArray();
            }
            set
            {
                int size = value.Length;
                if (size < 2) throw new ArgumentException();
                if (size % 2 != 0) throw new ArgumentException();

                m_size = size;
                AssignData(value.ToArray());
            }
        }

        // lazy evaluate these conversions since they're slow
        private void AssignData(byte[] data)
        {
            m_raw_data = data;
            m_text = null;
        }

        private void AssignText(String text)
        {
            m_text = text;
            m_raw_data = null;
        }
		
		public override string ToString()
		{
			return Text;
		}

        private static Dictionary<char, ushort> m_lookup_reverse = null;
        private static Dictionary<char, ushort> LookupReverse
        {
            get
            {
                if (m_lookup_reverse == null)
                {
                    Dictionary<char, ushort> reverse = new Dictionary<char, ushort>(Generation4TextLookupTable.Count);

                    foreach (KeyValuePair<ushort, char> pair in Generation4TextLookupTable)
                    {
                        //if (!reverse.ContainsKey(pair.Value))
                            reverse.Add(pair.Value, pair.Key);
                    }

                    m_lookup_reverse = reverse;
                }
                return m_lookup_reverse;
            }
        }

        // Lookup table courtesy of Aqua
        // https://github.com/projectpokemon/PPRE/blob/master/Table.tbl
		private static Dictionary<ushort, char> Generation4TextLookupTable = new Dictionary<ushort, char>
		{
            {0x0001, '\u3000'}, {0x0002, '\u3041'}, {0x0003, '\u3042'}, {0x0004, '\u3043'},
            {0x0005, '\u3044'}, {0x0006, '\u3045'}, {0x0007, '\u3046'}, {0x0008, '\u3047'},
            {0x0009, '\u3048'}, {0x000a, '\u3049'}, {0x000b, '\u304a'}, {0x000c, '\u304b'},
            {0x000d, '\u304c'}, {0x000e, '\u304d'}, {0x000f, '\u304e'}, {0x0010, '\u304f'},
            {0x0011, '\u3050'}, {0x0012, '\u3051'}, {0x0013, '\u3052'}, {0x0014, '\u3053'},
            {0x0015, '\u3054'}, {0x0016, '\u3055'}, {0x0017, '\u3056'}, {0x0018, '\u3057'},
            {0x0019, '\u3058'}, {0x001a, '\u3059'}, {0x001b, '\u305a'}, {0x001c, '\u305b'},
            {0x001d, '\u305c'}, {0x001e, '\u305d'}, {0x001f, '\u305e'}, {0x0020, '\u305f'},
            {0x0021, '\u3060'}, {0x0022, '\u3061'}, {0x0023, '\u3062'}, {0x0024, '\u3063'},
            {0x0025, '\u3064'}, {0x0026, '\u3065'}, {0x0027, '\u3066'}, {0x0028, '\u3067'},
            {0x0029, '\u3068'}, {0x002a, '\u3069'}, {0x002b, '\u306a'}, {0x002c, '\u306b'},
            {0x002d, '\u306c'}, {0x002e, '\u306d'}, {0x002f, '\u306e'}, {0x0030, '\u306f'},
            {0x0031, '\u3070'}, {0x0032, '\u3071'}, {0x0033, '\u3072'}, {0x0034, '\u3073'},
            {0x0035, '\u3074'}, {0x0036, '\u3075'}, {0x0037, '\u3076'}, {0x0038, '\u3077'},
            {0x0039, '\u3078'}, {0x003a, '\u3079'}, {0x003b, '\u307a'}, {0x003c, '\u307b'},
            {0x003d, '\u307c'}, {0x003e, '\u307d'}, {0x003f, '\u307e'}, {0x0040, '\u307f'},
            {0x0041, '\u3080'}, {0x0042, '\u3081'}, {0x0043, '\u3082'}, {0x0044, '\u3083'},
            {0x0045, '\u3084'}, {0x0046, '\u3085'}, {0x0047, '\u3086'}, {0x0048, '\u3087'},
            {0x0049, '\u3088'}, {0x004a, '\u3089'}, {0x004b, '\u308a'}, {0x004c, '\u308b'},
            {0x004d, '\u308c'}, {0x004e, '\u308d'}, {0x004f, '\u308f'}, {0x0050, '\u3092'},
            {0x0051, '\u3093'}, {0x0052, '\u30a1'}, {0x0053, '\u30a2'}, {0x0054, '\u30a3'},
            {0x0055, '\u30a4'}, {0x0056, '\u30a5'}, {0x0057, '\u30a6'}, {0x0058, '\u30a7'},
            {0x0059, '\u30a8'}, {0x005a, '\u30a9'}, {0x005b, '\u30aa'}, {0x005c, '\u30ab'},
            {0x005d, '\u30ac'}, {0x005e, '\u30ad'}, {0x005f, '\u30ae'}, {0x0060, '\u30af'},
            {0x0061, '\u30b0'}, {0x0062, '\u30b1'}, {0x0063, '\u30b2'}, {0x0064, '\u30b3'},
            {0x0065, '\u30b4'}, {0x0066, '\u30b5'}, {0x0067, '\u30b6'}, {0x0068, '\u30b7'},
            {0x0069, '\u30b8'}, {0x006a, '\u30b9'}, {0x006b, '\u30ba'}, {0x006c, '\u30bb'},
            {0x006d, '\u30bc'}, {0x006e, '\u30bd'}, {0x006f, '\u30be'}, {0x0070, '\u30bf'},
            {0x0071, '\u30c0'}, {0x0072, '\u30c1'}, {0x0073, '\u30c2'}, {0x0074, '\u30c3'},
            {0x0075, '\u30c4'}, {0x0076, '\u30c5'}, {0x0077, '\u30c6'}, {0x0078, '\u30c7'},
            {0x0079, '\u30c8'}, {0x007a, '\u30c9'}, {0x007b, '\u30ca'}, {0x007c, '\u30cb'},
            {0x007d, '\u30cc'}, {0x007e, '\u30cd'}, {0x007f, '\u30ce'}, {0x0080, '\u30cf'},
            {0x0081, '\u30d0'}, {0x0082, '\u30d1'}, {0x0083, '\u30d2'}, {0x0084, '\u30d3'},
            {0x0085, '\u30d4'}, {0x0086, '\u30d5'}, {0x0087, '\u30d6'}, {0x0088, '\u30d7'},
            {0x0089, '\u30d8'}, {0x008a, '\u30d9'}, {0x008b, '\u30da'}, {0x008c, '\u30db'},
            {0x008d, '\u30dc'}, {0x008e, '\u30dd'}, {0x008f, '\u30de'}, {0x0090, '\u30df'},
            {0x0091, '\u30e0'}, {0x0092, '\u30e1'}, {0x0093, '\u30e2'}, {0x0094, '\u30e3'},
            {0x0095, '\u30e4'}, {0x0096, '\u30e5'}, {0x0097, '\u30e6'}, {0x0098, '\u30e7'},
            {0x0099, '\u30e8'}, {0x009a, '\u30e9'}, {0x009b, '\u30ea'}, {0x009c, '\u30eb'},
            {0x009d, '\u30ec'}, {0x009e, '\u30ed'}, {0x009f, '\u30ef'}, {0x00a0, '\u30f2'},
            {0x00a1, '\u30f3'}, {0x00a2, '\uff10'}, {0x00a3, '\uff11'}, {0x00a4, '\uff12'},
            {0x00a5, '\uff13'}, {0x00a6, '\uff14'}, {0x00a7, '\uff15'}, {0x00a8, '\uff16'},
            {0x00a9, '\uff17'}, {0x00aa, '\uff18'}, {0x00ab, '\uff19'}, {0x00ac, '\uff21'},
            {0x00ad, '\uff22'}, {0x00ae, '\uff23'}, {0x00af, '\uff24'}, {0x00b0, '\uff25'},
            {0x00b1, '\uff26'}, {0x00b2, '\uff27'}, {0x00b3, '\uff28'}, {0x00b4, '\uff29'},
            {0x00b5, '\uff2a'}, {0x00b6, '\uff2b'}, {0x00b7, '\uff2c'}, {0x00b8, '\uff2d'},
            {0x00b9, '\uff2e'}, {0x00ba, '\uff2f'}, {0x00bb, '\uff30'}, {0x00bc, '\uff31'},
            {0x00bd, '\uff32'}, {0x00be, '\uff33'}, {0x00bf, '\uff34'}, {0x00c0, '\uff35'},
            {0x00c1, '\uff36'}, {0x00c2, '\uff37'}, {0x00c3, '\uff38'}, {0x00c4, '\uff39'},
            {0x00c5, '\uff3a'}, {0x00c6, '\uff41'}, {0x00c7, '\uff42'}, {0x00c8, '\uff43'},
            {0x00c9, '\uff44'}, {0x00ca, '\uff45'}, {0x00cb, '\uff46'}, {0x00cc, '\uff47'},
            {0x00cd, '\uff48'}, {0x00ce, '\uff49'}, {0x00cf, '\uff4a'}, {0x00d0, '\uff4b'},
            {0x00d1, '\uff4c'}, {0x00d2, '\uff4d'}, {0x00d3, '\uff4e'}, {0x00d4, '\uff4f'},
            {0x00d5, '\uff50'}, {0x00d6, '\uff51'}, {0x00d7, '\uff52'}, {0x00d8, '\uff53'},
            {0x00d9, '\uff54'}, {0x00da, '\uff55'}, {0x00db, '\uff56'}, {0x00dc, '\uff57'},
            {0x00dd, '\uff58'}, {0x00de, '\uff59'}, {0x00df, '\uff5a'}, {0x00e1, '\uff01'},
            {0x00e2, '\uff1f'}, {0x00e3, '\u3001'}, {0x00e4, '\u3002'}, {0x00e5, '\u22ef'},
            {0x00e6, '\u30fb'}, {0x00e7, '\uff0f'}, {0x00e8, '\u300c'}, {0x00e9, '\u300d'},
            {0x00ea, '\u300e'}, {0x00eb, '\u300f'}, {0x00ec, '\uff08'}, {0x00ed, '\uff09'},
            {0x00ee, '\u2642'}, {0x00ef, '\u2640'}, {0x00f0, '\uff0b'}, {0x00f1, '\uff0d'},
            {0x00f2, '\u00d7'}, {0x00f3, '\u00f7'}, {0x00f4, '\uff1d'}, {0x00f5, '\uff5a'},
            {0x00f6, '\uff1a'}, {0x00f7, '\uff1b'}, {0x00f8, '\uff0e'}, {0x00f9, '\uff0c'},
            {0x00fa, '\u2664'}, {0x00fb, '\u2667'}, {0x00fc, '\u2665'}, {0x00fd, '\u2662'},
            {0x00fe, '\u2606'}, {0x00ff, '\u25ce'}, {0x0100, '\u25cb'}, {0x0101, '\u25a1'},
            {0x0102, '\u25b3'}, {0x0103, '\u25c7'}, {0x0104, '\uff20'}, {0x0105, '\u266a'},
            {0x0106, '\uff05'}, {0x0107, '\u2600'}, {0x0108, '\u2601'}, {0x0109, '\u2602'},
            {0x010a, '\u2744'}, {0x010b, '\u260b'}, {0x010c, '\u2654'}, {0x010d, '\u2655'},
            {0x010e, '\u260a'}, {0x010f, '\u21d7'}, {0x0110, '\u21d8'}, {0x0111, '\u263e'},
            {0x0112, '\u00a5'}, {0x0113, '\u2648'}, {0x0114, '\u2649'}, {0x0115, '\u264a'},
            {0x0116, '\u264b'}, {0x0117, '\u264c'}, {0x0118, '\u264d'}, {0x0119, '\u264e'},
            {0x011a, '\u264f'}, {0x011b, '\u2190'}, {0x011c, '\u2191'}, {0x011d, '\u2193'},
            {0x011e, '\u2192'}, {0x011f, '\u2023'}, {0x0120, '\uff06'}, {0x0121, '\u0030'},
            {0x0122, '\u0031'}, {0x0123, '\u0032'}, {0x0124, '\u0033'}, {0x0125, '\u0034'},
            {0x0126, '\u0035'}, {0x0127, '\u0036'}, {0x0128, '\u0037'}, {0x0129, '\u0038'},
            {0x012a, '\u0039'}, {0x012b, '\u0041'}, {0x012c, '\u0042'}, {0x012d, '\u0043'},
            {0x012e, '\u0044'}, {0x012f, '\u0045'}, {0x0130, '\u0046'}, {0x0131, '\u0047'},
            {0x0132, '\u0048'}, {0x0133, '\u0049'}, {0x0134, '\u004a'}, {0x0135, '\u004b'},
            {0x0136, '\u004c'}, {0x0137, '\u004d'}, {0x0138, '\u004e'}, {0x0139, '\u004f'},
            {0x013a, '\u0050'}, {0x013b, '\u0051'}, {0x013c, '\u0052'}, {0x013d, '\u0053'},
            {0x013e, '\u0054'}, {0x013f, '\u0055'}, {0x0140, '\u0056'}, {0x0141, '\u0057'},
            {0x0142, '\u0058'}, {0x0143, '\u0059'}, {0x0144, '\u005a'}, {0x0145, '\u0061'},
            {0x0146, '\u0062'}, {0x0147, '\u0063'}, {0x0148, '\u0064'}, {0x0149, '\u0065'},
            {0x014a, '\u0066'}, {0x014b, '\u0067'}, {0x014c, '\u0068'}, {0x014d, '\u0069'},
            {0x014e, '\u006a'}, {0x014f, '\u006b'}, {0x0150, '\u006c'}, {0x0151, '\u006d'},
            {0x0152, '\u006e'}, {0x0153, '\u006f'}, {0x0154, '\u0070'}, {0x0155, '\u0071'},
            {0x0156, '\u0072'}, {0x0157, '\u0073'}, {0x0158, '\u0074'}, {0x0159, '\u0075'},
            {0x015a, '\u0076'}, {0x015b, '\u0077'}, {0x015c, '\u0078'}, {0x015d, '\u0079'},
            {0x015e, '\u007a'}, {0x015f, '\u00c0'}, {0x0160, '\u00c1'}, {0x0161, '\u00c2'},
            {0x0162, '\u00c3'}, {0x0163, '\u00c4'}, {0x0164, '\u00c5'}, {0x0165, '\u00c6'},
            {0x0166, '\u00c7'}, {0x0167, '\u00c8'}, {0x0168, '\u00c9'}, {0x0169, '\u00ca'},
            {0x016a, '\u00cb'}, {0x016b, '\u00cc'}, {0x016c, '\u00cd'}, {0x016d, '\u00ce'},
            {0x016e, '\u00cf'}, {0x016f, '\u00d0'}, {0x0170, '\u00d1'}, {0x0171, '\u00d2'},
            {0x0172, '\u00d3'}, {0x0173, '\u00d4'}, {0x0174, '\u00d5'}, {0x0175, '\u00d6'},
            {0x0176, '\u00d7'}, {0x0177, '\u00d8'}, {0x0178, '\u00d9'}, {0x0179, '\u00da'},
            {0x017a, '\u00db'}, {0x017b, '\u00dc'}, {0x017c, '\u00dd'}, {0x017d, '\u00de'},
            {0x017e, '\u00df'}, {0x017f, '\u00e0'}, {0x0180, '\u00e1'}, {0x0181, '\u00e2'},
            {0x0182, '\u00e3'}, {0x0183, '\u00e4'}, {0x0184, '\u00e5'}, {0x0185, '\u00e6'},
            {0x0186, '\u00e7'}, {0x0187, '\u00e8'}, {0x0188, '\u00e9'}, {0x0189, '\u00ea'},
            {0x018a, '\u00eb'}, {0x018b, '\u00ec'}, {0x018c, '\u00ed'}, {0x018d, '\u00ee'},
            {0x018e, '\u00ef'}, {0x018f, '\u00f0'}, {0x0190, '\u00f1'}, {0x0191, '\u00f2'},
            {0x0192, '\u00f3'}, {0x0193, '\u00f4'}, {0x0194, '\u00f5'}, {0x0195, '\u00f6'},
            {0x0196, '\u00f7'}, {0x0197, '\u00f8'}, {0x0198, '\u00f9'}, {0x0199, '\u00fa'},
            {0x019a, '\u00fb'}, {0x019b, '\u00fc'}, {0x019c, '\u00fd'}, {0x019d, '\u00fe'},
            {0x019e, '\u00ff'}, {0x019f, '\u0152'}, {0x01a0, '\u0153'}, {0x01a1, '\u015e'},
            {0x01a2, '\u015f'}, {0x01a3, '\u00aa'}, {0x01a4, '\u00ba'}, {0x01a5, '\u00b9'},
            {0x01a6, '\u00b2'}, {0x01a7, '\u00b3'}, {0x01a8, '\u0024'}, {0x01a9, '\u00a1'},
            {0x01aa, '\u00bf'}, {0x01ab, '\u0021'}, {0x01ac, '\u003f'}, {0x01ad, '\u002c'},
            {0x01ae, '\u002e'}, {0x01af, '\u2026'}, {0x01b0, '\uff65'}, {0x01b1, '\u002f'},
            {0x01b2, '\u2018'}, {0x01b3, '\u2019'}, {0x01b4, '\u201c'}, {0x01b5, '\u201d'},
            {0x01b6, '\u201e'}, {0x01b7, '\u300a'}, {0x01b8, '\u300b'}, {0x01b9, '\u0028'},
            {0x01ba, '\u0029'}, {0x01bb, '\u2642'}, {0x01bc, '\u2640'}, {0x01bd, '\u002b'},
            {0x01be, '\u002d'}, {0x01bf, '\u002a'}, {0x01c0, '\u0023'}, {0x01c1, '\u003d'},
            {0x01c2, '\u0026'}, {0x01c3, '\u007e'}, {0x01c4, '\u003a'}, {0x01c5, '\u003b'},
            {0x01c6, '\u246f'}, {0x01c7, '\u2470'}, {0x01c8, '\u2471'}, {0x01c9, '\u2472'},
            {0x01ca, '\u2473'}, {0x01cb, '\u2474'}, {0x01cc, '\u2475'}, {0x01cd, '\u2476'},
            {0x01ce, '\u2477'}, {0x01cf, '\u2478'}, {0x01d0, '\u0040'}, {0x01d1, '\u2479'},
            {0x01d2, '\u0025'}, {0x01d3, '\u247a'}, {0x01d4, '\u247b'}, {0x01d5, '\u247c'},
            {0x01d6, '\u247d'}, {0x01d7, '\u247e'}, {0x01d8, '\u247f'}, {0x01d9, '\u2480'},
            {0x01da, '\u2481'}, {0x01db, '\u2482'}, {0x01dc, '\u2483'}, {0x01dd, '\u2484'},
            {0x01de, '\u0020'}, {0x01df, '\u2485'}, {0x01e0, '\u2486'}, {0x01e1, '\u2487'},
            {0x01e8, '\u00b0'}, {0x01e9, '\u005f'}, {0x01ea, '\uff3f'}, {0x0400, '\uac00'},
            {0x0401, '\uac01'}, {0x0402, '\uac04'}, {0x0403, '\uac07'}, {0x0404, '\uac08'},
            {0x0405, '\uac09'}, {0x0406, '\uac0a'}, {0x0407, '\uac10'}, {0x0408, '\uac11'},
            {0x0409, '\uac12'}, {0x040a, '\uac13'}, {0x040b, '\uac14'}, {0x040c, '\uac15'},
            {0x040d, '\uac16'}, {0x040e, '\uac17'}, {0x0410, '\uac19'}, {0x0411, '\uac1a'},
            {0x0412, '\uac1b'}, {0x0413, '\uac1c'}, {0x0414, '\uac1d'}, {0x0415, '\uac20'},
            {0x0416, '\uac24'}, {0x0417, '\uac2c'}, {0x0418, '\uac2d'}, {0x0419, '\uac2f'},
            {0x041a, '\uac30'}, {0x041b, '\uac31'}, {0x041c, '\uac38'}, {0x041d, '\uac39'},
            {0x041e, '\uac3c'}, {0x041f, '\uac40'}, {0x0420, '\uac4b'}, {0x0421, '\uac4d'},
            {0x0422, '\uac54'}, {0x0423, '\uac58'}, {0x0424, '\uac5c'}, {0x0425, '\uac70'},
            {0x0426, '\uac71'}, {0x0427, '\uac74'}, {0x0428, '\uac77'}, {0x0429, '\uac78'},
            {0x042a, '\uac7a'}, {0x042b, '\uac80'}, {0x042c, '\uac81'}, {0x042d, '\uac83'},
            {0x042e, '\uac84'}, {0x042f, '\uac85'}, {0x0430, '\uac86'}, {0x0431, '\uac89'},
            {0x0432, '\uac8a'}, {0x0433, '\uac8b'}, {0x0434, '\uac8c'}, {0x0435, '\uac90'},
            {0x0436, '\uac94'}, {0x0437, '\uac9c'}, {0x0438, '\uac9d'}, {0x0439, '\uac9f'},
            {0x043a, '\uaca0'}, {0x043b, '\uaca1'}, {0x043c, '\uaca8'}, {0x043d, '\uaca9'},
            {0x043e, '\uacaa'}, {0x043f, '\uacac'}, {0x0440, '\uacaf'}, {0x0441, '\uacb0'},
            {0x0442, '\uacb8'}, {0x0443, '\uacb9'}, {0x0444, '\uacbb'}, {0x0445, '\uacbc'},
            {0x0446, '\uacbd'}, {0x0447, '\uacc1'}, {0x0448, '\uacc4'}, {0x0449, '\uacc8'},
            {0x044a, '\uaccc'}, {0x044b, '\uacd5'}, {0x044c, '\uacd7'}, {0x044d, '\uace0'},
            {0x044e, '\uace1'}, {0x044f, '\uace4'}, {0x0450, '\uace7'}, {0x0451, '\uace8'},
            {0x0452, '\uacea'}, {0x0453, '\uacec'}, {0x0454, '\uacef'}, {0x0455, '\uacf0'},
            {0x0456, '\uacf1'}, {0x0457, '\uacf3'}, {0x0458, '\uacf5'}, {0x0459, '\uacf6'},
            {0x045a, '\uacfc'}, {0x045b, '\uacfd'}, {0x045c, '\uad00'}, {0x045d, '\uad04'},
            {0x045e, '\uad06'}, {0x045f, '\uad0c'}, {0x0460, '\uad0d'}, {0x0461, '\uad0f'},
            {0x0462, '\uad11'}, {0x0463, '\uad18'}, {0x0464, '\uad1c'}, {0x0465, '\uad20'},
            {0x0466, '\uad29'}, {0x0467, '\uad2c'}, {0x0468, '\uad2d'}, {0x0469, '\uad34'},
            {0x046a, '\uad35'}, {0x046b, '\uad38'}, {0x046c, '\uad3c'}, {0x046d, '\uad44'},
            {0x046e, '\uad45'}, {0x046f, '\uad47'}, {0x0470, '\uad49'}, {0x0471, '\uad50'},
            {0x0472, '\uad54'}, {0x0473, '\uad58'}, {0x0474, '\uad61'}, {0x0475, '\uad63'},
            {0x0476, '\uad6c'}, {0x0477, '\uad6d'}, {0x0478, '\uad70'}, {0x0479, '\uad73'},
            {0x047a, '\uad74'}, {0x047b, '\uad75'}, {0x047c, '\uad76'}, {0x047d, '\uad7b'},
            {0x047e, '\uad7c'}, {0x047f, '\uad7d'}, {0x0480, '\uad7f'}, {0x0481, '\uad81'},
            {0x0482, '\uad82'}, {0x0483, '\uad88'}, {0x0484, '\uad89'}, {0x0485, '\uad8c'},
            {0x0486, '\uad90'}, {0x0487, '\uad9c'}, {0x0488, '\uad9d'}, {0x0489, '\uada4'},
            {0x048a, '\uadb7'}, {0x048b, '\uadc0'}, {0x048c, '\uadc1'}, {0x048d, '\uadc4'},
            {0x048e, '\uadc8'}, {0x048f, '\uadd0'}, {0x0490, '\uadd1'}, {0x0491, '\uadd3'},
            {0x0492, '\uaddc'}, {0x0493, '\uade0'}, {0x0494, '\uade4'}, {0x0495, '\uadf8'},
            {0x0496, '\uadf9'}, {0x0497, '\uadfc'}, {0x0498, '\uadff'}, {0x0499, '\uae00'},
            {0x049a, '\uae01'}, {0x049b, '\uae08'}, {0x049c, '\uae09'}, {0x049d, '\uae0b'},
            {0x049e, '\uae0d'}, {0x049f, '\uae14'}, {0x04a0, '\uae30'}, {0x04a1, '\uae31'},
            {0x04a2, '\uae34'}, {0x04a3, '\uae37'}, {0x04a4, '\uae38'}, {0x04a5, '\uae3a'},
            {0x04a6, '\uae40'}, {0x04a7, '\uae41'}, {0x04a8, '\uae43'}, {0x04a9, '\uae45'},
            {0x04aa, '\uae46'}, {0x04ab, '\uae4a'}, {0x04ac, '\uae4c'}, {0x04ad, '\uae4d'},
            {0x04ae, '\uae4e'}, {0x04af, '\uae50'}, {0x04b0, '\uae54'}, {0x04b1, '\uae56'},
            {0x04b2, '\uae5c'}, {0x04b3, '\uae5d'}, {0x04b4, '\uae5f'}, {0x04b5, '\uae60'},
            {0x04b6, '\uae61'}, {0x04b7, '\uae65'}, {0x04b8, '\uae68'}, {0x04b9, '\uae69'},
            {0x04ba, '\uae6c'}, {0x04bb, '\uae70'}, {0x04bc, '\uae78'}, {0x04bd, '\uae79'},
            {0x04be, '\uae7b'}, {0x04bf, '\uae7c'}, {0x04c0, '\uae7d'}, {0x04c1, '\uae84'},
            {0x04c2, '\uae85'}, {0x04c3, '\uae8c'}, {0x04c4, '\uaebc'}, {0x04c5, '\uaebd'},
            {0x04c6, '\uaebe'}, {0x04c7, '\uaec0'}, {0x04c8, '\uaec4'}, {0x04c9, '\uaecc'},
            {0x04ca, '\uaecd'}, {0x04cb, '\uaecf'}, {0x04cc, '\uaed0'}, {0x04cd, '\uaed1'},
            {0x04ce, '\uaed8'}, {0x04cf, '\uaed9'}, {0x04d0, '\uaedc'}, {0x04d1, '\uaee8'},
            {0x04d2, '\uaeeb'}, {0x04d3, '\uaeed'}, {0x04d4, '\uaef4'}, {0x04d5, '\uaef8'},
            {0x04d6, '\uaefc'}, {0x04d7, '\uaf07'}, {0x04d8, '\uaf08'}, {0x04d9, '\uaf0d'},
            {0x04da, '\uaf10'}, {0x04db, '\uaf2c'}, {0x04dc, '\uaf2d'}, {0x04dd, '\uaf30'},
            {0x04de, '\uaf32'}, {0x04df, '\uaf34'}, {0x04e0, '\uaf3c'}, {0x04e1, '\uaf3d'},
            {0x04e2, '\uaf3f'}, {0x04e3, '\uaf41'}, {0x04e4, '\uaf42'}, {0x04e5, '\uaf43'},
            {0x04e6, '\uaf48'}, {0x04e7, '\uaf49'}, {0x04e8, '\uaf50'}, {0x04e9, '\uaf5c'},
            {0x04ea, '\uaf5d'}, {0x04eb, '\uaf64'}, {0x04ec, '\uaf65'}, {0x04ed, '\uaf79'},
            {0x04ee, '\uaf80'}, {0x04ef, '\uaf84'}, {0x04f0, '\uaf88'}, {0x04f1, '\uaf90'},
            {0x04f2, '\uaf91'}, {0x04f3, '\uaf95'}, {0x04f4, '\uaf9c'}, {0x04f5, '\uafb8'},
            {0x04f6, '\uafb9'}, {0x04f7, '\uafbc'}, {0x04f8, '\uafc0'}, {0x04f9, '\uafc7'},
            {0x04fa, '\uafc8'}, {0x04fb, '\uafc9'}, {0x04fc, '\uafcb'}, {0x04fd, '\uafcd'},
            {0x04fe, '\uafce'}, {0x04ff, '\uafd4'}, {0x0500, '\uafdc'}, {0x0501, '\uafe8'},
            {0x0502, '\uafe9'}, {0x0503, '\uaff0'}, {0x0504, '\uaff1'}, {0x0505, '\uaff4'},
            {0x0506, '\uaff8'}, {0x0507, '\ub000'}, {0x0508, '\ub001'}, {0x0509, '\ub004'},
            {0x050a, '\ub00c'}, {0x050b, '\ub010'}, {0x050c, '\ub014'}, {0x050d, '\ub01c'},
            {0x050e, '\ub01d'}, {0x050f, '\ub028'}, {0x0510, '\ub044'}, {0x0511, '\ub045'},
            {0x0512, '\ub048'}, {0x0513, '\ub04a'}, {0x0514, '\ub04c'}, {0x0515, '\ub04e'},
            {0x0516, '\ub053'}, {0x0517, '\ub054'}, {0x0518, '\ub055'}, {0x0519, '\ub057'},
            {0x051a, '\ub059'}, {0x051b, '\ub05d'}, {0x051c, '\ub07c'}, {0x051d, '\ub07d'},
            {0x051e, '\ub080'}, {0x051f, '\ub084'}, {0x0520, '\ub08c'}, {0x0521, '\ub08d'},
            {0x0522, '\ub08f'}, {0x0523, '\ub091'}, {0x0524, '\ub098'}, {0x0525, '\ub099'},
            {0x0526, '\ub09a'}, {0x0527, '\ub09c'}, {0x0528, '\ub09f'}, {0x0529, '\ub0a0'},
            {0x052a, '\ub0a1'}, {0x052b, '\ub0a2'}, {0x052c, '\ub0a8'}, {0x052d, '\ub0a9'},
            {0x052e, '\ub0ab'}, {0x052f, '\ub0ac'}, {0x0530, '\ub0ad'}, {0x0531, '\ub0ae'},
            {0x0532, '\ub0af'}, {0x0533, '\ub0b1'}, {0x0534, '\ub0b3'}, {0x0535, '\ub0b4'},
            {0x0536, '\ub0b5'}, {0x0537, '\ub0b8'}, {0x0538, '\ub0bc'}, {0x0539, '\ub0c4'},
            {0x053a, '\ub0c5'}, {0x053b, '\ub0c7'}, {0x053c, '\ub0c8'}, {0x053d, '\ub0c9'},
            {0x053e, '\ub0d0'}, {0x053f, '\ub0d1'}, {0x0540, '\ub0d4'}, {0x0541, '\ub0d8'},
            {0x0542, '\ub0e0'}, {0x0543, '\ub0e5'}, {0x0544, '\ub108'}, {0x0545, '\ub109'},
            {0x0546, '\ub10b'}, {0x0547, '\ub10c'}, {0x0548, '\ub110'}, {0x0549, '\ub112'},
            {0x054a, '\ub113'}, {0x054b, '\ub118'}, {0x054c, '\ub119'}, {0x054d, '\ub11b'},
            {0x054e, '\ub11c'}, {0x054f, '\ub11d'}, {0x0550, '\ub123'}, {0x0551, '\ub124'},
            {0x0552, '\ub125'}, {0x0553, '\ub128'}, {0x0554, '\ub12c'}, {0x0555, '\ub134'},
            {0x0556, '\ub135'}, {0x0557, '\ub137'}, {0x0558, '\ub138'}, {0x0559, '\ub139'},
            {0x055a, '\ub140'}, {0x055b, '\ub141'}, {0x055c, '\ub144'}, {0x055d, '\ub148'},
            {0x055e, '\ub150'}, {0x055f, '\ub151'}, {0x0560, '\ub154'}, {0x0561, '\ub155'},
            {0x0562, '\ub158'}, {0x0563, '\ub15c'}, {0x0564, '\ub160'}, {0x0565, '\ub178'},
            {0x0566, '\ub179'}, {0x0567, '\ub17c'}, {0x0568, '\ub180'}, {0x0569, '\ub182'},
            {0x056a, '\ub188'}, {0x056b, '\ub189'}, {0x056c, '\ub18b'}, {0x056d, '\ub18d'},
            {0x056e, '\ub192'}, {0x056f, '\ub193'}, {0x0570, '\ub194'}, {0x0571, '\ub198'},
            {0x0572, '\ub19c'}, {0x0573, '\ub1a8'}, {0x0574, '\ub1cc'}, {0x0575, '\ub1d0'},
            {0x0576, '\ub1d4'}, {0x0577, '\ub1dc'}, {0x0578, '\ub1dd'}, {0x0579, '\ub1df'},
            {0x057a, '\ub1e8'}, {0x057b, '\ub1e9'}, {0x057c, '\ub1ec'}, {0x057d, '\ub1f0'},
            {0x057e, '\ub1f9'}, {0x057f, '\ub1fb'}, {0x0580, '\ub1fd'}, {0x0581, '\ub204'},
            {0x0582, '\ub205'}, {0x0583, '\ub208'}, {0x0584, '\ub20b'}, {0x0585, '\ub20c'},
            {0x0586, '\ub214'}, {0x0587, '\ub215'}, {0x0588, '\ub217'}, {0x0589, '\ub219'},
            {0x058a, '\ub220'}, {0x058b, '\ub234'}, {0x058c, '\ub23c'}, {0x058d, '\ub258'},
            {0x058e, '\ub25c'}, {0x058f, '\ub260'}, {0x0590, '\ub268'}, {0x0591, '\ub269'},
            {0x0592, '\ub274'}, {0x0593, '\ub275'}, {0x0594, '\ub27c'}, {0x0595, '\ub284'},
            {0x0596, '\ub285'}, {0x0597, '\ub289'}, {0x0598, '\ub290'}, {0x0599, '\ub291'},
            {0x059a, '\ub294'}, {0x059b, '\ub298'}, {0x059c, '\ub299'}, {0x059d, '\ub29a'},
            {0x059e, '\ub2a0'}, {0x059f, '\ub2a1'}, {0x05a0, '\ub2a3'}, {0x05a1, '\ub2a5'},
            {0x05a2, '\ub2a6'}, {0x05a3, '\ub2aa'}, {0x05a4, '\ub2ac'}, {0x05a5, '\ub2b0'},
            {0x05a6, '\ub2b4'}, {0x05a7, '\ub2c8'}, {0x05a8, '\ub2c9'}, {0x05a9, '\ub2cc'},
            {0x05aa, '\ub2d0'}, {0x05ab, '\ub2d2'}, {0x05ac, '\ub2d8'}, {0x05ad, '\ub2d9'},
            {0x05ae, '\ub2db'}, {0x05af, '\ub2dd'}, {0x05b0, '\ub2e2'}, {0x05b1, '\ub2e4'},
            {0x05b2, '\ub2e5'}, {0x05b3, '\ub2e6'}, {0x05b4, '\ub2e8'}, {0x05b5, '\ub2eb'},
            {0x05b6, '\ub2ec'}, {0x05b7, '\ub2ed'}, {0x05b8, '\ub2ee'}, {0x05b9, '\ub2ef'},
            {0x05ba, '\ub2f3'}, {0x05bb, '\ub2f4'}, {0x05bc, '\ub2f5'}, {0x05bd, '\ub2f7'},
            {0x05be, '\ub2f8'}, {0x05bf, '\ub2f9'}, {0x05c0, '\ub2fa'}, {0x05c1, '\ub2fb'},
            {0x05c2, '\ub2ff'}, {0x05c3, '\ub300'}, {0x05c4, '\ub301'}, {0x05c5, '\ub304'},
            {0x05c6, '\ub308'}, {0x05c7, '\ub310'}, {0x05c8, '\ub311'}, {0x05c9, '\ub313'},
            {0x05ca, '\ub314'}, {0x05cb, '\ub315'}, {0x05cc, '\ub31c'}, {0x05cd, '\ub354'},
            {0x05ce, '\ub355'}, {0x05cf, '\ub356'}, {0x05d0, '\ub358'}, {0x05d1, '\ub35b'},
            {0x05d2, '\ub35c'}, {0x05d3, '\ub35e'}, {0x05d4, '\ub35f'}, {0x05d5, '\ub364'},
            {0x05d6, '\ub365'}, {0x05d7, '\ub367'}, {0x05d8, '\ub369'}, {0x05d9, '\ub36b'},
            {0x05da, '\ub36e'}, {0x05db, '\ub370'}, {0x05dc, '\ub371'}, {0x05dd, '\ub374'},
            {0x05de, '\ub378'}, {0x05df, '\ub380'}, {0x05e0, '\ub381'}, {0x05e1, '\ub383'},
            {0x05e2, '\ub384'}, {0x05e3, '\ub385'}, {0x05e4, '\ub38c'}, {0x05e5, '\ub390'},
            {0x05e6, '\ub394'}, {0x05e7, '\ub3a0'}, {0x05e8, '\ub3a1'}, {0x05e9, '\ub3a8'},
            {0x05ea, '\ub3ac'}, {0x05eb, '\ub3c4'}, {0x05ec, '\ub3c5'}, {0x05ed, '\ub3c8'},
            {0x05ee, '\ub3cb'}, {0x05ef, '\ub3cc'}, {0x05f0, '\ub3ce'}, {0x05f1, '\ub3d0'},
            {0x05f2, '\ub3d4'}, {0x05f3, '\ub3d5'}, {0x05f4, '\ub3d7'}, {0x05f5, '\ub3d9'},
            {0x05f6, '\ub3db'}, {0x05f7, '\ub3dd'}, {0x05f8, '\ub3e0'}, {0x05f9, '\ub3e4'},
            {0x05fa, '\ub3e8'}, {0x05fb, '\ub3fc'}, {0x05fc, '\ub410'}, {0x05fd, '\ub418'},
            {0x05fe, '\ub41c'}, {0x05ff, '\ub420'}, {0x0600, '\ub428'}, {0x0601, '\ub429'},
            {0x0602, '\ub42b'}, {0x0603, '\ub434'}, {0x0604, '\ub450'}, {0x0605, '\ub451'},
            {0x0606, '\ub454'}, {0x0607, '\ub458'}, {0x0608, '\ub460'}, {0x0609, '\ub461'},
            {0x060a, '\ub463'}, {0x060b, '\ub465'}, {0x060c, '\ub46c'}, {0x060d, '\ub480'},
            {0x060e, '\ub488'}, {0x060f, '\ub49d'}, {0x0610, '\ub4a4'}, {0x0611, '\ub4a8'},
            {0x0612, '\ub4ac'}, {0x0613, '\ub4b5'}, {0x0614, '\ub4b7'}, {0x0615, '\ub4b9'},
            {0x0616, '\ub4c0'}, {0x0617, '\ub4c4'}, {0x0618, '\ub4c8'}, {0x0619, '\ub4d0'},
            {0x061a, '\ub4d5'}, {0x061b, '\ub4dc'}, {0x061c, '\ub4dd'}, {0x061d, '\ub4e0'},
            {0x061e, '\ub4e3'}, {0x061f, '\ub4e4'}, {0x0620, '\ub4e6'}, {0x0621, '\ub4ec'},
            {0x0622, '\ub4ed'}, {0x0623, '\ub4ef'}, {0x0624, '\ub4f1'}, {0x0625, '\ub4f8'},
            {0x0626, '\ub514'}, {0x0627, '\ub515'}, {0x0628, '\ub518'}, {0x0629, '\ub51b'},
            {0x062a, '\ub51c'}, {0x062b, '\ub524'}, {0x062c, '\ub525'}, {0x062d, '\ub527'},
            {0x062e, '\ub528'}, {0x062f, '\ub529'}, {0x0630, '\ub52a'}, {0x0631, '\ub530'},
            {0x0632, '\ub531'}, {0x0633, '\ub534'}, {0x0634, '\ub538'}, {0x0635, '\ub540'},
            {0x0636, '\ub541'}, {0x0637, '\ub543'}, {0x0638, '\ub544'}, {0x0639, '\ub545'},
            {0x063a, '\ub54b'}, {0x063b, '\ub54c'}, {0x063c, '\ub54d'}, {0x063d, '\ub550'},
            {0x063e, '\ub554'}, {0x063f, '\ub55c'}, {0x0640, '\ub55d'}, {0x0641, '\ub55f'},
            {0x0642, '\ub560'}, {0x0643, '\ub561'}, {0x0644, '\ub5a0'}, {0x0645, '\ub5a1'},
            {0x0646, '\ub5a4'}, {0x0647, '\ub5a8'}, {0x0648, '\ub5aa'}, {0x0649, '\ub5ab'},
            {0x064a, '\ub5b0'}, {0x064b, '\ub5b1'}, {0x064c, '\ub5b3'}, {0x064d, '\ub5b4'},
            {0x064e, '\ub5b5'}, {0x064f, '\ub5bb'}, {0x0650, '\ub5bc'}, {0x0651, '\ub5bd'},
            {0x0652, '\ub5c0'}, {0x0653, '\ub5c4'}, {0x0654, '\ub5cc'}, {0x0655, '\ub5cd'},
            {0x0656, '\ub5cf'}, {0x0657, '\ub5d0'}, {0x0658, '\ub5d1'}, {0x0659, '\ub5d8'},
            {0x065a, '\ub5ec'}, {0x065b, '\ub610'}, {0x065c, '\ub611'}, {0x065d, '\ub614'},
            {0x065e, '\ub618'}, {0x065f, '\ub625'}, {0x0660, '\ub62c'}, {0x0661, '\ub634'},
            {0x0662, '\ub648'}, {0x0663, '\ub664'}, {0x0664, '\ub668'}, {0x0665, '\ub69c'},
            {0x0666, '\ub69d'}, {0x0667, '\ub6a0'}, {0x0668, '\ub6a4'}, {0x0669, '\ub6ab'},
            {0x066a, '\ub6ac'}, {0x066b, '\ub6b1'}, {0x066c, '\ub6d4'}, {0x066d, '\ub6f0'},
            {0x066e, '\ub6f4'}, {0x066f, '\ub6f8'}, {0x0670, '\ub700'}, {0x0671, '\ub701'},
            {0x0672, '\ub705'}, {0x0673, '\ub728'}, {0x0674, '\ub729'}, {0x0675, '\ub72c'},
            {0x0676, '\ub72f'}, {0x0677, '\ub730'}, {0x0678, '\ub738'}, {0x0679, '\ub739'},
            {0x067a, '\ub73b'}, {0x067b, '\ub744'}, {0x067c, '\ub748'}, {0x067d, '\ub74c'},
            {0x067e, '\ub754'}, {0x067f, '\ub755'}, {0x0680, '\ub760'}, {0x0681, '\ub764'},
            {0x0682, '\ub768'}, {0x0683, '\ub770'}, {0x0684, '\ub771'}, {0x0685, '\ub773'},
            {0x0686, '\ub775'}, {0x0687, '\ub77c'}, {0x0688, '\ub77d'}, {0x0689, '\ub780'},
            {0x068a, '\ub784'}, {0x068b, '\ub78c'}, {0x068c, '\ub78d'}, {0x068d, '\ub78f'},
            {0x068e, '\ub790'}, {0x068f, '\ub791'}, {0x0690, '\ub792'}, {0x0691, '\ub796'},
            {0x0692, '\ub797'}, {0x0693, '\ub798'}, {0x0694, '\ub799'}, {0x0695, '\ub79c'},
            {0x0696, '\ub7a0'}, {0x0697, '\ub7a8'}, {0x0698, '\ub7a9'}, {0x0699, '\ub7ab'},
            {0x069a, '\ub7ac'}, {0x069b, '\ub7ad'}, {0x069c, '\ub7b4'}, {0x069d, '\ub7b5'},
            {0x069e, '\ub7b8'}, {0x069f, '\ub7c7'}, {0x06a0, '\ub7c9'}, {0x06a1, '\ub7ec'},
            {0x06a2, '\ub7ed'}, {0x06a3, '\ub7f0'}, {0x06a4, '\ub7f4'}, {0x06a5, '\ub7fc'},
            {0x06a6, '\ub7fd'}, {0x06a7, '\ub7ff'}, {0x06a8, '\ub800'}, {0x06a9, '\ub801'},
            {0x06aa, '\ub807'}, {0x06ab, '\ub808'}, {0x06ac, '\ub809'}, {0x06ad, '\ub80c'},
            {0x06ae, '\ub810'}, {0x06af, '\ub818'}, {0x06b0, '\ub819'}, {0x06b1, '\ub81b'},
            {0x06b2, '\ub81d'}, {0x06b3, '\ub824'}, {0x06b4, '\ub825'}, {0x06b5, '\ub828'},
            {0x06b6, '\ub82c'}, {0x06b7, '\ub834'}, {0x06b8, '\ub835'}, {0x06b9, '\ub837'},
            {0x06ba, '\ub838'}, {0x06bb, '\ub839'}, {0x06bc, '\ub840'}, {0x06bd, '\ub844'},
            {0x06be, '\ub851'}, {0x06bf, '\ub853'}, {0x06c0, '\ub85c'}, {0x06c1, '\ub85d'},
            {0x06c2, '\ub860'}, {0x06c3, '\ub864'}, {0x06c4, '\ub86c'}, {0x06c5, '\ub86d'},
            {0x06c6, '\ub86f'}, {0x06c7, '\ub871'}, {0x06c8, '\ub878'}, {0x06c9, '\ub87c'},
            {0x06ca, '\ub88d'}, {0x06cb, '\ub8a8'}, {0x06cc, '\ub8b0'}, {0x06cd, '\ub8b4'},
            {0x06ce, '\ub8b8'}, {0x06cf, '\ub8c0'}, {0x06d0, '\ub8c1'}, {0x06d1, '\ub8c3'},
            {0x06d2, '\ub8c5'}, {0x06d3, '\ub8cc'}, {0x06d4, '\ub8d0'}, {0x06d5, '\ub8d4'},
            {0x06d6, '\ub8dd'}, {0x06d7, '\ub8df'}, {0x06d8, '\ub8e1'}, {0x06d9, '\ub8e8'},
            {0x06da, '\ub8e9'}, {0x06db, '\ub8ec'}, {0x06dc, '\ub8f0'}, {0x06dd, '\ub8f8'},
            {0x06de, '\ub8f9'}, {0x06df, '\ub8fb'}, {0x06e0, '\ub8fd'}, {0x06e1, '\ub904'},
            {0x06e2, '\ub918'}, {0x06e3, '\ub920'}, {0x06e4, '\ub93c'}, {0x06e5, '\ub93d'},
            {0x06e6, '\ub940'}, {0x06e7, '\ub944'}, {0x06e8, '\ub94c'}, {0x06e9, '\ub94f'},
            {0x06ea, '\ub951'}, {0x06eb, '\ub958'}, {0x06ec, '\ub959'}, {0x06ed, '\ub95c'},
            {0x06ee, '\ub960'}, {0x06ef, '\ub968'}, {0x06f0, '\ub969'}, {0x06f1, '\ub96b'},
            {0x06f2, '\ub96d'}, {0x06f3, '\ub974'}, {0x06f4, '\ub975'}, {0x06f5, '\ub978'},
            {0x06f6, '\ub97c'}, {0x06f7, '\ub984'}, {0x06f8, '\ub985'}, {0x06f9, '\ub987'},
            {0x06fa, '\ub989'}, {0x06fb, '\ub98a'}, {0x06fc, '\ub98d'}, {0x06fd, '\ub98e'},
            {0x06fe, '\ub9ac'}, {0x06ff, '\ub9ad'}, {0x0700, '\ub9b0'}, {0x0701, '\ub9b4'},
            {0x0702, '\ub9bc'}, {0x0703, '\ub9bd'}, {0x0704, '\ub9bf'}, {0x0705, '\ub9c1'},
            {0x0706, '\ub9c8'}, {0x0707, '\ub9c9'}, {0x0708, '\ub9cc'}, {0x0709, '\ub9ce'},
            {0x070a, '\ub9cf'}, {0x070b, '\ub9d0'}, {0x070c, '\ub9d1'}, {0x070d, '\ub9d2'},
            {0x070e, '\ub9d8'}, {0x070f, '\ub9d9'}, {0x0710, '\ub9db'}, {0x0711, '\ub9dd'},
            {0x0712, '\ub9de'}, {0x0713, '\ub9e1'}, {0x0714, '\ub9e3'}, {0x0715, '\ub9e4'},
            {0x0716, '\ub9e5'}, {0x0717, '\ub9e8'}, {0x0718, '\ub9ec'}, {0x0719, '\ub9f4'},
            {0x071a, '\ub9f5'}, {0x071b, '\ub9f7'}, {0x071c, '\ub9f8'}, {0x071d, '\ub9f9'},
            {0x071e, '\ub9fa'}, {0x071f, '\uba00'}, {0x0720, '\uba01'}, {0x0721, '\uba08'},
            {0x0722, '\uba15'}, {0x0723, '\uba38'}, {0x0724, '\uba39'}, {0x0725, '\uba3c'},
            {0x0726, '\uba40'}, {0x0727, '\uba42'}, {0x0728, '\uba48'}, {0x0729, '\uba49'},
            {0x072a, '\uba4b'}, {0x072b, '\uba4d'}, {0x072c, '\uba4e'}, {0x072d, '\uba53'},
            {0x072e, '\uba54'}, {0x072f, '\uba55'}, {0x0730, '\uba58'}, {0x0731, '\uba5c'},
            {0x0732, '\uba64'}, {0x0733, '\uba65'}, {0x0734, '\uba67'}, {0x0735, '\uba68'},
            {0x0736, '\uba69'}, {0x0737, '\uba70'}, {0x0738, '\uba71'}, {0x0739, '\uba74'},
            {0x073a, '\uba78'}, {0x073b, '\uba83'}, {0x073c, '\uba84'}, {0x073d, '\uba85'},
            {0x073e, '\uba87'}, {0x073f, '\uba8c'}, {0x0740, '\ubaa8'}, {0x0741, '\ubaa9'},
            {0x0742, '\ubaab'}, {0x0743, '\ubaac'}, {0x0744, '\ubab0'}, {0x0745, '\ubab2'},
            {0x0746, '\ubab8'}, {0x0747, '\ubab9'}, {0x0748, '\ubabb'}, {0x0749, '\ubabd'},
            {0x074a, '\ubac4'}, {0x074b, '\ubac8'}, {0x074c, '\ubad8'}, {0x074d, '\ubad9'},
            {0x074e, '\ubafc'}, {0x074f, '\ubb00'}, {0x0750, '\ubb04'}, {0x0751, '\ubb0d'},
            {0x0752, '\ubb0f'}, {0x0753, '\ubb11'}, {0x0754, '\ubb18'}, {0x0755, '\ubb1c'},
            {0x0756, '\ubb20'}, {0x0757, '\ubb29'}, {0x0758, '\ubb2b'}, {0x0759, '\ubb34'},
            {0x075a, '\ubb35'}, {0x075b, '\ubb36'}, {0x075c, '\ubb38'}, {0x075d, '\ubb3b'},
            {0x075e, '\ubb3c'}, {0x075f, '\ubb3d'}, {0x0760, '\ubb3e'}, {0x0761, '\ubb44'},
            {0x0762, '\ubb45'}, {0x0763, '\ubb47'}, {0x0764, '\ubb49'}, {0x0765, '\ubb4d'},
            {0x0766, '\ubb4f'}, {0x0767, '\ubb50'}, {0x0768, '\ubb54'}, {0x0769, '\ubb58'},
            {0x076a, '\ubb61'}, {0x076b, '\ubb63'}, {0x076c, '\ubb6c'}, {0x076d, '\ubb88'},
            {0x076e, '\ubb8c'}, {0x076f, '\ubb90'}, {0x0770, '\ubba4'}, {0x0771, '\ubba8'},
            {0x0772, '\ubbac'}, {0x0773, '\ubbb4'}, {0x0774, '\ubbb7'}, {0x0775, '\ubbc0'},
            {0x0776, '\ubbc4'}, {0x0777, '\ubbc8'}, {0x0778, '\ubbd0'}, {0x0779, '\ubbd3'},
            {0x077a, '\ubbf8'}, {0x077b, '\ubbf9'}, {0x077c, '\ubbfc'}, {0x077d, '\ubbff'},
            {0x077e, '\ubc00'}, {0x077f, '\ubc02'}, {0x0780, '\ubc08'}, {0x0781, '\ubc09'},
            {0x0782, '\ubc0b'}, {0x0783, '\ubc0c'}, {0x0784, '\ubc0d'}, {0x0785, '\ubc0f'},
            {0x0786, '\ubc11'}, {0x0787, '\ubc14'}, {0x0788, '\ubc15'}, {0x0789, '\ubc16'},
            {0x078a, '\ubc17'}, {0x078b, '\ubc18'}, {0x078c, '\ubc1b'}, {0x078d, '\ubc1c'},
            {0x078e, '\ubc1d'}, {0x078f, '\ubc1e'}, {0x0790, '\ubc1f'}, {0x0791, '\ubc24'},
            {0x0792, '\ubc25'}, {0x0793, '\ubc27'}, {0x0794, '\ubc29'}, {0x0795, '\ubc2d'},
            {0x0796, '\ubc30'}, {0x0797, '\ubc31'}, {0x0798, '\ubc34'}, {0x0799, '\ubc38'},
            {0x079a, '\ubc40'}, {0x079b, '\ubc41'}, {0x079c, '\ubc43'}, {0x079d, '\ubc44'},
            {0x079e, '\ubc45'}, {0x079f, '\ubc49'}, {0x07a0, '\ubc4c'}, {0x07a1, '\ubc4d'},
            {0x07a2, '\ubc50'}, {0x07a3, '\ubc5d'}, {0x07a4, '\ubc84'}, {0x07a5, '\ubc85'},
            {0x07a6, '\ubc88'}, {0x07a7, '\ubc8b'}, {0x07a8, '\ubc8c'}, {0x07a9, '\ubc8e'},
            {0x07aa, '\ubc94'}, {0x07ab, '\ubc95'}, {0x07ac, '\ubc97'}, {0x07ad, '\ubc99'},
            {0x07ae, '\ubc9a'}, {0x07af, '\ubca0'}, {0x07b0, '\ubca1'}, {0x07b1, '\ubca4'},
            {0x07b2, '\ubca7'}, {0x07b3, '\ubca8'}, {0x07b4, '\ubcb0'}, {0x07b5, '\ubcb1'},
            {0x07b6, '\ubcb3'}, {0x07b7, '\ubcb4'}, {0x07b8, '\ubcb5'}, {0x07b9, '\ubcbc'},
            {0x07ba, '\ubcbd'}, {0x07bb, '\ubcc0'}, {0x07bc, '\ubcc4'}, {0x07bd, '\ubccd'},
            {0x07be, '\ubccf'}, {0x07bf, '\ubcd0'}, {0x07c0, '\ubcd1'}, {0x07c1, '\ubcd5'},
            {0x07c2, '\ubcd8'}, {0x07c3, '\ubcdc'}, {0x07c4, '\ubcf4'}, {0x07c5, '\ubcf5'},
            {0x07c6, '\ubcf6'}, {0x07c7, '\ubcf8'}, {0x07c8, '\ubcfc'}, {0x07c9, '\ubd04'},
            {0x07ca, '\ubd05'}, {0x07cb, '\ubd07'}, {0x07cc, '\ubd09'}, {0x07cd, '\ubd10'},
            {0x07ce, '\ubd14'}, {0x07cf, '\ubd24'}, {0x07d0, '\ubd2c'}, {0x07d1, '\ubd40'},
            {0x07d2, '\ubd48'}, {0x07d3, '\ubd49'}, {0x07d4, '\ubd4c'}, {0x07d5, '\ubd50'},
            {0x07d6, '\ubd58'}, {0x07d7, '\ubd59'}, {0x07d8, '\ubd64'}, {0x07d9, '\ubd68'},
            {0x07da, '\ubd80'}, {0x07db, '\ubd81'}, {0x07dc, '\ubd84'}, {0x07dd, '\ubd87'},
            {0x07de, '\ubd88'}, {0x07df, '\ubd89'}, {0x07e0, '\ubd8a'}, {0x07e1, '\ubd90'},
            {0x07e2, '\ubd91'}, {0x07e3, '\ubd93'}, {0x07e4, '\ubd95'}, {0x07e5, '\ubd99'},
            {0x07e6, '\ubd9a'}, {0x07e7, '\ubd9c'}, {0x07e8, '\ubda4'}, {0x07e9, '\ubdb0'},
            {0x07ea, '\ubdb8'}, {0x07eb, '\ubdd4'}, {0x07ec, '\ubdd5'}, {0x07ed, '\ubdd8'},
            {0x07ee, '\ubddc'}, {0x07ef, '\ubde9'}, {0x07f0, '\ubdf0'}, {0x07f1, '\ubdf4'},
            {0x07f2, '\ubdf8'}, {0x07f3, '\ube00'}, {0x07f4, '\ube03'}, {0x07f5, '\ube05'},
            {0x07f6, '\ube0c'}, {0x07f7, '\ube0d'}, {0x07f8, '\ube10'}, {0x07f9, '\ube14'},
            {0x07fa, '\ube1c'}, {0x07fb, '\ube1d'}, {0x07fc, '\ube1f'}, {0x07fd, '\ube44'},
            {0x07fe, '\ube45'}, {0x07ff, '\ube48'}, {0x0800, '\ube4c'}, {0x0801, '\ube4e'},
            {0x0802, '\ube54'}, {0x0803, '\ube55'}, {0x0804, '\ube57'}, {0x0805, '\ube59'},
            {0x0806, '\ube5a'}, {0x0807, '\ube5b'}, {0x0808, '\ube60'}, {0x0809, '\ube61'},
            {0x080a, '\ube64'}, {0x080b, '\ube68'}, {0x080c, '\ube6a'}, {0x080d, '\ube70'},
            {0x080e, '\ube71'}, {0x080f, '\ube73'}, {0x0810, '\ube74'}, {0x0811, '\ube75'},
            {0x0812, '\ube7b'}, {0x0813, '\ube7c'}, {0x0814, '\ube7d'}, {0x0815, '\ube80'},
            {0x0816, '\ube84'}, {0x0817, '\ube8c'}, {0x0818, '\ube8d'}, {0x0819, '\ube8f'},
            {0x081a, '\ube90'}, {0x081b, '\ube91'}, {0x081c, '\ube98'}, {0x081d, '\ube99'},
            {0x081e, '\ubea8'}, {0x081f, '\ubed0'}, {0x0820, '\ubed1'}, {0x0821, '\ubed4'},
            {0x0822, '\ubed7'}, {0x0823, '\ubed8'}, {0x0824, '\ubee0'}, {0x0825, '\ubee3'},
            {0x0826, '\ubee4'}, {0x0827, '\ubee5'}, {0x0828, '\ubeec'}, {0x0829, '\ubf01'},
            {0x082a, '\ubf08'}, {0x082b, '\ubf09'}, {0x082c, '\ubf18'}, {0x082d, '\ubf19'},
            {0x082e, '\ubf1b'}, {0x082f, '\ubf1c'}, {0x0830, '\ubf1d'}, {0x0831, '\ubf40'},
            {0x0832, '\ubf41'}, {0x0833, '\ubf44'}, {0x0834, '\ubf48'}, {0x0835, '\ubf50'},
            {0x0836, '\ubf51'}, {0x0837, '\ubf55'}, {0x0838, '\ubf94'}, {0x0839, '\ubfb0'},
            {0x083a, '\ubfc5'}, {0x083b, '\ubfcc'}, {0x083c, '\ubfcd'}, {0x083d, '\ubfd0'},
            {0x083e, '\ubfd4'}, {0x083f, '\ubfdc'}, {0x0840, '\ubfdf'}, {0x0841, '\ubfe1'},
            {0x0842, '\uc03c'}, {0x0843, '\uc051'}, {0x0844, '\uc058'}, {0x0845, '\uc05c'},
            {0x0846, '\uc060'}, {0x0847, '\uc068'}, {0x0848, '\uc069'}, {0x0849, '\uc090'},
            {0x084a, '\uc091'}, {0x084b, '\uc094'}, {0x084c, '\uc098'}, {0x084d, '\uc0a0'},
            {0x084e, '\uc0a1'}, {0x084f, '\uc0a3'}, {0x0850, '\uc0a5'}, {0x0851, '\uc0ac'},
            {0x0852, '\uc0ad'}, {0x0853, '\uc0af'}, {0x0854, '\uc0b0'}, {0x0855, '\uc0b3'},
            {0x0856, '\uc0b4'}, {0x0857, '\uc0b5'}, {0x0858, '\uc0b6'}, {0x0859, '\uc0bc'},
            {0x085a, '\uc0bd'}, {0x085b, '\uc0bf'}, {0x085c, '\uc0c0'}, {0x085d, '\uc0c1'},
            {0x085e, '\uc0c5'}, {0x085f, '\uc0c8'}, {0x0860, '\uc0c9'}, {0x0861, '\uc0cc'},
            {0x0862, '\uc0d0'}, {0x0863, '\uc0d8'}, {0x0864, '\uc0d9'}, {0x0865, '\uc0db'},
            {0x0866, '\uc0dc'}, {0x0867, '\uc0dd'}, {0x0868, '\uc0e4'}, {0x0869, '\uc0e5'},
            {0x086a, '\uc0e8'}, {0x086b, '\uc0ec'}, {0x086c, '\uc0f4'}, {0x086d, '\uc0f5'},
            {0x086e, '\uc0f7'}, {0x086f, '\uc0f9'}, {0x0870, '\uc100'}, {0x0871, '\uc104'},
            {0x0872, '\uc108'}, {0x0873, '\uc110'}, {0x0874, '\uc115'}, {0x0875, '\uc11c'},
            {0x0876, '\uc11d'}, {0x0877, '\uc11e'}, {0x0878, '\uc11f'}, {0x0879, '\uc120'},
            {0x087a, '\uc123'}, {0x087b, '\uc124'}, {0x087c, '\uc126'}, {0x087d, '\uc127'},
            {0x087e, '\uc12c'}, {0x087f, '\uc12d'}, {0x0880, '\uc12f'}, {0x0881, '\uc130'},
            {0x0882, '\uc131'}, {0x0883, '\uc136'}, {0x0884, '\uc138'}, {0x0885, '\uc139'},
            {0x0886, '\uc13c'}, {0x0887, '\uc140'}, {0x0888, '\uc148'}, {0x0889, '\uc149'},
            {0x088a, '\uc14b'}, {0x088b, '\uc14c'}, {0x088c, '\uc14d'}, {0x088d, '\uc154'},
            {0x088e, '\uc155'}, {0x088f, '\uc158'}, {0x0890, '\uc15c'}, {0x0891, '\uc164'},
            {0x0892, '\uc165'}, {0x0893, '\uc167'}, {0x0894, '\uc168'}, {0x0895, '\uc169'},
            {0x0896, '\uc170'}, {0x0897, '\uc174'}, {0x0898, '\uc178'}, {0x0899, '\uc185'},
            {0x089a, '\uc18c'}, {0x089b, '\uc18d'}, {0x089c, '\uc18e'}, {0x089d, '\uc190'},
            {0x089e, '\uc194'}, {0x089f, '\uc196'}, {0x08a0, '\uc19c'}, {0x08a1, '\uc19d'},
            {0x08a2, '\uc19f'}, {0x08a3, '\uc1a1'}, {0x08a4, '\uc1a5'}, {0x08a5, '\uc1a8'},
            {0x08a6, '\uc1a9'}, {0x08a7, '\uc1ac'}, {0x08a8, '\uc1b0'}, {0x08a9, '\uc1bd'},
            {0x08aa, '\uc1c4'}, {0x08ab, '\uc1c8'}, {0x08ac, '\uc1cc'}, {0x08ad, '\uc1d4'},
            {0x08ae, '\uc1d7'}, {0x08af, '\uc1d8'}, {0x08b0, '\uc1e0'}, {0x08b1, '\uc1e4'},
            {0x08b2, '\uc1e8'}, {0x08b3, '\uc1f0'}, {0x08b4, '\uc1f1'}, {0x08b5, '\uc1f3'},
            {0x08b6, '\uc1fc'}, {0x08b7, '\uc1fd'}, {0x08b8, '\uc200'}, {0x08b9, '\uc204'},
            {0x08ba, '\uc20c'}, {0x08bb, '\uc20d'}, {0x08bc, '\uc20f'}, {0x08bd, '\uc211'},
            {0x08be, '\uc218'}, {0x08bf, '\uc219'}, {0x08c0, '\uc21c'}, {0x08c1, '\uc21f'},
            {0x08c2, '\uc220'}, {0x08c3, '\uc228'}, {0x08c4, '\uc229'}, {0x08c5, '\uc22b'},
            {0x08c6, '\uc22d'}, {0x08c7, '\uc22f'}, {0x08c8, '\uc231'}, {0x08c9, '\uc232'},
            {0x08ca, '\uc234'}, {0x08cb, '\uc248'}, {0x08cc, '\uc250'}, {0x08cd, '\uc251'},
            {0x08ce, '\uc254'}, {0x08cf, '\uc258'}, {0x08d0, '\uc260'}, {0x08d1, '\uc265'},
            {0x08d2, '\uc26c'}, {0x08d3, '\uc26d'}, {0x08d4, '\uc270'}, {0x08d5, '\uc274'},
            {0x08d6, '\uc27c'}, {0x08d7, '\uc27d'}, {0x08d8, '\uc27f'}, {0x08d9, '\uc281'},
            {0x08da, '\uc288'}, {0x08db, '\uc289'}, {0x08dc, '\uc290'}, {0x08dd, '\uc298'},
            {0x08de, '\uc29b'}, {0x08df, '\uc29d'}, {0x08e0, '\uc2a4'}, {0x08e1, '\uc2a5'},
            {0x08e2, '\uc2a8'}, {0x08e3, '\uc2ac'}, {0x08e4, '\uc2ad'}, {0x08e5, '\uc2b4'},
            {0x08e6, '\uc2b5'}, {0x08e7, '\uc2b7'}, {0x08e8, '\uc2b9'}, {0x08e9, '\uc2dc'},
            {0x08ea, '\uc2dd'}, {0x08eb, '\uc2e0'}, {0x08ec, '\uc2e3'}, {0x08ed, '\uc2e4'},
            {0x08ee, '\uc2eb'}, {0x08ef, '\uc2ec'}, {0x08f0, '\uc2ed'}, {0x08f1, '\uc2ef'},
            {0x08f2, '\uc2f1'}, {0x08f3, '\uc2f6'}, {0x08f4, '\uc2f8'}, {0x08f5, '\uc2f9'},
            {0x08f6, '\uc2fb'}, {0x08f7, '\uc2fc'}, {0x08f8, '\uc300'}, {0x08f9, '\uc308'},
            {0x08fa, '\uc309'}, {0x08fb, '\uc30c'}, {0x08fc, '\uc30d'}, {0x08fd, '\uc313'},
            {0x08fe, '\uc314'}, {0x08ff, '\uc315'}, {0x0900, '\uc318'}, {0x0901, '\uc31c'},
            {0x0902, '\uc324'}, {0x0903, '\uc325'}, {0x0904, '\uc328'}, {0x0905, '\uc329'},
            {0x0906, '\uc345'}, {0x0907, '\uc368'}, {0x0908, '\uc369'}, {0x0909, '\uc36c'},
            {0x090a, '\uc370'}, {0x090b, '\uc372'}, {0x090c, '\uc378'}, {0x090d, '\uc379'},
            {0x090e, '\uc37c'}, {0x090f, '\uc37d'}, {0x0910, '\uc384'}, {0x0911, '\uc388'},
            {0x0912, '\uc38c'}, {0x0913, '\uc3c0'}, {0x0914, '\uc3d8'}, {0x0915, '\uc3d9'},
            {0x0916, '\uc3dc'}, {0x0917, '\uc3df'}, {0x0918, '\uc3e0'}, {0x0919, '\uc3e2'},
            {0x091a, '\uc3e8'}, {0x091b, '\uc3e9'}, {0x091c, '\uc3ed'}, {0x091d, '\uc3f4'},
            {0x091e, '\uc3f5'}, {0x091f, '\uc3f8'}, {0x0920, '\uc408'}, {0x0921, '\uc410'},
            {0x0922, '\uc424'}, {0x0923, '\uc42c'}, {0x0924, '\uc430'}, {0x0925, '\uc434'},
            {0x0926, '\uc43c'}, {0x0927, '\uc43d'}, {0x0928, '\uc448'}, {0x0929, '\uc464'},
            {0x092a, '\uc465'}, {0x092b, '\uc468'}, {0x092c, '\uc46c'}, {0x092d, '\uc474'},
            {0x092e, '\uc475'}, {0x092f, '\uc479'}, {0x0930, '\uc480'}, {0x0931, '\uc494'},
            {0x0932, '\uc49c'}, {0x0933, '\uc4b8'}, {0x0934, '\uc4bc'}, {0x0935, '\uc4e9'},
            {0x0936, '\uc4f0'}, {0x0937, '\uc4f1'}, {0x0938, '\uc4f4'}, {0x0939, '\uc4f8'},
            {0x093a, '\uc4fa'}, {0x093b, '\uc4ff'}, {0x093c, '\uc500'}, {0x093d, '\uc501'},
            {0x093e, '\uc50c'}, {0x093f, '\uc510'}, {0x0940, '\uc514'}, {0x0941, '\uc51c'},
            {0x0942, '\uc528'}, {0x0943, '\uc529'}, {0x0944, '\uc52c'}, {0x0945, '\uc530'},
            {0x0946, '\uc538'}, {0x0947, '\uc539'}, {0x0948, '\uc53b'}, {0x0949, '\uc53d'},
            {0x094a, '\uc544'}, {0x094b, '\uc545'}, {0x094c, '\uc548'}, {0x094d, '\uc549'},
            {0x094e, '\uc54a'}, {0x094f, '\uc54c'}, {0x0950, '\uc54d'}, {0x0951, '\uc54e'},
            {0x0952, '\uc553'}, {0x0953, '\uc554'}, {0x0954, '\uc555'}, {0x0955, '\uc557'},
            {0x0956, '\uc558'}, {0x0957, '\uc559'}, {0x0958, '\uc55d'}, {0x0959, '\uc55e'},
            {0x095a, '\uc560'}, {0x095b, '\uc561'}, {0x095c, '\uc564'}, {0x095d, '\uc568'},
            {0x095e, '\uc570'}, {0x095f, '\uc571'}, {0x0960, '\uc573'}, {0x0961, '\uc574'},
            {0x0962, '\uc575'}, {0x0963, '\uc57c'}, {0x0964, '\uc57d'}, {0x0965, '\uc580'},
            {0x0966, '\uc584'}, {0x0967, '\uc587'}, {0x0968, '\uc58c'}, {0x0969, '\uc58d'},
            {0x096a, '\uc58f'}, {0x096b, '\uc591'}, {0x096c, '\uc595'}, {0x096d, '\uc597'},
            {0x096e, '\uc598'}, {0x096f, '\uc59c'}, {0x0970, '\uc5a0'}, {0x0971, '\uc5a9'},
            {0x0972, '\uc5b4'}, {0x0973, '\uc5b5'}, {0x0974, '\uc5b8'}, {0x0975, '\uc5b9'},
            {0x0976, '\uc5bb'}, {0x0977, '\uc5bc'}, {0x0978, '\uc5bd'}, {0x0979, '\uc5be'},
            {0x097a, '\uc5c4'}, {0x097b, '\uc5c5'}, {0x097c, '\uc5c6'}, {0x097d, '\uc5c7'},
            {0x097e, '\uc5c8'}, {0x097f, '\uc5c9'}, {0x0980, '\uc5ca'}, {0x0981, '\uc5cc'},
            {0x0982, '\uc5ce'}, {0x0983, '\uc5d0'}, {0x0984, '\uc5d1'}, {0x0985, '\uc5d4'},
            {0x0986, '\uc5d8'}, {0x0987, '\uc5e0'}, {0x0988, '\uc5e1'}, {0x0989, '\uc5e3'},
            {0x098a, '\uc5e5'}, {0x098b, '\uc5ec'}, {0x098c, '\uc5ed'}, {0x098d, '\uc5ee'},
            {0x098e, '\uc5f0'}, {0x098f, '\uc5f4'}, {0x0990, '\uc5f6'}, {0x0991, '\uc5f7'},
            {0x0992, '\uc5fc'}, {0x0993, '\uc5fd'}, {0x0994, '\uc5fe'}, {0x0995, '\uc5ff'},
            {0x0996, '\uc600'}, {0x0997, '\uc601'}, {0x0998, '\uc605'}, {0x0999, '\uc606'},
            {0x099a, '\uc607'}, {0x099b, '\uc608'}, {0x099c, '\uc60c'}, {0x099d, '\uc610'},
            {0x099e, '\uc618'}, {0x099f, '\uc619'}, {0x09a0, '\uc61b'}, {0x09a1, '\uc61c'},
            {0x09a2, '\uc624'}, {0x09a3, '\uc625'}, {0x09a4, '\uc628'}, {0x09a5, '\uc62c'},
            {0x09a6, '\uc62d'}, {0x09a7, '\uc62e'}, {0x09a8, '\uc630'}, {0x09a9, '\uc633'},
            {0x09aa, '\uc634'}, {0x09ab, '\uc635'}, {0x09ac, '\uc637'}, {0x09ad, '\uc639'},
            {0x09ae, '\uc63b'}, {0x09af, '\uc640'}, {0x09b0, '\uc641'}, {0x09b1, '\uc644'},
            {0x09b2, '\uc648'}, {0x09b3, '\uc650'}, {0x09b4, '\uc651'}, {0x09b5, '\uc653'},
            {0x09b6, '\uc654'}, {0x09b7, '\uc655'}, {0x09b8, '\uc65c'}, {0x09b9, '\uc65d'},
            {0x09ba, '\uc660'}, {0x09bb, '\uc66c'}, {0x09bc, '\uc66f'}, {0x09bd, '\uc671'},
            {0x09be, '\uc678'}, {0x09bf, '\uc679'}, {0x09c0, '\uc67c'}, {0x09c1, '\uc680'},
            {0x09c2, '\uc688'}, {0x09c3, '\uc689'}, {0x09c4, '\uc68b'}, {0x09c5, '\uc68d'},
            {0x09c6, '\uc694'}, {0x09c7, '\uc695'}, {0x09c8, '\uc698'}, {0x09c9, '\uc69c'},
            {0x09ca, '\uc6a4'}, {0x09cb, '\uc6a5'}, {0x09cc, '\uc6a7'}, {0x09cd, '\uc6a9'},
            {0x09ce, '\uc6b0'}, {0x09cf, '\uc6b1'}, {0x09d0, '\uc6b4'}, {0x09d1, '\uc6b8'},
            {0x09d2, '\uc6b9'}, {0x09d3, '\uc6ba'}, {0x09d4, '\uc6c0'}, {0x09d5, '\uc6c1'},
            {0x09d6, '\uc6c3'}, {0x09d7, '\uc6c5'}, {0x09d8, '\uc6cc'}, {0x09d9, '\uc6cd'},
            {0x09da, '\uc6d0'}, {0x09db, '\uc6d4'}, {0x09dc, '\uc6dc'}, {0x09dd, '\uc6dd'},
            {0x09de, '\uc6e0'}, {0x09df, '\uc6e1'}, {0x09e0, '\uc6e8'}, {0x09e1, '\uc6e9'},
            {0x09e2, '\uc6ec'}, {0x09e3, '\uc6f0'}, {0x09e4, '\uc6f8'}, {0x09e5, '\uc6f9'},
            {0x09e6, '\uc6fd'}, {0x09e7, '\uc704'}, {0x09e8, '\uc705'}, {0x09e9, '\uc708'},
            {0x09ea, '\uc70c'}, {0x09eb, '\uc714'}, {0x09ec, '\uc715'}, {0x09ed, '\uc717'},
            {0x09ee, '\uc719'}, {0x09ef, '\uc720'}, {0x09f0, '\uc721'}, {0x09f1, '\uc724'},
            {0x09f2, '\uc728'}, {0x09f3, '\uc730'}, {0x09f4, '\uc731'}, {0x09f5, '\uc733'},
            {0x09f6, '\uc735'}, {0x09f7, '\uc737'}, {0x09f8, '\uc73c'}, {0x09f9, '\uc73d'},
            {0x09fa, '\uc740'}, {0x09fb, '\uc744'}, {0x09fc, '\uc74a'}, {0x09fd, '\uc74c'},
            {0x09fe, '\uc74d'}, {0x09ff, '\uc74f'}, {0x0a00, '\uc751'}, {0x0a01, '\uc752'},
            {0x0a02, '\uc753'}, {0x0a03, '\uc754'}, {0x0a04, '\uc755'}, {0x0a05, '\uc756'},
            {0x0a06, '\uc757'}, {0x0a07, '\uc758'}, {0x0a08, '\uc75c'}, {0x0a09, '\uc760'},
            {0x0a0a, '\uc768'}, {0x0a0b, '\uc76b'}, {0x0a0c, '\uc774'}, {0x0a0d, '\uc775'},
            {0x0a0e, '\uc778'}, {0x0a0f, '\uc77c'}, {0x0a10, '\uc77d'}, {0x0a11, '\uc77e'},
            {0x0a12, '\uc783'}, {0x0a13, '\uc784'}, {0x0a14, '\uc785'}, {0x0a15, '\uc787'},
            {0x0a16, '\uc788'}, {0x0a17, '\uc789'}, {0x0a18, '\uc78a'}, {0x0a19, '\uc78e'},
            {0x0a1a, '\uc790'}, {0x0a1b, '\uc791'}, {0x0a1c, '\uc794'}, {0x0a1d, '\uc796'},
            {0x0a1e, '\uc797'}, {0x0a1f, '\uc798'}, {0x0a20, '\uc79a'}, {0x0a21, '\uc7a0'},
            {0x0a22, '\uc7a1'}, {0x0a23, '\uc7a3'}, {0x0a24, '\uc7a4'}, {0x0a25, '\uc7a5'},
            {0x0a26, '\uc7a6'}, {0x0a27, '\uc7ac'}, {0x0a28, '\uc7ad'}, {0x0a29, '\uc7b0'},
            {0x0a2a, '\uc7b4'}, {0x0a2b, '\uc7bc'}, {0x0a2c, '\uc7bd'}, {0x0a2d, '\uc7bf'},
            {0x0a2e, '\uc7c0'}, {0x0a2f, '\uc7c1'}, {0x0a30, '\uc7c8'}, {0x0a31, '\uc7c9'},
            {0x0a32, '\uc7cc'}, {0x0a33, '\uc7ce'}, {0x0a34, '\uc7d0'}, {0x0a35, '\uc7d8'},
            {0x0a36, '\uc7dd'}, {0x0a37, '\uc7e4'}, {0x0a38, '\uc7e8'}, {0x0a39, '\uc7ec'},
            {0x0a3a, '\uc800'}, {0x0a3b, '\uc801'}, {0x0a3c, '\uc804'}, {0x0a3d, '\uc808'},
            {0x0a3e, '\uc80a'}, {0x0a3f, '\uc810'}, {0x0a40, '\uc811'}, {0x0a41, '\uc813'},
            {0x0a42, '\uc815'}, {0x0a43, '\uc816'}, {0x0a44, '\uc81c'}, {0x0a45, '\uc81d'},
            {0x0a46, '\uc820'}, {0x0a47, '\uc824'}, {0x0a48, '\uc82c'}, {0x0a49, '\uc82d'},
            {0x0a4a, '\uc82f'}, {0x0a4b, '\uc831'}, {0x0a4c, '\uc838'}, {0x0a4d, '\uc83c'},
            {0x0a4e, '\uc840'}, {0x0a4f, '\uc848'}, {0x0a50, '\uc849'}, {0x0a51, '\uc84c'},
            {0x0a52, '\uc84d'}, {0x0a53, '\uc854'}, {0x0a54, '\uc870'}, {0x0a55, '\uc871'},
            {0x0a56, '\uc874'}, {0x0a57, '\uc878'}, {0x0a58, '\uc87a'}, {0x0a59, '\uc880'},
            {0x0a5a, '\uc881'}, {0x0a5b, '\uc883'}, {0x0a5c, '\uc885'}, {0x0a5d, '\uc886'},
            {0x0a5e, '\uc887'}, {0x0a5f, '\uc88b'}, {0x0a60, '\uc88c'}, {0x0a61, '\uc88d'},
            {0x0a62, '\uc894'}, {0x0a63, '\uc89d'}, {0x0a64, '\uc89f'}, {0x0a65, '\uc8a1'},
            {0x0a66, '\uc8a8'}, {0x0a67, '\uc8bc'}, {0x0a68, '\uc8bd'}, {0x0a69, '\uc8c4'},
            {0x0a6a, '\uc8c8'}, {0x0a6b, '\uc8cc'}, {0x0a6c, '\uc8d4'}, {0x0a6d, '\uc8d5'},
            {0x0a6e, '\uc8d7'}, {0x0a6f, '\uc8d9'}, {0x0a70, '\uc8e0'}, {0x0a71, '\uc8e1'},
            {0x0a72, '\uc8e4'}, {0x0a73, '\uc8f5'}, {0x0a74, '\uc8fc'}, {0x0a75, '\uc8fd'},
            {0x0a76, '\uc900'}, {0x0a77, '\uc904'}, {0x0a78, '\uc905'}, {0x0a79, '\uc906'},
            {0x0a7a, '\uc90c'}, {0x0a7b, '\uc90d'}, {0x0a7c, '\uc90f'}, {0x0a7d, '\uc911'},
            {0x0a7e, '\uc918'}, {0x0a7f, '\uc92c'}, {0x0a80, '\uc934'}, {0x0a81, '\uc950'},
            {0x0a82, '\uc951'}, {0x0a83, '\uc954'}, {0x0a84, '\uc958'}, {0x0a85, '\uc960'},
            {0x0a86, '\uc961'}, {0x0a87, '\uc963'}, {0x0a88, '\uc96c'}, {0x0a89, '\uc970'},
            {0x0a8a, '\uc974'}, {0x0a8b, '\uc97c'}, {0x0a8c, '\uc988'}, {0x0a8d, '\uc989'},
            {0x0a8e, '\uc98c'}, {0x0a8f, '\uc990'}, {0x0a90, '\uc998'}, {0x0a91, '\uc999'},
            {0x0a92, '\uc99b'}, {0x0a93, '\uc99d'}, {0x0a94, '\uc9c0'}, {0x0a95, '\uc9c1'},
            {0x0a96, '\uc9c4'}, {0x0a97, '\uc9c7'}, {0x0a98, '\uc9c8'}, {0x0a99, '\uc9ca'},
            {0x0a9a, '\uc9d0'}, {0x0a9b, '\uc9d1'}, {0x0a9c, '\uc9d3'}, {0x0a9d, '\uc9d5'},
            {0x0a9e, '\uc9d6'}, {0x0a9f, '\uc9d9'}, {0x0aa0, '\uc9da'}, {0x0aa1, '\uc9dc'},
            {0x0aa2, '\uc9dd'}, {0x0aa3, '\uc9e0'}, {0x0aa4, '\uc9e2'}, {0x0aa5, '\uc9e4'},
            {0x0aa6, '\uc9e7'}, {0x0aa7, '\uc9ec'}, {0x0aa8, '\uc9ed'}, {0x0aa9, '\uc9ef'},
            {0x0aaa, '\uc9f0'}, {0x0aab, '\uc9f1'}, {0x0aac, '\uc9f8'}, {0x0aad, '\uc9f9'},
            {0x0aae, '\uc9fc'}, {0x0aaf, '\uca00'}, {0x0ab0, '\uca08'}, {0x0ab1, '\uca09'},
            {0x0ab2, '\uca0b'}, {0x0ab3, '\uca0c'}, {0x0ab4, '\uca0d'}, {0x0ab5, '\uca14'},
            {0x0ab6, '\uca18'}, {0x0ab7, '\uca29'}, {0x0ab8, '\uca4c'}, {0x0ab9, '\uca4d'},
            {0x0aba, '\uca50'}, {0x0abb, '\uca54'}, {0x0abc, '\uca5c'}, {0x0abd, '\uca5d'},
            {0x0abe, '\uca5f'}, {0x0abf, '\uca60'}, {0x0ac0, '\uca61'}, {0x0ac1, '\uca68'},
            {0x0ac2, '\uca7d'}, {0x0ac3, '\uca84'}, {0x0ac4, '\uca98'}, {0x0ac5, '\ucabc'},
            {0x0ac6, '\ucabd'}, {0x0ac7, '\ucac0'}, {0x0ac8, '\ucac4'}, {0x0ac9, '\ucacc'},
            {0x0aca, '\ucacd'}, {0x0acb, '\ucacf'}, {0x0acc, '\ucad1'}, {0x0acd, '\ucad3'},
            {0x0ace, '\ucad8'}, {0x0acf, '\ucad9'}, {0x0ad0, '\ucae0'}, {0x0ad1, '\ucaec'},
            {0x0ad2, '\ucaf4'}, {0x0ad3, '\ucb08'}, {0x0ad4, '\ucb10'}, {0x0ad5, '\ucb14'},
            {0x0ad6, '\ucb18'}, {0x0ad7, '\ucb20'}, {0x0ad8, '\ucb21'}, {0x0ad9, '\ucb41'},
            {0x0ada, '\ucb48'}, {0x0adb, '\ucb49'}, {0x0adc, '\ucb4c'}, {0x0add, '\ucb50'},
            {0x0ade, '\ucb58'}, {0x0adf, '\ucb59'}, {0x0ae0, '\ucb5d'}, {0x0ae1, '\ucb64'},
            {0x0ae2, '\ucb78'}, {0x0ae3, '\ucb79'}, {0x0ae4, '\ucb9c'}, {0x0ae5, '\ucbb8'},
            {0x0ae6, '\ucbd4'}, {0x0ae7, '\ucbe4'}, {0x0ae8, '\ucbe7'}, {0x0ae9, '\ucbe9'},
            {0x0aea, '\ucc0c'}, {0x0aeb, '\ucc0d'}, {0x0aec, '\ucc10'}, {0x0aed, '\ucc14'},
            {0x0aee, '\ucc1c'}, {0x0aef, '\ucc1d'}, {0x0af0, '\ucc21'}, {0x0af1, '\ucc22'},
            {0x0af2, '\ucc27'}, {0x0af3, '\ucc28'}, {0x0af4, '\ucc29'}, {0x0af5, '\ucc2c'},
            {0x0af6, '\ucc2e'}, {0x0af7, '\ucc30'}, {0x0af8, '\ucc38'}, {0x0af9, '\ucc39'},
            {0x0afa, '\ucc3b'}, {0x0afb, '\ucc3c'}, {0x0afc, '\ucc3d'}, {0x0afd, '\ucc3e'},
            {0x0afe, '\ucc44'}, {0x0aff, '\ucc45'}, {0x0b00, '\ucc48'}, {0x0b01, '\ucc4c'},
            {0x0b02, '\ucc54'}, {0x0b03, '\ucc55'}, {0x0b04, '\ucc57'}, {0x0b05, '\ucc58'},
            {0x0b06, '\ucc59'}, {0x0b07, '\ucc60'}, {0x0b08, '\ucc64'}, {0x0b09, '\ucc66'},
            {0x0b0a, '\ucc68'}, {0x0b0b, '\ucc70'}, {0x0b0c, '\ucc75'}, {0x0b0d, '\ucc98'},
            {0x0b0e, '\ucc99'}, {0x0b0f, '\ucc9c'}, {0x0b10, '\ucca0'}, {0x0b11, '\ucca8'},
            {0x0b12, '\ucca9'}, {0x0b13, '\uccab'}, {0x0b14, '\uccac'}, {0x0b15, '\uccad'},
            {0x0b16, '\uccb4'}, {0x0b17, '\uccb5'}, {0x0b18, '\uccb8'}, {0x0b19, '\uccbc'},
            {0x0b1a, '\uccc4'}, {0x0b1b, '\uccc5'}, {0x0b1c, '\uccc7'}, {0x0b1d, '\uccc9'},
            {0x0b1e, '\uccd0'}, {0x0b1f, '\uccd4'}, {0x0b20, '\ucce4'}, {0x0b21, '\uccec'},
            {0x0b22, '\uccf0'}, {0x0b23, '\ucd01'}, {0x0b24, '\ucd08'}, {0x0b25, '\ucd09'},
            {0x0b26, '\ucd0c'}, {0x0b27, '\ucd10'}, {0x0b28, '\ucd18'}, {0x0b29, '\ucd19'},
            {0x0b2a, '\ucd1b'}, {0x0b2b, '\ucd1d'}, {0x0b2c, '\ucd24'}, {0x0b2d, '\ucd28'},
            {0x0b2e, '\ucd2c'}, {0x0b2f, '\ucd39'}, {0x0b30, '\ucd5c'}, {0x0b31, '\ucd60'},
            {0x0b32, '\ucd64'}, {0x0b33, '\ucd6c'}, {0x0b34, '\ucd6d'}, {0x0b35, '\ucd6f'},
            {0x0b36, '\ucd71'}, {0x0b37, '\ucd78'}, {0x0b38, '\ucd88'}, {0x0b39, '\ucd94'},
            {0x0b3a, '\ucd95'}, {0x0b3b, '\ucd98'}, {0x0b3c, '\ucd9c'}, {0x0b3d, '\ucda4'},
            {0x0b3e, '\ucda5'}, {0x0b3f, '\ucda7'}, {0x0b40, '\ucda9'}, {0x0b41, '\ucdb0'},
            {0x0b42, '\ucdc4'}, {0x0b43, '\ucdcc'}, {0x0b44, '\ucdd0'}, {0x0b45, '\ucde8'},
            {0x0b46, '\ucdec'}, {0x0b47, '\ucdf0'}, {0x0b48, '\ucdf8'}, {0x0b49, '\ucdf9'},
            {0x0b4a, '\ucdfb'}, {0x0b4b, '\ucdfd'}, {0x0b4c, '\uce04'}, {0x0b4d, '\uce08'},
            {0x0b4e, '\uce0c'}, {0x0b4f, '\uce14'}, {0x0b50, '\uce19'}, {0x0b51, '\uce20'},
            {0x0b52, '\uce21'}, {0x0b53, '\uce24'}, {0x0b54, '\uce28'}, {0x0b55, '\uce30'},
            {0x0b56, '\uce31'}, {0x0b57, '\uce33'}, {0x0b58, '\uce35'}, {0x0b59, '\uce58'},
            {0x0b5a, '\uce59'}, {0x0b5b, '\uce5c'}, {0x0b5c, '\uce5f'}, {0x0b5d, '\uce60'},
            {0x0b5e, '\uce61'}, {0x0b5f, '\uce68'}, {0x0b60, '\uce69'}, {0x0b61, '\uce6b'},
            {0x0b62, '\uce6d'}, {0x0b63, '\uce74'}, {0x0b64, '\uce75'}, {0x0b65, '\uce78'},
            {0x0b66, '\uce7c'}, {0x0b67, '\uce84'}, {0x0b68, '\uce85'}, {0x0b69, '\uce87'},
            {0x0b6a, '\uce89'}, {0x0b6b, '\uce90'}, {0x0b6c, '\uce91'}, {0x0b6d, '\uce94'},
            {0x0b6e, '\uce98'}, {0x0b6f, '\ucea0'}, {0x0b70, '\ucea1'}, {0x0b71, '\ucea3'},
            {0x0b72, '\ucea4'}, {0x0b73, '\ucea5'}, {0x0b74, '\uceac'}, {0x0b75, '\ucead'},
            {0x0b76, '\ucec1'}, {0x0b77, '\ucee4'}, {0x0b78, '\ucee5'}, {0x0b79, '\ucee8'},
            {0x0b7a, '\uceeb'}, {0x0b7b, '\uceec'}, {0x0b7c, '\ucef4'}, {0x0b7d, '\ucef5'},
            {0x0b7e, '\ucef7'}, {0x0b7f, '\ucef8'}, {0x0b80, '\ucef9'}, {0x0b81, '\ucf00'},
            {0x0b82, '\ucf01'}, {0x0b83, '\ucf04'}, {0x0b84, '\ucf08'}, {0x0b85, '\ucf10'},
            {0x0b86, '\ucf11'}, {0x0b87, '\ucf13'}, {0x0b88, '\ucf15'}, {0x0b89, '\ucf1c'},
            {0x0b8a, '\ucf20'}, {0x0b8b, '\ucf24'}, {0x0b8c, '\ucf2c'}, {0x0b8d, '\ucf2d'},
            {0x0b8e, '\ucf2f'}, {0x0b8f, '\ucf30'}, {0x0b90, '\ucf31'}, {0x0b91, '\ucf38'},
            {0x0b92, '\ucf54'}, {0x0b93, '\ucf55'}, {0x0b94, '\ucf58'}, {0x0b95, '\ucf5c'},
            {0x0b96, '\ucf64'}, {0x0b97, '\ucf65'}, {0x0b98, '\ucf67'}, {0x0b99, '\ucf69'},
            {0x0b9a, '\ucf70'}, {0x0b9b, '\ucf71'}, {0x0b9c, '\ucf74'}, {0x0b9d, '\ucf78'},
            {0x0b9e, '\ucf80'}, {0x0b9f, '\ucf85'}, {0x0ba0, '\ucf8c'}, {0x0ba1, '\ucfa1'},
            {0x0ba2, '\ucfa8'}, {0x0ba3, '\ucfb0'}, {0x0ba4, '\ucfc4'}, {0x0ba5, '\ucfe0'},
            {0x0ba6, '\ucfe1'}, {0x0ba7, '\ucfe4'}, {0x0ba8, '\ucfe8'}, {0x0ba9, '\ucff0'},
            {0x0baa, '\ucff1'}, {0x0bab, '\ucff3'}, {0x0bac, '\ucff5'}, {0x0bad, '\ucffc'},
            {0x0bae, '\ud000'}, {0x0baf, '\ud004'}, {0x0bb0, '\ud011'}, {0x0bb1, '\ud018'},
            {0x0bb2, '\ud02d'}, {0x0bb3, '\ud034'}, {0x0bb4, '\ud035'}, {0x0bb5, '\ud038'},
            {0x0bb6, '\ud03c'}, {0x0bb7, '\ud044'}, {0x0bb8, '\ud045'}, {0x0bb9, '\ud047'},
            {0x0bba, '\ud049'}, {0x0bbb, '\ud050'}, {0x0bbc, '\ud054'}, {0x0bbd, '\ud058'},
            {0x0bbe, '\ud060'}, {0x0bbf, '\ud06c'}, {0x0bc0, '\ud06d'}, {0x0bc1, '\ud070'},
            {0x0bc2, '\ud074'}, {0x0bc3, '\ud07c'}, {0x0bc4, '\ud07d'}, {0x0bc5, '\ud081'},
            {0x0bc6, '\ud0a4'}, {0x0bc7, '\ud0a5'}, {0x0bc8, '\ud0a8'}, {0x0bc9, '\ud0ac'},
            {0x0bca, '\ud0b4'}, {0x0bcb, '\ud0b5'}, {0x0bcc, '\ud0b7'}, {0x0bcd, '\ud0b9'},
            {0x0bce, '\ud0c0'}, {0x0bcf, '\ud0c1'}, {0x0bd0, '\ud0c4'}, {0x0bd1, '\ud0c8'},
            {0x0bd2, '\ud0c9'}, {0x0bd3, '\ud0d0'}, {0x0bd4, '\ud0d1'}, {0x0bd5, '\ud0d3'},
            {0x0bd6, '\ud0d4'}, {0x0bd7, '\ud0d5'}, {0x0bd8, '\ud0dc'}, {0x0bd9, '\ud0dd'},
            {0x0bda, '\ud0e0'}, {0x0bdb, '\ud0e4'}, {0x0bdc, '\ud0ec'}, {0x0bdd, '\ud0ed'},
            {0x0bde, '\ud0ef'}, {0x0bdf, '\ud0f0'}, {0x0be0, '\ud0f1'}, {0x0be1, '\ud0f8'},
            {0x0be2, '\ud10d'}, {0x0be3, '\ud130'}, {0x0be4, '\ud131'}, {0x0be5, '\ud134'},
            {0x0be6, '\ud138'}, {0x0be7, '\ud13a'}, {0x0be8, '\ud140'}, {0x0be9, '\ud141'},
            {0x0bea, '\ud143'}, {0x0beb, '\ud144'}, {0x0bec, '\ud145'}, {0x0bed, '\ud14c'},
            {0x0bee, '\ud14d'}, {0x0bef, '\ud150'}, {0x0bf0, '\ud154'}, {0x0bf1, '\ud15c'},
            {0x0bf2, '\ud15d'}, {0x0bf3, '\ud15f'}, {0x0bf4, '\ud161'}, {0x0bf5, '\ud168'},
            {0x0bf6, '\ud16c'}, {0x0bf7, '\ud17c'}, {0x0bf8, '\ud184'}, {0x0bf9, '\ud188'},
            {0x0bfa, '\ud1a0'}, {0x0bfb, '\ud1a1'}, {0x0bfc, '\ud1a4'}, {0x0bfd, '\ud1a8'},
            {0x0bfe, '\ud1b0'}, {0x0bff, '\ud1b1'}, {0x0c00, '\ud1b3'}, {0x0c01, '\ud1b5'},
            {0x0c02, '\ud1ba'}, {0x0c03, '\ud1bc'}, {0x0c04, '\ud1c0'}, {0x0c05, '\ud1d8'},
            {0x0c06, '\ud1f4'}, {0x0c07, '\ud1f8'}, {0x0c08, '\ud207'}, {0x0c09, '\ud209'},
            {0x0c0a, '\ud210'}, {0x0c0b, '\ud22c'}, {0x0c0c, '\ud22d'}, {0x0c0d, '\ud230'},
            {0x0c0e, '\ud234'}, {0x0c0f, '\ud23c'}, {0x0c10, '\ud23d'}, {0x0c11, '\ud23f'},
            {0x0c12, '\ud241'}, {0x0c13, '\ud248'}, {0x0c14, '\ud25c'}, {0x0c15, '\ud264'},
            {0x0c16, '\ud280'}, {0x0c17, '\ud281'}, {0x0c18, '\ud284'}, {0x0c19, '\ud288'},
            {0x0c1a, '\ud290'}, {0x0c1b, '\ud291'}, {0x0c1c, '\ud295'}, {0x0c1d, '\ud29c'},
            {0x0c1e, '\ud2a0'}, {0x0c1f, '\ud2a4'}, {0x0c20, '\ud2ac'}, {0x0c21, '\ud2b1'},
            {0x0c22, '\ud2b8'}, {0x0c23, '\ud2b9'}, {0x0c24, '\ud2bc'}, {0x0c25, '\ud2bf'},
            {0x0c26, '\ud2c0'}, {0x0c27, '\ud2c2'}, {0x0c28, '\ud2c8'}, {0x0c29, '\ud2c9'},
            {0x0c2a, '\ud2cb'}, {0x0c2b, '\ud2d4'}, {0x0c2c, '\ud2d8'}, {0x0c2d, '\ud2dc'},
            {0x0c2e, '\ud2e4'}, {0x0c2f, '\ud2e5'}, {0x0c30, '\ud2f0'}, {0x0c31, '\ud2f1'},
            {0x0c32, '\ud2f4'}, {0x0c33, '\ud2f8'}, {0x0c34, '\ud300'}, {0x0c35, '\ud301'},
            {0x0c36, '\ud303'}, {0x0c37, '\ud305'}, {0x0c38, '\ud30c'}, {0x0c39, '\ud30d'},
            {0x0c3a, '\ud30e'}, {0x0c3b, '\ud310'}, {0x0c3c, '\ud314'}, {0x0c3d, '\ud316'},
            {0x0c3e, '\ud31c'}, {0x0c3f, '\ud31d'}, {0x0c40, '\ud31f'}, {0x0c41, '\ud320'},
            {0x0c42, '\ud321'}, {0x0c43, '\ud325'}, {0x0c44, '\ud328'}, {0x0c45, '\ud329'},
            {0x0c46, '\ud32c'}, {0x0c47, '\ud330'}, {0x0c48, '\ud338'}, {0x0c49, '\ud339'},
            {0x0c4a, '\ud33b'}, {0x0c4b, '\ud33c'}, {0x0c4c, '\ud33d'}, {0x0c4d, '\ud344'},
            {0x0c4e, '\ud345'}, {0x0c4f, '\ud37c'}, {0x0c50, '\ud37d'}, {0x0c51, '\ud380'},
            {0x0c52, '\ud384'}, {0x0c53, '\ud38c'}, {0x0c54, '\ud38d'}, {0x0c55, '\ud38f'},
            {0x0c56, '\ud390'}, {0x0c57, '\ud391'}, {0x0c58, '\ud398'}, {0x0c59, '\ud399'},
            {0x0c5a, '\ud39c'}, {0x0c5b, '\ud3a0'}, {0x0c5c, '\ud3a8'}, {0x0c5d, '\ud3a9'},
            {0x0c5e, '\ud3ab'}, {0x0c5f, '\ud3ad'}, {0x0c60, '\ud3b4'}, {0x0c61, '\ud3b8'},
            {0x0c62, '\ud3bc'}, {0x0c63, '\ud3c4'}, {0x0c64, '\ud3c5'}, {0x0c65, '\ud3c8'},
            {0x0c66, '\ud3c9'}, {0x0c67, '\ud3d0'}, {0x0c68, '\ud3d8'}, {0x0c69, '\ud3e1'},
            {0x0c6a, '\ud3e3'}, {0x0c6b, '\ud3ec'}, {0x0c6c, '\ud3ed'}, {0x0c6d, '\ud3f0'},
            {0x0c6e, '\ud3f4'}, {0x0c6f, '\ud3fc'}, {0x0c70, '\ud3fd'}, {0x0c71, '\ud3ff'},
            {0x0c72, '\ud401'}, {0x0c73, '\ud408'}, {0x0c74, '\ud41d'}, {0x0c75, '\ud440'},
            {0x0c76, '\ud444'}, {0x0c77, '\ud45c'}, {0x0c78, '\ud460'}, {0x0c79, '\ud464'},
            {0x0c7a, '\ud46d'}, {0x0c7b, '\ud46f'}, {0x0c7c, '\ud478'}, {0x0c7d, '\ud479'},
            {0x0c7e, '\ud47c'}, {0x0c7f, '\ud47f'}, {0x0c80, '\ud480'}, {0x0c81, '\ud482'},
            {0x0c82, '\ud488'}, {0x0c83, '\ud489'}, {0x0c84, '\ud48b'}, {0x0c85, '\ud48d'},
            {0x0c86, '\ud494'}, {0x0c87, '\ud4a9'}, {0x0c88, '\ud4cc'}, {0x0c89, '\ud4d0'},
            {0x0c8a, '\ud4d4'}, {0x0c8b, '\ud4dc'}, {0x0c8c, '\ud4df'}, {0x0c8d, '\ud4e8'},
            {0x0c8e, '\ud4ec'}, {0x0c8f, '\ud4f0'}, {0x0c90, '\ud4f8'}, {0x0c91, '\ud4fb'},
            {0x0c92, '\ud4fd'}, {0x0c93, '\ud504'}, {0x0c94, '\ud508'}, {0x0c95, '\ud50c'},
            {0x0c96, '\ud514'}, {0x0c97, '\ud515'}, {0x0c98, '\ud517'}, {0x0c99, '\ud53c'},
            {0x0c9a, '\ud53d'}, {0x0c9b, '\ud540'}, {0x0c9c, '\ud544'}, {0x0c9d, '\ud54c'},
            {0x0c9e, '\ud54d'}, {0x0c9f, '\ud54f'}, {0x0ca0, '\ud551'}, {0x0ca1, '\ud558'},
            {0x0ca2, '\ud559'}, {0x0ca3, '\ud55c'}, {0x0ca4, '\ud560'}, {0x0ca5, '\ud565'},
            {0x0ca6, '\ud568'}, {0x0ca7, '\ud569'}, {0x0ca8, '\ud56b'}, {0x0ca9, '\ud56d'},
            {0x0caa, '\ud574'}, {0x0cab, '\ud575'}, {0x0cac, '\ud578'}, {0x0cad, '\ud57c'},
            {0x0cae, '\ud584'}, {0x0caf, '\ud585'}, {0x0cb0, '\ud587'}, {0x0cb1, '\ud588'},
            {0x0cb2, '\ud589'}, {0x0cb3, '\ud590'}, {0x0cb4, '\ud5a5'}, {0x0cb5, '\ud5c8'},
            {0x0cb6, '\ud5c9'}, {0x0cb7, '\ud5cc'}, {0x0cb8, '\ud5d0'}, {0x0cb9, '\ud5d2'},
            {0x0cba, '\ud5d8'}, {0x0cbb, '\ud5d9'}, {0x0cbc, '\ud5db'}, {0x0cbd, '\ud5dd'},
            {0x0cbe, '\ud5e4'}, {0x0cbf, '\ud5e5'}, {0x0cc0, '\ud5e8'}, {0x0cc1, '\ud5ec'},
            {0x0cc2, '\ud5f4'}, {0x0cc3, '\ud5f5'}, {0x0cc4, '\ud5f7'}, {0x0cc5, '\ud5f9'},
            {0x0cc6, '\ud600'}, {0x0cc7, '\ud601'}, {0x0cc8, '\ud604'}, {0x0cc9, '\ud608'},
            {0x0cca, '\ud610'}, {0x0ccb, '\ud611'}, {0x0ccc, '\ud613'}, {0x0ccd, '\ud614'},
            {0x0cce, '\ud615'}, {0x0ccf, '\ud61c'}, {0x0cd0, '\ud620'}, {0x0cd1, '\ud624'},
            {0x0cd2, '\ud62d'}, {0x0cd3, '\ud638'}, {0x0cd4, '\ud639'}, {0x0cd5, '\ud63c'},
            {0x0cd6, '\ud640'}, {0x0cd7, '\ud645'}, {0x0cd8, '\ud648'}, {0x0cd9, '\ud649'},
            {0x0cda, '\ud64b'}, {0x0cdb, '\ud64d'}, {0x0cdc, '\ud651'}, {0x0cdd, '\ud654'},
            {0x0cde, '\ud655'}, {0x0cdf, '\ud658'}, {0x0ce0, '\ud65c'}, {0x0ce1, '\ud667'},
            {0x0ce2, '\ud669'}, {0x0ce3, '\ud670'}, {0x0ce4, '\ud671'}, {0x0ce5, '\ud674'},
            {0x0ce6, '\ud683'}, {0x0ce7, '\ud685'}, {0x0ce8, '\ud68c'}, {0x0ce9, '\ud68d'},
            {0x0cea, '\ud690'}, {0x0ceb, '\ud694'}, {0x0cec, '\ud69d'}, {0x0ced, '\ud69f'},
            {0x0cee, '\ud6a1'}, {0x0cef, '\ud6a8'}, {0x0cf0, '\ud6ac'}, {0x0cf1, '\ud6b0'},
            {0x0cf2, '\ud6b9'}, {0x0cf3, '\ud6bb'}, {0x0cf4, '\ud6c4'}, {0x0cf5, '\ud6c5'},
            {0x0cf6, '\ud6c8'}, {0x0cf7, '\ud6cc'}, {0x0cf8, '\ud6d1'}, {0x0cf9, '\ud6d4'},
            {0x0cfa, '\ud6d7'}, {0x0cfb, '\ud6d9'}, {0x0cfc, '\ud6e0'}, {0x0cfd, '\ud6e4'},
            {0x0cfe, '\ud6e8'}, {0x0cff, '\ud6f0'}, {0x0d00, '\ud6f5'}, {0x0d01, '\ud6fc'},
            {0x0d02, '\ud6fd'}, {0x0d03, '\ud700'}, {0x0d04, '\ud704'}, {0x0d05, '\ud711'},
            {0x0d06, '\ud718'}, {0x0d07, '\ud719'}, {0x0d08, '\ud71c'}, {0x0d09, '\ud720'},
            {0x0d0a, '\ud728'}, {0x0d0b, '\ud729'}, {0x0d0c, '\ud72b'}, {0x0d0d, '\ud72d'},
            {0x0d0e, '\ud734'}, {0x0d0f, '\ud735'}, {0x0d10, '\ud738'}, {0x0d11, '\ud73c'},
            {0x0d12, '\ud744'}, {0x0d13, '\ud747'}, {0x0d14, '\ud749'}, {0x0d15, '\ud750'},
            {0x0d16, '\ud751'}, {0x0d17, '\ud754'}, {0x0d18, '\ud756'}, {0x0d19, '\ud757'},
            {0x0d1a, '\ud758'}, {0x0d1b, '\ud759'}, {0x0d1c, '\ud760'}, {0x0d1d, '\ud761'},
            {0x0d1e, '\ud763'}, {0x0d1f, '\ud765'}, {0x0d20, '\ud769'}, {0x0d21, '\ud76c'},
            {0x0d22, '\ud770'}, {0x0d23, '\ud774'}, {0x0d24, '\ud77c'}, {0x0d25, '\ud77d'},
            {0x0d26, '\ud781'}, {0x0d27, '\ud788'}, {0x0d28, '\ud789'}, {0x0d29, '\ud78c'},
            {0x0d2a, '\ud790'}, {0x0d2b, '\ud798'}, {0x0d2c, '\ud799'}, {0x0d2d, '\ud79b'},
            {0x0d2e, '\ud79d'}, {0x0d31, '\u1100'}, {0x0d32, '\u1101'}, {0x0d33, '\u1102'},
            {0x0d34, '\u1103'}, {0x0d35, '\u1104'}, {0x0d36, '\u1105'}, {0x0d37, '\u1106'},
            {0x0d38, '\u1107'}, {0x0d39, '\u1108'}, {0x0d3a, '\u1109'}, {0x0d3b, '\u110a'},
            {0x0d3c, '\u110b'}, {0x0d3d, '\u110c'}, {0x0d3e, '\u110d'}, {0x0d3f, '\u110e'},
            {0x0d40, '\u110f'}, {0x0d41, '\u1110'}, {0x0d42, '\u1111'}, {0x0d43, '\u1112'},
            {0x0d44, '\u1161'}, {0x0d45, '\u1162'}, {0x0d46, '\u1163'}, {0x0d47, '\u1164'},
            {0x0d48, '\u1165'}, {0x0d49, '\u1166'}, {0x0d4a, '\u1167'}, {0x0d4b, '\u1168'},
            {0x0d4c, '\u1169'}, {0x0d4d, '\u116d'}, {0x0d4e, '\u116e'}, {0x0d4f, '\u1172'},
            {0x0d50, '\u1173'}, {0x0d51, '\u1175'}, {0x0d61, '\ub894'}, {0x0d62, '\uc330'},
            {0x0d63, '\uc3bc'}, {0x0d64, '\uc4d4'}, {0x0d65, '\ucb2c'},
        };
	}
}
