using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinFormsSaveFileDialog = System.Windows.Forms.SaveFileDialog;
using WpfSaveFileDialog = Microsoft.Win32.SaveFileDialog;

using MediaColor = System.Windows.Media.Color;
using MediaColors = System.Windows.Media.Colors;
using MediaColorConverter = System.Windows.Media.ColorConverter;

namespace ColorPicker
{
    public partial class MainWindow : Window
    {
        private MediaColor currentColor = MediaColors.White;
        private string currentColorHex = "#FFFFFF";
        private MediaColor? closestMatchedColor = null;
        private string closestMatchedColorHex = "";
        private string closestMatchedColorName = "";

        private static readonly string[] DefaultCategorySelection = { "--Select Category--" };
        private static readonly Regex RgbPattern = new Regex(@"^\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(\d{1,3})\s*$");

        // Dictionaries for colors and languages
        private readonly Dictionary<string, Dictionary<string, string>> colors = new()
        {
            {
                "Reds", new Dictionary<string, string>
                {
                    { "BeanRed", "#F75D59" },
                    { "BlackBean", "#3D0C02" },
                    { "BloodNight", "#551606" },
                    { "Burgundy", "#8C001A" },
                    { "CarbonRed", "#A70D2A" },
                    { "CherryRed", "#C24641" },
                    { "ChilliPepper", "#C11B17" },
                    { "ChocolateBrown", "#3F000F" },
                    { "Cranberry", "#9F000F" },
                    { "Crimson", "#DC143C" },
                    { "CrimsonRed", "#990000" },
                    { "DarkBurgundy", "#800020" },
                    { "DarkMaroon", "#2F0909" },
                    { "DarkRed", "#8B0000" },
                    { "DarkSalmon", "#E9967A" },
                    { "DarkScarlet", "#560319" },
                    { "DeepRed", "#800517" },
                    { "FerrariRed", "#F70D1A" },
                    { "FireBrick", "#B22222" },
                    { "FireEngineRed", "#F62817" },
                    { "GarnetRed", "#733635" },
                    { "Grapefruit", "#DC381F" },
                    { "IndianRed", "#CD5C5C" },
                    { "LavaRed", "#E42217" },
                    { "LightCoral", "#F08080" },
                    { "LoveRed", "#E41B17" },
                    { "MaroonRed", "#8F0B0B" },
                    { "Midnight", "#2B1B17" },
                    { "NeonRed", "#FD1C03" },
                    { "PastelRed", "#F67280" },
                    { "PinkCoral", "#E77471" },
                    { "Red", "#FF0000" },
                    { "RedBlood", "#660000" },
                    { "RedWine", "#990012" },
                    { "RubyRed", "#F62217" },
                    { "SaffronRed", "#931314" },
                    { "Salmon", "#FA8072" },
                    { "ScarletRed", "#FF2400" },
                    { "ShockingOrange", "#E55B3C" },
                    { "StrawberryRed", "#C83F49" },
                    { "TomatoSauceRed", "#B21807" },
                    { "ValentineRed", "#E55451" },
                    { "Vermilion", "#7E191B" }
                }
            },
            {
                "Pinks", new Dictionary<string, string>
                {
                    { "BlossomPink", "#F9B7FF" },
                    { "BlushPink", "#E6A9EC" },
                    { "CottonCandy", "#FCDFFF" },
                    { "DarkRaspberry", "#872657" },
                    { "DeepPeach", "#FFCBA4" },
                    { "DeepPink", "#FF1493" },
                    { "DesertSand", "#EDC9AF" },
                    { "DullPurple", "#7F525D" },
                    { "DuskyPink", "#CC7A8B" },
                    { "DustyPink", "#D58A94" },
                    { "DustyRose", "#C9A9A6" },
                    { "GoldPink", "#E6C7C2" },
                    { "HotPink", "#FF69B4" },
                    { "KhakiRose", "#C5908E" },
                    { "LightPink", "#FFB6C1" },
                    { "LipstickPink", "#C48793" },
                    { "MediumVioletRed", "#C71585" },
                    { "OldRose", "#C08081" },
                    { "PaleVioletRed", "#DB7093" },
                    { "PastelBrown", "#B1907F" },
                    { "PastelOrange", "#F8B88B" },
                    { "PastelPurple", "#F2A2E8" },
                    { "PeriwinklePink", "#E9CFEC" },
                    { "PigPink", "#FDD7E4" },
                    { "Pink", "#FFC0CB" },
                    { "PinkBrown", "#C48189" },
                    { "PinkDaisy", "#E799A3" },
                    { "PlumPie", "#7D0541" },
                    { "PlumVelvet", "#7D0552" },
                    { "Puce", "#7F5A58" },
                    { "PurpleLily", "#550A35" },
                    { "PurpleMaroon", "#810541" },
                    { "PurpleThistle", "#D2B9D3" },
                    { "Rose", "#E8ADAA" },
                    { "RoseDust", "#997070" },
                    { "RoseGold", "#ECC5C0" },
                    { "Rosy-Finch", "#7F4E52" },
                    { "RosyPink", "#B38481" },
                    { "SilverPink", "#C4AEAD" },
                    { "UnbleachedSilk", "#FFDDCA" },
                    { "VelvetMaroon", "#7E354D" },
                    { "VioletRed", "#F6358A" },
                    { "WatermelonPink", "#FC6C85" },
                    { "WisteriaPurple", "#C6AEC7" }
                }
            },
            {
                "Oranges", new Dictionary<string, string>
                {
                    { "BasketBallOrange", "#F88158" },
                    { "ConstructionConeOrange", "#F87431" },
                    { "Coral", "#FF7F50" },
                    { "DarkOrange", "#FF8C00" },
                    { "IndianSaffron", "#FF7722" },
                    { "LightCopper", "#DA8A67" },
                    { "LightSalmon", "#FFA07A" },
                    { "LightSalmonRose", "#F9966B" },
                    { "MangoOrange", "#FF8040" },
                    { "Orange", "#FFA500" },
                    { "OrangeRed", "#FF4500" },
                    { "PinkOrange", "#F89880" },
                    { "SalmonPink", "#FF8674" },
                    { "SunriseOrange", "#E67451" },
                    { "Tangerine", "#E78A61" },
                    { "Tomato", "#FF6347" }
                }
            },
            {
                "Yellows", new Dictionary<string, string>
                {
                    { "Amber", "#FFBF00" },
                    { "BananaYellow", "#F5E216" },
                    { "Beer", "#FBB117" },
                    { "Blonde", "#FBF6D9" },
                    { "BoldYellow", "#F9DB24" },
                    { "BrightGold", "#FDD017" },
                    { "BrownSugar", "#E2A76F" },
                    { "CamelBrown", "#C19A6B" },
                    { "CanaryYellow", "#FFEF00" },
                    { "Cantaloupe", "#FFA62F" },
                    { "CardboardBrown", "#EDDA74" },
                    { "Champagne", "#F7E7CE" },
                    { "CheeseOrange", "#FFA600" },
                    { "ChromeGold", "#FFCE44" },
                    { "CoralPeach", "#FBD5AB" },
                    { "CornYellow", "#FFF380" },
                    { "Cream", "#FFFFCC" },
                    { "CreamWhite", "#FFFDD0" },
                    { "DarkBlonde", "#F0E2B6" },
                    { "DarkKhaki", "#BDB76B" },
                    { "DeepYellow", "#F6BE00" },
                    { "DirtyWhite", "#E8E4C9" },
                    { "Gold", "#FFD700" },
                    { "GoldenBlonde", "#FBE7A1" },
                    { "GoldenBrown", "#EAC117" },
                    { "GoldenSilk", "#F3E3C3" },
                    { "GoldenYellow", "#FFDF00" },
                    { "HarvestGold", "#EDE275" },
                    { "Khaki", "#F0E68C" },
                    { "LemonChiffon", "#FFFACD" },
                    { "LemonYellow", "#FEF250" },
                    { "LightBeige", "#FFF0DB" },
                    { "LightGold", "#F1E5AC" },
                    { "LightGoldenrodYellow", "#FAFAD2" },
                    { "LightOrange", "#FED8B1" },
                    { "LightYellow", "#FFFFE0" },
                    { "MacaroniAndCheese", "#F2BB66" },
                    { "Moccasin", "#FFE4B5" },
                    { "MustardYellow", "#FFDB58" },
                    { "NeonGold", "#FDBD01" },
                    { "NeonYellow", "#FFFF33" },
                    { "PaleGoldenrod", "#EEE8AA" },
                    { "PapayaWhip", "#FFEFD5" },
                    { "Parchment", "#FFFFC2" },
                    { "PastelYellow", "#FAF884" },
                    { "Peach", "#FFE5B4" },
                    { "PeachPuff", "#FFDAB9" },
                    { "RubberDuckyYellow", "#FFD801" },
                    { "SafetyYellow", "#EED202" },
                    { "Saffron", "#FBB917" },
                    { "SunYellow", "#FFE87C" },
                    { "TanBrown", "#ECE5B6" },
                    { "Vanilla", "#F3E5AB" },
                    { "WhiteYellow", "#F2F0DF" },
                    { "Yellow", "#FFFF00" },
                    { "YellowOrange", "#FFAE42" }
                }
            },
            {
                "Greens", new Dictionary<string, string>
                {
                    { "AcidGreen", "#B0BF1A" },
                    { "AlgaeGreen", "#64E986" },
                    { "AlienGreen", "#6CC417" },
                    { "AloeVeraGreen", "#98F516" },
                    { "AquaGreen", "#12E193" },
                    { "ArmyGreen", "#4B5320" },
                    { "AvocadoGreen", "#B2C248" },
                    { "BasilGreen", "#829F82" },
                    { "BrightGreen", "#66FF00" },
                    { "BroccoliGreen", "#026C3D" },
                    { "CactusGreen", "#227442" },
                    { "CamouflageGreen", "#78866B" },
                    { "ChameleonGreen", "#BDF516" },
                    { "Chartreuse", "#7FFF00" },
                    { "ChromeWhite", "#E8F1D4" },
                    { "CitronGreen", "#8FB31D" },
                    { "CloverGreen", "#3EA055" },
                    { "DarkCyan", "#008B8B" },
                    { "DarkForestGreen", "#254117" },
                    { "DarkGreen", "#006400" },
                    { "DarkLimeGreen", "#41A317" },
                    { "DarkOliveGreen", "#556B2F" },
                    { "DarkSeaGreen", "#8FBC8F" },
                    { "DeepEmeraldGreen", "#046307" },
                    { "DeepGreen", "#056608" },
                    { "DinosaurGreen", "#73A16C" },
                    { "DollarBillGreen", "#85BB65" },
                    { "DragonGreen", "#6AFB92" },
                    { "DullGreenYellow", "#B1FB17" },
                    { "Ebony", "#555D50" },
                    { "EmeraldGreen", "#5FFB17" },
                    { "FallForestGreen", "#4E9258" },
                    { "FernGreen", "#667C26" },
                    { "ForestGreen", "#228B22" },
                    { "FrogGreen", "#99C68E" },
                    { "GrassGreen", "#3F9B0B" },
                    { "GrayGreen", "#A2AD9C" },
                    { "Green", "#008000" },
                    { "GreenApple", "#4CC417" },
                    { "GreenLeaves", "#3A5F0B" },
                    { "GreenOnion", "#6AA121" },
                    { "GreenPeas", "#89C35C" },
                    { "GreenPepper", "#4AA02C" },
                    { "GreenSnake", "#6CBB3C" },
                    { "GreenThumb", "#B5EAAA" },
                    { "GreenYellow", "#ADFF2F" },
                    { "HazelGreen", "#617C58" },
                    { "HummingbirdGreen", "#7FE817" },
                    { "HunterGreen", "#355E3B" },
                    { "IguanaGreen", "#9CB071" },
                    { "IrishGreen", "#08A04B" },
                    { "JadeGreen", "#5EFB6E" },
                    { "JungleGreen", "#347C2C" },
                    { "KellyGreen", "#4CC552" },
                    { "LawnGreen", "#7CFC00" },
                    { "LemonGreen", "#ADF802" },
                    { "LightGreen", "#90EE90" },
                    { "LightJade", "#C3FDB8" },
                    { "LightMintGreen", "#C2E5D3" },
                    { "LightOliveGreen", "#B8BC86" },
                    { "LightRoseGreen", "#DBF9DB" },
                    { "LightSeaGreen", "#20B2AA" },
                    { "Lime", "#00FF00" },
                    { "LimeGreen", "#32CD32" },
                    { "LimeMintGreen", "#36F57F" },
                    { "LotusGreen", "#004225" },
                    { "MediumAquamarine", "#66CDAA" },
                    { "MediumForestGreen", "#347235" },
                    { "MediumSeaGreen", "#3CB371" },
                    { "MediumSpringGreen", "#00FA9A" },
                    { "MetallicGreen", "#7C9D8E" },
                    { "MilitaryGreen", "#4E5B31" },
                    { "MintGreen", "#98FF98" },
                    { "MossGreen", "#8A9A5B" },
                    { "NebulaGreen", "#59E817" },
                    { "NeonGreen", "#16F529" },
                    { "NeonYellowGreen", "#DAEE01" },
                    { "OceanGreen", "#00FF80" },
                    { "Olive", "#808000" },
                    { "OliveDrab", "#6B8E23" },
                    { "OrganicBrown", "#E3F9A6" },
                    { "PaleGreen", "#98FB98" },
                    { "ParrotGreen", "#12AD2B" },
                    { "PastelGreen", "#77DD77" },
                    { "PeaGreen", "#52D017" },
                    { "PineGreen", "#387C44" },
                    { "PistachioGreen", "#9DC209" },
                    { "RacingGreen", "#27742C" },
                    { "SageGreen", "#848B79" },
                    { "SaladGreen", "#A1C935" },
                    { "SeaGreen", "#2E8B57" },
                    { "SeaweedGreen", "#437C17" },
                    { "ShamrockGreen", "#347C17" },
                    { "SlimeGreen", "#BCE954" },
                    { "SpringGreen", "#00FF7F" },
                    { "StoplightGoGreen", "#57E964" },
                    { "Teal", "#008080" },
                    { "TeaGreen", "#CCFB5D" },
                    { "TurquoiseGreen", "#A0D6B4" },
                    { "VenomGreen", "#728C00" },
                    { "YellowGreen", "#9ACD32" },
                    { "YellowGreenGrosbeak", "#E2F516" },
                    { "YellowLawnGreen", "#87F717" },
                    { "ZombieGreen", "#54C571" }
                }
            },
            {
                "Blues", new Dictionary<string, string>
                {
                    { "Aqua", "#00FFFF" },
                    { "Aquamarine", "#7FFFD4" },
                    { "AquamarineStone", "#348781" },
                    { "AquaSeafoamGreen", "#93E9BE" },
                    { "AzureBlue", "#4863A0" },
                    { "BabyBlue", "#95B9C7" },
                    { "BalloonBlue", "#2B60DE" },
                    { "BeetleGreen", "#4C787E" },
                    { "Blue", "#0000FF" },
                    { "BlueAngel", "#B7CEEC" },
                    { "BlueberryBlue", "#0041C2" },
                    { "BlueDiamond", "#4EE2EC" },
                    { "BlueDress", "#157DEC" },
                    { "BlueEyes", "#1569C7" },
                    { "BlueGreen", "#7BCCB5" },
                    { "BlueHosta", "#77BFC7" },
                    { "BlueIvy", "#3090C7" },
                    { "BlueJay", "#2B547E" },
                    { "BlueKoi", "#659EC7" },
                    { "BlueLagoon", "#8EEBEC" },
                    { "BlueMossGreen", "#3C565B" },
                    { "BlueOrchid", "#1F45FC" },
                    { "BlueRibbon", "#306EFF" },
                    { "BlueTurquoise", "#43C6DB" },
                    { "BlueZircon", "#57FEFF" },
                    { "BottleGreen", "#006A4E" },
                    { "BrightBlue", "#0909FF" },
                    { "BrightCyan", "#0AFFFF" },
                    { "BrightNavyBlue", "#1974D2" },
                    { "BrightTeal", "#01F9C6" },
                    { "BrightTurquoise", "#16E2F5" },
                    { "ButterflyBlue", "#38ACEC" },
                    { "CadetBlue", "#5F9EA0" },
                    { "CanaryBlue", "#2916F5" },
                    { "Celeste", "#50EBEC" },
                    { "CharcoalBlue", "#36454F" },
                    { "ChromeGreen", "#1AA260" },
                    { "CobaltBlue", "#0020C2" },
                    { "ColumbiaBlue", "#87AFC7" },
                    { "CoralBlue", "#AFDCEC" },
                    { "CornflowerBlue", "#6495ED" },
                    { "CrystalBlue", "#5CB3FF" },
                    { "Cyan", "#00FFFF" },
                    { "CyanBlue", "#14A3C7" },
                    { "CyanOpaque", "#92C7C7" },
                    { "DarkBlue", "#00008B" },
                    { "DarkBlueGray", "#29465B" },
                    { "DarkGreenBlue", "#1F6357" },
                    { "DarkMint", "#31906E" },
                    { "DarkSkyBlue", "#0059FF" },
                    { "DarkSlate", "#2B3856" },
                    { "DarkTeal", "#045D5D" },
                    { "DarkTurquoise", "#00CED1" },
                    { "DaySkyBlue", "#82CAFF" },
                    { "DeepSea", "#3B9C9C" },
                    { "DeepSeaGreen", "#306754" },
                    { "DeepSkyBlue", "#00BFFF" },
                    { "DeepTeal", "#033E3E" },
                    { "DeepTurquoise", "#48CCCD" },
                    { "DenimBlue", "#79BAEC" },
                    { "DenimDarkBlue", "#151B8D" },
                    { "DodgerBlue", "#1E90FF" },
                    { "DullSeaGreen", "#4E8975" },
                    { "EarthBlue", "#0000A5" },
                    { "EarthGreen", "#34A56F" },
                    { "ElectricBlue", "#9AFEFF" },
                    { "ElfGreen", "#1B8A6B" },
                    { "Emerald", "#50C878" },
                    { "EstorilBlue", "#2F539B" },
                    { "GlacialBlueIce", "#368BC1" },
                    { "GrayishTurquoise", "#5E7D7E" },
                    { "GreenishBlue", "#307D7E" },
                    { "GulfBlue", "#C9DFEC" },
                    { "Gunmetal", "#2C3539" },
                    { "HeavenlyBlue", "#C6DEFF" },
                    { "Iceberg", "#56A5EC" },
                    { "IsleOfManGreen", "#22CE83" },
                    { "Jade", "#00A36C" },
                    { "JeansBlue", "#A0CFEC" },
                    { "Jellyfish", "#46C7C7" },
                    { "LapisBlue", "#15317E" },
                    { "LavenderBlue", "#E3E4FA" },
                    { "LightAquamarine", "#93FFE8" },
                    { "LightBlue", "#ADD8E6" },
                    { "LightCyan", "#E0FFFF" },
                    { "LightDayBlue", "#ADDFFF" },
                    { "LightPurpleBlue", "#728FCE" },
                    { "LightSkyBlue", "#87CEFA" },
                    { "LightSlate", "#CCFFFF" },
                    { "LightSteelBlue", "#B0C4DE" },
                    { "LightTeal", "#B3D9D9" },
                    { "MacawBlueGreen", "#43BFC7" },
                    { "MagicMint", "#AAF0D1" },
                    { "MarbleBlue", "#566D7E" },
                    { "MediumBlue", "#0000CD" },
                    { "MediumSlateBlue", "#7B68EE" },
                    { "MediumTeal", "#045F5F" },
                    { "MediumTurquoise", "#48D1CC" },
                    { "MidnightBlue", "#191970" },
                    { "MiddayBlue", "#3BB9FF" },
                    { "Mint", "#3EB489" },
                    { "MistBlue", "#646D7E" },
                    { "Navy", "#000080" },
                    { "NeonBlue", "#1589FF" },
                    { "NewMidnightBlue", "#0000A0" },
                    { "NorthernLightsBlue", "#78C7C7" },
                    { "OceanBlue", "#2B65EC" },
                    { "PaleBlueLily", "#CFECEC" },
                    { "PaleTurquoise", "#AFEEEE" },
                    { "PastelBlue", "#B4CFEC" },
                    { "PastelLightBlue", "#D5D6EA" },
                    { "PowderBlue", "#B0E0E6" },
                    { "RatGray", "#6D7B8D" },
                    { "RobinEggBlue", "#BDEDFF" },
                    { "RoyalBlue", "#4169E1" },
                    { "SamcoBlue", "#0002FF" },
                    { "SapphireBlue", "#2554C7" },
                    { "SeaBlue", "#C2DFFF" },
                    { "SeafoamGreen", "#3EA99F" },
                    { "SeaTurtleGreen", "#438D80" },
                    { "SilkBlue", "#488AC7" },
                    { "SkyBlue", "#87CEEB" },
                    { "SkyBlueDress", "#6698FF" },
                    { "SlateBlueGray", "#737CA1" },
                    { "SteelBlue", "#4682B4" },
                    { "SteelGray", "#71797E" },
                    { "TealBlue", "#007C80" },
                    { "TealGreen", "#00827F" },
                    { "TiffanyBlue", "#81D8D0" },
                    { "TronBlue", "#7DFDFE" },
                    { "Turquoise", "#40E0D0" },
                    { "Water", "#EBF4FA" },
                    { "WaterBlue", "#0E87CC" },
                    { "WhiteBlue", "#DBE9FA" },
                    { "WindowsBlue", "#357EC7" }
                }
            },
            {
                "Purples", new Dictionary<string, string>
                {
                    { "Amethyst", "#9966CC" },
                    { "BlueViolet", "#8A2BE2" },
                    { "DarkMagenta", "#8B008B" },
                    { "DarkOrchid", "#9932CC" },
                    { "DarkSlateBlue", "#483D8B" },
                    { "DarkViolet", "#9400D3" },
                    { "Fuchsia", "#FF00FF" },
                    { "Grape", "#5E5A80" },
                    { "Indigo", "#4B0082" },
                    { "Lavender", "#E6E6FA" },
                    { "Magenta", "#FF00FF" },
                    { "MediumOrchid", "#BA55D3" },
                    { "MediumPurple", "#9370DB" },
                    { "Orchid", "#DA70D6" },
                    { "Plum", "#DDA0DD" },
                    { "Purple", "#800080" },
                    { "PurpleNavy", "#4E5180" },
                    { "PurpleWhite", "#DFD3E3" },
                    { "RebeccaPurple", "#663399" },
                    { "SlateBlue", "#6A5ACD" },
                    { "Thistle", "#D8BFD8" },
                    { "Violet", "#EE82EE" }
                }
            },
            {
                "Browns", new Dictionary<string, string>
                {
                    { "AntiqueBronze", "#665D1E" },
                    { "ArmyBrown", "#827B60" },
                    { "BakersBrown", "#5C3317" },
                    { "BeeYellow", "#E9AB17" },
                    { "Bisque", "#FFE4C4" },
                    { "BlanchedAlmond", "#FFEBCD" },
                    { "Bronze", "#CD7F32" },
                    { "Brown", "#A52A2A" },
                    { "BrownBear", "#835C3B" },
                    { "BulletShell", "#AF9B60" },
                    { "BurlyWood", "#DEB887" },
                    { "Caramel", "#C68E17" },
                    { "ChampagneGold", "#D29F51" },
                    { "Chocolate", "#D2691E" },
                    { "Cinnamon", "#C58917" },
                    { "Coffee", "#6F4E37" },
                    { "CookieBrown", "#C7A317" },
                    { "Cooper", "#B87333" },
                    { "Cornsilk", "#FFF8DC" },
                    { "DarkAlmond", "#AB784E" },
                    { "DarkBeige", "#9F8C76" },
                    { "DarkBronze", "#804A00" },
                    { "DarkBrown", "#654321" },
                    { "DarkCoffee", "#3B2F2F" },
                    { "DarkGoldenrod", "#B8860B" },
                    { "DarkGrayishOlive", "#4A412A" },
                    { "DarkHazelBrown", "#473810" },
                    { "DarkMoccasin", "#827839" },
                    { "DarkSienna", "#8A4117" },
                    { "DarkYellow", "#8B8000" },
                    { "Goldenrod", "#DAA520" },
                    { "GrayBrown", "#3D3635" },
                    { "Hazel", "#8E7618" },
                    { "KhakiBrown", "#906E3E" },
                    { "KhakiGreen", "#8A865D" },
                    { "Marigold", "#EBA832" },
                    { "Maroon", "#800000" },
                    { "MetallicGold", "#D4AF37" },
                    { "MilkChocolate", "#513B1C" },
                    { "MillenniumJade", "#93917C" },
                    { "Mocha", "#493D26" },
                    { "Mustard", "#E1AD01" },
                    { "NavajoWhite", "#FFDEAD" },
                    { "OakBrown", "#806517" },
                    { "OldBurgundy", "#43302E" },
                    { "OrangeGold", "#D4A017" },
                    { "Peru", "#CD853F" },
                    { "PullmanBrown", "#644117" },
                    { "PumpkinPie", "#CA762B" },
                    { "RedBrown", "#622F22" },
                    { "RedDirt", "#7F5217" },
                    { "RosyBrown", "#BC8F8F" },
                    { "SaddleBrown", "#8B4513" },
                    { "SandyBrown", "#F4A460" },
                    { "SchoolBusYellow", "#E8A317" },
                    { "Sepia", "#7F462C" },
                    { "SepiaBrown", "#704214" },
                    { "Sienna", "#A0522D" },
                    { "Sandstone", "#786D5F" },
                    { "Tan", "#D2B48C" },
                    { "Taupe", "#483C32" },
                    { "TigerOrange", "#C88141" },
                    { "WesternCharcoal", "#49413F" },
                    { "Wheat", "#F5DEB3" },
                    { "Wood", "#966F33" }
                }
            },
            {
                "Whites", new Dictionary<string, string>
                {
                    { "AliceBlue", "#F0F8FF" },
                    { "AntiqueWhite", "#FAEBD7" },
                    { "Azure", "#F0FFFF" },
                    { "Beige", "#F5F5DC" },
                    { "FloralWhite", "#FFFAF0" },
                    { "GhostWhite", "#F8F8FF" },
                    { "HoneyDew", "#F0FFF0" },
                    { "Ivory", "#FFFFF0" },
                    { "LavenderBlush", "#FFF0F5" },
                    { "Linen", "#FAF0E6" },
                    { "MintCream", "#F5FFFA" },
                    { "OldLace", "#FDF5E6" },
                    { "PearlWhite", "#F8F6F0" },
                    { "Platinum", "#E5E4E2" },
                    { "RedWhite", "#F3E8EA" },
                    { "SeaShell", "#FFF5EE" },
                    { "Snow", "#FFFAFA" },
                    { "White", "#FFFFFF" },
                    { "WhiteSmoke", "#F5F5F5" }
                }
            },
            {
                "Grays", new Dictionary<string, string>
                {
                    { "AlienGray", "#736F6E" },
                    { "AshGray", "#666362" },
                    { "BattleshipGray", "#848482" },
                    { "Black", "#000000" },
                    { "BlackCat", "#413839" },
                    { "BlackCow", "#4C4646" },
                    { "CarbonGray", "#625D5D" },
                    { "Charcoal", "#34282C" },
                    { "ChromeAluminum", "#A8A9AD" },
                    { "CloudyGray", "#6D6968" },
                    { "ColdMetal", "#9B9A96" },
                    { "DarkGainsboro", "#8C8C8C" },
                    { "DarkGray", "#A9A9A9" },
                    { "DarkSlateGray", "#2F4F4F" },
                    { "DimGray", "#696969" },
                    { "Gainsboro", "#DCDCDC" },
                    { "GearSteelGray", "#C0C6C7" },
                    { "Granite", "#837E7C" },
                    { "Gray", "#808080" },
                    { "GrayCloud", "#B6B6B4" },
                    { "GrayDolphin", "#5C5858" },
                    { "GrayGoose", "#D1D0CE" },
                    { "GrayWolf", "#504A4B" },
                    { "GunmetalGray", "#8D918D" },
                    { "Iridium", "#3D3C3A" },
                    { "IronGray", "#52595D" },
                    { "LightBlack", "#454545" },
                    { "LightGray", "#D3D3D3" },
                    { "LightSlateGray", "#778899" },
                    { "LightSteelGray", "#E0E5E5" },
                    { "Metal", "#B6B6B6" },
                    { "NardoGray", "#686A6C" },
                    { "Night", "#0C090A" },
                    { "PaleSilver", "#C9C0BB" },
                    { "PlatinumGray", "#797979" },
                    { "PlatinumSilver", "#CECECE" },
                    { "SheetMetal", "#888B90" },
                    { "Silver", "#C0C0C0" },
                    { "SilverWhite", "#DADBDD" },
                    { "SlateGray", "#708090" },
                    { "SmokeyGray", "#726E6D" },
                    { "SonicSilver", "#757575" },
                    { "StainlessSteelGray", "#99A3A3" },
                    { "Steampunk", "#C9C1C1" },
                    { "VampireGray", "#565051" },
                    { "WhiteGray", "#EEEEEE" }
                }
            }
        };

