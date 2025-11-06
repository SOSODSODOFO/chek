local Backgrounds = {}
Backgrounds.__index = Backgrounds

function Backgrounds.new(event_bus)
    return setmetatable({
        event_bus = event_bus,
        variants = {
            particles = {
                intensity = 0.5,
                interaction = "gravity",
                on_cursor = true
            },
            waves = {
                amplitude = 0.3,
                frequency = 1.2,
                parallax = 0.2
            },
            geometry = {
                rotation_speed = 0.6,
                depth = 0.4,
                parallax = 0.5
            }
        },
        active = "particles"
    }, Backgrounds)
end

function Backgrounds:activate(name)
    if self.variants[name] then
        self.active = name
        self.event_bus:emit("background_changed", { name = name, settings = self.variants[name] })
    end
end

function Backgrounds:update_settings(name, settings)
    if not self.variants[name] then
        return
    end
    for key, value in pairs(settings) do
        self.variants[name][key] = value
    end
    if self.active == name then
        self.event_bus:emit("background_settings_updated", { name = name, settings = self.variants[name] })
    end
end

return Backgrounds
