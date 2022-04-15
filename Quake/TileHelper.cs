using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;


namespace Quake
{
    public class RoomTheme
    {
        public ushort tile = TileID.Dirt;
        public ushort wall = WallID.Dirt;
        public TileInfo platform = new TileInfo(TileID.Platforms, 0);
        public TileInfo chair = new TileInfo(TileID.Chairs, 0);
        public TileInfo bench = new TileInfo(TileID.WorkBenches, 0);
        public TileInfo torch = new TileInfo(TileID.Torches, 0);
        public TileInfo chest = new TileInfo(21, 0);
        public RoomTheme()
        {
        }

        public static RoomTheme GetGlass()
        {
            // 玻璃消耗 边框19 墙16-4 平台12-6 椅子1-4 工作台1-10 火把1-1凝胶1木材
            RoomTheme th = new RoomTheme
            {
                tile = TileID.Glass,
                wall = WallID.Glass
            };
            th.platform.style = 14;
            th.chair.style = 18;
            th.bench.style = 25;
            th.torch.style = TorchID.White;
            th.chest.style = 47;

            return th;
        }

        public static RoomTheme GetWood()
        {
            RoomTheme th = new RoomTheme
            {
                tile = TileID.WoodBlock,
                wall = WallID.Wood,
            };
            //th.platform.style = 0;
            //th.chair.style = 0;
            //th.bench.style = 0;
            //th.torch.style = 0;

            return th;
        }
    }


    public class TileInfo
    {
        public ushort type;
        public short style;
        public TileInfo(ushort _type, short _style)
        {
            type = _type;
            style = _style;
        }

        public static TileInfo GetGlassChest() { return new TileInfo(TileID.Glass, 47); }
    }