        private readonly Dictionary<string, string> languages = new()
        {
            { "css", "CSS" },
            { "html", "HTML" },
            { "xml", "XML" },
            { "json", "JSON" },
            { "yaml", "YAML" },
            { "toml", "TOML" },
            { "ini", "INI" },
            { "javascript", "JavaScript" },
            { "typescript", "TypeScript" },
            { "python", "Python" },
            { "java", "Java" },
            { "csharp", "C#" },
            { "vbnet", "VB.NET" },
            { "fsharp", "F#" },
            { "php", "PHP" },
            { "ruby", "Ruby" },
            { "swift", "Swift" },
            { "kotlin", "Kotlin" },
            { "scala", "Scala" },
            { "groovy", "Groovy" },
            { "dart", "Dart" },
            { "go", "Go" },
            { "rust", "Rust" },
            { "cpp", "C++" },
            { "c", "C" },
            { "haskell", "Haskell" },
            { "erlang", "Erlang" },
            { "elixir", "Elixir" },
            { "clojure", "Clojure" },
            { "lisp", "Common Lisp" },
            { "scheme", "Scheme" },
            { "racket", "Racket" },
            { "r", "R" },
            { "matlab", "MATLAB" },
            { "julia", "Julia" },
            { "sql", "SQL" },
            { "plsql", "PL/SQL" },
            { "powershell", "PowerShell" },
            { "bash", "Bash" },
            { "perl", "Perl" },
            { "lua", "Lua" },
            { "tcl", "Tcl" },
            { "awk", "AWK" },
            { "sed", "Sed" },
            { "zig", "Zig" },
            { "nim", "Nim" },
            { "crystal", "Crystal" },
            { "fortran", "Fortran" },
            { "cobol", "COBOL" },
            { "pascal", "Pascal" },
            { "delphi", "Delphi" },
            { "ada", "Ada" },
            { "assembly", "Assembly" },
            { "prolog", "Prolog" },
            { "mercury", "Mercury" },
            { "ocaml", "OCaml" },
            { "reasonml", "ReasonML" },
            { "elm", "Elm" },
            { "sml", "SML" },
            { "janet", "Janet" },
            { "regex", "Regex" },
            { "objc", "Objective-C" },
            { "vba", "VBA" },
            { "logo", "Logo" },
            { "forth", "Forth" },
            { "haxe", "Haxe" },
            { "smalltalk", "Smalltalk" },
            { "postscript", "PostScript" },
            { "qsharp", "Q#" },
            { "processing", "Processing" },
            { "shell", "Shell" },
            { "markdown", "Markdown" },
            { "dockerfile", "Dockerfile" },
            { "graphql", "GraphQL" },
            { "jsx", "JSX" },
            { "sass", "Sass" },
            { "less", "Less" },
            { "solidity", "Solidity" },
            { "gdscript", "GDScript" },
            { "latex", "LaTeX" },
            { "verilog", "Verilog" },
            { "vhdl", "VHDL" },
            { "raku", "Raku" },
            { "apl", "APL" },
            { "wasm", "WebAssembly" },
            { "d", "D" },
            { "vala", "Vala" },
            { "purescript", "PureScript" },
            { "idris", "Idris" },
            { "agda", "Agda" },
            { "coffeescript", "CoffeeScript" },
            { "livescript", "LiveScript" },
            { "actionscript", "ActionScript" },
            { "autoit", "AutoIt" },
            { "autohotkey", "AutoHotkey" },
            { "dylan", "Dylan" },
            { "eiffel", "Eiffel" },
            { "io", "Io" },
            { "mongodb", "MongoDB" },
            { "mysql", "MySQL" },
            { "postgresql", "PostgreSQL" },
            { "rebol", "REBOL" },
            { "red", "Red" },
            { "ring", "Ring" },
            { "sas", "SAS" },
            { "stata", "Stata" },
            { "systemverilog", "SystemVerilog" },
            { "unrealscript", "UnrealScript" },
            { "wolfram", "Wolfram Language" },
            { "zsh", "Zsh" }
        };

