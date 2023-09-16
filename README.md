首次击败boss时会创建新世界。

<br>

使用说明：[TShock插件：大地动 | quake（哔哩哔哩专栏）](https://www.bilibili.com/read/cv16022901)

<br>

1.1 更新说明：
- 新增 /quake ic 指令，读取chest.txt导入箱子，自动记录最后一个失败的箱子，其它的请结合 日志和record.json来修改 chest.txt；
- 新增 /quake report 指令，报告搬家情况；
- 新增 /quake backup 指令，备份地图信息；
- 新增 /quake recover 指令，还原地图信息，即创建世界后会做的事情；
- 自动备份的文件名，会带上击败的boss名；
- 查找全图的晶塔，并放到新家的箱子里；
- 查找基地附近的放置物，并放到新家的箱子里，支持搬家的东西： 
  ● 绝大多数玩家放置的方块、墙、平台 和 种植盆（会收获种植盆上面 成熟的草药） ； 
  ● 绝大多数玩家放置的1x1的东西； 
  ● 全部的制作站；
  ● 4种银行类家具；
  ● 10种buff类家具；
  ● 物品框及上面的物品； 
  ● 武器架及上面的武器；
  ● 附魔日晷、丛林蜥蜴祭坛；
  ● 其它变装家具，工作台、篝火、圣物、雕像等等；
- 支持自动创建 地狱直通车，可通过配置文件进行开关，开启后仍需击败才会创建；

<br>

插件参考了：

https://github.com/Illuminousity/WorldRefill

https://github.com/Megghy/DamageStatistic
