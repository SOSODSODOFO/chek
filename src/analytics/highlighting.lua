local HighlightSystem = {}
HighlightSystem.__index = HighlightSystem

function HighlightSystem.new(event_bus, config)
    return setmetatable({
        event_bus = event_bus,
        config = config,
        highlights = {},
        color_schemes = {
            default = {
                ally = { 0, 0.8, 1, 0.7 },
                enemy = { 1, 0.2, 0.2, 0.8 },
                neutral = { 1, 1, 1, 0.4 }
            }
        },
        active_scheme = "default",
        transparency = 0.6
    }, HighlightSystem)
end

function HighlightSystem:set_transparency(value)
    self.transparency = value
end

function HighlightSystem:add_method(name, fn)
    self.highlights[name] = {
        fn = fn,
        enabled = true
    }
end

function HighlightSystem:set_enabled(name, value)
    if self.highlights[name] then
        self.highlights[name].enabled = value
    end
end

function HighlightSystem:set_color_scheme(name, scheme)
    self.color_schemes[name] = scheme
end

function HighlightSystem:use_color_scheme(name)
    if self.color_schemes[name] then
        self.active_scheme = name
    end
end

function HighlightSystem:get_color(tag)
    local scheme = self.color_schemes[self.active_scheme] or self.color_schemes.default
    local color = scheme[tag] or scheme.neutral
    if not color then
        return { 1, 1, 1, self.transparency }
    end
    local clone = { color[1], color[2], color[3], color[4] or self.transparency }
    clone[4] = self.transparency
    return clone
end

function HighlightSystem:update(dt)
    local results = {}
    for name, highlight in pairs(self.highlights) do
        if highlight.enabled then
            local ok, items = pcall(highlight.fn, dt)
            if ok then
                results[name] = items
            end
        end
    end

    self.event_bus:emit("highlight_updated", {
        highlights = results,
        scheme = self.active_scheme,
        transparency = self.transparency
    })
end

return HighlightSystem