        // Languages metadata: key -> (Display, snippet generator)
        private readonly Dictionary<string, (string display, Func<MediaColor,string> snippet)> languageSnippets;

        public MainWindow()
        {
            InitializeComponent();
            languageSnippets = BuildLanguageSnippets();
            PopulateDropdowns();
            UpdateColor(MediaColors.White, "#FFFFFF");
        }

        private static string ColorToHex(MediaColor color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

        private Dictionary<string,(string display, Func<MediaColor,string> snippet)> BuildLanguageSnippets()
        {
            return new(StringComparer.OrdinalIgnoreCase)
            {
                ["css"] = ("CSS", c => $"color: {ColorToHex(c)};\r\n/* or */\r\ncolor: rgb({c.R}, {c.G}, {c.B});"),
                ["html"] = ("HTML", c => $"<span style=\"color: {ColorToHex(c)};\"></span>"),
                ["xml"] = ("XML", c => $"<color hex=\"{ColorToHex(c)}\" rgb=\"{c.R},{c.G},{c.B}\" />"),
                ["json"] = ("JSON", c => $"{{\r\n  \"hex\": \"{ColorToHex(c)}\",\r\n  \"rgb\": {{ \"r\": {c.R}, \"g\": {c.G}, \"b\": {c.B} }}\r\n}}"),
                ["yaml"] = ("YAML", c => $"hex: {ColorToHex(c)}\nrgb: [{c.R}, {c.G}, {c.B}]"),
                ["toml"] = ("TOML", c => $"hex = \"{ColorToHex(c)}\"\nrgb = [{c.R}, {c.G}, {c.B}]"),
                ["ini"] = ("INI", c => $"[color]\nhex={ColorToHex(c)}\nr={c.R}\ng={c.G}\nb={c.B}"),
                ["javascript"] = ("JavaScript", c => $"const colorHex = '{ColorToHex(c)}';\nconst colorRGB = {{ r: {c.R}, g: {c.G}, b: {c.B} }};"),
                ["typescript"] = ("TypeScript", c => $"const colorHex: string = '{ColorToHex(c)}';\ninterface RGB {{ r:number; g:number; b:number }}\nconst colorRGB: RGB = {{ r:{c.R}, g:{c.G}, b:{c.B} }};"),
                ["python"] = ("Python", c => $"color_hex = '{ColorToHex(c)}'\ncolor_rgb = ({c.R}, {c.G}, {c.B})"),
                ["java"] = ("Java", c => $"String colorHex = \"{ColorToHex(c)}\";\nint[] rgb = {{{c.R}, {c.G}, {c.B}}};"),
                ["csharp"] = ("C#", c => $"string colorHex = \"{ColorToHex(c)}\";\nvar rgb = new {{ R = {c.R}, G = {c.G}, B = {c.B} }};"),
                ["vbnet"] = ("VB.NET", c => $"Dim colorHex = \"{ColorToHex(c)}\"\nDim r = {c.R}, g = {c.G}, b = {c.B}"),
                ["fsharp"] = ("F#", c => $"let colorHex = \"{ColorToHex(c)}\"\nlet rgb = {c.R}, {c.G}, {c.B}"),
                ["php"] = ("PHP", c => $"$colorHex = '{ColorToHex(c)}';\n$rgb = ['r'=>{c.R},'g'=>{c.G},'b'=>{c.B}];"),
                ["ruby"] = ("Ruby", c => $"color_hex = '{ColorToHex(c)}'\nrgb = {{ r: {c.R}, g: {c.G}, b: {c.B} }}"),
                ["swift"] = ("Swift", c => $"let colorHex = \"{ColorToHex(c)}\"\nlet rgb = ({c.R}, {c.G}, {c.B})"),
                ["kotlin"] = ("Kotlin", c => $"val colorHex = \"{ColorToHex(c)}\"\ndata class RGB(val r:Int, val g:Int, val b:Int)\nval rgb = RGB({c.R}, {c.G}, {c.B})"),
                ["scala"] = ("Scala", c => $"val colorHex = \"{ColorToHex(c)}\"\ncase class RGB(r:Int,g:Int,b:Int)\nval rgb = RGB({c.R},{c.G},{c.B})"),
                ["groovy"] = ("Groovy", c => $"def colorHex = '{ColorToHex(c)}'\ndef rgb = [r:{c.R}, g:{c.G}, b:{c.B}]"),
                ["dart"] = ("Dart", c => $"const colorHex = '{ColorToHex(c)}';\nconst color = Color.fromARGB(255,{c.R},{c.G},{c.B});"),
                ["go"] = ("Go", c => $"colorHex := \"{ColorToHex(c)}\"\ntype RGB struct{{R,G,B uint8}}\nrgb := RGB{{{c.R},{c.G},{c.B}}}"),
                ["rust"] = ("Rust", c => $"let color_hex = \"{ColorToHex(c)}\";\nlet rgb = ({c.R}u8,{c.G}u8,{c.B}u8);"),
                ["cpp"] = ("C++", c => $"std::string colorHex = \"{ColorToHex(c)}\";\nstruct RGB{{int r,g,b;}} rgb{{{c.R},{c.G},{c.B}}};"),
                ["c"] = ("C", c => $"const char* colorHex = \"{ColorToHex(c)}\";\nunsigned char rgb[3] = {{{c.R}, {c.G}, {c.B}}};"),
                ["haskell"] = ("Haskell", c => $"colorHex = \"{ColorToHex(c)}\"\nrgb = ({c.R},{c.G},{c.B})"),
                ["erlang"] = ("Erlang", c => $"-define(COLOR_HEX, \"{ColorToHex(c)}\").\n-define(COLOR_RGB, {{{c.R},{c.G},{c.B}}})."),
                ["elixir"] = ("Elixir", c => $"color_hex = \"{ColorToHex(c)}\"\nrgb = {{{c.R}, {c.G}, {c.B}}}"),
                ["clojure"] = ("Clojure", c => $"(def color-hex \"{ColorToHex(c)}\")\n(def rgb [{c.R} {c.G} {c.B}])"),
                ["lisp"] = ("Common Lisp", c => $"(defparameter *color-hex* \"{ColorToHex(c)}\")\n(defparameter *rgb* '({c.R} {c.G} {c.B}))"),
                ["scheme"] = ("Scheme", c => $"(define color-hex \"{ColorToHex(c)}\")\n(define rgb '({c.R} {c.G} {c.B}))"),
                ["racket"] = ("Racket", c => $"(define color-hex \"{ColorToHex(c)}\")\n(define rgb '({c.R} {c.G} {c.B}))"),
                ["r"] = ("R", c => $"color_hex <- \"{ColorToHex(c)}\"\nrgb <- c({c.R},{c.G},{c.B})"),
                ["matlab"] = ("MATLAB", c => $"colorHex = '{ColorToHex(c)}';\nrgb = [{c.R} {c.G} {c.B}];\nrgbNorm = [{c.R}/255 {c.G}/255 {c.B}/255];"),
                ["julia"] = ("Julia", c => $"color_hex = \"{ColorToHex(c)}\"\nrgb = ({c.R},{c.G},{c.B})"),
                ["sql"] = ("SQL", c => $"DECLARE @colorHex VARCHAR(7) = '{ColorToHex(c)}';\nDECLARE @r INT={c.R}, @g INT={c.G}, @b INT={c.B};"),
                ["plsql"] = ("PL/SQL", c => $"DECLARE color_hex VARCHAR2(7):='{ColorToHex(c)}'; r PLS_INTEGER:={c.R}; g PLS_INTEGER:={c.G}; b PLS_INTEGER:={c.B}; BEGIN NULL; END;"),
                ["powershell"] = ("PowerShell", c => $"$colorHex = '{ColorToHex(c)}'\n$rgb = @{{r={c.R}; g={c.G}; b={c.B}}}"),
                ["bash"] = ("Bash", c => $"COLOR_HEX='{ColorToHex(c)}'\nR={c.R}; G={c.G}; B={c.B}"),
                ["perl"] = ("Perl", c => $"my $color_hex = '{ColorToHex(c)}';\nmy @rgb = ({c.R},{c.G},{c.B});"),
                ["lua"] = ("Lua", c => $"local color_hex = '{ColorToHex(c)}'\nlocal rgb = {{ r={c.R}, g={c.G}, b={c.B} }}"),
                ["tcl"] = ("Tcl", c => $"set color_hex \"{ColorToHex(c)}\"\nset rgb {{{c.R} {c.G} {c.B}}}"),
                ["awk"] = ("AWK", c => $"BEGIN{{colorHex=\"{ColorToHex(c)}\"; r={c.R}; g={c.G}; b={c.B};}}"),
                ["sed"] = ("Sed", c => $"# usage examples replacing with the color\ns/{ColorToHex(c)}/replacement/g"),
                ["zig"] = ("Zig", c => $"const COLOR_HEX = \"{ColorToHex(c)}\"; const rgb = [_]u8{{{c.R},{c.G},{c.B}}};"),
                ["nim"] = ("Nim", c => $"let colorHex = \"{ColorToHex(c)}\"\nlet rgb = [{c.R}, {c.G}, {c.B}]"),
                ["crystal"] = ("Crystal", c => $"color_hex = \"{ColorToHex(c)}\"\nrgb = {{ {c.R}, {c.G}, {c.B} }}"),
                ["fortran"] = ("Fortran", c => $"character(len=7) :: colorHex = '{ColorToHex(c)}'\ninteger, dimension(3) :: rgb = (/ {c.R},{c.G},{c.B} /)"),
                ["cobol"] = ("COBOL", c => $"01 COLOR-HEX PIC X(7) VALUE '{ColorToHex(c)}'.\n01 COLOR-R PIC 9(3) VALUE {c.R}."),
                ["pascal"] = ("Pascal", c => $"const ColorHex = '{ColorToHex(c)}';\nvar rgb: array[0..2] of Byte = ({c.R},{c.G},{c.B});"),
                ["delphi"] = ("Delphi", c => $"const ColorHex = '{ColorToHex(c)}';\nvar RGB: record R,G,B:Byte end = (R:{c.R};G:{c.G};B:{c.B});"),
                ["ada"] = ("Ada", c => $"Color_Hex : constant String := \"{ColorToHex(c)}\";\nRGB : constant array(1..3) of Integer := ({c.R}, {c.G}, {c.B});"),
                ["assembly"] = ("Assembly", c => $"colorHex db '{ColorToHex(c)}',0\nrgb db {c.R},{c.G},{c.B}"),
                ["prolog"] = ("Prolog", c => $"color_hex('{ColorToHex(c)}').\nrgb({c.R},{c.G},{c.B})."),
                ["mercury"] = ("Mercury", c => $"color_hex(\"{ColorToHex(c)}\").\nrgb({c.R},{c.G},{c.B})."),
                ["ocaml"] = ("OCaml", c => $"let color_hex = \"{ColorToHex(c)}\"\nlet rgb = ({c.R},{c.G},{c.B})"),
                ["reasonml"] = ("ReasonML", c => $"let colorHex = \"{ColorToHex(c)}\";\nlet rgb = ({c.R},{c.G},{c.B});"),
                ["elm"] = ("Elm", c => $"colorHex = \"{ColorToHex(c)}\"\nrgb = ({c.R},{c.G},{c.B})"),
                ["sml"] = ("SML", c => $"val colorHex = \"{ColorToHex(c)}\"\nval rgb = ({c.R}, {c.G}, {c.B})"),
                ["janet"] = ("Janet", c => $"(def color-hex \"{ColorToHex(c)}\")\n(def rgb [{c.R} {c.G} {c.B}])"),
                ["regex"] = ("Regex", c => $"{ColorToHex(c).Substring(1)}|{c.R},{c.G},{c.B}"),
                ["objc"] = ("Objective-C", c => $"NSString *colorHex = @\"{ColorToHex(c)}\";\nCGFloat r = {c.R}/255.0, g = {c.G}/255.0, b = {c.B}/255.0;\nUIColor *color = [UIColor colorWithRed:r green:g blue:b alpha:1.0];"),
                ["vba"] = ("VBA", c => $"Dim ColorHex As String: ColorHex = \"{ColorToHex(c)}\"\nDim R As Integer: R={c.R}\nDim G As Integer: G={c.G}\nDim B As Integer: B={c.B}"),
                ["logo"] = ("Logo", c => $"make \"colorHex \"{ColorToHex(c)}\nmake \"rgb [{c.R} {c.G} {c.B}]"),
                ["forth"] = ("Forth", c => $"\\ Hex {ColorToHex(c)}\n{c.R} {c.G} {c.B} \" RGB"),
                ["haxe"] = ("Haxe", c => $"var colorHex:String = \"{ColorToHex(c)}\";\nvar rgb = {{ r:{c.R}, g:{c.G}, b:{c.B} }};"),
                ["smalltalk"] = ("Smalltalk", c => $"colorHex := '{ColorToHex(c)}'.\nrgb := {{ {c.R}. {c.G}. {c.B} }}."),
                ["postscript"] = ("PostScript", c => $"/R {c.R} def /G {c.G} def /B {c.B} def\nR 255 div G 255 div B 255 div setrgbcolor"),
                ["qsharp"] = ("Q#", c => $"let colorHex = \"{ColorToHex(c)}\";\nlet rgb = ({c.R}, {c.G}, {c.B});"),
                ["processing"] = ("Processing", c => $"String colorHex = \"{ColorToHex(c)}\";\ncolor c = color({c.R}, {c.G}, {c.B});"),
                ["shell"] = ("Shell", c => $"COLOR_HEX='{ColorToHex(c)}'\nR={c.R}; G={c.G}; B={c.B}"),
                ["markdown"] = ("Markdown", c => $"`{ColorToHex(c)}`\nRGB: {c.R}, {c.G}, {c.B}"),
                ["dockerfile"] = ("Dockerfile", c => $"ENV COLOR_HEX={ColorToHex(c)} COLOR_R={c.R} COLOR_G={c.G} COLOR_B={c.B}"),
                ["graphql"] = ("GraphQL", c => $"fragment ColorInfo on Color {{\n  hex: \"{ColorToHex(c)}\"\n  rgb {{ r: {c.R}, g: {c.G}, b: {c.B} }}\n}}"),
                ["jsx"] = ("JSX", c => $"const style = {{ color: '{ColorToHex(c)}' }};\n<span style={{{{color: '{ColorToHex(c)}'}}}}></span>"),
                ["sass"] = ("Sass", c => $"$color: {ColorToHex(c)};\n.selector {{ color: $color; }}"),
                ["less"] = ("Less", c => $"@color: {ColorToHex(c)};\n.selector {{ color: @color; }}"),
                ["solidity"] = ("Solidity", c => $"string constant COLOR_HEX = \"{ColorToHex(c)}\";\nuint8 constant R={c.R}; uint8 constant G={c.G}; uint8 constant B={c.B};"),
                ["gdscript"] = ("GDScript", c => $"var COLOR_HEX = \"{ColorToHex(c)}\"\nvar rgb = Color8({c.R}, {c.G}, {c.B})"),
                ["latex"] = ("LaTeX", c => $"\\definecolor{{custom}}{{RGB}}{{{c.R},{c.G},{c.B}}} % {ColorToHex(c)}"),
                ["verilog"] = ("Verilog", c => $"localparam COLOR_HEX = 24'h{ColorToHex(c).Substring(1)};"),
                ["vhdl"] = ("VHDL", c => $"constant COLOR_HEX : std_logic_vector(23 downto 0) := x\"{ColorToHex(c).Substring(1)}\";"),
                ["raku"] = ("Raku", c => $"my $color-hex = '{ColorToHex(c)}';\nmy @rgb = {c.R},{c.G},{c.B};"),
                ["apl"] = ("APL", c => $"⍝ Color\ncolorHex←'{ColorToHex(c)}' ⋄ rgb←{c.R} {c.G} {c.B}"),
                ["wasm"] = ("WebAssembly", c => $";; 0x{ColorToHex(c).Substring(1)}\n(global $color (mut i32) (i32.const 0x{ColorToHex(c).Substring(1)}))"),
                ["d"] = ("D", c => $"string colorHex = \"{ColorToHex(c)}\";\nauto rgb = [{c.R}, {c.G}, {c.B}];"),
                ["vala"] = ("Vala", c => $"string color_hex = \"{ColorToHex(c)}\";\nvar rgb = {{ {c.R}, {c.G}, {c.B} }};"),
                ["purescript"] = ("PureScript", c => $"colorHex :: String\ncolorHex = \"{ColorToHex(c)}\"\n\nrgb :: {{ r :: Int, g :: Int, b :: Int }}\nrgb = {{ r: {c.R}, g: {c.G}, b: {c.B} }}"),
                ["idris"] = ("Idris", c => $"colorHex : String\ncolorHex = \"{ColorToHex(c)}\"\n\nrgb : (Int, Int, Int)\nrgb = ({c.R}, {c.G}, {c.B})"),
                ["agda"] = ("Agda", c => $"colorHex : String\ncolorHex = \"{ColorToHex(c)}\"\n\nrgb : {c.R} {c.G} {c.B}"),
                ["coffeescript"] = ("CoffeeScript", c => $"colorHex = '{ColorToHex(c)}'\nrgb = r:{c.R}, g:{c.G}, b:{c.B}"),
                ["livescript"] = ("LiveScript", c => $"color-hex = '{ColorToHex(c)}'\nrgb = r: {c.R}, g: {c.G}, b: {c.B}"),
                ["actionscript"] = ("ActionScript", c => $"var colorHex:String = \"{ColorToHex(c)}\";\nvar rgb:Object = {{r:{c.R}, g:{c.G}, b:{c.B}}};"),
                ["autoit"] = ("AutoIt", c => $"Global $sColorHex = \"{ColorToHex(c)}\"\nGlobal $iR = {c.R}, $iG = {c.G}, $iB = {c.B}"),
                ["autohotkey"] = ("AutoHotkey", c => $"colorHex := \"{ColorToHex(c)}\"\nrgb := {{r:{c.R}, g:{c.G}, b:{c.B}}}"),
                ["dylan"] = ("Dylan", c => $"define constant $color-hex = \"{ColorToHex(c)}\";\ndefine constant $rgb = #({c.R}, {c.G}, {c.B});"),
                ["eiffel"] = ("Eiffel", c => $"color_hex: STRING = \"{ColorToHex(c)}\"\nrgb: ARRAY[INTEGER] is [{c.R}, {c.G}, {c.B}]"),
                ["io"] = ("Io", c => $"colorHex := \"{ColorToHex(c)}\"\nrgb := list({c.R}, {c.G}, {c.B})"),
                ["mongodb"] = ("MongoDB", c => $"db.colors.insert({{ hex: \"{ColorToHex(c)}\", rgb: {{ r: {c.R}, g: {c.G}, b: {c.B} }} }})"),
                ["mysql"] = ("MySQL", c => $"SET @color_hex = '{ColorToHex(c)}';\nSET @r = {c.R}, @g = {c.G}, @b = {c.B};"),
                ["postgresql"] = ("PostgreSQL", c => $"SELECT '{ColorToHex(c)}' AS color_hex, ROW({c.R}, {c.G}, {c.B}) AS rgb;"),
                ["rebol"] = ("REBOL", c => $"color-hex: {ColorToHex(c)}\nrgb: make tuple! [{c.R} {c.G} {c.B}]"),
                ["red"] = ("Red", c => $"color-hex: #{ColorToHex(c).Substring(1)}\nrgb: make tuple! [{c.R} {c.G} {c.B}]"),
                ["ring"] = ("Ring", c => $"colorHex = \"{ColorToHex(c)}\"\nrgb = [{c.R},{c.G},{c.B}]"),
                ["sas"] = ("SAS", c => $"%let color_hex = {ColorToHex(c)};\n%let r = {c.R}; %let g = {c.G}; %let b = {c.B};"),
                ["stata"] = ("Stata", c => $"local color_hex \"{ColorToHex(c)}\"\nglobal rgb \"{c.R} {c.G} {c.B}\""),
                ["systemverilog"] = ("SystemVerilog", c => $"localparam COLOR_HEX = 24'h{ColorToHex(c).Substring(1)};"),
                ["unrealscript"] = ("UnrealScript", c => $"var string ColorHex = \"{ColorToHex(c)}\";\nvar Color RGB = (R={c.R},G={c.G},B={c.B});"),
                ["wolfram"] = ("Wolfram Language", c => $"colorHex = \"{ColorToHex(c)}\";\nrgb = RGBColor[{c.R}/255, {c.G}/255, {c.B}/255];"),
                ["zsh"] = ("Zsh", c => $"COLOR_HEX='{ColorToHex(c)}'\ndeclare -A rgb=(r {c.R} g {c.G} b {c.B})")
            };
        }

        private void PopulateDropdowns()
        {
            cboCategory.ItemsSource = DefaultCategorySelection.Concat(colors.Keys.OrderBy(k => k));
            cboCategory.SelectedIndex = 0;
            PopulateLanguageDropdown();
        }

        private void PopulateLanguageDropdown()
        {
            cboLanguage.ItemsSource = languageSnippets.Values.Select(v => v.display).OrderBy(s => s).ToList();
            if (cboLanguage.Items.Count > 0)
                cboLanguage.SelectedIndex = 0;
        }

        private void CboCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var colorNames = new List<string> { "--Select Color--" };
            if (cboCategory.SelectedIndex > 0 && cboCategory.SelectedItem is string category && colors.TryGetValue(category, out var categoryColors))
            {
                colorNames.AddRange(categoryColors.Keys.OrderBy(k => k));
            }
            cboColorName.ItemsSource = colorNames;
            cboColorName.SelectedIndex = 0;
        }