    public class TileHelper
    {
        // 获得图格对应的物品
        public static int GetItem(int tileX, int tileY)
        {
            int id = 0;
            int type = Main.tile[tileX, tileY].type;
            int frameX = Main.tile[tileX, tileY].frameX;
            int frameY = Main.tile[tileX, tileY].frameY;
            bool check(int w, int h)
            {
                bool pass = true;
                for (int i = tileX; i < tileX + w; i++)
                {
                    for (int k = tileY; k < tileY + h; k++)
                    {
                        if (ContainsSkip(i, k) || !Main.tile[i, k].active() || Main.tile[i, k].type != type)
                        {
                            pass = false;
                            break;
                        }
                    }
                }
                if (pass)
                {
                    skip.Add(new Rectangle(tileX, tileY, w, h));

                    //if (type == 79) utils.Log($"type={type}：{tileX},{tileY} frame：{frameX},{frameY}");
                }
                return pass;
            }

            switch (type)
            {
                // 图格114，宽3高2，物品id398
                case 114: if (check(3, 2)) id = 398; break;  // 工匠作坊 
                case 16: if (check(2, 1)) id = frameX == 0 ? 35 : 716; break;  // 铁砧35/铅716
                case 134: if (check(2, 1)) id = frameX == 0 ? 525 : 1220; break;  // 秘银砧525/山铜砧1220
                case 133: if (check(3, 2)) id = frameX == 0 ? 524 : 1221; break;  // 精金熔炉524/钛金熔炉1221
                case 96: if (check(2, 2)) id = frameX == 0 ? 345 : 1791; break;  // 烹饪锅345/大锅1791
                case 18: if (check(2, 1)) id = ItemID.Sets.Workbenches[frameX / 36]; break;  // 工作台（很多种）world
                case 101: if (check(3, 4)) id = GetBookcases(frameX / 54); break;  // 书架（很多种）
                case 172: if (check(2, 2)) id = GetSinks(frameY / 38); break;  // 水槽（很多种）

                case 17: if (check(3, 2)) id = 33; break;  // 熔炉
                case 77: if (check(3, 2)) id = 221; break;  // 地狱熔炉
                case 355: if (check(3, 3)) id = 3000; break;  // 炼药桌
                case 106: if (check(3, 3)) id = 363; break;  // 锯木机
                case 86: if (check(3, 2)) id = 332; break;  // 织布机
                case 283: if (check(3, 3)) id = 2172; break;  // 重型工作台
                case 228: if (check(3, 3)) id = 1120; break;  // 染缸
                case 243: if (check(3, 3)) id = 1430; break;  // 灌注站
                case 247: if (check(3, 3)) id = 1551; break;  // 自动锤炼机
                case 412: if (check(3, 3)) id = 3549; break;  // 远古操纵机
                case 94: if (check(2, 2)) id = 352; break;  // 酒桶
                case 217: if (check(3, 2)) id = 995; break;  // 搅拌机
                case 218: if (check(3, 2)) id = 996; break;  // 绞肉机
                case 300: if (check(3, 3)) id = 2192; break;  // 骨头焊机
                case 302: if (check(3, 3)) id = 2194; break;  // 玻璃窑
                case 308: if (check(3, 3)) id = 2204; break;  // 蜂蜜分配机
                case 306: if (check(3, 3)) id = 2198; break;  // 冰雪机
                case 304: if (check(3, 3)) id = 2196; break;  // 生命木织机
                case 305: if (check(3, 3)) id = 2197; break;  // 天磨
                case 220: if (check(3, 3)) id = 998; break;  // 固化机
                case 303: if (check(3, 3)) id = 2195; break;  // 丛林蜥蜴熔炉
                case 307: if (check(3, 3)) id = 2203; break;  // 蒸汽朋克锅炉
                case 301: if (check(3, 3)) id = 2193; break;  // 血肉克隆台
                case 499: if (check(3, 3)) id = 4142; break;  // 腐变室
                case 219: if (check(3, 3)) id = 997; break;  // 提炼机

                case 125: if (check(2, 2)) id = 487; break;  // 水晶球
                case 287: if (check(2, 2)) id = 2177; break;  // 弹药箱
                case 377: if (check(3, 2)) id = 3198; break;  // 利器站
                case 621: if (check(2, 2)) id = 3750; break;  // 蛋糕块
                case 354: if (check(3, 3)) id = 2999; break;  // 施法桌

                case 506: if (check(2, 3)) id = 4276; break;  // 韧皮雕像
                case 49: return 148; // 水蜡烛
                case 372: return 3117; // 和平蜡烛
                case 567: if (check(1, 2)) id = 4609; break;  // 花园侏儒
                case 237: if (check(3, 2)) id = 1292; break;  // 丛林蜥蜴祭坛

                case 29: if (check(2, 1)) id = 87; break;  // 猪猪存钱罐
                case 463: if (check(3, 4)) id = 3813; break;  // 护卫熔炉
                case 97: if (check(2, 2)) id = 346; break;  // 保险箱
                case 491: if (check(3, 3)) id = 4076; break;  // 虚空保险库

                case 617: if (check(3, 4)) id = frameX >= 1458 ? 5110 : 4924 + frameX / 54; break;  // 圣物（很多种）（Extra_198）(克眼4924 鹿角怪5110)
                case 215: if (check(3, 2)) id = GetCampfires(frameX / 54); break;// 篝火（很多种）
                case 42: if (check(1, 2)) id = GetLanterns(frameY / 36); break;  // 灯笼（很多种，包括红心灯笼）

                case 10: if (check(1, 3)) id = GetDoors(frameY / 54); break;  // 各种门
                case 11: if (check(2, 3)) id = GetDoors(frameY / 54); break;  // 各种门（打开）
                case 14: if (check(3, 2)) id = GetTables(frameX / 54); break;  // 各种桌子
                case 469: if (check(3, 2)) id = GetTables2(frameX / 54); break;  // 水晶桌子等
                case 15: if (check(1, 2)) id = WorldGen.GetItemDrop_Chair(frameY / 40); break;  // 各种椅子

                case 337: if (check(2, 3)) id = 2702 + frameX / 36; break; // 数字雕像
                case 105: if (check(2, 3)) id = GetStatues(frameX, frameY); break; // 雕像
                case 349: if (check(2, 3)) id = 470; break; // 蘑菇雕像
                case 622: if (check(2, 2)) id = 5008; break; // 茶壶

                case 356: if (check(2, 3)) id = 3064; break; // 附魔日晷
                case 395: if (check(2, 2)) id = 3270; break; // 物品框
                case 471: if (check(3, 3)) id = 2699; break; // 武器架

                case 91: if (check(1, 3)) id = GetBanners(frameX, frameY); break; // 各种旗帜
                case 464: if (check(5, 4)) id = 3814; break; // 战争桌
                case 465: if (check(2, 3)) id = 3815; break; // 战争桌旗
                case 466: if (check(5, 3)) id = 3816; break; // 艾德尼亚水晶座
                case 455: if (check(3, 3)) id = 3747; break; // 派对中心



                case 89: if (check(3, 2)) id = WorldGen.GetItemDrop_Benches(frameX / 54); break; // 沙发
                case 85: if (check(2, 2)) id = GetTombstones(frameX / 36); break; // 墓碑

                //case 4: return GetTorches(frameY / 22); // 火把
                //case 13: return GetBottles(frameX / 18);  // 各种玻璃杯
                //case 19: return GetPlatfroms(frameY / 18); // 各种平台
                //case 380: return 3215 + (frameY / 18); // 种植盆

                case 21: if (check(2, 2)) id = Chest.chestItemSpawn[frameX / 36]; break; // 箱子1
                case 467: if (check(2, 2)) id = Chest.chestItemSpawn2[frameX / 36]; break; // 箱子2 水晶箱，金箱子，死人宝箱
                case 88: if (check(3, 2)) id = GetDressers(frameX / 54); break;  // 梳妆台
                case 79: if (check(4, 2)) id = GetBeds(frameY / 36); break;  // 床
                case 497: if (check(1, 2)) id = WorldGen.GetItemDrop_Toilet(frameY / 40); break;  // 马桶
                case 27: if (check(4, 2)) id=63; break; // 向日葵

                // 人体模特 128 女性人体模型 269
                // 喷泉 207
                // 克苏鲁之眼纪念章 240
            }
            // WorldGen.Check1x2Top();
            // Check3x3
            // TEHatRack.cs
            // WorldGen.KillTile_GetItemDrops   打草
            return id;
        }
        private static List<Rectangle> skip = new List<Rectangle>();
        public static void ResetSkip() { skip.Clear(); }
        private static bool ContainsSkip(int x, int y)
        {
            foreach (Rectangle r in skip)
            {
                if (r.Contains(x, y)) return true;
            }
            return false;
        }


