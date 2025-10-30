--!strict

export type ThemeConfig = {
presets: { [string]: { [string]: Color3 } }?,
defaultTheme: string?,
}

local DEFAULT_THEME = {
primary = Color3.fromRGB(44, 120, 255),
accent = Color3.fromRGB(255, 180, 64),
background = Color3.fromRGB(12, 12, 14),
panel = Color3.fromRGB(24, 24, 32),
text = Color3.new(1, 1, 1),
}

local ThemeManager = {}
ThemeManager.__index = ThemeManager

function ThemeManager.new(config: ThemeConfig?)
local self = setmetatable({}, ThemeManager)
self._presets = config and config.presets or {
Default = DEFAULT_THEME,
Neon = {
primary = Color3.fromRGB(129, 228, 255),
accent = Color3.fromRGB(255, 107, 214),
background = Color3.fromRGB(4, 4, 8),
panel = Color3.fromRGB(24, 8, 40),
text = Color3.fromRGB(255, 255, 255),
},
Solar = {
primary = Color3.fromRGB(255, 136, 0),
accent = Color3.fromRGB(255, 215, 112),
background = Color3.fromRGB(18, 14, 4),
panel = Color3.fromRGB(44, 28, 4),
text = Color3.fromRGB(255, 244, 229),
},
}
self._currentName = config and config.defaultTheme or "Default"
self._currentTheme = self._presets[self._currentName] or DEFAULT_THEME
self._changed = Instance.new("BindableEvent")
return self
end

function ThemeManager:Init()
self._changed:Fire(self._currentTheme)
end

function ThemeManager:ApplyTheme(name: string?)
if name and self._presets[name] then
self._currentName = name
self._currentTheme = self._presets[name]
end
self._changed:Fire(self._currentTheme)
end

function ThemeManager:Customize(partial: { [string]: Color3 })
for key, value in partial do
self._currentTheme[key] = value
end
self._changed:Fire(self._currentTheme)
end

function ThemeManager:GetTheme(): { [string]: Color3 }
return table.clone(self._currentTheme)
end

function ThemeManager:Changed()
return self._changed.Event
end

function ThemeManager:Destroy()
self._changed:Destroy()
end

return ThemeManager
