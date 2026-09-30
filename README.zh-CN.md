# AC8 MouseYaw Bridge

《皇牌空战 8》鼠标偏航桥接器。

它读取键盘现有 XInput 手柄状态，原样转发到虚拟 Xbox 手柄，再把鼠标速度叠加到虚拟手柄的：

- 鼠标向上 / 向下 = 左摇杆 Y 向上 / 向下
- 鼠标向左 = LT 左扳机
- 鼠标向右 = RT 右扳机

这是一个 Remapping 工具，不注入游戏、不 Hook、不拦截或屏蔽原始鼠标输入。

---

## 一、游戏内必须先设置

### 1. 把扳机键从油门改成偏航

进入游戏：

```text
Controller Settings / 控制器按键设置
```

把扳机键的用途从油门改为偏航：

- 左偏航：左扳机 LT
- 右偏航：右扳机 RT

如果游戏里左右方向反了，就把 LT/RT 反过来绑定。  
鼠标左右只是输出 LT/RT，游戏内按钮最终绑定到哪个方向，以游戏设置为准。

### 2. 关闭鼠标自机控制

进入游戏：

```text
Mouse Settings / 鼠标设置
```

关闭：

```text
Mouse Aircraft Control / 鼠标自机操纵
```

这个关闭后，原始鼠标只负责菜单和视角，不再控制机体。MouseYaw 额外输出一份虚拟手柄信号，两者互不冲突。

---

## 二、启动顺序很重要：AC8 只认 XInput slot 0

本工具的虚拟 Xbox 手柄必须占据 XInput slot 0。  
如果启动前已经有硬件手柄、硬件虚拟摇杆或键盘的手柄接口占了 slot 0，AC8 就可能只认那个设备，MouseYaw 的虚拟手柄不会被当作主输入。

推荐流程：

1. 关闭游戏和旧的 MouseYaw 进程。
2. 断开所有会占用 XInput 的硬件手柄、虚拟摇杆或键盘手柄接口。
3. 运行：
   ```text
   Start-MouseYaw.cmd
   ```
   或直接运行：
   ```text
   MouseYawBridge.exe
   ```
4. 在黑窗口中确认日志出现：
   ```text
   Virtual Xbox controller connected. XInput slot=0.
   ```
5. 再接入键盘、硬件手柄或其他控制器。
6. 确认日志出现：
   ```text
   Source XInput slot=1
   ```
7. 最后启动《皇牌空战 8》。

注意：`Start-MouseYaw.cmd` 内有暂停语句，需要键盘按一下才继续。  
如果你必须完全断开键盘才能让虚拟手柄拿到 slot 0，就直接双击 `MouseYawBridge.exe`，或者先把 bat 里的 `pause` 删掉。

如果日志里虚拟手柄不是 slot 0，不要进游戏，先退出程序，处理设备顺序后重来。

---

## 三、鼠标映射的死区问题：需要优先调 y_boost

鼠标上下映射到左摇杆 Y，但 AC8 对左摇杆有一个很大的内置死区。

只把鼠标速度映射到摇杆，常常会出现：

- 鼠标已经动了，但机体没有反应；
- 鼠标甩得很大，才会有一下摇杆输入；
- 鼠标速度输出已经变化，但被游戏当小信号过滤掉。

所以鼠标上下轴通常需要提高：

```ini
boost_y
```

`boost_y` 不是灵敏度，也不是曲线。它是在超过该轴死区后，额外加一段同方向的固定杆量，用来强行越过游戏内置死区。

当前参考值：

```ini
deadzone_y=0
boost_y=0.24
```

调法建议：

- 如果鼠标上下几乎没有反应：先小幅增加 `boost_y`；
- 如果刚进入有效输入就突然跳得太大：降低 `boost_y`；
- 如果鼠标越过后仍然没有反应：检查游戏内左摇杆是否真的绑定了俯仰/偏航，以及虚拟手柄是否占 slot 0；
- 如果反向：修改游戏内绑定或设置 `invert_y=true`。

X 轴映射到 LT/RT，扳机死区通常较小，所以当前参考值：

```ini
deadzone_x=0.02
boost_x=0
```