        public static bool CanGetKillItemDrop(int type)
        {
            //可以走 WorldGen.KillTile_GetItemDrops 方法的图格id
            // 需是人工方块
            List<int> ids = new List<int>()
            {
                84, // 草药
                426, 430, 431, 432,433,434, // 团队块
                383, // 生命红木
                384, // 红木树叶块
                54, // 玻璃
                561,562,563,
                30,208,157,158,159,
                38,39,41,321,322,

                427, // 红团队平台
                435, // 绿团队平台
                436, // 蓝团队平台
                437, // 黄团队平台
                438, // 粉团队平台
                439, // 白团队平台

                446, 447, 448,  // 呆萌气球
                449, 450, 451,  // 饰带

                324,315, // 贝壳
                419,420, 428,423,
                476,494,

                520, 385,
                178,//宝石
                149,
                13, // 空瓶
                19, // 平台
                33, // 蜡烛
                137, // 飞镖机关
                442, 135, // 压力板
                144, // 计时器
                50, // 书
                171, // 圣诞树
                380, // 种植盆


                239, // 锭
                4, // 火把
                255, 256, 257, 258, 259, 260, 261,
                262,263,264,265,266,267,268,
                190,357,
                325,367,368,369,
                379, // 泡泡块
                507,508,326,327,328,329,

                353, 365, 366, // 绳子
                314, // 矿车轨道

                574, 575, 576, 577, 578, 579,
                424,
                445,
                429,
                272,273,274,
                618,
                460,
                541,
                472,473,474,
                478,479,
                496,495,
                346,347,348,
                350,
                351,

                336, 340, 341, 342, 343, 344,

                500,501,502,503,
                311,312,313,

                251,252,253,
                370,
                396,

                397,
                404,
                407,
                593,
                170,
                284,
                214,213,

                202,
                226,
                224,
                36,
                229,
                230,
                221,222,223,

                248,249,250,
                191,
                203,204,
                124,
                161,
                206,
                232,

                198,189,195,194,193,196,197,

                140,
                371,
                174,



                43,44,45,46,47,

                151,152,

                56,
                75,
                78,
                81,
                188,
                150,

                118,119,120,121,122,
                136,


                141,145,146,147,148,
                153,154,155,156,160,


                175,176,177,
                163,164,
                200,
                210,
                130,131,

                458,459,345,

                330,331,332,333, // 钱
                408, 409,
                415,416,417,418,
                421,422,
                498,
                546,557,


            };

            return ids.Contains(type);
        }

