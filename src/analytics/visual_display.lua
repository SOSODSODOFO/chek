local VisualDisplay = {}
VisualDisplay.__index = VisualDisplay

function VisualDisplay.new(config, event_bus)
    local display = setmetatable({
        config = config,
        event_bus = event_bus,
        indicators = {},
        color_schemes = {
            default = {
                health = { 0, 1, 0 },
                stamina = { 0, 0.6, 1 },
                shield = { 0.8, 0.8, 1 },
                priority = { 1, 0.5, 0 }
            }
        },
        active_scheme = "default"
    }, VisualDisplay)

    display:event_listeners()

    return display
end

function VisualDisplay:event_listeners()
    self.event_bus:on("movement_updated", function(payload)
        self.indicators["movement"] = {
            position = payload.position,
            info = string.format("%s | %.2f", payload.mode, math.sqrt(payload.velocity.x^2 + payload.velocity.y^2 + payload.velocity.z^2))
        }
    end)

    self.event_bus:on("data_tracking_updated", function(payload)
        self.indicators["participants"] = payload
    end)
end

function VisualDisplay:set_color_scheme(name, scheme)
    self.color_schemes[name] = scheme
end

function VisualDisplay:use_color_scheme(name)
    if self.color_schemes[name] then
        self.active_scheme = name
    end
end

function VisualDisplay:get_indicator_color(metric)
    local scheme = self.color_schemes[self.active_scheme] or self.color_schemes.default
    return scheme[metric] or { 1, 1, 1 }
end

function VisualDisplay:render_distance_info(target)
    local viewer = self.indicators.movement and self.indicators.movement.position or { x = 0, y = 0, z = 0 }
    local dx = target.x - viewer.x
    local dy = target.y - viewer.y
    local dz = target.z - viewer.z
    local distance = math.sqrt(dx * dx + dy * dy + dz * dz)
    return string.format("Distance: %.2f", distance)
end

function VisualDisplay:render()
    local output = {}
    for key, indicator in pairs(self.indicators) do
        table.insert(output, string.format("Indicator[%s]=%s", key, type(indicator) == "table" and "table" or tostring(indicator)))
    end
    return table.concat(output, "\n")
end

return VisualDisplay
