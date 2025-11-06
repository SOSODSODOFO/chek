package.path = package.path .. ';./?.lua;./src/?.lua;./src/?/init.lua;./src/?/?.lua'

local System = require("src.core.system")

local system = System.new()

local dt = 0.016
for frame = 1, 120 do
    system:update(dt)
    if frame % 30 == 0 then
        local modifiers = system:apply_environment_modifiers()
        system.performance:record("frame", dt)
        system.logger:info("Frame milestone", { frame = frame, modifiers = modifiers })
    end
end

local content = system.panel:get_active_content()
local display = system.display:render()

print("Active panel content:")
for key, value in pairs(content) do
    if type(value) == "table" then
        print("  " .. key .. ":")
        for innerKey, innerValue in pairs(value) do
            print(string.format("    %s: %s", innerKey, tostring(innerValue)))
        end
    else
        print(string.format("  %s: %s", key, tostring(value)))
    end
end

print("\nDisplay snapshot:\n" .. display)
