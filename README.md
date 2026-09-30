# AC8 MouseYaw Bridge

A mouse-yaw bridge for **Ace Combat 8: Wings of Theve**.

The tool reads the existing XInput gamepad state from the keyboard, forwards it unchanged to a virtual Xbox controller, and adds mouse velocity to the virtual controller:

- Mouse Up / Down: Left Stick Y Up / Down
- Mouse Left: LT / Left Trigger
- Mouse Right: RT / Right Trigger

This is a remapping utility. It does not inject into the game, hook the process, or block/consume the original mouse input.

---

## 1. Required In-Game Setup

### 1.1 Move the triggers from throttle to yaw

Open:

```text
Controller Settings
```

Set the trigger actions as follows:

- Yaw Left: LT / Left Trigger
- Yaw Right: RT / Right Trigger

If the yaw direction is reversed, swap the LT and RT bindings in the game. The bridge always outputs LT for mouse-left and RT for mouse-right; the final direction is determined by the game's controller bindings.

### 1.2 Disable mouse aircraft control

Open:

```text
Mouse Settings
```

Disable:

```text
Mouse Aircraft Control
```

The original mouse input continues to work for menus and camera behavior. MouseYaw simply adds a second, virtual gamepad signal on top of it.

---

## 2. Startup Order: Ace Combat 8 Only Uses XInput Slot 0

The virtual Xbox controller created by this tool must occupy **XInput slot 0**. If another hardware gamepad, virtual joystick, or keyboard gamepad interface already owns slot 0, the game may use that device instead and ignore the virtual controller.

Recommended order:

1. Close the game and any old MouseYaw process.
2. Disconnect every device that can occupy XInput, including hardware gamepads, virtual joysticks, and the keyboard's gamepad interface.
3. Run:
   ```text
   Start-MouseYaw.cmd
   ```
   or launch:
   ```text
   MouseYawBridge.exe
   ```
4. Confirm that the console log shows:
   ```text
   Virtual Xbox controller connected. XInput slot=0.
   ```
5. Reconnect the keyboard, hardware controller, or other input device.
6. Confirm that the console log shows:
   ```text
   Source XInput slot=1
   ```
7. Launch Ace Combat 8.

`Start-MouseYaw.cmd` contains a pause prompt, so it requires a key press before the program starts. If you must fully disconnect the keyboard to free XInput slot 0, launch `MouseYawBridge.exe` directly, or remove the `pause` line from the batch file first.

If the virtual controller is not assigned to slot 0, close everything and repeat the startup sequence before launching the game.

---

## 3. The Large Left-Stick Deadzone: Start with `y_boost`

Mouse up/down is mapped to the left stick Y axis, but Ace Combat 8 uses a large built-in deadzone for the left stick.

With a plain speed-to-stick mapping, common symptoms are:

- The mouse moves, but the aircraft does not respond.
- The mouse must be moved very far before any input appears.
- Small stick outputs are filtered out by the game.

For this reason, the Y axis usually needs a non-zero value for:

```ini
boost_y
```

`boost_y` is not a sensitivity curve. After the configured deadzone is exceeded, it adds a fixed amount of output in the current movement direction. This is used to push the virtual stick past the game's built-in deadzone.

Current reference values:

```ini
deadzone_y=0
boost_y=0.24
```

Suggested tuning:

- No noticeable vertical response: increase `boost_y` in small steps.
- The aircraft suddenly jumps after the input threshold: reduce `boost_y`.
- Still no response after increasing boost: verify the in-game left-stick binding and make sure the virtual controller is in XInput slot 0.
- Direction is reversed: change the in-game binding or set `invert_y=true`.

The X axis maps to LT/RT. Trigger deadzones are usually much smaller, so the current reference uses:

```ini
deadzone_x=0.02
boost_x=0
```

If in-game yaw requires too much trigger travel, adjust `boost_x`. Both `boost_x` and `boost_y` may be negative; a negative boost pulls the output back toward the center.

---

## 4. Tuning from the Console Window

The program keeps a console window open. Type commands and press Enter. Changes take effect immediately.

```text
list
help
```

Common commands:

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

Every `set` command is applied immediately and written back to `config.ini`.

Chinese command aliases are also supported, for example:

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

## 5. Configuration Reference

### `enabled_on_start`

Enables or disables the mouse contribution at startup:

```ini
enabled_on_start=true
```

When disabled, the keyboard's original XInput state is still forwarded. Only the mouse contribution is disabled.

### `toggle_key`

Toggles only the mouse contribution:

```ini
toggle_key=F10
```

The key is not consumed and remains available to other programs. Keyboard XInput forwarding is not disabled by this toggle.

F11 may conflict with fullscreen/window switching, and F12 may be used by other software. F10 is recommended.

### `poll_hz`

Input polling rate:

```ini
poll_hz=1000
```

A 1000 Hz mouse can use `1000` for the fastest response. If CPU usage becomes a problem, try `500`.

### `source_xinput_slot`

Selects which XInput slot is read as the keyboard/gamepad source:

```ini
source_xinput_slot=-1
```

- `-1`: auto-detect the source slot.
- `0..3`: force a specific slot.

In the intended setup:

```text
slot 0 = MouseYaw virtual controller
slot 1 = keyboard/gamepad source
```

If the wrong source is selected, set it manually:

