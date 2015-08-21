using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.XPath;
using umbraco.DataLayer;

namespace OurUmbraco.Wiki.BusinessLogic {
   public class Data {

        private static string _ConnString = umbraco.GlobalSettings.DbDSN;
        private static ISqlHelper _sqlHelper;

       /// <summary>
       /// Gets the SQL helper.
       /// </summary>
       /// <value>The SQL helper.</value>
       public static ISqlHelper SqlHelper
       {
           get { return _sqlHelper ?? (_sqlHelper = DataLayerHelper.CreateSqlHelper(_ConnString)); }
       }


       public static XmlNode GetDataSetAsNode(string sql, string elementName) {
            try {
                DataSet ds = getDataSetFromSql(_ConnString, sql, elementName);
                XmlDataDocument dataDoc = new XmlDataDocument(ds);
                return (XmlNode)dataDoc;
            } catch (Exception e) {
                // If there's an exception we'll output an error element instead
                XmlDocument errorDoc = new XmlDocument();
                return umbraco.xmlHelper.addTextNode(errorDoc, "error", System.Web.HttpUtility.HtmlEncode(e.ToString()));
            }
        }

        public static XPathNodeIterator GetDataSet(string sql, string elementName) {
            return GetDataSetAsNode(sql, elementName).CreateNavigator().Select(".");
        }

        /// <summary>
        /// Gets the dataset from SQL using ADO.NET.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="setName">Name of the set.</param>
        /// <returns></returns>
        private static DataSet getDataSetFromSql(string connection, string sql, string tableName) {
            SqlConnection con = new SqlConnection(connection);
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet(Pluralizer.ToPlural(tableName));
            try {
                con.Open();
                adapter.Fill(ds, tableName);
            } finally {
                con.Close();
            }
            return ds;
        }



