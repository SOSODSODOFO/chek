--!strict

local Lighting = game:GetService("Lighting")

export type AtmosphereConfig = {
Density: number?,
Offset: number?,
Color: Color3?,
}

export type ModifierConfig = {
Atmosphere: AtmosphereConfig?,
FogEnd: number?,
Ambient: Color3?,
}

local EnvironmentModifiers = {}
EnvironmentModifiers.__index = EnvironmentModifiers

function EnvironmentModifiers.new()
local self = setmetatable({}, EnvironmentModifiers)
self._applied = {}
return self
end

function EnvironmentModifiers:Apply(config: ModifierConfig)
if config.Atmosphere then
local atmosphere = Lighting:FindFirstChildOfClass("Atmosphere") or Instance.new("Atmosphere")
atmosphere.Name = "ChekAtmosphere"
atmosphere.Density = config.Atmosphere.Density or atmosphere.Density
atmosphere.Offset = config.Atmosphere.Offset or atmosphere.Offset
atmosphere.Color = config.Atmosphere.Color or atmosphere.Color
atmosphere.Parent = Lighting
self._applied.Atmosphere = atmosphere
end

if config.FogEnd then
Lighting.FogEnd = config.FogEnd
self._applied.FogEnd = config.FogEnd
end

if config.Ambient then
Lighting.Ambient = config.Ambient
self._applied.Ambient = config.Ambient
end
end

function EnvironmentModifiers:Reset()
if self._applied.Atmosphere then
self._applied.Atmosphere:Destroy()
end
if self._applied.FogEnd then
Lighting.FogEnd = 100000
end
if self._applied.Ambient then
Lighting.Ambient = Color3.new(0, 0, 0)
end
self._applied = {}
end

return EnvironmentModifiers
