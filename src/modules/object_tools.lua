local ObjectTools = {}
ObjectTools.__index = ObjectTools

function ObjectTools.new(event_bus)
    return setmetatable({
        event_bus = event_bus,
        registry = {},
        modifiers = {}
    }, ObjectTools)
end

function ObjectTools:register(object)
    self.registry[object.id] = object
    self.event_bus:emit("object_registered", object)
end

function ObjectTools:update(id, properties)
    local object = self.registry[id]
    if not object then
        return
    end
    for key, value in pairs(properties) do
        object[key] = value
    end
    self.event_bus:emit("object_updated", object)
end

function ObjectTools:apply_modifier(name, fn)
    self.modifiers[name] = fn
end

function ObjectTools:process()
    for _, object in pairs(self.registry) do
        for _, modifier in pairs(self.modifiers) do
            modifier(object)
        end
    end
end

return ObjectTools
