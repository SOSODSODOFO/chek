local ConfigManager = {}
ConfigManager.__index = ConfigManager

function ConfigManager.new(defaults)
    return setmetatable({
        defaults = defaults or {},
        overrides = {}
    }, ConfigManager)
end

function ConfigManager:get(key)
    if self.overrides[key] ~= nil then
        return self.overrides[key]
    end
    return self.defaults[key]
end

function ConfigManager:set(key, value)
    self.overrides[key] = value
end

function ConfigManager:merge(values)
    for key, value in pairs(values) do
        self.overrides[key] = value
    end
end

function ConfigManager:reset(key)
    if key then
        self.overrides[key] = nil
    else
        self.overrides = {}
    end
end

return ConfigManager