如果游戏内左右偏航几乎都要等很大行程才触发，可以再调 `boost_x`。  
`boost_x` 和 `boost_y` 都可以为负值，负值会把对应方向的输出往中间拉。

---

## 四、调参入口：黑窗口

程序运行时保留一个黑窗口。直接输入命令，回车生效。

```text
list
help
```

常用设置：

```text
set sensitivity <value>
set x <value>
set y <value>

set dpi <value>
set x_dpi <value>
set y_dpi <value>
set speed <value>

set deadzone <value>
set x_deadzone <value>
set y_deadzone <value>

set boost <value>
set x_boost <value>
set y_boost <value>

set hold <ms>
set poll_hz <value>
set invert_y true|false
set enabled true|false
set toggle_key F10|F11|F12
set source_slot -1|0|1|2|3

save
exit
```

每次 `set` 都会立即生效并写回 `config.ini`。  
`save` 主要用于手动强制保存。

中文别名也支持，例如：

```text
set 灵敏度 1
set X轴灵敏度 0.4
set Y轴灵敏度 2.5
set X轴DPI 3000
set Y轴DPI 7500
set X轴死区 0.02
set Y轴死区 0
set Y轴boost 0.24
list
退出
```

---

## 五、字段说明

### `enabled_on_start`

启动时是否启用鼠标增量。

```ini
enabled_on_start=true
```

关闭后仍然会转发键盘原始 XInput 状态，只是不叠加鼠标信号。

### `toggle_key`

鼠标增量开关，默认：

```ini
toggle_key=F10
```

只切换“鼠标叠加到虚拟手柄”的部分，不会关闭键盘 XInput 转发。  
F11 会与部分程序的窗口切换冲突，F12 也可能被其他软件占用，推荐 F10。

### `poll_hz`

程序采样频率：

```ini
poll_hz=1000
```

鼠标 poll rate 为 1000 Hz 时，使用 1000 可以保持最快响应。  
如果 CPU 占用过高或出现异常，可以降到 500。

### `source_xinput_slot`

读取哪个 XInput slot 作为键盘手柄原始状态：

```ini
source_xinput_slot=-1
```

- `-1`：自动寻找可用的原始手柄 slot；
- `0..3`：强制读取指定 slot。

正常配置下：

```text
slot 0 = MouseYaw 虚拟手柄
slot 1 = 键盘手柄接口
```

如果程序选错源设备，可以手动设置为 `1`：

```text
set source_slot 1
```

不要把它设成虚拟手柄正在占用的 slot 0。

### `dpi`、`x_dpi`、`y_dpi`

鼠标硬件 DPI。当前参考值：

```ini
dpi=3000
x_dpi=3000
y_dpi=7500
```

`dpi` 是旧的通用默认值，`x_dpi` 和 `y_dpi` 是真正用于换算的独立值。  
必须填写鼠标驱动中的实际 DPI，否则“纯速度”换算会错。

### `sensitivity`、`x_sensitivity`、`y_sensitivity`

最终有效灵敏度为：

```text
X 最终灵敏度 = sensitivity * x_sensitivity
Y 最终灵敏度 = sensitivity * y_sensitivity
```

当前参考值：

```ini
sensitivity=1
x_sensitivity=0.4
y_sensitivity=2.5
```

- X 轴控制 LT/RT 偏航；
- Y 轴控制左摇杆 Y；
- 先调全局 `sensitivity`，再用 X/Y 独立倍率补偿两轴手感。

### `full_speed_inches_per_second`

鼠标物理速度达到多少英寸/秒时，映射为满杆/满扳机：

```ini
full_speed_inches_per_second=2
```

数值越小越灵敏，越大越迟钝。  
它不是游戏灵敏度，而是将鼠标位移换算为速度后的归一化标准。

### `deadzone_x`、`deadzone_y`

小于该速度阈值的输入直接按 0 算：

```ini
deadzone_x=0.02
deadzone_y=0
```

范围 `0..0.95`。  
`deadzone_x` 对应 LT/RT，`deadzone_y` 对应左摇杆 Y。  
如果手抖导致轻微漂移，可以增加对应值；如果想让鼠标一有明确速度就生效，就保持 0 或很小的值。