        public static int GetWallItem(int wall)
        {
            // 排除自然生成的墙壁
            // WorldGen.KillWall_GetItemDrops();
            switch (wall)
            {
                case 237: return 4233;
                case 238: return 4234;
                case 239: return 4235;
                case 240: return 4236;
                case 246: return 4486;
                case 247: return 4487;
                case 248: return 4488;
                case 249: return 4489;
                case 250: return 4490;
                case 251: return 4491;
                case 252: return 4492;
                case 253: return 4493;
                case 254: return 4494;
                case 255: return 4495;
                case 314: return 4647;
                case 256: return 4496;
                case 257: return 4497;
                case 258: return 4498;
                case 259: return 4499;
                case 260: return 4500;
                case 261: return 4501;
                case 262: return 4502;
                case 263: return 4503;
                case 264: return 4504;
                case 265: return 4505;
                case 266: return 4506;
                case 267: return 4507;
                case 268: return 4508;
                case 269: return 4509;
                case 270: return 4510;
                case 271: return 4511;
                case 274: return 4512;
                case 275: return 3273;
                case 276: return 4513;
                case 277: return 4514;
                case 278: return 4515;
                case 279: return 4516;
                case 280: return 4517;
                case 281: return 4518;
                case 282: return 4519;
                case 283: return 4520;
                case 284: return 4521;
                case 285: return 4522;
                case 286: return 4523;
                case 287: return 4524;
                case 288: return 4525;
                case 289: return 4526;
                case 290: return 4527;
                case 291: return 4528;
                case 292: return 4529;
                case 293: return 4530;
                case 294: return 4531;
                case 295: return 4532;
                case 296: return 4533;
                case 297: return 4534;
                case 298: return 4535;
                case 299: return 4536;
                case 300: return 4537;
                case 301: return 4538;
                case 302: return 4539;
                case 303: return 4540;
                case 304: return 3340;
                case 305: return 3341;
                case 306: return 3342;
                case 307: return 3343;
                case 308: return 3344;
                case 309: return 3345;
                case 310: return 3346;
                case 311: return 3348;
                default:
                    {
                        int id = 0;
                        switch (wall)
                        {
                            case 168: id = 2696; break;
                            case 169: id = 2698; break;
                            case 226: id = 3752; break;
                            case 227: id = 3753; break;
                            case 228: id = 3760; break;
                            case 229: id = 3761; break;
                            case 230: id = 3762; break;
                            case 142: id = 2263; break;
                            case 143: id = 2264; break;
                            case 144: id = 2271; break;
                            case 149: id = 2505; break;
                            case 150: id = 2507; break;
                            case 151: id = 2506; break;
                            case 152: id = 2508; break;
                            case 245: id = 4424; break;
                            case 315: id = 4667; break;
                            case 1: id = 26; break;
                            case 4: id = 93; break;
                            case 5: id = 130; break;
                            case 6: id = 132; break;
                            // case 7: id = 135; break;  //蓝砖墙（天然）
                            // case 8: id = 138; break;
                            // case 9: id = 140; break;
                            case 10: id = 142; break;
                            case 11: id = 144; break;
                            case 12: id = 146; break;
                            // case 14: id = 330; break; //（天然）
                            case 224: id = 3472; break;
                            case 177: id = 3067; break;
                            case 167: id = 2691; break;
                            // case 60: id = 3584; break;
                            case 231: id = 3952; break;
                            case 232: id = 3954; break;
                            case 225: id = 3751; break;
                            case 233: id = 3956; break;
                            case 234: id = 4052; break;
                            case 235: id = 4053; break;
                            case 236: id = 4140; break;
                            case 312: id = 4565; break;
                            case 313: id = 4548; break;
                            case 179: id = 3083; break;
                            case 183: id = 3082; break;
                            case 181: id = 3089; break;
                            case 184: id = 3088; break;
                            case 186: id = 3238; break;
                        }

                        if (wall >= 153 && wall <= 166)
                        {
                            switch (wall)
                            {
                                case 154: id = 2679; break;
                                case 158: id = 2680; break;
                                case 166: id = 2689; break;
                                case 163: id = 2690; break;
                                case 165: id = 2687; break;
                                case 162: id = 2688; break;
                                case 156: id = 2683; break;
                                case 160: id = 2684; break;
                                case 164: id = 2685; break;
                                case 161: id = 2686; break;
                                case 155: id = 2681; break;
                                case 159: id = 2682; break;
                                case 153: id = 2677; break;
                                case 157: id = 2678; break;
                            }
                        }
                        if (wall == 136) id = 2169;
                        if (wall == 137) id = 2170;
                        if (wall == 172) id = 2788;
                        if (wall == 242) id = 4279;
                        if (wall == 243) id = 4280;
                        if (wall == 145) id = 2333;
                        if (wall == 16) id = 30;
                        if (wall == 17) id = 135;
                        if (wall == 18) id = 138;
                        if (wall == 19) id = 140;
                        if (wall == 20) id = 330;
                        if (wall == 21) id = 392;
                        if (wall == 108) id = 1126;//wall == 86 || 
                        if (wall == 173) id = 2789;
                        if (wall == 174) id = 2790;
                        if (wall == 175) id = 2791;
                        if (wall == 176) id = 2861;
                        if (wall == 182) id = 3101;
                        if (wall == 133) id = 2158;
                        if (wall == 134) id = 2159;
                        if (wall == 135) id = 2160;
                        else if (wall == 113) id = 1726;
                        else if (wall == 114) id = 1728;
                        else if (wall == 115) id = 1730;
                        else if (wall == 146) id = 2432;
                        else if (wall == 147) id = 2433;
                        else if (wall == 148) id = 2434;
                        if (wall >= 116 && wall <= 125) id = 1948 + wall - 116;
                        if (wall >= 126 && wall <= 132) id = 2008 + wall - 126;
                        if (wall == 22) id = 417;
                        if (wall == 23) id = 418;
                        if (wall == 24) id = 419;
                        if (wall == 25) id = 420;
                        if (wall == 26) id = 421;
                        if (wall == 29) id = 587;
                        if (wall == 30) id = 592;
                        if (wall == 31) id = 595;
                        if (wall == 32) id = 605;
                        if (wall == 33) id = 606;
                        if (wall == 34) id = 608;
                        if (wall == 35) id = 610;
                        if (wall == 36) id = 615;
                        if (wall == 37) id = 616;
                        if (wall == 38) id = 617;
                        if (wall == 39) id = 618;
                        if (wall == 41) id = 622;
                        if (wall == 42) id = 623;
                        if (wall == 43) id = 624;
                        if (wall == 44) id = 663;
                        if (wall == 45) id = 720;
                        if (wall == 46) id = 721;
                        if (wall == 47) id = 722;
                        if (wall == 66) id = 745;
                        if (wall == 67) id = 746;
                        if (wall == 68) id = 747;
                        if (wall == 84) id = 884;
                        if (wall == 72) id = 750;
                        if (wall == 73) id = 752;
                        if (wall == 74) id = 764;
                        if (wall == 85) id = 927;
                        if (wall == 75) id = 768;
                        if (wall == 76) id = 769;
                        if (wall == 77) id = 770;
                        if (wall == 82) id = 825;
                        if (wall == 27) id = 479;
                        if (wall == 106) id = 1447;
                        if (wall == 107) id = 1448;
                        if (wall == 109) id = 1590;
                        if (wall == 110) id = 1592;
                        if (wall == 111) id = 1594;
                        if (wall == 78) id = 1723;
                        if (wall == 112) id = 1102; //wall == 87 || 
                        if (wall == 100) id = 1378;//wall == 94 || 
                        if (wall == 101) id = 1379;
                        if (wall == 102) id = 1380;
                        if (wall == 103) id = 1381;
                        if (wall == 104) id = 1382;
                        if (wall == 105) id = 1383;
                        if (wall == 241) id = 4260;
                        if (wall >= 88 && wall <= 93) id = 1267 + wall - 88;
                        if (wall >= 138 && wall <= 141) id = 2210 + wall - 138;
                        return id;
                    }
            }
        }
        
