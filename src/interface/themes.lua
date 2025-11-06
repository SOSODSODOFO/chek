local Themes = {}
Themes.__index = Themes

function Themes.new(event_bus)
    return setmetatable({
        event_bus = event_bus,
        presets = {
            dark = {
                palette = {
                    background = {0.05, 0.05, 0.08},
                    primary = {0.3, 0.6, 1},
                    accent = {1, 0.4, 0.2}
                },
                typography = {
                    font = "Inter",
                    size = 14
                },
                transparency = 0.85
            },
            light = {
                palette = {
                    background = {0.95, 0.95, 1},
                    primary = {0.1, 0.4, 0.8},
                    accent = {0.8, 0.3, 0.3}
                },
                typography = {
                    font = "Roboto",
                    size = 13
                },
                transparency = 0.95
            }
        },
        active = "dark"
    }, Themes)
end

function Themes:add_preset(name, preset)
    self.presets[name] = preset
end

function Themes:apply(name)
    if self.presets[name] then
        self.active = name
        self.event_bus:emit("theme_changed", { name = name, preset = self.presets[name] })
    end
end

function Themes:update_palette(values)
    local preset = self.presets[self.active]
    for key, value in pairs(values) do
        preset.palette[key] = value
    end
    self.event_bus:emit("theme_palette_updated", preset.palette)
end

function Themes:update_typography(options)
    local preset = self.presets[self.active]
    for key, value in pairs(options) do
        preset.typography[key] = value
    end
    self.event_bus:emit("theme_typography_updated", preset.typography)
end

return Themes