 /* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive
 * License (MS-PL). A copy of the license can be found in the license.htm file
 * included in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/





public static class Pluralizer {
    #region public APIs

    public static string ToPlural(string noun) {
        return AdjustCase(ToPluralInternal(noun), noun);
    }

    public static string ToSingular(string noun) {
        return AdjustCase(ToSingularInternal(noun), noun);
    }

    public static bool IsNounPluralOfNoun(string plural, string singular) {
        return String.Compare(ToSingularInternal(plural), singular, StringComparison.OrdinalIgnoreCase) == 0;
    }

    #endregion

    #region Special Words Table

    static string[] _specialWordsStringTable = new string[] {
        "agendum",          "agenda",           "",
        "albino",           "albinos",          "",
        "alga",             "algae",            "",
        "alumna",           "alumnae",          "",
        "apex",             "apices",           "apexes",
        "archipelago",      "archipelagos",     "",
        "bacterium",        "bacteria",         "",
        "beef",             "beefs",            "beeves",
        "bison",            "",                 "",
        "brother",          "brothers",         "brethren",
        "candelabrum",      "candelabra",       "",
        "carp",             "",                 "",
        "casino",           "casinos",          "",
        "child",            "children",         "",
        "chassis",          "",                 "",
        "chinese",          "",                 "",
        "clippers",         "",                 "",
        "cod",              "",                 "",
        "codex",            "codices",          "",
        "commando",         "commandos",        "",
        "corps",            "",                 "",
        "cortex",           "cortices",         "cortexes",
        "cow",              "cows",             "kine",
        "criterion",        "criteria",         "",
        "datum",            "data",             "",
        "debris",           "",                 "",
        "diabetes",         "",                 "",
        "ditto",            "dittos",           "",
        "djinn",            "",                 "",
        "dynamo",           "",                 "",
        "elk",              "",                 "",
        "embryo",           "embryos",          "",
        "ephemeris",        "ephemeris",        "ephemerides",
        "erratum",          "errata",           "",
        "extremum",         "extrema",          "",
        "fiasco",           "fiascos",          "",
        "fish",             "fishes",           "fish",
        "flounder",         "",                 "",
        "focus",            "focuses",          "foci",
        "fungus",           "fungi",            "funguses",
        "gallows",          "",                 "",
        "genie",            "genies",           "genii",
        "ghetto",           "ghettos",           "",
        "graffiti",         "",                 "",
        "headquarters",     "",                 "",
        "herpes",           "",                 "",
        "homework",         "",                 "",
        "index",            "indices",          "indexes",
        "inferno",          "infernos",         "",
        "japanese",         "",                 "",
        "jumbo",            "jumbos",            "",
        "latex",            "latices",          "latexes",
        "lingo",            "lingos",           "",
        "mackerel",         "",                 "",
        "macro",            "macros",           "",
        "manifesto",        "manifestos",       "",
        "measles",          "",                 "",
        "money",            "moneys",           "monies",
        "mongoose",         "mongooses",        "mongoose",
        "mumps",            "",                 "",
        "murex",            "murecis",          "",
        "mythos",           "mythos",           "mythoi",
        "news",             "",                 "",
        "octopus",          "octopuses",        "octopodes",
        "ovum",             "ova",              "",
        "ox",               "ox",               "oxen",
        "photo",            "photos",           "",
        "pincers",          "",                 "",
        "pliers",           "",                 "",
        "pro",              "pros",             "",
        "rabies",           "",                 "",
        "radius",           "radiuses",         "radii",
        "rhino",            "rhinos",           "",
        "salmon",           "",                 "",
        "scissors",         "",                 "",
        "series",           "",                 "",
        "shears",           "",                 "",
        "silex",            "silices",          "",
        "simplex",          "simplices",        "simplexes",
        "soliloquy",        "soliloquies",      "soliloquy",
        "species",          "",                 "",
        "stratum",          "strata",           "",
        "swine",            "",                 "",
        "trout",            "",                 "",
        "tuna",             "",                 "",
        "vertebra",         "vertebrae",        "",
        "vertex",           "vertices",         "vertexes",
        "vortex",           "vortices",         "vortexes",
    };

    #endregion

    #region Suffix Rules Table

    static string[] _suffixRulesStringTable = new string[] {
        "ch",       "ches",
        "sh",       "shes",
        "ss",       "sses",

        "ay",       "ays",
        "ey",       "eys",
        "iy",       "iys",
        "oy",       "oys",
        "uy",       "uys",
        "y",        "ies",

        "ao",       "aos",
        "eo",       "eos",
        "io",       "ios",
        "oo",       "oos",
        "uo",       "uos",
        "o",        "oes",

        "cis",      "ces",
        "sis",      "ses",
        "xis",      "xes",

        "louse",    "lice",
        "mouse",    "mice",

        "zoon",     "zoa",

        "man",      "men",

        "deer",     "deer",
        "fish",     "fish",
        "sheep",    "sheep",
        "itis",     "itis",
        "ois",      "ois",
        "pox",      "pox",
        "ox",       "oxes",

        "foot",     "feet",
        "goose",    "geese",
        "tooth",    "teeth",

        "alf",      "alves",
        "elf",      "elves",
        "olf",      "olves",
        "arf",      "arves",
        "leaf",     "leaves",
        "nife",     "nives",
        "life",     "lives",
        "wife",     "wives",
    };

    #endregion

    #region Implementation Details

    class Word {
        public readonly string Singular;
        public readonly string Plural;
        public readonly string Plural2;

        public Word(string singular, string plural, string plural2) {
            Singular = singular;
            Plural = plural;
            Plural2 = plural2;
        }
    }

    class SuffixRule {
        string _singularSuffix;
        string _pluralSuffix;

        public SuffixRule(string singular, string plural) {
            _singularSuffix = singular;
            _pluralSuffix = plural;
        }

        public bool TryToPlural(string word, out string plural) {
            if (word.EndsWith(_singularSuffix, StringComparison.OrdinalIgnoreCase)) {
                plural = word.Substring(0, word.Length - _singularSuffix.Length) + _pluralSuffix;
                return true;
            }
            else {
                plural = null;
                return false;
            }
        }

        public bool TryToSingular(string word, out string singular) {
            if (word.EndsWith(_pluralSuffix, StringComparison.OrdinalIgnoreCase)) {
                singular = word.Substring(0, word.Length - _pluralSuffix.Length) + _singularSuffix;
                return true;
            }
            else {
                singular = null;
                return false;
            }
        }
    }

    static Dictionary<string, Word> _specialSingulars;
    static Dictionary<string, Word> _specialPlurals;
    static List<SuffixRule> _suffixRules;

    static Pluralizer() {
        // populate lookup tables for special words
        _specialSingulars = new Dictionary<string, Word>(StringComparer.OrdinalIgnoreCase);
        _specialPlurals = new Dictionary<string, Word>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < _specialWordsStringTable.Length; i += 3) {
            string s = _specialWordsStringTable[i];
            string p = _specialWordsStringTable[i + 1];
            string p2 = _specialWordsStringTable[i + 2];

            if (string.IsNullOrEmpty(p)) {
                p = s;
            }

            Word w = new Word(s, p, p2);

            _specialSingulars.Add(s, w);
            _specialPlurals.Add(p, w);

            if (!string.IsNullOrEmpty(p2)) {
                _specialPlurals.Add(p2, w);
            }
        }

        // populate suffix rules list
        _suffixRules = new List<SuffixRule>();

        for (int i = 0; i < _suffixRulesStringTable.Length; i += 2) {
            string singular = _suffixRulesStringTable[i];
            string plural = _suffixRulesStringTable[i + 1];
            _suffixRules.Add(new SuffixRule(singular, plural));
        }
    }

    static string ToPluralInternal(string s) {
        if (string.IsNullOrEmpty(s)) {
            return s;
        }

        // lookup special words
        Word word;

        if (_specialSingulars.TryGetValue(s, out word)) {
            return word.Plural;
        }

        // apply suffix rules
        string plural;

        foreach (SuffixRule rule in _suffixRules) {
            if (rule.TryToPlural(s, out plural)) {
                return plural;
            }
        }

        // apply the default rule
        return s + "s";
    }

    static string ToSingularInternal(string s) {
        if (string.IsNullOrEmpty(s)) {
            return s;
        }

        // lookup special words
        Word word;

        if (_specialPlurals.TryGetValue(s, out word)) {
            return word.Singular;
        }

        // apply suffix rules
        string singular;

        foreach (SuffixRule rule in _suffixRules) {
            if (rule.TryToSingular(s, out singular)) {
                return singular;
            }
        }

        // apply the default rule
        if (s.EndsWith("s", StringComparison.OrdinalIgnoreCase)) {
            return s.Substring(0, s.Length-1);
        }

        return s;
    }

    static string AdjustCase(string s, string template) {
        if (string.IsNullOrEmpty(s)) {
            return s;
        }

        // determine the type of casing of the template string
        bool foundUpperOrLower = false;
        bool allLower = true;
        bool allUpper = true;
        bool firstUpper = false;

        for (int i = 0; i < template.Length; i++) {
            if (Char.IsUpper(template[i])) {
                if (i == 0) firstUpper = true;
                allLower = false;
                foundUpperOrLower = true;
            }
            else if (Char.IsLower(template[i])) {
                allUpper = false;
                foundUpperOrLower = true;
            }
        }

        // change the case according to template
        if (foundUpperOrLower) {
            if (allLower) {
                s = s.ToLowerInvariant();
            }
            else if (allUpper) {
                s = s.ToUpperInvariant();
            }
            else if (firstUpper) {
                if (!Char.IsUpper(s[0])) {
                    s = s.Substring(0, 1).ToUpperInvariant() + s.Substring(1);
                }
            }
        }

        return s;
    }
    #endregion
}



    }
}

