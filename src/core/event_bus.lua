local EventBus = {}
EventBus.__index = EventBus

function EventBus.new()
    return setmetatable({
        listeners = {}
    }, EventBus)
end

function EventBus:on(event, handler)
    local list = self.listeners[event]
    if not list then
        list = {}
        self.listeners[event] = list
    end
    table.insert(list, handler)
    return function()
        for i, cb in ipairs(list) do
            if cb == handler then
                table.remove(list, i)
                break
            end
        end
    end
end

function EventBus:emit(event, payload)
    local list = self.listeners[event]
    if not list then
        return
    end

    for _, handler in ipairs(list) do
        local ok, err = pcall(handler, payload)
        if not ok then
            io.stderr:write(string.format("Event handler error for '%s': %s\n", event, err))
        end
    end
end

return EventBus
