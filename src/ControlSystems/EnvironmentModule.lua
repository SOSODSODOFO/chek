--!strict

local CollectionService = game:GetService("CollectionService")

export type CollisionProfile = {
tag: string?,
enabled: boolean?,
friction: number?,
elasticity: number?,
density: number?,
frictionWeight: number?,
elasticityWeight: number?,
}

export type Config = {
collisionProfiles: { CollisionProfile }?,
passThroughSpeed: number?,
autoPhysics: boolean?,
}

local DEFAULT_CONFIG: Config = {
collisionProfiles = {},
passThroughSpeed = 12,
autoPhysics = true,
}

local EnvironmentModule = {}
EnvironmentModule.__index = EnvironmentModule

function EnvironmentModule.new(config: Config?)
local merged = table.clone(DEFAULT_CONFIG)
if config then
for key, value in config do
merged[key] = value
end
end

local self = setmetatable({}, EnvironmentModule)
self._config = merged
self._profiles = merged.collisionProfiles
self._stateChanged = Instance.new("BindableEvent")
self._lastUpdate = 0
return self
end

function EnvironmentModule:Init()
if self._config.autoPhysics then
self:_applyProfiles()
end
end

function EnvironmentModule:Start()
-- Placeholder for hooking into world events.
end

function EnvironmentModule:_applyProfiles()
for _, profile in self._profiles do
if profile.tag then
for _, instance in CollectionService:GetTagged(profile.tag) do
if instance:IsA("BasePart") then
instance.CanCollide = profile.enabled ~= false
instance.CustomPhysicalProperties = PhysicalProperties.new(
profile.density or 1,
profile.friction or 0.3,
profile.elasticity or 0,
profile.frictionWeight or 1,
profile.elasticityWeight or 1
)
end
end
end
end
end

function EnvironmentModule:SetPassThroughSpeed(speed: number)
self._config.passThroughSpeed = speed
self._stateChanged:Fire({ type = "passThrough", value = speed })
end

function EnvironmentModule:Update(dt: number)
self._lastUpdate += dt
if self._lastUpdate > 1 then
self._lastUpdate = 0
self._stateChanged:Fire({ type = "heartbeat" })
end
end

function EnvironmentModule:ApplyProfile(profile: Config)
for key, value in profile do
self._config[key] = value
end
self._profiles = self._config.collisionProfiles or {}
self:_applyProfiles()
end

function EnvironmentModule:OnStateChanged()
return self._stateChanged.Event
end

function EnvironmentModule:Destroy()
self._stateChanged:Destroy()
end

return EnvironmentModule
