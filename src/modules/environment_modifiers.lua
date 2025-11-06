local EnvironmentModifiers = {}
EnvironmentModifiers.__index = EnvironmentModifiers

function EnvironmentModifiers.new(event_bus)
    return setmetatable({
        event_bus = event_bus,
        modifiers = {}
    }, EnvironmentModifiers)
end

function EnvironmentModifiers:add(name, modifier)
    self.modifiers[name] = modifier
end

function EnvironmentModifiers:apply(environment_state)
    local state = environment_state
    for _, modifier in pairs(self.modifiers) do
        state = modifier(state)
    end
    self.event_bus:emit("environment_modified", state)
    return state
end

return EnvironmentModifiers
