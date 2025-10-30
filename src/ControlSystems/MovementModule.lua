--!strict

export type Config = {
navigationModes: { string }?,
defaultMode: string?,
altitudeRange: Vector2?,
speedRange: Vector2?,
smoothing: number?,
}

type MovementState = {
Mode: string,
Altitude: number,
Speed: number,
TargetAltitude: number,
TargetSpeed: number,
}

local DEFAULT_CONFIG: Config = {
navigationModes = { "Standard", "Orbit", "Spline" },
defaultMode = "Standard",
altitudeRange = Vector2.new(-100, 1000),
speedRange = Vector2.new(4, 120),
smoothing = 0.35,
}

local MovementModule = {}
MovementModule.__index = MovementModule

function MovementModule.new(config: Config?)
local merged = table.clone(DEFAULT_CONFIG)
if config then
for key, value in config do
merged[key] = value
end
end

local self = setmetatable({}, MovementModule)
self._config = merged
self._state = {
Mode = merged.defaultMode or DEFAULT_CONFIG.defaultMode,
Altitude = 0,
Speed = merged.speedRange and merged.speedRange.X or DEFAULT_CONFIG.speedRange.X,
TargetAltitude = 0,
TargetSpeed = merged.speedRange and merged.speedRange.X or DEFAULT_CONFIG.speedRange.X,
}
self._tweens = {}
self._stateChanged = Instance.new("BindableEvent")
return self
end

function MovementModule:Init()
-- Setup runtime dependencies or read persisted state if required.
end

function MovementModule:Start()
-- No-op placeholder for future logic.
end

function MovementModule:Update(dt: number)
local smoothing = self._config.smoothing or DEFAULT_CONFIG.smoothing
self._state.Altitude = self._state.Altitude + (self._state.TargetAltitude - self._state.Altitude) * smoothing
self._state.Speed = self._state.Speed + (self._state.TargetSpeed - self._state.Speed) * smoothing
self._stateChanged:Fire(table.clone(self._state))
end

function MovementModule:SetMode(mode: string)
if not table.find(self._config.navigationModes, mode) then
warn("Attempted to set unsupported navigation mode", mode)
return
end
self._state.Mode = mode
self._stateChanged:Fire(table.clone(self._state))
end

function MovementModule:SetAltitude(target: number)
local range = self._config.altitudeRange or DEFAULT_CONFIG.altitudeRange
self._state.TargetAltitude = math.clamp(target, range.X, range.Y)
end

function MovementModule:SetSpeed(target: number)
local range = self._config.speedRange or DEFAULT_CONFIG.speedRange
self._state.TargetSpeed = math.clamp(target, range.X, range.Y)
end

function MovementModule:ApplyProfile(profile: Config)
for key, value in profile do
self._config[key] = value
end
if profile.defaultMode then
self:SetMode(profile.defaultMode)
end
end

function MovementModule:GetState(): MovementState
return table.clone(self._state)
end

function MovementModule:OnStateChanged()
return self._stateChanged.Event
end

function MovementModule:Destroy()
for _, tween in self._tweens do
tween:Cancel()
end
self._stateChanged:Destroy()
end

return MovementModule