        // 床
        private static int GetBeds(int style)
        {
            int switchStyle(int _style)
            {
                switch (_style)
                {
                    case 19: return 2139;
                    case 20: return 2140;
                    case 21: return 2231;
                    case 22: return 2520;
                    case 23: return 2538;
                    case 24: return 2553;
                    case 25: return 2568;
                    case 26: return 2669;
                    case 27: return 2811;
                    case 28: return 3162;
                    case 29: return 3164;
                    case 30: return 3163;
                    case 31: return 3897;
                    case 32: return 3932;
                    case 33: return 3959;
                    case 34: return 4146;
                    case 35: return 4167;
                    case 36: return 4188;
                    case 37: return 4209;
                    case 38: return 4299;
                    case 39: return 4567;
                    case 0: default: return _style + 643;
                }
            }

            switch (style)
            {
                case 0: return 224;
                case 4: return 920;
                case 9:
                case 10:
                case 11:
                case 12: return 1710 + style;
                default:
                    return (style >= 5 && style <= 8) ? (1465 + style) : ((style >= 13 && style <= 18) ? (2066 + style - 13) : switchStyle(style));
            }
        }
        // 梳妆台
        private static int GetDressers(int style)
        {
            int switchStyle(int _style)
            {
                switch (_style)
                {
                    case 16: return 2529;
                    case 17: return 2545;
                    case 18: return 2562;
                    case 19: return 2577;
                    case 20: return 2637;
                    case 21: return 2638;
                    case 22: return 2639;
                    case 23: return 2640;
                    case 24: return 2816;
                    case 25: return 3132;
                    case 26: return 3134;
                    case 27: return 3133;
                    case 28: return 3911;
                    case 29: return 3912;
                    case 30: return 3913;
                    case 31: return 3914;
                    case 32: return 3934;
                    case 33: return 3968;
                    case 34: return 4148;
                    case 35: return 4169;
                    case 36: return 4190;
                    case 37: return 4211;
                    case 38: return 4301;
                    case 39: return 4569;
                    case 0: default: return 334;
                }
            }

            if (style >= 1 && style <= 3) return 646 + style;

            switch (style)
            {
                case 4: return 918;
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15: return 2386 + style - 5;
                default: return switchStyle(style);
            }
        }

