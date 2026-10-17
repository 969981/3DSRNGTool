# 3DSRNGTool — 异色类型筛选

分支：`feature/advanced-shiny-filters`

## 筛选档位

| 档位 | 条件 |
| --- | --- |
| 任意 | 不按异色状态过滤 |
| 不闪光 | `!Shiny` |
| 仅星闪 | `Shiny && ShinyTypeKnown && !SquareShiny` |
| 仅方闪 | `Shiny && ShinyTypeKnown && SquareShiny` |
| 星闪 / 方闪 | `Shiny` |

## 判定边界

第六、七世代的原生判闪条件是 `(TID ^ SID ^ PID_hi ^ PID_lo) < 16`，没有可见的星闪、方闪特效之分。按照后续世代通常采用的数值分类，XOR=0 对应方闪，XOR=1..15 对应星闪。

`TSV = (TID ^ SID) >> 4`，`TRV = (TID ^ SID) & 15`。仅 TSV 无法判定低四位，因此无法可靠地区分星/方。界面首次选择星/方细分时要求用户确认所填 TRV。结果字段 `ShinyTypeKnown` 用于排除仅凭「其他 TSV」列表触发的未知分类结果。

注：活动配信强制异色、固定训练家 ID 等特殊生成途径有独立逻辑，不能直接套用普通野生/蛋的分类；需要逐类验证。

## 手动回归矩阵

- 5 档逐一检查第六、七世代野生、固定、蛋搜索。
- 使用 TSV + 正确 TRV，验证 PID XOR：XOR=0 只命中方闪；XOR=1..15 只命中星闪；XOR>=16 只命中不闪光。
- 仅 TSV 时选择细分档位弹出提示；拒绝确认则不启用细分筛选。
- 修改 TRV 或切换存档档案时细分档位恢复为“任意”，需要重新确认。
- 其他 TSV 列表仅凭 TSV 匹配的闪光应命中“两种闪光”，不能误入“仅星闪”。
- 验证旧版筛选预设（只含 ShinyOnly）仍可导入，新版预设能保存、读取 ShinyMode。
- 测试重置筛选、语言切换、页面切换时的状态；验证“取消筛选”仍按原行为工作。

本次开发环境不具备 Windows/.NET SDK，应通过仓库 GitHub Actions 和 Windows 实际运行结果完成最终验收。
