local UpdateLoop = {}
UpdateLoop.__index = UpdateLoop

function UpdateLoop.new(logger)
    return setmetatable({
        logger = logger,
        components = {},
        paused = false
    }, UpdateLoop)
end

function UpdateLoop:register(component)
    table.insert(self.components, component)
end

function UpdateLoop:set_paused(value)
    self.paused = value
end

function UpdateLoop:step(dt)
    if self.paused then
        return
    end

    for _, component in ipairs(self.components) do
        if component.update then
            local ok, err = pcall(component.update, component, dt)
            if not ok and self.logger then
                self.logger:error("Component update failure", { component = tostring(component), error = err })
            end
        end
    end
end

return UpdateLoop