        // 书架
        private static int GetBookcases(int style)
        {
            switch (style)
            {
                case 1: return 1414;
                case 2: return 1415;
                case 3: return 1416;
                case 4: return 1463;
                case 5: return 1512;
                case 6: return 2020;
                case 7: return 2021;
                case 8: return 2022;
                case 9: return 2023;
                case 10: return 2024;
                case 11: return 2025;
                case 12: return 2026;
                case 13: return 2027;
                case 14: return 2028;
                case 15: return 2029;
                case 16: return 2030;
                case 17: return 2031;
                case 18: case 19: case 20: case 21: return 2135 + style - 18;
                default:
                    switch (style)
                    {
                        case 22: return 2233;
                        case 23: return 2536;
                        case 24: return 2540;
                        case 25: return 2554;
                        case 26: return 2569;
                        case 27: return 2670;
                        case 28: return 2817;
                        case 29: return 3165;
                        case 30: return 3167;
                        case 31: return 3166;
                        case 32: return 3917;
                        case 33: return 3933;
                        case 34: return 3960;
                        case 35: return 4147;
                        case 36: return 4168;
                        case 37: return 4189;
                        case 38: return 4210;
                        case 39: return 4300;
                        case 40: return 4568;
                        case 0: default: return 354;
                    }
            }
        }

        // 水槽
        private static int GetSinks(int style)
        {
            switch (style)
            {
                case 29: return 3147;
                case 30: return 3149;
                case 31: return 3148;
                case 32: return 3896;
                case 33: return 3946;
                case 34: return 3972;
                case 35: return 4160;
                case 36: return 4181;
                case 37: return 4202;
                case 38: return 4223;
                case 39: return 4312;
                case 40: return 4581;
                default: return 2827 + style;
            }
        }

        // 篝火
        private static int GetCampfires(int style)
        {
            switch (style)
            {
                case 0: default: return 966;  // 篝火
                case 1: return 3046;  // 恶诅咒篝火
                case 2: return 3047;  // 恶魔篝火
                case 3: return 3048; // 冰冻篝火
                case 4: return 3049; // 灵液篝火
                case 5: return 4693; // 神圣篝火
                case 6: return 3723; // 超亮篝火
                case 7: return 3724; // 骨头篝火
                case 8: return 4689; // 沙漠篝火
                case 9: return 4690; // 珊瑚篝火
                case 10: return 4691; // 腐化篝火
                case 11: return 4692; // 猩红篝火
                case 12: return 4693; // 神圣篝火
                case 13: return 4694; // 丛林篝火
            }
        }

