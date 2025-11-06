local Renderer = {}
Renderer.__index = Renderer

function Renderer.new(event_bus, logger)
    return setmetatable({
        event_bus = event_bus,
        logger = logger,
        batches = {},
        last_frame_time = 0,
        frame_budget = 0.016
    }, Renderer)
end

function Renderer:add_batch(name, elements)
    self.batches[name] = {
        elements = elements,
        visible = true
    }
end

function Renderer:set_visibility(name, value)
    if self.batches[name] then
        self.batches[name].visible = value
    end
end

function Renderer:update(dt)
    self.last_frame_time = dt
    for name, batch in pairs(self.batches) do
        if batch.visible then
            self:render_batch(name, batch.elements)
        end
    end
    self.event_bus:emit("renderer_frame", { dt = dt, batches = self.batches })
end

function Renderer:render_batch(name, elements)
    local count = #elements
    if count > 0 then
        self.logger:trace("Rendering batch", { name = name, count = count })
    end
end

return Renderer