### `boost_x`、`boost_y`

超过对应死区后，额外增加一段同方向的固定输出：

```ini
boost_x=0
boost_y=0.24
```

范围 `-1..1`。  
正值往当前方向推得更远，负值往中间拉。  
左摇杆 Y 受 AC8 内置大死区影响最大，所以当前参数主要依赖 `boost_y` 来穿透它。

### `hold_ms`

最新速度的保持时间：

```ini
hold_ms=1
```

这不是队列，不会把过去的历史动作排队后慢慢重放。  
它只保留“最新一次速度”，反向移动会立即替换，超过时间没有新位移就归零。

1000 Hz 鼠标下使用 `1` ms 已经足够。  
如果游戏偶尔漏掉极短脉冲，可以尝试 `2` 到 `4`；不要把旧版的长时间队列重新加回来，否则会变成鼠标动作回放。

### `invert_y`

鼠标上下方向反了就改为：

```ini
invert_y=true
```

也可以运行：

```text
set invert_y true
```

X 轴方向不在软件里翻转，直接通过游戏内 LT/RT 绑定方向解决。

### `show_notifications`

是否显示托盘/状态通知。  
只影响提示，不影响输入。

---

## 六、当前参考参数

```ini
enabled_on_start=true
toggle_key=F10
poll_hz=1000
source_xinput_slot=-1

dpi=3000
x_dpi=3000
y_dpi=7500
sensitivity=1
x_sensitivity=0.4
y_sensitivity=2.5
full_speed_inches_per_second=2
deadzone_x=0.02
deadzone_y=0
boost_x=0
boost_y=0.24
hold_ms=1
invert_y=false
show_notifications=true
```

这套值只是一个参考起点。推荐进入游戏：

```text
Free Flight / 自由飞行
```

边飞边调，优先调 `y_boost`，再调 X/Y 灵敏度和死区。

---

## 七、使用效果与限制

游戏机体本身有一定惯性，游戏内舵面也有回正延迟。

因此：

- 鼠标偏航比纯按键输入更连续，但仍然需要预判杆量；
- 用鼠标偏航做机炮微调时，仍然需要提前回拉鼠标；
- 对大型目标和固定目标，完全可以使用机炮，不再依赖导弹；
- 对小型、高速、快速改变方向的空中目标，仍会受到机体惯性和舵面响应限制。

这不是自瞄、自动压枪或自动追瞄工具。  
它只做输入转换，不做目标识别，也不读取游戏内部状态。

---

## 八、Roadmap

计划过的一个改进方向：

> 维持一个时间窗口，但只计算加速度；最终杆量由鼠标速度和加速度加权，用于抵消游戏内机体惯性。

当前版本不做这个改进。  
原因见下面的免责声明：这个项目保持为简单的 Remapping 工具，不引入复杂控制算法和自动修正逻辑。

---

## 九、免责声明

严格来说，这只是一个 Remapping：

- 把鼠标事实上的模拟量转换为虚拟手柄摇杆/扳机量；
- 类似于“一只鼠标形状的手柄”；
- 没有逆向游戏程序；
- 没有侵入、注入或 Hook 游戏进程；
- 手柄和初级飞控原生就具有非按键的偏航支持，所以这种映射才能成立。

当前实现仅对鼠标输入做 bias、deadzone、boost 这一类线性变换，没有做复杂算法优化，也没有自动压枪、自动瞄准或目标修正。

理论上不会触发 EAC，但实际没有试验过。  
建议只在单机内容中使用，不要在联机或反作弊环境里冒险。

---

## 十、客制化与源码

源码位于：

```text
src\MouseYaw.cs
```

如果需要客制化、增加参数、修改算法或排查问题，建议使用 Codex 或类似的编码工具检查并修改源代码，再重新编译。

不要直接凭猜测修改二进制文件。

## 第三方依赖

虚拟手柄依赖 ViGEmBus 驱动和 `Nefarius.ViGEm.Client.dll`。  
驱动需要单独安装；DLL 只是客户端库，不是驱动本身。

ViGEm.NET 客户端许可证见：

```text
licenses\ViGEm.NET-MIT.txt
```