        // 雕像
        private static int GetStatues(int frameX, int frameY)
        {
            int transSwitch(int num)
            {
                switch (num)
                {
                    case 76: return 4397;
                    case 77: return 4360;
                    case 78: return 4342;
                    case 79: return 4466;
                    case 0: default: return 438 + num - 2;
                }
            }

            int id = frameX / 36;
            int style = frameY / 54;
            style %= 3;
            id += style * 55;
            switch (id)
            {
                case 0: return 360;
                case 1: return 52;
                case 43: return 1152;
                case 44: return 1153;
                case 45: return 1154;
                case 46: return 1408;
                case 47: return 1409;
                case 48: return 1410;
                case 49: return 1462;
                case 50: return 2672;
                case 51:
                case 52:
                case 53:
                case 54:
                case 55:
                case 56:
                case 57:
                case 58:
                case 59:
                case 60:
                case 61:
                case 62: return 3651 + id - 51;
                default: return (id >= 63 && id <= 75) ? (3708 + id - 63) : transSwitch(id);
            }
        }

        // 灯笼
        private static int GetLanterns(int style)
        {
            if (style == 0) return 136;
            else if (style == 7) return 1431;
            else if (style == 8) return 1808;
            else if (style == 9) return 1859;
            else if (style < 10) return 1389 + style;
            else
            {
                switch (style)
                {
                    case 10: return 2032;
                    case 11: return 2033;
                    case 12: return 2034;
                    case 13: return 2035;
                    case 14: return 2036;
                    case 15: return 2037;
                    case 16: return 2038;
                    case 17: return 2039;
                    case 18: return 2040;
                    case 19: return 2041;
                    case 20: return 2042;
                    case 21: return 2043;
                    case 22:
                    case 23:
                    case 24:
                    case 25: return 2145 + style - 22;
                    default:
                        switch (style)
                        {
                            case 26: return 2226;
                            case 27: return 2530;
                            case 28: return 2546;
                            case 29: return 2564;
                            case 30: return 2579;
                            case 31: return 2641;
                            case 32: return 2642;
                            case 33: return 2820;
                            case 34: return 3138;
                            case 35: return 3140;
                            case 36: return 3139;
                            case 37: return 3891;
                            case 38: return 3943;
                            case 39: return 3970;
                            case 40: return 4157;
                            case 41: return 4178;
                            case 42: return 4199;
                            case 43: return 4220;
                            case 44: return 4309;
                            case 45: return 4578;
                        }
                        return 0;
                }
            }
        }

        // 门
        private static int GetDoors(int style)
        {
            //WorldGen.DropDoorItem
            switch (style)
            {
                case 0: return 25;
                case 9: return 837;
                case 10: return 912;
                case 12: return 1137;
                case 13: return 1138;
                case 14: return 1139;
                case 15: return 1140;
                case 16: return 1411;
                case 17: return 1412;
                case 18: return 1413;
                case 19: return 1458;
                case 20:
                case 21:
                case 22:
                case 23: return 1709 + style - 20;
                default:
                    switch (style)
                    {
                        case 24: return 1793;
                        case 25: return 1815;
                        case 26: return 1924;
                        case 27: return 2044;
                        case 28: return 2265;
                        case 29: return 2528;
                        case 30: return 2561;
                        case 31: return 2576;
                        case 32: return 2815;
                        case 33: return 3129;
                        case 34: return 3131;
                        case 35: return 3130;
                        case 36: return 3888;
                        case 37: return 3941;
                        case 38: return 3967;
                        case 39: return 4155;
                        case 40: return 4176;
                        case 41: return 4197;
                        case 42: return 4218;
                        case 43: return 4307;
                        case 44: return 4415;
                        case 45: return 4576;
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8: return 812 + style;
                        default: return (style != 11) ? 649 + style : 0;
                    }
            }
        }

        // 桌子
        private static int GetTables(int style)
        {
            int switchStyle(int _style)
            {
                switch (_style)
                {
                    case 0: default: return 32;
                    case 8: return 917;
                    case 9: return 1144;
                    case 10: return 1397;
                    case 11: return 1400;
                    case 12: return 1403;
                    case 13: return 1460;
                    case 14: return 1510;
                    case 23: return 1926;
                    case 21: return 1794;
                    case 22: return 1816;
                    case 24: return 2248;
                    case 25: return 2259;
                    case 26: return 2532;
                    case 27: return 2550;
                    case 28: return 677;
                    case 29: return 2583;
                    case 30: return 2743;
                    case 31: return 2824;
                    case 32: return 3153;
                    case 33: return 3155;
                    case 34: return 3154;
                }
            }
            return (style >= 1 && style <= 3) ? (637 + style) : ((style >= 15 && style <= 20) ? (1698 + style) : ((style >= 4 && style <= 7) ? (823 + style) : switchStyle(style)));
        }
        