```text
set source_slot 1
```

Do not select the slot occupied by the virtual controller.

### `dpi`, `x_dpi`, and `y_dpi`

Hardware mouse DPI values:

```ini
dpi=3000
x_dpi=3000
y_dpi=7500
```

`dpi` is the legacy shared default. `x_dpi` and `y_dpi` are the values used for the actual per-axis speed calculation.

Set these to the real DPI values configured in the mouse software. Incorrect DPI values will produce incorrect physical-speed calculations.

### `sensitivity`, `x_sensitivity`, and `y_sensitivity`

Effective sensitivity is calculated as:

```text
effective X sensitivity = sensitivity * x_sensitivity
effective Y sensitivity = sensitivity * y_sensitivity
```

Current reference values:

```ini
sensitivity=1
x_sensitivity=0.4
y_sensitivity=2.5
```

- X controls LT/RT yaw.
- Y controls the left stick Y axis.
- Tune global `sensitivity` first, then compensate each axis with its independent multiplier.

### `full_speed_inches_per_second`

The physical mouse speed that maps to full stick or trigger deflection:

```ini
full_speed_inches_per_second=2
```

Lower values are more sensitive; higher values are less sensitive. This is a normalization speed, not the in-game sensitivity setting.

### `deadzone_x` and `deadzone_y`

Input below the threshold is treated as zero:

```ini
deadzone_x=0.02
deadzone_y=0
```

Range: `0..0.95`.

- `deadzone_x`: LT/RT.
- `deadzone_y`: left stick Y.
- Increase the value slightly for hand tremor or drift.
- Keep it at `0` or very small if you want the input to start as soon as meaningful mouse movement occurs.

### `boost_x` and `boost_y`

After the axis deadzone is exceeded, boost adds a fixed amount of output in the current direction:

```ini
boost_x=0
boost_y=0.24
```

Range: `-1..1`.

Positive values push farther in the current direction. Negative values pull back toward center. The left stick's large in-game deadzone is the main reason the current reference profile relies on `boost_y`.

### `hold_ms`

How long the latest speed sample is held:

```ini
hold_ms=1
```

This is not a queue. It never replays a sequence of older mouse movements. It only keeps the latest velocity vector; a reverse movement immediately replaces it, and the value returns to zero after the hold time expires.

At 1000 Hz, `1` ms is enough. If the game occasionally misses an extremely short pulse, try `2` to `4`. Do not restore a long history queue, because that turns mouse input into delayed playback.

### `invert_y`

Flip the vertical mouse direction:

```ini
invert_y=true
```

Or use:

```text
set invert_y true
```

The X axis is not inverted in software. Use the in-game LT/RT bindings to change yaw direction.

### `show_notifications`

Enables or disables status notifications. This only affects notifications, not input.

---

## 6. Current Reference Configuration

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

These values are a starting point, not a universal preset. The recommended place to tune them is:

```text
Free Flight
```

Adjust `y_boost` first, then the X/Y sensitivity and deadzone values.

---

## 7. Behavior and Limitations

The aircraft has its own inertia, and the game's control surfaces have return delay.

As a result:

- Mouse yaw is more continuous than a simple button input, but it still requires anticipation.
- Mouse yaw plus gun aiming still requires leading the target and pulling the mouse back before the aircraft reaches the desired angle.
- Large targets and fixed targets can be attacked with the gun without relying on missiles.
- Small, fast, or rapidly maneuvering air targets remain limited by airframe inertia and control-surface response.

This is not an aimbot, auto-aim, recoil-control, or target-tracking tool. It performs input conversion only. It does not inspect the game state or identify targets.

---

## 8. Roadmap

A considered but intentionally unimplemented improvement:

> Keep a time window and calculate acceleration only. The final stick output would be a weighted combination of mouse velocity and acceleration to compensate for airframe inertia.

This is not part of the current version. See the disclaimer below: the project is intended to remain a simple remapping utility without complex automatic correction logic.

---

## 9. Disclaimer

Strictly speaking, this is only a remapping tool:

- It converts mouse motion, which is already an analog input, into virtual gamepad stick and trigger values.
- It behaves similarly to a mouse-shaped controller.
- It does not reverse-engineer the game.
- It does not inject into, hook, or modify the game process.
- The gamepad and basic flight-control path already support analog yaw, which is why this mapping can work.

The current implementation only applies bias, deadzone, and boost-style linear transforms to mouse input. It does not perform complex algorithm optimization, auto-aim, automatic recoil control, or target correction.

It is theoretically unlikely to trigger EAC, but this has not been tested in practice. Use it only in single-player content, and do not take the risk in online or anti-cheat environments.

---

## 10. Customization and Source

The source is included at:

```text
src\MouseYaw.cs
```

For customization, new parameters, algorithm changes, or debugging, use Codex or a similar coding assistant to inspect and modify the source, then rebuild the executable.

Do not patch the compiled binary by guessing.

## Third-Party Dependency

The virtual controller requires the ViGEmBus driver and `Nefarius.ViGEm.Client.dll`.

Install ViGEmBus from the official releases page:
https://github.com/nefarius/ViGEmBus/releases

The DLL is only the client library. The ViGEmBus driver must be installed separately.

The ViGEm.NET client license is included at:

```text
licenses\ViGEm.NET-MIT.txt
```


