using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TESVSnip.Domain.Services
{
    static class RecordLayout
    {
        /// <summary>
        /// When copy as New do not include these records.
        /// </summary>
        public static readonly string[] NoNewCopyTypes = new[]
        {
           "CELL", "WRLD", "LAND", "NAVM", "TES4"
        };

        public static readonly string[] LooseGroups = new[]
        {
           "CELL", "WRLD", "REFR", "ACRE", "ACHR", "NAVM", "DIAL", "INFO"
        };

        /// <summary>
        /// The sanitize order.
        /// </summary>
        /// HAZD appears twice in Skyrim.esm.  2nd entry is empty.
        public static readonly string[] SanitizeOrder = new[]
        {
            //"GMST", "KYWD", "LCRT", "AACT", "TXST", "GLOB", "CLAS", "FACT", "HDPT", "HAIR", "EYES", "RACE", "SOUN", "ASPC", "MGEF", "SCPT", "LTEX", "ENCH", "SPEL", "SCRL", "ACTI", "TACT", "ARMO", 
            //"BOOK", "CONT", "DOOR", "INGR", "LIGH", "MISC", "APPA", "STAT", "SCOL", "MSTT", "PWAT", "GRAS", "TREE", "CLDC", "FLOR", "FURN", "WEAP", "AMMO", "NPC_", "LVLN", "KEYM", "ALCH", "IDLM", 
            //"COBJ", "PROJ", "HAZD", "SLGM", "LVLI", "WTHR", "CLMT", "SPGD", "RFCT", "REGN", "NAVI", "CELL", "WRLD", "DIAL", "QUST", "IDLE", "PACK", "CSTY", "LSCR", "LVSP", "ANIO", "WATR", "EFSH", 
            //"EXPL", "DEBR", "IMGS", "IMAD", "FLST", "PERK", "BPTD", "ADDN", "AVIF", "CAMS", "CPTH", "VTYP", "MATT", "IPCT", "IPDS", "ARMA", "ECZN", "LCTN", "MESG", "RGDL", "DOBJ", "LGTM", "MUSC", 
            //"FSTP", "FSTS", "SMBN", "SMQN", "SMEN", "DLBR", "MUST", "DLVW", "WOOP", "SHOU", "EQUP", "RELA", "SCEN", "ASTP", "OTFT", "ARTO", "MATO", "MOVT", "HAZD", "SNDR", "DUAL", "SNCT", "SOPM", 
            //"COLL", "CLFM", "REVB"
            "GMST", "KYWD", "LCRT", "AACT", "TXST", "GLOB", "CLAS",
            "FACT", "HDPT", "HAIR", "EYES", "RACE", "SOUN", "ASPC",
            "MGEF", "SCPT", "LTEX", "ENCH", "SPEL", "SCRL", "ACTI",
            "TACT", "ARMO", "BOOK", "CONT", "DOOR", "INGR", "LIGH",
            "MISC", "APPA", "STAT", "SCOL", "MSTT", "PWAT", "GRAS",
            "TREE", "CLDC", "FLOR", "FURN", "WEAP", "AMMO", "NPC_",
            "LVLN", "KEYM", "ALCH", "IDLM", "COBJ", "PROJ", "HAZD",
            "SLGM", "LVLI", "WTHR", "CLMT", "SPGD", "RFCT", "REGN",
            "NAVI", "CELL", "REFR", "ACHR", "NAVM", "PGRE", "PHZD",
            "WRLD", "LAND", "DIAL", "INFO", "QUST", "IDLE", "PACK",
            "CSTY", "LSCR", "LVSP", "ANIO", "WATR", "EFSH", "EXPL",
            "DEBR", "IMGS", "IMAD", "FLST", "PERK", "BPTD", "ADDN",
            "AVIF", "CAMS", "CPTH", "VTYP", "MATT", "IPCT", "IPDS",
            "ARMA", "ECZN", "LCTN", "MESG", "RGDL", "DOBJ", "LGTM",
            "MUSC", "FSTP", "FSTS", "SMBN", "SMQN", "SMEN", "DLBR",
            "MUST", "DLVW", "WOOP", "SHOU", "EQUP", "RELA", "SCEN",
            "ASTP", "OTFT", "ARTO", "MATO", "MOVT", "HAZD", "SNDR",
            "DUAL", "SNCT", "SOPM", "COLL", "CLFM", "REVB"
        };
    }
}