        private void CboColorName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboColorName.SelectedItem is string colorName && colorName != "--Select Color--" && cboCategory.SelectedItem is string category)
            {
                if (colors.TryGetValue(category, out var categoryColors) && categoryColors.TryGetValue(colorName, out var hexColor))
                {
                    UpdateColor((MediaColor)MediaColorConverter.ConvertFromString(hexColor), hexColor);
                }
            }
        }

        private void CboLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e) => GenerateCode();

        private void BtnColorPicker_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var wpfColor = MediaColor.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                UpdateColor(wpfColor, ColorToHex(wpfColor));
            }
        }

        private async void BtnEyedropper_Click(object sender, RoutedEventArgs e)
        {
            var originalState = this.WindowState;
            var originalVisibility = this.Visibility;
            try
            {
                this.Visibility = Visibility.Hidden;
                await Task.Delay(100);
                var eyedropper = new Eyedropper();
                var color = eyedropper.PickColor();
                if (color.HasValue)
                {
                    UpdateColor(color.Value, ColorToHex(color.Value));
                }
            }
            finally
            {
                this.Visibility = originalVisibility;
                this.WindowState = originalState;
                this.Activate();
            }
        }

        private void BtnGetCurrent_Click(object sender, RoutedEventArgs e)
        {
            txtColorInput.Text = currentColorHex;
            lblResult.Text = "Loaded current color. Click Find & Set to evaluate nearest named color.";
            lblResult.Background = new SolidColorBrush(MediaColor.FromArgb(255, 40, 60, 80));
            btnSetNearest.IsEnabled = false;
        }

        private void BtnFindColor_Click(object sender, RoutedEventArgs e)
        {
            if (ParseColorInput(txtColorInput.Text) is MediaColor parsedColor)
            {
                string hex = ColorToHex(parsedColor);
                UpdateColor(parsedColor, hex);
                var (closestName, closestHex) = FindClosestNamedColor(parsedColor);
                if (!string.IsNullOrEmpty(closestName))
                {
                    closestMatchedColor = (MediaColor)MediaColorConverter.ConvertFromString(closestHex);
                    closestMatchedColorHex = closestHex;
                    closestMatchedColorName = closestName;
                    btnSetNearest.IsEnabled = true;

                    if (string.Equals(closestHex, hex, StringComparison.OrdinalIgnoreCase))
                    {
                        lblResult.Text = $"✓ Set: {hex} | Exact Match: {closestName}";
                        lblResult.Background = new SolidColorBrush(MediaColor.FromArgb(255, 40, 80, 40));
                    }
                    else
                    {
                        lblResult.Text = $"✓ Set: {hex} | Closest Match: {closestName} ({closestHex})";
                        lblResult.Background = new SolidColorBrush(MediaColor.FromArgb(255, 60, 60, 40));
                    }
                }
                else
                {
                    btnSetNearest.IsEnabled = false;
                }
            }
            else
            {
                lblResult.Text = "✗ Invalid format. Use hex (#RRGGBB) or RGB (R, G, B).";
                lblResult.Background = new SolidColorBrush(MediaColor.FromArgb(255, 80, 40, 40));
                btnSetNearest.IsEnabled = false;
                closestMatchedColor = null;
                closestMatchedColorHex = "";
                closestMatchedColorName = "";
            }
        }

        private void BtnSetNearest_Click(object sender, RoutedEventArgs e)
        {
            if (closestMatchedColor.HasValue)
            {
                UpdateColor(closestMatchedColor.Value, closestMatchedColorHex);
                lblResult.Text = $"✓ Color set to: {closestMatchedColorName} ({closestMatchedColorHex})";
                lblResult.Background = new SolidColorBrush(MediaColor.FromArgb(255, 40, 80, 40));
            }
        }

        private void BtnCreatePNG_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(numWidth.Text, out int width) || !int.TryParse(numHeight.Text, out int height))
                {
                    lblPngResult.Text = "Error: Invalid width or height.";
                    lblPngResult.Foreground = new SolidColorBrush(MediaColors.IndianRed);
                    return;
                }
                if (width <= 0 || height <= 0)
                {
                    lblPngResult.Text = "Error: Width and height must be greater than 0.";
                    lblPngResult.Foreground = new SolidColorBrush(MediaColors.IndianRed);
                    return;
                }
                if (width > 5000 || height > 5000)
                {
                    lblPngResult.Text = "Error: Maximum dimension is 5000 pixels.";
                    lblPngResult.Foreground = new SolidColorBrush(MediaColors.IndianRed);
                    return;
                }
                var renderBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                var visual = new DrawingVisual();
                using (var context = visual.RenderOpen())
                {
                    context.DrawRectangle(new SolidColorBrush(currentColor), null, new Rect(0, 0, width, height));
                }
                renderBitmap.Render(visual);
                var pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                var saveFileDialog = new WpfSaveFileDialog
                {
                    Filter = "PNG Image|*.png",
                    FileName = $"Color_{currentColorHex.Substring(1)}_{width}x{height}_{DateTime.Now:yyyyMMdd_HHmmss}.png"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    using var fs = File.OpenWrite(saveFileDialog.FileName);
                    pngEncoder.Save(fs);
                    lblPngResult.Text = $"✓ PNG created successfully: {Path.GetFileName(saveFileDialog.FileName)}";
                    lblPngResult.Foreground = new SolidColorBrush(MediaColors.LightGreen);
                }
            }
            catch (Exception ex)
            {
                lblPngResult.Text = $"Error creating PNG: {ex.Message}";
                lblPngResult.Foreground = new SolidColorBrush(MediaColors.IndianRed);
            }
        }

        private void UpdateColor(MediaColor color, string hex)
        {
            currentColor = color;
            currentColorHex = hex.ToUpper();
            pnlColorDisplay.Background = new SolidColorBrush(color);
            GenerateCode();
        }

        private void GenerateCode()
        {
            if (cboLanguage.SelectedItem is not string display) return;
            var kv = languageSnippets.FirstOrDefault(k => k.Value.display == display);
            if (kv.Value.display == null) return;
            txtColorOutput.Text = kv.Value.snippet(currentColor);
        }

        private static MediaColor? ParseColorInput(string input)
        {
            input = input.Trim();
            if (input.StartsWith('#'))
            {
                try { return (MediaColor)MediaColorConverter.ConvertFromString(input); } catch { return null; }
            }
            var rgbMatch = RgbPattern.Match(input);
            if (rgbMatch.Success && byte.TryParse(rgbMatch.Groups[1].Value, out byte r) && byte.TryParse(rgbMatch.Groups[2].Value, out byte g) && byte.TryParse(rgbMatch.Groups[3].Value, out byte b))
            {
                return MediaColor.FromArgb(255, r, g, b);
            }
            return null;
        }

        private (string, string) FindClosestNamedColor(MediaColor targetColor)
        {
            return colors.SelectMany(c => c.Value)
                         .Select(colorEntry => new
                         {
                             Name = colorEntry.Key,
                             Hex = colorEntry.Value,
                             Color = (MediaColor)MediaColorConverter.ConvertFromString(colorEntry.Value)
                         })
                         .MinBy(item => Math.Pow(targetColor.R - item.Color.R, 2) + Math.Pow(targetColor.G - item.Color.G, 2) + Math.Pow(targetColor.B - item.Color.B, 2))
                         is var closest && closest != null
                         ? (closest.Name, closest.Hex)
                         : (string.Empty, string.Empty);
        }

        // End of class
    }
}