        // 水晶桌等
        private static int GetTables2(int style)
        {
            switch (style)
            {
                default: return 3920;
                case 1: return 3948;
                case 2: return 3974;
                case 3: return 4162;
                case 4: return 4183;
                case 5: return 4204;
                case 6: return 4225;
                case 7: return 4314;
                case 8: return 4583;
            }

        }

        // 旗帜
        private static int GetBanners(int frameX, int frameY)
        {
            int num = frameY / 18;
            int num2 = 0;
            while (num >= 3)
            {
                num -= 3;
                num2++;
            }

            int num3 = frameX / 18;
            num3 += num2 * 111;
            if (num3 >= 297) return 4668 + num3;
            else if (num3 >= 295) return 4392 + num3;
            else if (num3 >= 294) return 4602;
            else if (num3 >= 288) return 4253 + num3;
            else if (num3 >= 278) return 3559 + num3;
            else if (num3 >= 273) return 3516 + num3;
            else if (num3 >= 272) return 3780;
            else if (num3 >= 270) return 3323 + num3;
            else if (num3 >= 207) return 3183 + num3;
            else if (num3 >= 109) return 2788 + num3;
            else if (num3 >= 22) return 1593 + num3;
            else if (num3 >= 10 && num3 <= 15) return 1441 + num3;
            else if (num3 >= 16 && num3 <= 21) return 1448 + num3;
            else if (num3 >= 7 && num3 <= 9) return 838 + num3;
            else if (num3 >= 4 && num3 <= 6) return 785 + num3;
            else return 337 + num3;
        }

        // 墓碑
        private static int GetTombstones(int style)
        {
            if (style >= 6 && style <= 10) return 3229 + style - 6;
            else if (style >= 1 && style <= 5) return 1173 + style - 1;
            return 321;
        }

        // 火把
        //private static int GetTorches(int style)
        //{
        //    switch (style)
        //    {
        //        case 0: return 8;
        //        case 8: return 523;
        //        case 9: return 974;
        //        case 10: return 1245;
        //        case 11: return 1333;
        //        case 12: return 2274;
        //        case 13: return 3004;
        //        case 14: return 3045;
        //        case 15: return 3114;
        //        case 16: return 4383;
        //        case 17: return 4384;
        //        case 18: return 4385;
        //        case 19: return 4386;
        //        case 20: return 4387;
        //        case 21: return 4388;
        //        default: return 426 + style;
        //    }
        //}

        // 平台
        //private static int GetPlatfroms(int num)
        //{
        //    switch (num)
        //    {
        //        case 0: return 94;
        //        case 1: return 631;
        //        case 2: return 632;
        //        case 3: return 633;
        //        case 4: return 634;
        //        case 5: return 913;
        //        case 6: return 1384;
        //        case 7: return 1385;
        //        case 8: return 1386;
        //        case 9: return 1387;
        //        case 10: return 1388;
        //        case 11: return 1389;
        //        case 12: return 1418;
        //        case 13: return 1457;
        //        case 14: return 1702;
        //        case 15: return 1796;
        //        case 16: return 1818;
        //        case 17: return 2518;
        //        case 18: return 2549;
        //        case 19: return 2566;
        //        case 20: return 2581;
        //        case 21: return 2627;
        //        case 22: return 2628;
        //        case 23: return 2629;
        //        case 24: return 2630;
        //        case 25: return 2744;
        //        case 26: return 2822;
        //        case 27: return 3144;
        //        case 28: return 3146;
        //        case 29: return 3145;
        //        case 30:
        //        case 31:
        //        case 32:
        //        case 33:
        //        case 34:
        //        case 35: return 3903 + num - 30;
        //        default:
        //            switch (num)
        //            {
        //                case 36: return 3945;
        //                case 37: return 3957;
        //                case 38: return 4159;
        //                case 39: return 4180;
        //                case 40: return 4201;
        //                case 41: return 4222;
        //                case 42: return 4311;
        //                case 43: return 4416;
        //                case 44: return 4580;
        //            }
        //            return 0;
        //    }
        //}

        // 玻璃杯
        //private static int GetBottles(int style)
        //{
        //    switch (style)
        //    {
        //        case 0: default: return 31;
        //        case 1: return 28;
        //        case 2: return 110;
        //        case 3: return 350;
        //        case 4: return 351;
        //        case 5: return 2234;
        //        case 6: return 2244;
        //        case 7: return 2257;
        //        case 8: return 2258;
        //    }
        //}
    }
}
